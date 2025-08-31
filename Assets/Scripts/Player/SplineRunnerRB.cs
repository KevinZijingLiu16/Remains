using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

public class SplineRunnerRB : MonoBehaviour
{
    [Header("Spline Path")]
    public SplineContainer splineContainer;
    public bool loop = true;
    [Range(0.01f, 1f)] public float stick = 0.9f;   // 0.85~1

    [Header("Movement")]
    public float moveSpeed = 6f;
    [Tooltip("Air control factor (0~1). 0 = no control in air, 1 = same as on ground.")]
    [Range(0f, 1f)] public float airControl = 0.6f;
    [Tooltip("Reserved bank angle in degrees. Can be used for visual tilt effects.")]
    public float bankDegrees = 0f;

    [Header("Jump & Ground")]
    public float jumpHeight = 2f;
    public LayerMask groundLayers = ~0;     
    public LayerMask obstacleLayers = ~0;  
   
    public LayerMask movableLayers = 0;    
    public float groundCheckRadius = 0.25f;
    [Tooltip("Offset for the ground check sphere (world space). Typically slightly upward to avoid false negatives.")]
    public Vector3 groundCheckOffset = new Vector3(0f, 0.1f, 0f);

    [Header("Push Movable Settings")]
    [Tooltip("Ratio of movement transferred to movable rigidbodies. 1 = full transfer.")]
    public float pushTransferRatio = 1.0f;
    [Tooltip("Maximum push distance per frame (m). Prevents excessive displacement.")]
    public float pushMaxPerStep = 1.0f;
    [Tooltip("Push skin (m). Small offset to avoid overlapping with other colliders during pushing.")]
    public float pushSkin = 0.02f;

    [Header("Input (New Input System)")]
    public InputActionProperty moveAction;
    public InputActionProperty jumpAction;

    [Header("Anti Jitter")]
   
    public bool decoupleYFromSpline = true;

    public bool faceHorizontalOnly = true;

    Rigidbody _rb;
    Spline _spline;
    float _t;                  // 0..1
    float _approxLen = 1f;
    float _cachedMove;
    bool _cachedJump;
    bool _grounded;
    bool _lastMovePositive = true;

