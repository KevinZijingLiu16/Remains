using UnityEngine;

public class NPCProximityReaction : MonoBehaviour
{
    [SerializeField] private string reactionKey = "Smile";
    [SerializeField] private float radius = 3f;
    [SerializeField] private float holdDuration = 2f;
    [SerializeField] private int priority = 0;

    private Transform player;
    private bool showing;

    void Start()
    {
        var go = GameObject.FindGameObjectWithTag("Player");
        if (go != null) player = go.transform;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= radius)
        {
            if (!showing)
            {
                Show();
                showing = true;
            }
        }
        else
        {
            if (showing)
            {
                Hide();
                showing = false;
            }
        }
    }

    private void Show()
    {
        var svc = ReactionManager.Instance;
        if (svc == null) return;

        var opts = new ReactionOptions
        {
            duration = holdDuration,
            priority = priority
        };

        svc.ShowReaction(gameObject, reactionKey, opts);
    }

    private void Hide()
    {
        var svc = ReactionManager.Instance;
        if (svc == null) return;

        svc.HideReaction(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, radius);
    }
}
