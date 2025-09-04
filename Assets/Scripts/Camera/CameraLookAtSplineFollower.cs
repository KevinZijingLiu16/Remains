using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;


[DisallowMultipleComponent]
public class CameraLookAtSplineFollower : MonoBehaviour
{
    [Header("References")]
  
    [SerializeField] private SplineRunnerRB playerRunner;

    [SerializeField] private Transform playerTransform;

    public SplineContainer splineContainer;

    [Header("Input (New Input System)")]

    public InputActionProperty moveAction;

    [Header("Motion Settings")]

    [SerializeField] private bool loop = true;

    [SerializeField] private bool decoupleYFromSpline = true;

    [SerializeField] private float offsetAlongSplineMeters = 2.0f;

    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.6f, 0f);
    [Range(0.01f, 1f)]

    [SerializeField] private float stick = 0.9f;

    [Header("Facing")]
   
    [SerializeField] private bool faceHorizontalOnly = true;
 
    [SerializeField] private bool rotateAlongTangent = false;

    // runtime
    Spline _spline;
    float _t;              
    float _approxLen = 1f;   
    float _cachedMove;       

    void Awake()
    {
        ResolveRefs();

        if (!ValidateSpline(out _spline))
        {
            Debug.LogWarning("[CameraLookAtSplineFollower] No valid spline.");
            enabled = false;
            return;
        }

        RecomputeLength();

        
        float tPlayer = GetNearestTOnSpline(playerTransform ? playerTransform.position : transform.position);
        float dtOffset = Mathf.Clamp(offsetAlongSplineMeters / math.max(0.001f, _approxLen), -1f, 1f);
        _t = loop ? Mathf.Repeat(tPlayer + dtOffset, 1f) : Mathf.Clamp01(tPlayer + dtOffset);

        SnapToT(_t, immediate: true);
    }

    void OnEnable()
    {
        moveAction.action?.Enable();
    }

    void OnDisable()
    {
        moveAction.action?.Disable();
    }

    void Update()
    {
        _cachedMove = moveAction.action != null ? moveAction.action.ReadValue<float>() : 0f;
    }

    void FixedUpdate()
    {
        if (!ValidateSpline(out _spline)) return;
        if (playerRunner == null) return;

   
        float playerT = playerRunner.GetCurrentT(); 

        float dtOffset = offsetAlongSplineMeters / math.max(0.001f, _approxLen);
        _t = loop ? Mathf.Repeat(playerT + dtOffset, 1f) : Mathf.Clamp01(playerT + dtOffset);

        SnapToT(_t, immediate: false);
    }



    void SnapToT(float t, bool immediate)
    {
        float4x4 world = (float4x4)splineContainer.transform.localToWorldMatrix;

        float3 wp = math.transform(world, _spline.EvaluatePosition(t));
        float3 wt = math.rotate(world, _spline.EvaluateTangent(t));
        if (math.lengthsq(wt) < 1e-6f) wt = new float3(0, 0, 1);

        Vector3 pos = (Vector3)wp;

        if (decoupleYFromSpline)
        {
           
            float baseY = (playerTransform ? playerTransform.position.y : pos.y) + worldOffset.y;
            pos.y = baseY;

          
            pos.x += worldOffset.x;
            pos.z += worldOffset.z;
        }
        else
        {
           
            pos += worldOffset;
        }

        
        if (immediate || stick >= 0.999f) transform.position = pos;
        else transform.position = Vector3.Lerp(transform.position, pos, Mathf.Clamp01(stick));

     
        if (rotateAlongTangent)
        {
            Vector3 fwd = (Vector3)wt;
            if (faceHorizontalOnly)
            {
                fwd.y = 0f;
                if (fwd.sqrMagnitude > 1e-8f) fwd.Normalize();
                else fwd = transform.forward;
            }
            transform.rotation = Quaternion.LookRotation(fwd, Vector3.up);
        }
    }

 
    void ResolveRefs()
    {
        if (playerRunner == null) playerRunner = FindFirstObjectByType<SplineRunnerRB>();
        if (playerRunner != null && playerTransform == null) playerTransform = playerRunner.transform;
        if (splineContainer == null && playerRunner != null) splineContainer = playerRunner.splineContainer;

        //reuse player input actions
        if (moveAction.action == null && playerRunner != null && playerRunner.moveAction.action != null)
        {
            moveAction = playerRunner.moveAction;
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
        float4x4 world = (float4x4)splineContainer.transform.localToWorldMatrix;
        _approxLen = math.max(0.1f, SplineUtility.CalculateLength(splineContainer.Spline, world));
    }

    float GetNearestTOnSpline(Vector3 worldPos)
    {
        float4x4 world = (float4x4)splineContainer.transform.localToWorldMatrix;
        float4x4 inv = math.inverse(world);
        float3 pLocal = math.transform(inv, (float3)worldPos);

        SplineUtility.GetNearestPoint(splineContainer.Spline, pLocal, out float3 _, out float t);
        return math.isfinite(t) ? t : 0f;
    }
}
