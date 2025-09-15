// ReactionBubblePresenter.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class ReactionBubblePresenter : MonoBehaviour, IReactionPresenter
{
    [Header("Anchor & UI")]
    [SerializeField] private Transform anchor;      
    [SerializeField] private Image iconImage;        
    [SerializeField] private Canvas canvas;         

    [Header("Style")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0, 0.8f, 0);
    [SerializeField] private AnimationCurve fade = AnimationCurve.Linear(0, 1, 1, 1);
    [SerializeField] private float popScale = 1.1f;

    public Transform Anchor => anchor != null ? anchor : transform;
    public bool IsBusy => _showRoutine != null;
    public int CurrentPriority { get; private set; } = int.MinValue;

    private Coroutine _showRoutine;
    private Camera _cam;
    private Vector3 _baseScale;

    void Reset()
    {
        canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        canvas.sortingOrder = 1000;
        _cam = Camera.main;
    }

    void Awake()
    {
        if (canvas == null) canvas = GetComponent<Canvas>();
        if (_cam == null) _cam = Camera.main;
        _baseScale = transform.localScale == Vector3.zero ? Vector3.one : transform.localScale;
        HideImmediate();
    }

    void LateUpdate()
    {
       
        if (Anchor != null)
        {
            transform.position = Anchor.position + worldOffset;
        }
        if (_cam != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - _cam.transform.position);
        }
    }

    public void ShowReaction(Sprite sprite, in ReactionOptions options)
    {
        if (sprite == null) return;

     
        if (IsBusy && options.overrideIfLowerPriority == false && options.priority < CurrentPriority)
            return;

        CurrentPriority = options.priority;

        if (_showRoutine != null) StopCoroutine(_showRoutine);
        _showRoutine = StartCoroutine(CoShow(sprite, options.duration <= 0 ? 1.5f : options.duration));
    }

    public void HideReaction()
    {
        if (_showRoutine != null)
        {
            StopCoroutine(_showRoutine);
            _showRoutine = null;
        }
        HideImmediate();
    }

    private void HideImmediate()
    {
        if (iconImage != null) iconImage.enabled = false;
        var grp = GetComponent<CanvasGroup>();
        if (grp == null) grp = gameObject.AddComponent<CanvasGroup>();
        grp.alpha = 0f;
        transform.localScale = _baseScale;
        CurrentPriority = int.MinValue;
    }

    private IEnumerator CoShow(Sprite sprite, float duration)
    {
        
        var grp = GetComponent<CanvasGroup>();
        if (grp == null) grp = gameObject.AddComponent<CanvasGroup>();

        if (iconImage != null)
        {
            iconImage.sprite = sprite;
            iconImage.enabled = true;
        }

        // Pop-in
        float t = 0f;
        while (t < 0.12f)
        {
            t += Time.deltaTime;
            float k = t / 0.12f;
            grp.alpha = Mathf.Lerp(0f, 1f, k);
            transform.localScale = Vector3.Lerp(_baseScale, _baseScale * popScale, k);
            yield return null;
        }
        transform.localScale = _baseScale * popScale;

        // Hold
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            grp.alpha = fade.Evaluate(Mathf.Clamp01(1f - elapsed / duration)) * 1f; 
            yield return null;
        }

        // Out
        t = 0f;
        while (t < 0.1f)
        {
            t += Time.deltaTime;
            float k = t / 0.1f;
            grp.alpha = Mathf.Lerp(grp.alpha, 0f, k);
            transform.localScale = Vector3.Lerp(_baseScale * popScale, _baseScale, k);
            yield return null;
        }

        HideImmediate();
        _showRoutine = null;
    }
}
