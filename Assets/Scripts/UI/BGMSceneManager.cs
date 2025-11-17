using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BGMSceneManager : MonoBehaviour
{
    public static BGMSceneManager Instance { get; private set; }

    [System.Serializable]
    public class SceneBGMConfig
    {
        public string sceneName;
        public string bgmIdentifier;
        public string bgmSoundName;
        [Range(0f, 1f)] public float volume = 0.5f;
        public float fadeInTime = 1f;
        public float fadeOutTime = 0.5f;
    }

    [Header("Scene BGM Configurations")]
    [SerializeField]
    private SceneBGMConfig[] sceneConfigs = new SceneBGMConfig[]
    {
        new SceneBGMConfig { sceneName = "OpeningUI", bgmIdentifier = "BGM_Opening", bgmSoundName = "BGM_Opening", volume = 0.5f },
        new SceneBGMConfig { sceneName = "DemoV1New", bgmIdentifier = "BGM_Game", bgmSoundName = "GameTheme", volume = 0.5f },
        new SceneBGMConfig { sceneName = "Level1", bgmIdentifier = "BGM_Level1", bgmSoundName = "Level1Theme", volume = 0.5f }
    };

    private string currentBGMIdentifier;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("[BGMSceneManager] Initialized");
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[BGMSceneManager] Scene loaded: {scene.name}");

        // Find the BGM config for this scene
        SceneBGMConfig config = GetSceneConfig(scene.name);

        if (config != null)
        {
            if (currentBGMIdentifier != config.bgmIdentifier)
            {
                // Different BGM needed, switch it
                SwitchBGM(config);
            }
            else
            {
                Debug.Log($"[BGMSceneManager] Same BGM already playing: {currentBGMIdentifier}");
            }
        }
        else
        {
            // No BGM config for this scene, stop current BGM
            Debug.Log($"[BGMSceneManager] No BGM config for scene: {scene.name}");
            StopCurrentBGM();
        }
    }

    private SceneBGMConfig GetSceneConfig(string sceneName)
    {
        foreach (var config in sceneConfigs)
        {
            if (config.sceneName == sceneName)
            {
                return config;
            }
        }
        return null;
    }

    private void SwitchBGM(SceneBGMConfig newConfig)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeBGMTransition(newConfig));
    }

    private IEnumerator FadeBGMTransition(SceneBGMConfig newConfig)
    {
        if (!string.IsNullOrEmpty(currentBGMIdentifier))
        {
            Debug.Log($"[BGMSceneManager] Fading out: {currentBGMIdentifier}");

            float fadeOutTime = 0.5f;
            float startVolume = SoundManager.Instance?.GetBGMVolume() ?? 1f;
            float elapsedTime = 0f;

            while (elapsedTime < fadeOutTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeOutTime;
                float volume = Mathf.Lerp(startVolume, 0f, t);

                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.SetNamedLoopVolume(currentBGMIdentifier, volume);
                }

                yield return null;
            }

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.StopNamedLoop(currentBGMIdentifier);
            }
        }

        yield return new WaitForSeconds(0.1f);

        currentBGMIdentifier = newConfig.bgmIdentifier;

        if (SoundManager.Instance != null)
        {
            Debug.Log($"[BGMSceneManager] Starting new BGM: {currentBGMIdentifier}");
            SoundManager.Instance.PlayNamedLoop(currentBGMIdentifier, newConfig.bgmSoundName, newConfig.volume);
        }

        fadeCoroutine = null;
    }

    private void StopCurrentBGM()
    {
        if (!string.IsNullOrEmpty(currentBGMIdentifier))
        {
            Debug.Log($"[BGMSceneManager] Stopping BGM: {currentBGMIdentifier}");

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.StopNamedLoop(currentBGMIdentifier);
            }

            currentBGMIdentifier = null;
        }
    }

    public void PlaySceneBGM(string sceneName)
    {
        SceneBGMConfig config = GetSceneConfig(sceneName);
        if (config != null)
        {
            SwitchBGM(config);
        }
    }

    public void StopAllBGM()
    {
        StopCurrentBGM();
    }

    [ContextMenu("Add Current Scene Config")]
    private void AddCurrentSceneConfig()
    {
#if UNITY_EDITOR
        string currentSceneName = SceneManager.GetActiveScene().name;

     
        foreach (var config in sceneConfigs)
        {
            if (config.sceneName == currentSceneName)
            {
                Debug.Log($"[BGMSceneManager] Config for {currentSceneName} already exists");
                return;
            }
        }

     
        var newConfig = new SceneBGMConfig
        {
            sceneName = currentSceneName,
            bgmIdentifier = $"BGM_{currentSceneName}",
            bgmSoundName = $"{currentSceneName}Theme",
            volume = 0.5f
        };

        var list = new System.Collections.Generic.List<SceneBGMConfig>(sceneConfigs);
        list.Add(newConfig);
        sceneConfigs = list.ToArray();

        Debug.Log($"[BGMSceneManager] Added config for {currentSceneName}");
#endif
    }
}