    [Header("Visual (mesh only)")]
    public Transform meshRoot;
    public float meshFacingOffsetY = 0f;
    [Tooltip("Flip interpolation speed (0 = instant, 1 = very slow).")]
    [Range(0f, 1f)] public float meshFlipLerp = 0.25f;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous; 
    }

    void OnEnable()
    {
        moveAction.action?.Enable();
        jumpAction.action?.Enable();

        if (!ValidateSpline(out _spline))
        {
            Debug.LogWarning("[SplineRunnerRB] SplineContainer 未设置或样条点数<2。");
            enabled = false;
            return;
        }
        RecomputeLength();
        SnapToNearestOnSpline();
    }

    void OnDisable()
    {
        moveAction.action?.Disable();
        jumpAction.action?.Disable();
    }

    void Update()
    {
        _cachedMove = moveAction.action != null ? moveAction.action.ReadValue<float>() : 0f;
        if (jumpAction.action != null && jumpAction.action.WasPressedThisFrame())
            _cachedJump = true;
    }

    void FixedUpdate()
    {
        if (!ValidateSpline(out _spline)) return;

        float4x4 world = (float4x4)splineContainer.transform.localToWorldMatrix;

        _grounded = IsGrounded();
        float control = _grounded ? 1f : Mathf.Clamp01(airControl);

        if (!math.isfinite(_approxLen) || _approxLen < 0.001f) RecomputeLength();

     
        float dt = Time.fixedDeltaTime;
        float dir = Mathf.Sign(_cachedMove);
        float speed = Mathf.Abs(_cachedMove) * moveSpeed * control;      // m/s
        float dtAsT = (speed * dt) / math.max(0.001f, _approxLen);
        _t += dir * dtAsT;
        _t = loop ? Mathf.Repeat(_t, 1f) : Mathf.Clamp01(_t);
        if (!math.isfinite(_t)) _t = 0f;

    
        float3 localPos = _spline.EvaluatePosition(_t);
        float3 localTan = _spline.EvaluateTangent(_t);
        float3 worldPos = math.transform(world, localPos);
        float3 worldTan = math.rotate(world, localTan);
        if (math.lengthsq(worldTan) < 1e-6f)
        {
            float t2 = loop ? math.frac(_t + 0.001f) : math.clamp(_t + 0.001f, 0f, 1f);
            float3 lp2 = _spline.EvaluatePosition(t2);
            float3 wp2 = math.transform(world, lp2);
            worldTan = wp2 - worldPos;
        }
        worldTan = math.normalize(worldTan);

       
        Vector3 current = _rb.position;
        Vector3 splineXZ = new Vector3(worldPos.x, current.y, worldPos.z);
        Vector3 targetPos = decoupleYFromSpline ? splineXZ : (Vector3)worldPos;

       
        Vector3 newPos = Vector3.Lerp(current, targetPos, Mathf.Clamp01(stick));
        if (!float.IsNaN(newPos.x))
        {
            Vector3 from = _rb.position;
            Vector3 to = newPos;

          
            if (SweepAnyCollider(from, to, out Vector3 safe, out RaycastHit hit, obstacleLayers | movableLayers))
            {
                
                if (IsInLayerMask(hit.collider.gameObject.layer, movableLayers))
                {
                    
                    Vector3 remaining = to - safe;

                   
                    Vector3 horizSplineFwdAtPlayer = (Vector3)worldTan;
                    horizSplineFwdAtPlayer.y = 0f;
                    if (horizSplineFwdAtPlayer.sqrMagnitude > 1e-6f) horizSplineFwdAtPlayer.Normalize();
                    else horizSplineFwdAtPlayer = transform.forward; // 兜底

                    
                    Vector3 effectivePush = Vector3.Project(remaining, horizSplineFwdAtPlayer);
                    float pushDist = Mathf.Clamp(effectivePush.magnitude * Mathf.Sign(Vector3.Dot(effectivePush, horizSplineFwdAtPlayer)) * pushTransferRatio,
                                                 -pushMaxPerStep, pushMaxPerStep);

                    Rigidbody targetRB = hit.rigidbody;
                    if (targetRB != null && targetRB.isKinematic == false)
                    {
                       
                        MoveMovableAlongSpline(targetRB, pushDist);
                    }

                    
                    _rb.MovePosition(safe);

                    
                    _t -= dir * dtAsT;
                }
                else
                {
                   
                    Vector3 remaining = to - safe;
                    Vector3 slide = SlideAlongNormal(remaining, hit.normal);

                    if (slide.sqrMagnitude > 1e-6f)
                    {
                        Vector3 slideTarget = safe + slide;
                        if (SweepAnyCollider(safe, slideTarget, out Vector3 safe2, out _, obstacleLayers | movableLayers))
                            _rb.MovePosition(safe2);
                        else
                            _rb.MovePosition(slideTarget);
                    }
                    else
                    {
                        _rb.MovePosition(safe);
                    }

                    _t -= dir * dtAsT;
                }
            }
            else
            {
                _rb.MovePosition(to); 
            }
        }

      
        if (_cachedJump && _grounded)
        {
            float g = -Physics.gravity.y;
            float vy = Mathf.Sqrt(2f * g * Mathf.Max(0.01f, jumpHeight));
            var v = _rb.linearVelocity; v.y = vy; _rb.linearVelocity = v; 
        }
        _cachedJump = false;

     
        if (Mathf.Abs(_cachedMove) > 0.001f) _lastMovePositive = _cachedMove > 0f;

        Vector3 fwdPhysics = (Vector3)worldTan;
        if (faceHorizontalOnly)
        {
            fwdPhysics.y = 0f;
            if (fwdPhysics.sqrMagnitude < 1e-8f) fwdPhysics = transform.forward;
            else fwdPhysics.Normalize();
        }

        Vector3 up = Vector3.up;
        Quaternion rootLook = Quaternion.LookRotation(fwdPhysics, up);
        _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, rootLook, _grounded ? 0.2f : 0.1f));

       
        if (meshRoot != null)
        {
            float targetYaw = (_lastMovePositive ? 0f : 180f) + meshFacingOffsetY;
            Quaternion targetLocal = Quaternion.Euler(0f, targetYaw, 0f);
            meshRoot.localRotation = (meshFlipLerp <= 0f)
                ? targetLocal
                : Quaternion.Slerp(meshRoot.localRotation, targetLocal, meshFlipLerp);
        }
    }

   
    bool ValidateSpline(out Spline spline)
    {
        spline = null;
        if (!splineContainer) return false;
        spline = splineContainer.Spline;
        return spline != null && spline.Count >= 2;
    }

    void RecomputeLength()
    {
        if (!ValidateSpline(out _spline)) return;
        float4x4 world = (float4x4)splineContainer.transform.localToWorldMatrix;
        _approxLen = math.max(0.1f, SplineUtility.CalculateLength(_spline, world));
    }

    void SnapToNearestOnSpline()
    {
        if (!ValidateSpline(out _spline)) return;
        float4x4 world = (float4x4)splineContainer.transform.localToWorldMatrix;
        float4x4 worldToLocal = math.inverse(world);

        float3 posLocal = math.transform(worldToLocal, (float3)_rb.position);
        SplineUtility.GetNearestPoint(_spline, posLocal, out float3 nearestLocal, out float t);
        _t = math.isfinite(t) ? t : 0f;
        float3 nearestWorld = math.transform(world, nearestLocal);

        Vector3 snap = decoupleYFromSpline
            ? new Vector3(nearestWorld.x, _rb.position.y, nearestWorld.z)
            : (Vector3)nearestWorld;

        if (!float.IsNaN(snap.x)) _rb.position = snap;
    }

    bool IsGrounded()
    {
        Vector3 origin = _rb.worldCenterOfMass + groundCheckOffset;
        return Physics.CheckSphere(origin, groundCheckRadius, groundLayers, QueryTriggerInteraction.Ignore);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (_rb)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(_rb.worldCenterOfMass + groundCheckOffset, groundCheckRadius);
        }
    }
