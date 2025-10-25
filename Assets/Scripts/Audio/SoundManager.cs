using UnityEngine;

public class SoundManager : MonoBehaviour, ISoundPlayer
{
    public static ISoundPlayer Instance { get; private set; }

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource loopSource;
    [SerializeField] private SoundLibrary soundLibrary;

    private float _volume = 1f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

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
    }

    public void PlayLoop(string soundName, float volume = 1f)
    {
        Debug.Log($"PlayLoop 被调用: {soundName}, volume: {volume}");

        var clip = soundLibrary.GetClip(soundName);
        if (clip == null)
        {
            Debug.LogError($"找不到音效: {soundName}");
            return;
        }

        if (loopSource == null)
        {
            Debug.LogError("loopSource 未分配！");
            return;
        }

        Debug.Log($"成功获取音效片段: {clip.name}");

        if (loopSource.clip != clip)
        {
            loopSource.clip = clip;
        }

        loopSource.volume = Mathf.Clamp01(volume) * _volume;

        if (!loopSource.isPlaying)
        {
            loopSource.Play();
            Debug.Log("开始播放循环音效");
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

    public void SetVolume(float volume)
    {
        _volume = Mathf.Clamp01(volume);
    }

    public float GetVolume() => _volume;
}