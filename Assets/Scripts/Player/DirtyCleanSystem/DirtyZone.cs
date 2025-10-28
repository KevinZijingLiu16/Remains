using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DirtZone : MonoBehaviour
{
    [Header("Dirt Settings")]
    [SerializeField] private int partsToDirt = 1;
    [SerializeField] private bool dirtyAllParts = false;
    [SerializeField] private bool onlyTriggerOnce = false;

    [Header("Continuous Dirt")]
    [SerializeField, Tooltip("每秒增加多少脏污(0~1)，比如 0.2 表示约5秒从干净到全黑")]
    private float dirtPerSecond = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    private bool _hasTriggered = false;

    private void Start()
    {
        var col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning("[DirtZone] Collider was not set as trigger, automatically fixed.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (onlyTriggerOnce && _hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        var dirtSystem = other.GetComponent<PlayerDirtSystem>();
        if (!dirtSystem) return;

        // 可选：进入时就先给一点点脏污的“起步值”
        // dirtSystem.AddDirtToAll(0.05f);

        _hasTriggered = true;

        if (enableDebugLogs)
            Debug.Log($"[DirtZone] Player ENTER at {transform.position}");
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var dirtSystem = other.GetComponent<PlayerDirtSystem>();
        if (!dirtSystem) return;

        float delta = dirtPerSecond * Time.deltaTime;

        if (dirtyAllParts)
            dirtSystem.AddDirtToAll(delta);
        else
            dirtSystem.AddDirtToRandom(partsToDirt, delta);
    }
}
