using UnityEngine;

public class PowerReactionController : MonoBehaviour
{
    [SerializeField] private PlayerPower playerPower;
    [SerializeField] private string lowPowerKey = "LowPower";
    [SerializeField] private string highPowerKey = "HighPower";
    [SerializeField] private int lowThreshold = 20;
    [SerializeField] private int highThreshold = 80;
    [SerializeField] private float showDuration = 1.5f;
    [SerializeField] private int priority = 100;
    [SerializeField] private bool holdWhileInState = true;
    [SerializeField] private float refreshInterval = 1.0f;

    private enum State { None, Low, High, Normal }
    private State state = State.None;
    private float nextRefresh;

    void Awake()
    {
        if (playerPower == null) playerPower = GetComponentInParent<PlayerPower>();
    }

    void OnEnable()
    {
        if (playerPower != null)
        {
            playerPower.OnPowerChanged += OnPowerChanged;
            EvaluateImmediate(playerPower.Current, playerPower.Max);
        }
    }

    void OnDisable()
    {
        if (playerPower != null) playerPower.OnPowerChanged -= OnPowerChanged;
    }

    void Update()
    {
        if (!holdWhileInState) return;
        if (state == State.Low || state == State.High)
        {
            if (Time.time >= nextRefresh)
            {
                ShowCurrentState();
                nextRefresh = Time.time + refreshInterval;
            }
        }
    }

    private void OnPowerChanged(int current, int max)
    {
        EvaluateImmediate(current, max);
    }

    private void EvaluateImmediate(int current, int max)
    {
        if (current <= lowThreshold)
        {
            if (state != State.Low)
            {
                state = State.Low;
                Show(lowPowerKey);
                nextRefresh = Time.time + refreshInterval;
            }
        }
        else if (current >= highThreshold)
        {
            if (state != State.High)
            {
                state = State.High;
                Show(highPowerKey);
                nextRefresh = Time.time + refreshInterval;
            }
        }
        else
        {
            if (state != State.Normal)
            {
                state = State.Normal;
                Hide();
            }
        }
    }

    private void ShowCurrentState()
    {
        if (state == State.Low) Show(lowPowerKey);
        else if (state == State.High) Show(highPowerKey);
    }

    private void Show(string key)
    {
        var svc = ReactionManager.Instance;
        if (svc == null) return;
        var opts = new ReactionOptions
        {
            duration = showDuration,
            screenOffset = Vector2.zero,
            priority = priority,
            overrideIfLowerPriority = false,
            queueIfBusy = false
        };
        svc.ShowReaction(gameObject, key, opts);
    }

    private void Hide()
    {
        var svc = ReactionManager.Instance;
        if (svc == null) return;
        svc.HideReaction(gameObject);
    }
}
