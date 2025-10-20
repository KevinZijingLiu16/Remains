using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CleanZone : MonoBehaviour
{
    [Header("Clean Settings")]
    [SerializeField] private int partsToClean = 1; // 每次接触清洁多少个部位
    [SerializeField] private bool cleanAllParts = false; // 是否一次性全部清洁
    [SerializeField] private bool continuousCleaning = false; // 是否持续清洁
    [SerializeField] private float cleaningInterval = 1f; // 持续清洁的间隔
    [SerializeField] private bool onlyTriggerOnce = false; // 是否只触发一次

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    private bool _hasTriggered = false;
    private float _cleaningTimer = 0f;
    private PlayerDirtSystem _currentPlayerInZone;

    void Start()
    {
        // 确保Collider是Trigger
        var collider = GetComponent<Collider>();
        if (!collider.isTrigger)
        {
            collider.isTrigger = true;
            Debug.LogWarning("[CleanZone] Collider was not set as trigger, automatically fixed.");
        }
    }

    void Update()
    {
        if (continuousCleaning && _currentPlayerInZone != null)
        {
            _cleaningTimer += Time.deltaTime;

            if (_cleaningTimer >= cleaningInterval)
            {
                CleanPlayer(_currentPlayerInZone);
                _cleaningTimer = 0f;
            }
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
                _currentPlayerInZone = dirtSystem;
                CleanPlayer(dirtSystem);

                _hasTriggered = true;

                if (enableDebugLogs)
                {
                    Debug.Log($"[CleanZone] Player entered clean zone at {transform.position}");
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _currentPlayerInZone = null;
            _cleaningTimer = 0f;
        }
    }

    private void CleanPlayer(PlayerDirtSystem dirtSystem)
    {
        if (cleanAllParts)
        {
            dirtSystem.CleanAllBodyParts();
        }
        else
        {
            for (int i = 0; i < partsToClean; i++)
            {
                if (!dirtSystem.CleanRandomDirtyPart())
                    break; // 没有脏的部位了
            }
        }
    }
}