using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// 负责管理spline轨道相关的逻辑，可以被多个对象共享使用
/// </summary>
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
        // 确保在其他组件使用前就准备好
        RefreshMatrices();
    }

    /// <summary>
    /// 验证并缓存spline引用
    /// </summary>
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

    /// <summary>
    /// 刷新变换矩阵缓存
    /// </summary>
    public void RefreshMatrices()
    {
        if (splineContainer != null)
        {
            _worldMatrix = (float4x4)splineContainer.transform.localToWorldMatrix;
            _worldToLocalMatrix = math.inverse(_worldMatrix);
        }
    }

    /// <summary>
    /// 重新计算spline长度
    /// </summary>
    public void RecomputeLength()
    {
        if (!ValidateAndCacheSpline()) return;

        RefreshMatrices();
        _approxLength = math.max(0.1f, SplineUtility.CalculateLength(_spline, _worldMatrix));
    }

    /// <summary>
    /// 在spline上移动指定的t值距离
    /// </summary>
    /// <param name="currentT">当前t值</param>
    /// <param name="speedMetersPerSec">移动速度(米/秒)</param>
    /// <param name="deltaTime">时间增量</param>
    /// <param name="direction">移动方向(1=正向, -1=反向)</param>
    /// <returns>新的t值</returns>
    public float MoveAlongSpline(float currentT, float speedMetersPerSec, float deltaTime, float direction)
    {
        if (!ValidateAndCacheSpline()) return currentT;
        if (!math.isfinite(_approxLength) || _approxLength < 0.001f) RecomputeLength();

        float dtAsT = (speedMetersPerSec * deltaTime) / math.max(0.001f, _approxLength);
        float newT = currentT + direction * dtAsT;

        newT = loop ? Mathf.Repeat(newT, 1f) : Mathf.Clamp01(newT);
        return math.isfinite(newT) ? newT : currentT;
    }

    /// <summary>
    /// 获取指定t值处的世界坐标位置
    /// </summary>
    public Vector3 GetWorldPositionAtT(float t)
    {
        if (!ValidateAndCacheSpline()) return Vector3.zero;

        float3 localPos = _spline.EvaluatePosition(t);
        float3 worldPos = math.transform(_worldMatrix, localPos);
        return (Vector3)worldPos;
    }

    /// <summary>
    /// 获取指定t值处的世界坐标切线方向
    /// </summary>
    public Vector3 GetWorldTangentAtT(float t)
    {
        if (!ValidateAndCacheSpline()) return Vector3.forward;

        float3 localTan = _spline.EvaluateTangent(t);
        float3 worldTan = math.rotate(_worldMatrix, localTan);

        // 如果切线长度太小，尝试使用稍微偏移的点来计算方向
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

    /// <summary>
    /// 将世界坐标点投影到spline上，返回最近点的t值和世界坐标
    /// </summary>
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

    /// <summary>
    /// 将对象快照到spline上最近的点
    /// </summary>
    public Vector3 SnapToNearestPoint(Vector3 currentWorldPos, bool decoupleY = true)
    {
        var projection = ProjectWorldPointToSpline(currentWorldPos);
        if (!projection.isValid) return currentWorldPos;

        return decoupleY
            ? new Vector3(projection.worldPosition.x, currentWorldPos.y, projection.worldPosition.z)
            : projection.worldPosition;
    }

    /// <summary>
    /// 获取在spline上移动特定距离后的t值
    /// </summary>
    public float GetTAfterDistance(float startT, float distanceMeters)
    {
        if (!ValidateAndCacheSpline()) return startT;

        float dt = distanceMeters / math.max(0.1f, _approxLength);
        float newT = startT + dt;
        return loop ? Mathf.Repeat(newT, 1f) : Mathf.Clamp01(newT);
    }

    /// <summary>
    /// 验证当前spline是否有效
    /// </summary>
    public bool IsValid()
    {
        return splineContainer != null && _spline != null && _spline.Count >= 2;
    }
}

/// <summary>
/// Spline投影结果
/// </summary>
public struct SplineProjectionResult
{
    public float t;
    public Vector3 worldPosition;
    public bool isValid;
}