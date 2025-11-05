using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour, ISoundPlayer
{
    public static ISoundPlayer Instance { get; private set; }

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource loopSource;
    [SerializeField] private SoundLibrary soundLibrary;

    private float _sfxVolume = 1f;
    private float _bgmVolume = 1f;

    private Dictionary<string, float> _bgmTargetVolumes = new Dictionary<string, float>();
    private Dictionary<string, AudioSource> _namedLoopSources = new Dictionary<string, AudioSource>();

    private void Awake()
    {
        if (Instance != null && Instance != this as ISoundPlayer)
        {
            Debug.LogWarning($"[SoundManager] Duplicate instance detected! Destroying {gameObject.name}");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        AudioListener listener = FindFirstObjectByType<AudioListener>();
        if (listener == null)
        {
            Debug.LogError("[SoundManager] No AudioListener found in scene!");
            Camera.main?.gameObject.AddComponent<AudioListener>();
        }
        else
        {
            Debug.Log($"[SoundManager] AudioListener found on: {listener.gameObject.name}");
        }

        if (loopSource != null)
        {
            loopSource.loop = true;
            loopSource.playOnAwake = false;
        }

        Debug.Log($"[SoundManager] Initialized on {gameObject.name}");
    }

    public void Play(string soundName)
    {
        var clip = soundLibrary.GetClip(soundName);
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, _sfxVolume);
            Debug.Log($"[SoundManager] Play SFX: {soundName}, volume: {_sfxVolume}");
        }
        else
        {
            Debug.LogWarning($"[SoundManager] Sound not found: {soundName}");
        }
    }

    public void PlayLoop(string soundName, float volume = 1f)
    {
        var clip = soundLibrary.GetClip(soundName);
        if (clip != null && loopSource != null)
        {
            if (loopSource.clip != clip)
            {
                loopSource.clip = clip;
            }

            loopSource.volume = Mathf.Clamp01(volume) * _bgmVolume;

            if (!loopSource.isPlaying)
            {
                loopSource.Play();
            }

            Debug.Log($"[SoundManager] PlayLoop (OLD API): {soundName}, volume: {loopSource.volume} (base: {volume} * bgm: {_bgmVolume})");
        }
    }

    public void StopLoop()
    {
        if (loopSource != null && loopSource.isPlaying)
        {
            loopSource.Stop();
            Debug.Log("[SoundManager] StopLoop (OLD API)");
        }
    }

    public void SetLoopVolume(float volume)
    {
        if (loopSource != null)
        {
            loopSource.volume = Mathf.Clamp01(volume) * _bgmVolume;
            Debug.Log($"[SoundManager] SetLoopVolume (OLD API): {loopSource.volume} (base: {volume} * bgm: {_bgmVolume})");
        }
    }

    public void PlayNamedLoop(string identifier, string soundName, float volume = 1f)
    {
        var clip = soundLibrary.GetClip(soundName);
        if (clip == null)
        {
            Debug.LogWarning($"[SoundManager] Sound not found: {soundName}");
            return;
        }

        if (!_namedLoopSources.TryGetValue(identifier, out AudioSource source))
        {
            GameObject sourceObj = new GameObject($"AudioSource_{identifier}");
            sourceObj.transform.SetParent(transform);
            source = sourceObj.AddComponent<AudioSource>();
            source.loop = true;
            source.playOnAwake = false;
            _namedLoopSources[identifier] = source;

            Debug.Log($"[SoundManager] Created new AudioSource for: {identifier}");
        }

        if (source.clip != clip)
        {
            source.clip = clip;
        }

        _bgmTargetVolumes[identifier] = Mathf.Clamp01(volume);

     
        bool isBGM = identifier.StartsWith("BGM_");
        float volumeMultiplier = isBGM ? _bgmVolume : _sfxVolume;

        source.volume = _bgmTargetVolumes[identifier] * volumeMultiplier;

        if (!source.isPlaying)
        {
            source.Play();
        }

        Debug.Log($"[SoundManager] PlayNamedLoop: {identifier}, type: {(isBGM ? "BGM" : "SFX")}, target: {_bgmTargetVolumes[identifier]}, multiplier: {volumeMultiplier}, final: {source.volume}");
    }

    public void StopNamedLoop(string identifier)
    {
        if (_namedLoopSources.TryGetValue(identifier, out AudioSource source))
        {
            if (source != null && source.isPlaying)
            {
                source.Stop();
                Debug.Log($"[SoundManager] StopNamedLoop: {identifier}");
            }
        }
    }

    public void SetNamedLoopVolume(string identifier, float volume)
    {
        if (_namedLoopSources.TryGetValue(identifier, out AudioSource source))
        {
            if (source != null)
            {
                _bgmTargetVolumes[identifier] = Mathf.Clamp01(volume);

                bool isBGM = identifier.StartsWith("BGM_");
                float volumeMultiplier = isBGM ? _bgmVolume : _sfxVolume;

                source.volume = _bgmTargetVolumes[identifier] * volumeMultiplier;

                Debug.Log($"[SoundManager] SetNamedLoopVolume: {identifier}, type: {(isBGM ? "BGM" : "SFX")}, target: {_bgmTargetVolumes[identifier]}, multiplier: {volumeMultiplier}, final: {source.volume}");
            }
        }
    }

    public void SetVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);

        Debug.Log($"[SoundManager] SetVolume (SFX): {_sfxVolume}");

      
        foreach (var kvp in _namedLoopSources)
        {
            if (!kvp.Key.StartsWith("BGM_") && kvp.Value != null && _bgmTargetVolumes.ContainsKey(kvp.Key))
            {
                kvp.Value.volume = _bgmTargetVolumes[kvp.Key] * _sfxVolume;
                Debug.Log($"[SoundManager] Updated SFX {kvp.Key}: target={_bgmTargetVolumes[kvp.Key]}, multiplier={_sfxVolume}, final={kvp.Value.volume}");
            }
        }
    }

    public void SetBGMVolume(float volume)
    {
        _bgmVolume = Mathf.Clamp01(volume);

        Debug.Log($"[SoundManager] SetBGMVolume: {_bgmVolume}");

       
        foreach (var kvp in _namedLoopSources)
        {
            if (kvp.Key.StartsWith("BGM_") && kvp.Value != null && _bgmTargetVolumes.ContainsKey(kvp.Key))
            {
                kvp.Value.volume = _bgmTargetVolumes[kvp.Key] * _bgmVolume;
                Debug.Log($"[SoundManager] Updated BGM {kvp.Key}: target={_bgmTargetVolumes[kvp.Key]}, multiplier={_bgmVolume}, final={kvp.Value.volume}");
            }
        }
    }

    public float GetVolume() => _sfxVolume;

    public float GetBGMVolume() => _bgmVolume;

    private void OnDestroy()
    {
        foreach (var source in _namedLoopSources.Values)
        {
            if (source != null)
            {
                Destroy(source.gameObject);
            }
        }
        _namedLoopSources.Clear();
        _bgmTargetVolumes.Clear();

        Debug.Log("[SoundManager] Destroyed");
    }
}