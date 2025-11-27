using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using System.Collections;

public class TimelineSceneTransition : MonoBehaviour
{
    [Header("Timeline Reference")]
    [Tooltip("The PlayableDirector (Timeline) to monitor")]
    public PlayableDirector timeline;

    [Header("Scene Settings")]
    [Tooltip("Name of the scene to load after timeline completes")]
    public string nextSceneName;

    [Tooltip("OR use scene build index instead (leave nextSceneName empty to use this)")]
    public int nextSceneBuildIndex = -1;

    [Header("Transition Timing")]
    [Tooltip("How many seconds before the timeline ends should we transition? (0 = wait for timeline to finish)")]
    public float transitionBeforeEnd = 1.0f;

    [Tooltip("Additional delay after the trigger point (can be 0)")]
    public float delayBeforeTransition = 0.0f;

    [Tooltip("Should the transition happen automatically?")]
    public bool autoTransition = true;

    [Header("Optional Fade Effect")]
    [Tooltip("Fade to black before scene transition")]
    public bool useFadeEffect = false;
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1.0f;

    [Header("Debug")]
    public bool enableDebugLogs = true;

    private bool _hasTransitioned = false;
    private bool _isMonitoring = false;
    private double _timelineDuration = 0;

    void Start()
    {
        // Find PlayableDirector if not assigned
        if (timeline == null)
        {
            timeline = GetComponent<PlayableDirector>();
            if (timeline == null)
            {
                timeline = FindFirstObjectByType<PlayableDirector>();
            }
        }

        if (timeline == null)
        {
            Debug.LogError("[TimelineSceneTransition] No PlayableDirector found! Please assign a Timeline.");
            return;
        }

        // Validate scene settings
        if (string.IsNullOrEmpty(nextSceneName) && nextSceneBuildIndex < 0)
        {
            Debug.LogError("[TimelineSceneTransition] No next scene specified! Set either nextSceneName or nextSceneBuildIndex.");
            return;
        }

        // Get timeline duration
        if (timeline.playableAsset != null)
        {
            _timelineDuration = timeline.playableAsset.duration;

            if (enableDebugLogs)
                Debug.Log($"[TimelineSceneTransition] Timeline duration: {_timelineDuration:F2}s, will transition at: {(_timelineDuration - transitionBeforeEnd):F2}s");
        }

        // Setup fade canvas if using fade effect
        if (useFadeEffect && fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
        }

        // Start monitoring
        _isMonitoring = true;
    }

    void Update()
    {
        if (!_isMonitoring || _hasTransitioned || !autoTransition)
            return;

        if (timeline == null || timeline.playableAsset == null)
            return;

        // Check if timeline is playing
        if (timeline.state != PlayState.Playing)
            return;

        // Calculate trigger time (duration - transitionBeforeEnd)
        double triggerTime = _timelineDuration - transitionBeforeEnd;
        double currentTime = timeline.time;

        // Trigger transition when we reach the trigger time
        if (currentTime >= triggerTime && !_hasTransitioned)
        {
            if (enableDebugLogs)
                Debug.Log($"[TimelineSceneTransition] Reached trigger time ({currentTime:F2}s >= {triggerTime:F2}s). Starting transition...");

            _hasTransitioned = true;
            _isMonitoring = false;
            StartCoroutine(TransitionToNextScene());
        }
    }

    void OnDestroy()
    {
        _isMonitoring = false;
    }

    private IEnumerator TransitionToNextScene()
    {
        // Optional delay
        if (delayBeforeTransition > 0f)
        {
            if (enableDebugLogs)
                Debug.Log($"[TimelineSceneTransition] Waiting {delayBeforeTransition}s before transition...");

            yield return new WaitForSeconds(delayBeforeTransition);
        }

        // Optional fade to black
        if (useFadeEffect && fadeCanvasGroup != null)
        {
            if (enableDebugLogs)
                Debug.Log("[TimelineSceneTransition] Fading to black...");

            yield return StartCoroutine(FadeOut());
        }

        // Load next scene
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            if (enableDebugLogs)
                Debug.Log($"[TimelineSceneTransition] Loading scene: {nextSceneName}");

            SceneManager.LoadScene(nextSceneName);
        }
        else if (nextSceneBuildIndex >= 0)
        {
            if (enableDebugLogs)
                Debug.Log($"[TimelineSceneTransition] Loading scene at build index: {nextSceneBuildIndex}");

            SceneManager.LoadScene(nextSceneBuildIndex);
        }
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;
    }

    // Public method to manually trigger transition
    public void TriggerTransition()
    {
        if (_hasTransitioned)
        {
            Debug.LogWarning("[TimelineSceneTransition] Transition already triggered!");
            return;
        }

        if (enableDebugLogs)
            Debug.Log("[TimelineSceneTransition] Manually triggering transition...");

        _hasTransitioned = true;
        _isMonitoring = false;
        StartCoroutine(TransitionToNextScene());
    }

    // Public method to skip to next scene immediately
    public void SkipToNextScene()
    {
        if (enableDebugLogs)
            Debug.Log("[TimelineSceneTransition] Skipping directly to next scene...");

        _isMonitoring = false;

        // Stop the timeline
        if (timeline != null && timeline.state == PlayState.Playing)
        {
            timeline.Stop();
        }

        // Load scene immediately
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else if (nextSceneBuildIndex >= 0)
        {
            SceneManager.LoadScene(nextSceneBuildIndex);
        }
    }
}