using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class StickyFoamPlatform : MonoBehaviour, IFoamPlatform
{
    [Header("Sticky Settings")]
    public float stickDistance = 0.3f;
    public LayerMask stickableLayers = ~0;
    public float stickForce = 100f;
    public bool canStickToFoam = true;

    [Header("Platform Settings")]
    public float lifetime = 60f;
    public float stepHeight = 0.4f;
    public LayerMask playerLayer = 1;

    [Header("Visual")]
    public Color unstuckColor = Color.white;
    public Color stuckColor = Color.cyan;
    public Material foamMaterial;

    [Header("Trigger Control")]
    [SerializeField] private bool enableTriggerControl = true;

    private bool _isStuck = false;
    private bool _isTryingToStick = false;
    private Transform _stuckTo = null;
    private Vector3 _stuckLocalPosition;
    private Quaternion _stuckLocalRotation;
    private float _creationTime;

    private Collider _collider;
    private Rigidbody _rigidbody;
    private Renderer _renderer;
    private FixedJoint _stickJoint;

    public bool IsSteppable => _isStuck;
    public float PlatformHeight => stepHeight;
    public bool IsStuck => _isStuck;

    void Awake()
    {
        _collider = GetComponent<Collider>();
        _rigidbody = GetComponent<Rigidbody>();
        _renderer = GetComponent<Renderer>();
        _creationTime = Time.time;

        SetupFoamPhysics();
        SetupVisuals();
    }

    void Start()
    {
        _isTryingToStick = true;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (_isTryingToStick && !_isStuck)
        {
            if (_rigidbody.linearVelocity.magnitude < 4f)
            {
                TryToStick();
            }
        }

        if (_isStuck && _stuckTo == null)
        {
            UnstickFromSurface();
        }

        UpdateVisualFeedback();
    }

    private void SetupFoamPhysics()
    {
        _rigidbody.mass = 0.1f;
        _rigidbody.linearDamping = 0.5f;
        _rigidbody.angularDamping = 1f;

        _collider.isTrigger = false;

        PhysicsMaterial foamPhysics = new PhysicsMaterial("StickyFoamPhysics");
        foamPhysics.dynamicFriction = 0.8f;
        foamPhysics.bounciness = 0.1f;
        _collider.material = foamPhysics;
    }

    private void SetupVisuals()
    {
        if (_renderer == null) return;

        if (foamMaterial != null)
        {
            _renderer.material = foamMaterial;
        }

        if (_renderer.material.HasProperty("_Color"))
        {
            _renderer.material.color = unstuckColor;
        }
    }

    private void TryToStick()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, stickDistance, stickableLayers);

        Transform bestTarget = null;
        float bestScore = float.MinValue;

        foreach (var collider in nearbyColliders)
        {
            if (collider == _collider) continue;

            float score = EvaluateStickyTarget(collider);
            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = collider.transform;
            }
        }

        if (bestTarget != null && bestScore > 0f)
        {
            StickToSurface(bestTarget);
        }
    }

    private float EvaluateStickyTarget(Collider targetCollider)
    {
        float distance = Vector3.Distance(transform.position, targetCollider.ClosestPoint(transform.position));

        if (distance > stickDistance) return float.MinValue;

        var otherFoam = targetCollider.GetComponent<StickyFoamPlatform>();
        if (otherFoam != null)
        {
            if (!canStickToFoam || !otherFoam.IsStuck) return float.MinValue;

            Rigidbody otherRb = otherFoam.GetComponent<Rigidbody>();
            if (otherRb != null && otherRb.linearVelocity.magnitude > 1f) return float.MinValue;

            if (_rigidbody != null && _rigidbody.linearVelocity.magnitude > 1.5f) return float.MinValue;

            float ageBonus = (Time.time - otherFoam._creationTime) * 10f;
            float score = (stickDistance - distance) + ageBonus;
            return score;
        }
        else
        {
            Rigidbody targetRb = targetCollider.GetComponent<Rigidbody>();
            if (targetRb == null || targetRb.isKinematic)
            {
                return (stickDistance - distance) + 100f;
            }
            else
            {
                float speedPenalty = targetRb.linearVelocity.magnitude * 5f;
                return (stickDistance - distance) - speedPenalty + 50f;
            }
        }
    }

    private void StickToSurface(Transform target)
    {
        if (_isStuck) return;

        _isStuck = true;
        _isTryingToStick = false;
        _stuckTo = target;

        Collider targetCollider = target.GetComponent<Collider>();
        Vector3 closestPoint = targetCollider != null
            ? targetCollider.ClosestPoint(transform.position)
            : target.position;

        Vector3 directionToTarget = (closestPoint - transform.position).normalized;
        Vector3 stickPosition = closestPoint - directionToTarget * (stepHeight * 0.5f);

        transform.position = stickPosition;

        Vector3 surfaceNormal = (transform.position - closestPoint).normalized;
        if (surfaceNormal.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(surfaceNormal);
        }

        _stuckLocalPosition = target.InverseTransformPoint(transform.position);
        _stuckLocalRotation = Quaternion.Inverse(target.rotation) * transform.rotation;

        if (_rigidbody != null)
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.isKinematic = true;
        }

        CreateStickJoint(target);

        Debug.Log($"[StickyFoamPlatform] Stuck to {target.name} and became kinematic");
    }

    private void CreateStickJoint(Transform target)
    {
        Rigidbody targetRb = target.GetComponent<Rigidbody>();

        _rigidbody.isKinematic = true;

        if (targetRb != null && !targetRb.isKinematic)
        {
            _stickJoint = gameObject.AddComponent<FixedJoint>();
            _stickJoint.connectedBody = targetRb;
            _stickJoint.breakForce = float.MaxValue;
            _stickJoint.breakTorque = float.MaxValue;
        }

        _rigidbody.linearDamping = 10f;
        _rigidbody.angularDamping = 10f;
    }

    private void UnstickFromSurface()
    {
        if (!_isStuck) return;

        _isStuck = false;
        _stuckTo = null;

        if (_stickJoint != null)
        {
            Destroy(_stickJoint);
            _stickJoint = null;
        }

        _rigidbody.isKinematic = false;
        _rigidbody.linearDamping = 1f;
        _rigidbody.angularDamping = 3f;
        _collider.isTrigger = true;

        Debug.Log("[StickyFoamPlatform] Unstuck from surface");
    }

    void FixedUpdate()
    {
        if (_isStuck && _stuckTo != null && _stickJoint == null)
        {
            Vector3 targetWorldPos = _stuckTo.TransformPoint(_stuckLocalPosition);
            Quaternion targetWorldRot = _stuckTo.rotation * _stuckLocalRotation;

            transform.position = targetWorldPos;
            transform.rotation = targetWorldRot;
        }
    }

    private void UpdateVisualFeedback()
    {
        if (_renderer == null || !_renderer.material.HasProperty("_Color")) return;

        Color targetColor = _isStuck ? stuckColor : unstuckColor;

        float remainingTime = lifetime - (Time.time - _creationTime);
        float alpha = Mathf.Clamp01(remainingTime / lifetime);

        if (remainingTime < 10f)
        {
            alpha *= (Mathf.Sin(Time.time * 5f) * 0.3f + 0.7f);
        }

        targetColor.a = alpha;
        _renderer.material.color = targetColor;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (IsInLayerMask(collision.gameObject.layer, playerLayer))
        {
            OnPlayerStepped();
        }

        if (_isTryingToStick && !_isStuck)
        {
            if (IsInLayerMask(collision.gameObject.layer, stickableLayers))
            {
                StickToSurface(collision.transform);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        HandleTriggerCollision(other, "OnTriggerEnter");
    }

    private void HandleTriggerCollision(Collider other, string eventType)
    {
      
        if (!_isStuck || !enableTriggerControl || other == null) return;

        if (other.CompareTag("head"))
        {
          
            _collider.isTrigger = true;
            Debug.Log($"[{eventType}] {gameObject.name} (stuck) hit 'head' object: {other.name}. Set to Trigger.");
        }
        else if (other.CompareTag("wheel"))
        {
           
            _collider.isTrigger = false;
            Debug.Log($"[{eventType}] {gameObject.name} (stuck) hit 'wheel' object: {other.name}. Set to NOT Trigger.");
        }
    }

    public void OnPlayerStepped()
    {
        Debug.Log("[StickyFoamPlatform] Player stepped on sticky foam platform");
    }

    private bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

    public void ForceStickTo(Transform target)
    {
        if (!_isStuck)
        {
            StickToSurface(target);
        }
    }

    public void ForceUnstick()
    {
        UnstickFromSurface();
    }

    public void ExtendLifetime(float additionalTime)
    {
        lifetime += additionalTime;
    }

    public void SetTriggerStateWhenStuck(bool isTrigger)
    {
        if (_isStuck)
        {
            _collider.isTrigger = isTrigger;
            Debug.Log($"[Manual] {gameObject.name} (stuck) trigger state set to: {isTrigger}");
        }
        else
        {
            Debug.LogWarning($"[Manual] {gameObject.name} is not stuck, cannot change trigger state.");
        }
    }

    void OnDestroy()
    {
        if (_stickJoint != null)
        {
            Destroy(_stickJoint);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = _isStuck ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stickDistance);

        if (_isStuck && _stuckTo != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, _stuckTo.position);
        }
    }
}