#endif

  
    bool _warnedNoColliderOnce = false;

 
    bool GetCapsule(out Vector3 p1, out Vector3 p2, out float radius)
    {
        p1 = p2 = default; radius = 0f;
        var cap = GetComponentInChildren<CapsuleCollider>();
        if (!cap) return false;

        Vector3 center = cap.transform.TransformPoint(cap.center);
        Vector3 up = cap.transform.up;
        Vector3 s = cap.transform.lossyScale;

        float r = cap.radius * Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.z));
        float h = Mathf.Max(cap.height * Mathf.Abs(s.y), r * 2f);
        float half = h * 0.5f - r;

        p1 = center + up * half;
        p2 = center - up * half;
        radius = r;
        return true;
    }

 
    bool GetSphere(out Vector3 center, out float radius)
    {
        center = default; radius = 0f;
        var sph = GetComponentInChildren<SphereCollider>();
        if (!sph) return false;

        Vector3 s = sph.transform.lossyScale;
        float maxScale = Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));
        radius = sph.radius * maxScale;
        center = sph.transform.TransformPoint(sph.center);
        return true;
    }

   
    bool GetBox(out Vector3 center, out Vector3 halfExtents, out Quaternion orientation)
    {
        center = default; halfExtents = default; orientation = Quaternion.identity;
        var box = GetComponentInChildren<BoxCollider>();
        if (!box) return false;

        Vector3 s = box.transform.lossyScale;
        Vector3 sizeWS = new Vector3(
            box.size.x * Mathf.Abs(s.x),
            box.size.y * Mathf.Abs(s.y),
            box.size.z * Mathf.Abs(s.z)
        );
        halfExtents = sizeWS * 0.5f;
        center = box.transform.TransformPoint(box.center);
        orientation = box.transform.rotation;
        return true;
    }

   
    bool SweepAnyCollider(Vector3 from, Vector3 to, out Vector3 safePos, out RaycastHit hit, int layerMask)
    {
        safePos = to; hit = default;

        Vector3 delta = to - from;
        float dist = delta.magnitude;
        if (dist < 1e-5f) return false;

        Vector3 dir = delta / dist;
        bool any = false;

    
        Vector3 preOffset = from - _rb.position;

        if (GetCapsule(out var c1, out var c2, out var cr))
        {
            any = true;
            c1 += preOffset; c2 += preOffset;

            if (Physics.CapsuleCast(c1, c2, cr, dir, out hit, dist, layerMask, QueryTriggerInteraction.Ignore))
            {
                float skin = 0.02f;
                safePos = from + dir * Mathf.Max(hit.distance - skin, 0f);
                return true;
            }
            return false;
        }

        if (GetSphere(out var sc, out var sr))
        {
            any = true;
            sc += preOffset;

            if (Physics.SphereCast(sc, sr, dir, out hit, dist, layerMask, QueryTriggerInteraction.Ignore))
            {
                float skin = 0.02f;
                safePos = from + dir * Mathf.Max(hit.distance - skin, 0f);
                return true;
            }
            return false;
        }

        if (GetBox(out var bc, out var bhe, out var brot))
        {
            any = true;
            bc += preOffset;

            if (Physics.BoxCast(bc, bhe, dir, out hit, brot, dist, layerMask, QueryTriggerInteraction.Ignore))
            {
                float skin = 0.02f;
                safePos = from + dir * Mathf.Max(hit.distance - skin, 0f);
                return true;
            }
            return false;
        }

        if (!any && !_warnedNoColliderOnce)
        {
            _warnedNoColliderOnce = true;
            Debug.LogWarning("[SplineRunnerRB] No Collider found on self or children. Sweep is disabled -> risk of tunneling.");
        }

        return false;
    }

    
    Vector3 SlideAlongNormal(Vector3 desiredDelta, Vector3 hitNormal)
    {
        return Vector3.ProjectOnPlane(desiredDelta, hitNormal);
    }

   
    void MoveMovableAlongSpline(Rigidbody targetRB, float pushDistance)
    {
        if (!ValidateSpline(out var spl) || Mathf.Abs(pushDistance) < 1e-6f) return;

     
        float4x4 world = (float4x4)splineContainer.transform.localToWorldMatrix;
        float4x4 worldToLocal = math.inverse(world);

        Vector3 objPos = targetRB.position;
        float3 objPosLocal = math.transform(worldToLocal, (float3)objPos);
        SplineUtility.GetNearestPoint(spl, objPosLocal, out float3 nearestLocal, out float tObj);

        if (!math.isfinite(tObj)) return;

       
        float approxLen = math.max(0.1f, _approxLen);
        float dt = pushDistance / approxLen;
        float tTarget = loop ? Mathf.Repeat(tObj + dt, 1f) : Mathf.Clamp01(tObj + dt);

      
        float3 worldOnSpline = math.transform(world, spl.EvaluatePosition(tTarget));
        Vector3 objTarget = new Vector3(worldOnSpline.x, objPos.y, worldOnSpline.z);

      
        if (SweepForBody(targetRB, objPos, objTarget, out Vector3 safe, out RaycastHit _,
                         obstacleLayers | movableLayers))
        {
            targetRB.MovePosition(safe);
        }
        else
        {
            targetRB.MovePosition(objTarget);
        }
    }


    bool SweepForBody(Rigidbody body, Vector3 from, Vector3 to, out Vector3 safePos, out RaycastHit hit, int mask)
    {
        safePos = to; hit = default;
        Vector3 delta = to - from;
        float dist = delta.magnitude;
        if (dist < 1e-5f) return false;

        Vector3 dir = delta / dist;
        bool any = false;
        Vector3 preOffset = from - body.position;

     
        if (GetCapsule(body.transform, out var c1, out var c2, out var cr))
        {
            any = true;
            c1 += preOffset; c2 += preOffset;
            if (Physics.CapsuleCast(c1, c2, cr, dir, out hit, dist, mask, QueryTriggerInteraction.Ignore))
            {
                safePos = from + dir * Mathf.Max(hit.distance - pushSkin, 0f);
                return true;
            }
            return false;
        }

   
        if (GetSphere(body.transform, out var sc, out var sr))
        {
            any = true; sc += preOffset;
            if (Physics.SphereCast(sc, sr, dir, out hit, dist, mask, QueryTriggerInteraction.Ignore))
            {
                safePos = from + dir * Mathf.Max(hit.distance - pushSkin, 0f);
                return true;
            }
            return false;
        }

       
        if (GetBox(body.transform, out var bc, out var bhe, out var brot))
        {
            any = true; bc += preOffset;
            if (Physics.BoxCast(bc, bhe, dir, out hit, brot, dist, mask, QueryTriggerInteraction.Ignore))
            {
                safePos = from + dir * Mathf.Max(hit.distance - pushSkin, 0f);
                return true;
            }
            return false;
        }

      
        return false;
    }

 
    bool GetCapsule(Transform root, out Vector3 p1, out Vector3 p2, out float radius)
    {
        p1 = p2 = default; radius = 0f;
        var cap = root.GetComponentInChildren<CapsuleCollider>();
        if (!cap) return false;

        Vector3 center = cap.transform.TransformPoint(cap.center);
        Vector3 up = cap.transform.up;
        Vector3 s = cap.transform.lossyScale;

        float r = cap.radius * Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.z));
        float h = Mathf.Max(cap.height * Mathf.Abs(s.y), r * 2f);
        float half = h * 0.5f - r;

        p1 = center + up * half;
        p2 = center - up * half;
        radius = r;
        return true;
    }
    bool GetSphere(Transform root, out Vector3 center, out float radius)
    {
        center = default; radius = 0f;
        var sph = root.GetComponentInChildren<SphereCollider>();
        if (!sph) return false;

        Vector3 s = sph.transform.lossyScale;
        float maxScale = Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));
        radius = sph.radius * maxScale;
        center = sph.transform.TransformPoint(sph.center);
        return true;
    }
    bool GetBox(Transform root, out Vector3 center, out Vector3 halfExtents, out Quaternion orientation)
    {
        center = default; halfExtents = default; orientation = Quaternion.identity;
        var box = root.GetComponentInChildren<BoxCollider>();
        if (!box) return false;

        Vector3 s = box.transform.lossyScale;
        Vector3 sizeWS = new Vector3(
            box.size.x * Mathf.Abs(s.x),
            box.size.y * Mathf.Abs(s.y),
            box.size.z * Mathf.Abs(s.z)
        );
        halfExtents = sizeWS * 0.5f;
        center = box.transform.TransformPoint(box.center);
        orientation = box.transform.rotation;
        return true;
    }


    static bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

    public float GetCurrentT()
    {
        return _t;
    }
}
