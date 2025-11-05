using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour, ISoundPlayer
{
    public static ISoundPlayer Instance { get; private set; }

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource loopSource;
    [SerializeField] private SoundLibrary soundLibrary;

    private float _volume = 1f;

    private Dictionary<string, AudioSource> _namedLoopSources = new Dictionary<string, AudioSource>();

    private void Awake()
    {
        if (Instance != null)
        {
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
    }

    public void Play(string soundName)
    {
        var clip = soundLibrary.GetClip(soundName);
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, _volume);
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

            loopSource.volume = Mathf.Clamp01(volume) * _volume;

            if (!loopSource.isPlaying)
            {
                loopSource.Play();
            }
        }
    }

    public void StopLoop()
    {
        if (loopSource != null && loopSource.isPlaying)
        {
            loopSource.Stop();
        }
    }

    public void SetLoopVolume(float volume)
    {
        if (loopSource != null)
        {
            loopSource.volume = Mathf.Clamp01(volume) * _volume;
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
        }

     
        if (source.clip != clip)
        {
            source.clip = clip;
        }

        source.volume = Mathf.Clamp01(volume) * _volume;

        if (!source.isPlaying)
        {
            source.Play();
        }
    }

    public void StopNamedLoop(string identifier)
    {
        if (_namedLoopSources.TryGetValue(identifier, out AudioSource source))
        {
            if (source != null && source.isPlaying)
            {
                source.Stop();
            }
        }
    }

    public void SetNamedLoopVolume(string identifier, float volume)
    {
        if (_namedLoopSources.TryGetValue(identifier, out AudioSource source))
        {
            if (source != null)
            {
                source.volume = Mathf.Clamp01(volume) * _volume;
            }
        }
    }

    public void SetVolume(float volume)
    {
        _volume = Mathf.Clamp01(volume);

      
        foreach (var source in _namedLoopSources.Values)
        {
            if (source != null)
            {
                source.volume *= _volume;
            }
        }
    }

    public float GetVolume() => _volume;

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
    }
}