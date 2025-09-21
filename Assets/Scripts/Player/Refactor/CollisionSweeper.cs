using UnityEngine;


public class CollisionSweeper : MonoBehaviour
{
    [Header("Collision Settings")]
    [SerializeField] private float defaultSkin = 0.02f;

    private bool _warnedNoColliderOnce = false;

   
    public bool SweepPath(Rigidbody rb, Vector3 from, Vector3 to, out Vector3 safePos, out RaycastHit hit, int layerMask, float skin = -1f)
    {
        if (skin < 0f) skin = defaultSkin;

        safePos = to;
        hit = default;

        Vector3 delta = to - from;
        float dist = delta.magnitude;
        if (dist < 1e-5f) return false;

        Vector3 dir = delta / dist;
        Vector3 preOffset = from - rb.position;



     
        if (GetCapsuleFromRigidbody(rb, out var c1, out var c2, out var cr))
        {
            c1 += preOffset;
            c2 += preOffset;

            if (Physics.CapsuleCast(c1, c2, cr, dir, out hit, dist, layerMask, QueryTriggerInteraction.Ignore))
            {
                safePos = from + dir * Mathf.Max(hit.distance - skin, 0f);
                return true;
            }
            return false;
        }

     
        if (GetSphereFromRigidbody(rb, out var sc, out var sr))
        {
            sc += preOffset;

            if (Physics.SphereCast(sc, sr, dir, out hit, dist, layerMask, QueryTriggerInteraction.Ignore))
            {
                safePos = from + dir * Mathf.Max(hit.distance - skin, 0f);
                return true;
            }
            return false;
        }

     
        if (GetBoxFromRigidbody(rb, out var bc, out var bhe, out var brot))
        {
            bc += preOffset;

            if (Physics.BoxCast(bc, bhe, dir, out hit, brot, dist, layerMask, QueryTriggerInteraction.Ignore))
            {
                safePos = from + dir * Mathf.Max(hit.distance - skin, 0f);
                return true;
            }
            return false;
        }

    
        if (!_warnedNoColliderOnce)
        {
            _warnedNoColliderOnce = true;
            Debug.LogWarning($"[CollisionSweeper] No suitable Collider found on {rb.name}. Sweep is disabled.");
        }

        return false;
    }

    public Vector3 SlideAlongNormal(Vector3 desiredDelta, Vector3 hitNormal)
    {
        return Vector3.ProjectOnPlane(desiredDelta, hitNormal);
    }


    private bool GetCapsuleFromRigidbody(Rigidbody rb, out Vector3 p1, out Vector3 p2, out float radius)
    {
        p1 = p2 = default;
        radius = 0f;

        var cap = rb.GetComponentInChildren<CapsuleCollider>();
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


    private bool GetSphereFromRigidbody(Rigidbody rb, out Vector3 center, out float radius)
    {
        center = default;
        radius = 0f;

        var sph = rb.GetComponentInChildren<SphereCollider>();
        if (!sph) return false;

        Vector3 s = sph.transform.lossyScale;
        float maxScale = Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));
        radius = sph.radius * maxScale;
        center = sph.transform.TransformPoint(sph.center);
        return true;
    }


    private bool GetBoxFromRigidbody(Rigidbody rb, out Vector3 center, out Vector3 halfExtents, out Quaternion orientation)
    {
        center = default;
        halfExtents = default;
        orientation = Quaternion.identity;

        var box = rb.GetComponentInChildren<BoxCollider>();
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


    public static bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}