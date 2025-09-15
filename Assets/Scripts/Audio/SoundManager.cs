using UnityEngine;

public class SoundManager : MonoBehaviour, ISoundPlayer
{
    public static ISoundPlayer Instance { get; private set; }

    [SerializeField] private AudioSource sfxSource;
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
    }

    public void Play(string soundName)
    {
        var clip = soundLibrary.GetClip(soundName);
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, _volume);
        }
    }

    public void SetVolume(float volume)
    {
        _volume = Mathf.Clamp01(volume);
    }

    public float GetVolume() => _volume;
}