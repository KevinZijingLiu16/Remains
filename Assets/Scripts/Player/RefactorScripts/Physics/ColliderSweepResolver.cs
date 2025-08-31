using UnityEngine;

[DisallowMultipleComponent]
public class ColliderSweepResolver : MonoBehaviour, ISweepResolver
{
    [SerializeField]private float defaultSkin = 0.02f;

    public bool SweepSelf(Transform selfRoot, Rigidbody selfRb, Vector3 from, Vector3 to,
                          LayerMask layerMask, float skin,
                          out Vector3 safePos, out RaycastHit hit)
    {
        float s = skin > 0f ? skin : defaultSkin;
        return SweepWithRoot(selfRoot, selfRb ? selfRb.position : selfRoot.position,
                             from, to, layerMask, s, out safePos, out hit);
    }

    public bool SweepBody(Rigidbody body, Vector3 from, Vector3 to,
                          LayerMask layerMask, float skin,
                          out Vector3 safePos, out RaycastHit hit)
    {
        float s = skin > 0f ? skin : defaultSkin;
        return SweepWithRoot(body.transform, body.position, from, to, layerMask, s, out safePos, out hit);
    }


    private static bool SweepWithRoot(Transform root, Vector3 bodyReferencePos,
                                      Vector3 from, Vector3 to, LayerMask mask, float skin,
                                      out Vector3 safePos, out RaycastHit hit)
    {
        safePos = to; hit = default;

        Vector3 delta = to - from;
        float dist = delta.magnitude;
        if (dist < 1e-5f) return false;

        Vector3 dir = delta / Mathf.Max(dist, 1e-6f);
        Vector3 preOffset = from - bodyReferencePos;

    
        if (TryGetCapsule(root, out var c1, out var c2, out var cr))
        {
            c1 += preOffset; c2 += preOffset;
            if (Physics.CapsuleCast(c1, c2, cr, dir, out hit, dist, mask, QueryTriggerInteraction.Ignore))
            {
                safePos = from + dir * Mathf.Max(hit.distance - skin, 0f);
                return true;
            }
            return false;
        }

        if (TryGetSphere(root, out var sc, out var sr))
        {
            sc += preOffset;
            if (Physics.SphereCast(sc, sr, dir, out hit, dist, mask, QueryTriggerInteraction.Ignore))
            {
                safePos = from + dir * Mathf.Max(hit.distance - skin, 0f);
                return true;
            }
            return false;
        }

        if (TryGetBox(root, out var bc, out var bhe, out var brot))
        {
            bc += preOffset;
            if (Physics.BoxCast(bc, bhe, dir, out hit, brot, dist, mask, QueryTriggerInteraction.Ignore))
            {
                safePos = from + dir * Mathf.Max(hit.distance - skin, 0f);
                return true;
            }
            return false;
        }

        
        return false;
    }

    private static bool TryGetCapsule(Transform root, out Vector3 p1, out Vector3 p2, out float radius)
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

    private static bool TryGetSphere(Transform root, out Vector3 center, out float radius)
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

    private static bool TryGetBox(Transform root, out Vector3 center, out Vector3 halfExtents, out Quaternion orientation)
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
}
