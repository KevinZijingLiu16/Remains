using UnityEngine;

public class PlatformColliderController : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    [Header("Compatibility")]
    [SerializeField] private bool respectStickyFoamState = true;

    private Collider selfCollider;
    private StickyFoamPlatform stickyFoam;

    void Awake()
    {
       
        selfCollider = GetComponent<Collider>();
        stickyFoam = GetComponent<StickyFoamPlatform>();

        if (selfCollider == null)
        {
            Debug.LogError($"[ColliderTriggerController] No Collider found on {gameObject.name}!");
            enabled = false;
        }

        if (stickyFoam != null && enableDebugLogs)
        {
            Debug.Log($"[ColliderTriggerController] StickyFoamPlatform detected. Compatibility mode enabled.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        HandleCollision(other, "OnTriggerEnter");
    }

    void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.collider, "OnCollisionEnter");
    }

    private void HandleCollision(Collider other, string eventType)
    {
        if (other == null) return;

       
        if (respectStickyFoamState && stickyFoam != null && stickyFoam.IsStuck)
        {
            if (enableDebugLogs)
                Debug.Log($"[{eventType}] {gameObject.name} is stuck to surface, skipping trigger state change.");
            return;
        }

        if (other.CompareTag("head"))
        {
           
            selfCollider.isTrigger = true;

            if (enableDebugLogs)
                Debug.Log($"[{eventType}] {gameObject.name} hit 'head' object: {other.name}. Set to Trigger.");
        }
        else if (other.CompareTag("wheel"))
        {
            
            selfCollider.isTrigger = false;

            if (enableDebugLogs)
                Debug.Log($"[{eventType}] {gameObject.name} hit 'wheel' object: {other.name}. Set to NOT Trigger.");
        }
    }

   
    public void SetTriggerState(bool isTrigger, bool forceOverride = false)
    {
        if (selfCollider == null) return;

        if (!forceOverride && respectStickyFoamState && stickyFoam != null && stickyFoam.IsStuck)
        {
            if (enableDebugLogs)
                Debug.Log($"[Manual] {gameObject.name} is stuck, cannot change trigger state unless forced.");
            return;
        }

        selfCollider.isTrigger = isTrigger;

        if (enableDebugLogs)
            Debug.Log($"[Manual] {gameObject.name} trigger state set to: {isTrigger}");
    }

    public bool IsTrigger()
    {
        return selfCollider != null ? selfCollider.isTrigger : false;
    }
}
