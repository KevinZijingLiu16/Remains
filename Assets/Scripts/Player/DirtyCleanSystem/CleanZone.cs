using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CleanZone : MonoBehaviour
{
    [Header("Clean Settings")]
    [SerializeField] private bool cleanAllParts = true;     
    [SerializeField] private int partsToClean = 1;         
    [SerializeField]
    private float cleanPerSecond = 0.25f;

 
    [SerializeField] private bool cleanOnEnter = false;
    [SerializeField, Range(0f, 1f)] private float enterCleanAmount = 0.1f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    private void Start()
    {
        var col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning("[CleanZone] Collider was not set as trigger, automatically fixed.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var dirtSystem = other.GetComponent<PlayerDirtSystem>();
        if (!dirtSystem) return;

        if (cleanOnEnter)
        {
            if (cleanAllParts) dirtSystem.RemoveDirtFromAll(enterCleanAmount);
            else dirtSystem.RemoveDirtFromRandom(partsToClean, enterCleanAmount);
        }

        if (enableDebugLogs)
            Debug.Log($"[CleanZone] Player ENTER at {transform.position}");
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var dirtSystem = other.GetComponent<PlayerDirtSystem>();
        if (!dirtSystem) return;

        float delta = cleanPerSecond * Time.deltaTime;

        if (cleanAllParts) dirtSystem.RemoveDirtFromAll(delta);
        else dirtSystem.RemoveDirtFromRandom(partsToClean, delta);
    }
}
