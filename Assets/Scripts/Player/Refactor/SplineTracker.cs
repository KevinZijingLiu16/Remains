using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;


public class SplineTracker : MonoBehaviour
{
    [Header("Spline Configuration")]
    public SplineContainer splineContainer;
    [SerializeField] private bool loop = true;

    private Spline _spline;
    private float _approxLength = 1f;
    private float4x4 _worldMatrix;
    private float4x4 _worldToLocalMatrix;

    public bool Loop => loop;
    public float ApproximateLength => _approxLength;
    public Spline Spline => _spline;

    void Awake()
    {
        ValidateAndCacheSpline();
        RecomputeLength();
    }

    void Start()
    {
     
        RefreshMatrices();
    }

 
    public bool ValidateAndCacheSpline()
    {
        if (!splineContainer)
        {
            Debug.LogWarning("[SplineTracker] SplineContainer 未设置");
            return false;
        }

        _spline = splineContainer.Spline;
        if (_spline == null || _spline.Count < 2)
        {
            Debug.LogWarning("[SplineTracker] Spline无效或点数小于2");
            return false;
        }

        RefreshMatrices();
        return true;
    }

 
    public void RefreshMatrices()
    {
        if (splineContainer != null)
        {
            _worldMatrix = (float4x4)splineContainer.transform.localToWorldMatrix;
            _worldToLocalMatrix = math.inverse(_worldMatrix);
        }
    }

    public void RecomputeLength()
    {
        if (!ValidateAndCacheSpline()) return;

        RefreshMatrices();
        _approxLength = math.max(0.1f, SplineUtility.CalculateLength(_spline, _worldMatrix));
    }


    public float MoveAlongSpline(float currentT, float speedMetersPerSec, float deltaTime, float direction)
    {
        if (!ValidateAndCacheSpline()) return currentT;
        if (!math.isfinite(_approxLength) || _approxLength < 0.001f) RecomputeLength();

        float dtAsT = (speedMetersPerSec * deltaTime) / math.max(0.001f, _approxLength);
        float newT = currentT + direction * dtAsT;

        newT = loop ? Mathf.Repeat(newT, 1f) : Mathf.Clamp01(newT);
        return math.isfinite(newT) ? newT : currentT;
    }

 
    public Vector3 GetWorldPositionAtT(float t)
    {
        if (!ValidateAndCacheSpline()) return Vector3.zero;

        float3 localPos = _spline.EvaluatePosition(t);
        float3 worldPos = math.transform(_worldMatrix, localPos);
        return (Vector3)worldPos;
    }

   
    public Vector3 GetWorldTangentAtT(float t)
    {
        if (!ValidateAndCacheSpline()) return Vector3.forward;

        float3 localTan = _spline.EvaluateTangent(t);
        float3 worldTan = math.rotate(_worldMatrix, localTan);

     
        if (math.lengthsq(worldTan) < 1e-6f)
        {
            float t2 = loop ? math.frac(t + 0.001f) : math.clamp(t + 0.001f, 0f, 1f);
            float3 lp1 = _spline.EvaluatePosition(t);
            float3 lp2 = _spline.EvaluatePosition(t2);
            float3 wp1 = math.transform(_worldMatrix, lp1);
            float3 wp2 = math.transform(_worldMatrix, lp2);
            worldTan = wp2 - wp1;
        }

        return math.lengthsq(worldTan) > 1e-6f ? (Vector3)math.normalize(worldTan) : Vector3.forward;
    }

    public SplineProjectionResult ProjectWorldPointToSpline(Vector3 worldPoint)
    {
        if (!ValidateAndCacheSpline())
        {
            return new SplineProjectionResult { t = 0f, worldPosition = worldPoint, isValid = false };
        }

        RefreshMatrices();
        float3 localPoint = math.transform(_worldToLocalMatrix, (float3)worldPoint);
        SplineUtility.GetNearestPoint(_spline, localPoint, out float3 nearestLocal, out float t);

        if (!math.isfinite(t)) t = 0f;

        float3 nearestWorld = math.transform(_worldMatrix, nearestLocal);

        return new SplineProjectionResult
        {
            t = t,
            worldPosition = (Vector3)nearestWorld,
            isValid = true
        };
    }

  
    public Vector3 SnapToNearestPoint(Vector3 currentWorldPos, bool decoupleY = true)
    {
        var projection = ProjectWorldPointToSpline(currentWorldPos);
        if (!projection.isValid) return currentWorldPos;

        return decoupleY
            ? new Vector3(projection.worldPosition.x, currentWorldPos.y, projection.worldPosition.z)
            : projection.worldPosition;
    }

   
    public float GetTAfterDistance(float startT, float distanceMeters)
    {
        if (!ValidateAndCacheSpline()) return startT;

        float dt = distanceMeters / math.max(0.1f, _approxLength);
        float newT = startT + dt;
        return loop ? Mathf.Repeat(newT, 1f) : Mathf.Clamp01(newT);
    }


    public bool IsValid()
    {
        return splineContainer != null && _spline != null && _spline.Count >= 2;
    }
}


public struct SplineProjectionResult
{
    public float t;
    public Vector3 worldPosition;
    public bool isValid;
}