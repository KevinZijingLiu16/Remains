using UnityEngine;

public interface ISweepResolver
{

    bool SweepSelf(Transform selfRoot, Rigidbody selfRb, Vector3 from, Vector3 to,
                   LayerMask layerMask, float skin,
                   out Vector3 safePos, out RaycastHit hit);

   
    bool SweepBody(Rigidbody body, Vector3 from, Vector3 to,
                   LayerMask layerMask, float skin,
                   out Vector3 safePos, out RaycastHit hit);
}
