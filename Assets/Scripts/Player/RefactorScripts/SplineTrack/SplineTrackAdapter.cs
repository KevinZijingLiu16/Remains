using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[DisallowMultipleComponent]
public class SplineTrackAdapter : MonoBehaviour, ISplineTrack
{
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private bool loop = true;

    private Spline _spline;
    private float _approxLen = 1f;

    public bool Loop
    {
        get => loop;
        set => loop = value;
    }

    public float LengthMeters => _approxLen;

    void Reset()
    {
        if (splineContainer == null)
            splineContainer = GetComponent<SplineContainer>();
    }

    void Awake()
    {
        ValidateSpline(out _spline);
        RecomputeLength();
    }

    public void RecomputeLength()
    {
        if (!ValidateSpline(out _spline)) return;
        float4x4 world = (float4x4)splineContainer.transform.localToWorldMatrix;
        _approxLen = math.max(0.1f, SplineUtility.CalculateLength(_spline, world));
    }

    public Vector3 EvaluatePosition(float t01)
    {
        if (!ValidateSpline(out _spline)) return transform.position;
        float4x4 world = (float4x4)splineContainer.transform.localToWorldMatrix;
        t01 = loop ? Mathf.Repeat(t01, 1f) : Mathf.Clamp01(t01);
        float3 local = _spline.EvaluatePosition(t01);
        return math.transform(world, local);
    }

    public Vector3 EvaluateTangent(float t01)
    {
        if (!ValidateSpline(out _spline)) return transform.forward;
        float4x4 world = (float4x4)splineContainer.transform.localToWorldMatrix;
        t01 = loop ? Mathf.Repeat(t01, 1f) : Mathf.Clamp01(t01);

        float3 localTan = _spline.EvaluateTangent(t01);
        float3 worldTan = math.rotate(world, localTan);

        // 若切线长度异常小，用一个极小偏移的差分近似
        if (math.lengthsq(worldTan) < 1e-6f)
        {
            float t2 = loop ? math.frac(t01 + 0.001f) : math.clamp(t01 + 0.001f, 0f, 1f);
            float3 lp1 = _spline.EvaluatePosition(t01);
            float3 lp2 = _spline.EvaluatePosition(t2);
            float4x4 w = (float4x4)splineContainer.transform.localToWorldMatrix;
            worldTan = math.transform(w, lp2) - math.transform(w, lp1);
        }

        return ((Vector3)worldTan).sqrMagnitude > 1e-8f
            ? ((Vector3)worldTan).normalized
            : transform.forward;
    }

    public float GetNearestT(Vector3 worldPos)
    {
        if (!ValidateSpline(out _spline)) return 0f;
        float4x4 world = (float4x4)splineContainer.transform.localToWorldMatrix;
        float4x4 inv = math.inverse(world);
        float3 local = math.transform(inv, (float3)worldPos);
        SplineUtility.GetNearestPoint(_spline, local, out _, out float t);
        return math.isfinite(t) ? t : 0f;
    }

    private bool ValidateSpline(out Spline spline)
    {
        spline = null;
        if (!splineContainer) return false;
        spline = splineContainer.Spline;
        return spline != null && spline.Count >= 2;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (splineContainer == null)
            splineContainer = GetComponent<SplineContainer>();
        RecomputeLength();
    }
#endif
}
