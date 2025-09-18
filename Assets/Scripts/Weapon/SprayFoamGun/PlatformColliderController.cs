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
        // 获取自身的Collider组件
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

        // 如果有StickyFoamPlatform组件且已经粘附，则不修改trigger状态
        if (respectStickyFoamState && stickyFoam != null && stickyFoam.IsStuck)
        {
            if (enableDebugLogs)
                Debug.Log($"[{eventType}] {gameObject.name} is stuck to surface, skipping trigger state change.");
            return;
        }

        if (other.CompareTag("head"))
        {
            // 碰到head标签的物体，设置为trigger
            selfCollider.isTrigger = true;

            if (enableDebugLogs)
                Debug.Log($"[{eventType}] {gameObject.name} hit 'head' object: {other.name}. Set to Trigger.");
        }
        else if (other.CompareTag("wheel"))
        {
            // 碰到wheel标签的物体，设置为非trigger
            selfCollider.isTrigger = false;

            if (enableDebugLogs)
                Debug.Log($"[{eventType}] {gameObject.name} hit 'wheel' object: {other.name}. Set to NOT Trigger.");
        }
    }

    /// <summary>
    /// 手动设置Collider状态（兼容StickyFoam）
    /// </summary>
    /// <param name="isTrigger">是否为trigger</param>
    /// <param name="forceOverride">是否强制覆盖（忽略StickyFoam状态）</param>
    public void SetTriggerState(bool isTrigger, bool forceOverride = false)
    {
        if (selfCollider == null) return;

        // 检查是否需要尊重StickyFoam状态
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

    /// <summary>
    /// 获取当前trigger状态
    /// </summary>
    /// <returns>当前是否为trigger</returns>
    public bool IsTrigger()
    {
        return selfCollider != null ? selfCollider.isTrigger : false;
    }
}
