using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StateTransitionTrigger : MonoBehaviour
{
    [Header("Trigger Configuration")]
    [SerializeField] private StateTransitionConfig config;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private Color gizmoColor = new Color(1f, 0.5f, 0f, 0.3f);

    private bool hasTriggered = false;
    private float lastTriggerTime = -999f;
    private Collider triggerCollider;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
    
        if (Time.time - lastTriggerTime < config.cooldownTime)
            return;

       
        if (hasTriggered && config.triggerOnce)
            return;

       
        if (!other.CompareTag("Player"))
            return;

     
        TriggerStateTransition();
    }

 
    private void TriggerStateTransition()
    {
      
        Collider[] colliders = Physics.OverlapSphere(transform.position, config.affectRadius);

        foreach (Collider col in colliders)
        {
            ITriggerable triggerable = col.GetComponent<ITriggerable>();
            if (triggerable == null) continue;

        
            if (!config.IsTargetValid(triggerable)) continue;

         
            if (!triggerable.CanBeTriggered(config)) continue;

          
            triggerable.OnTriggered(config);
        }

        hasTriggered = true;
        lastTriggerTime = Time.time;

        Debug.Log($"[StateTransitionTrigger] Triggered: {config.targetStateName}");
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos || config == null) return;

        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, config.affectRadius);

 
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }

    private void OnDrawGizmosSelected()
    {
        if (config == null) return;

   
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawSphere(transform.position, config.affectRadius);
    }
}