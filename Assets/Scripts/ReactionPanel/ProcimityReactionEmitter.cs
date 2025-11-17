using UnityEngine;

public class ProximityReactionEmitter : MonoBehaviour
{
    [SerializeField] private string reactionKey = "Smile";
    [SerializeField] private float radius = 3f;
    [SerializeField] private LayerMask playerMask = ~0;
    [SerializeField] private Transform playerOverride;
    [SerializeField] private float holdDuration = 2f;
    [SerializeField] private int priority = 0;
    [SerializeField] private bool refreshWhileInside = false;
    [SerializeField] private float refreshInterval = 1f;

    private Transform player;
    private bool inside;
    private float nextRefresh;

    void Start()
    {
        if (playerOverride != null) player = playerOverride;
        else
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        var dist = Vector3.Distance(transform.position, player.position);
        bool nowInside = dist <= radius && IsInMask(player.gameObject.layer);

        if (nowInside && !inside)
        {
            Show();
            nextRefresh = Time.time + refreshInterval;
        }
        else if (!nowInside && inside)
        {
            Hide();
        }
        else if (nowInside && inside && refreshWhileInside && Time.time >= nextRefresh)
        {
            Show();
            nextRefresh = Time.time + refreshInterval;
        }

        inside = nowInside;
    }
    void OnDestroy()
    {
        
        if (player != null)
        {
            var svc = ReactionManager.Instance;
            if (svc != null)
            {
                svc.HideReaction(player.gameObject);
            }
        }
    }

    void OnDisable()
    {
       
        if (inside && player != null)
        {
            Hide();
            inside = false;

        }
    }
    private void Show()
    {
        var svc = ReactionManager.Instance;
        if (svc == null) return;
        var opts = new ReactionOptions
        {
            duration = holdDuration,
            screenOffset = Vector2.zero,
            priority = priority,
            overrideIfLowerPriority = false,
            queueIfBusy = false
        };
        svc.ShowReaction(player.gameObject, reactionKey, opts);
    }

    private void Hide()
    {
        var svc = ReactionManager.Instance;
        if (svc == null) return;
        svc.HideReaction(player.gameObject);
    }

    private bool IsInMask(int layer)
    {
        return (playerMask.value & (1 << layer)) != 0;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.25f);
        Gizmos.DrawSphere(transform.position, radius);
        Gizmos.color = new Color(1f, 0.6f, 0f, 1f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
