using UnityEngine;

/// <summary>
/// 让玩家变脏的区域
/// </summary>
[RequireComponent(typeof(Collider))]
public class DirtZone : MonoBehaviour
{
    [Header("Dirt Settings")]
    [SerializeField] private int partsToDirt = 1; // 每次接触变脏多少个部位
    [SerializeField] private bool dirtyAllParts = false; // 是否一次性全部变脏
    [SerializeField] private bool onlyTriggerOnce = false; // 是否只触发一次

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    private bool _hasTriggered = false;

    void Start()
    {
        // 确保Collider是Trigger
        var collider = GetComponent<Collider>();
        if (!collider.isTrigger)
        {
            collider.isTrigger = true;
            Debug.LogWarning("[DirtZone] Collider was not set as trigger, automatically fixed.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (onlyTriggerOnce && _hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            var dirtSystem = other.GetComponent<PlayerDirtSystem>();
            if (dirtSystem != null)
            {
                if (dirtyAllParts)
                {
                    dirtSystem.DirtyAllBodyParts();
                }
                else
                {
                    dirtSystem.DirtyRandomParts(partsToDirt);
                }

                _hasTriggered = true;

                if (enableDebugLogs)
                {
                    Debug.Log($"[DirtZone] Player entered dirt zone at {transform.position}");
                }
            }
        }
    }
}