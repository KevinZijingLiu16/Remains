using UnityEngine;
using System;


public class GlobalSettingsManager : MonoBehaviour
{
    public static GlobalSettingsManager Instance { get; private set; }

   
    [SerializeField][Range(0f, 1f)] private float defaultSFXVolume = 0.5f;
    [SerializeField][Range(0f, 1f)] private float defaultBGMVolume = 0.5f;
    [SerializeField][Range(0.5f, 1f)] private float defaultBrightness = 1f;

    [SerializeField][Range(0f, 1f)] private float minBrightness = 0.5f;
    [SerializeField][Range(0f, 1f)] private float maxBrightness = 1f;

  
    public event Action<float> OnSFXVolumeChanged;
    public event Action<float> OnBGMVolumeChanged;
    public event Action<float> OnBrightnessChanged;


    private float _sfxVolume;
    private float _bgmVolume;
    private float _brightness;

 
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string BGM_VOLUME_KEY = "BGMVolume";
    private const string BRIGHTNESS_KEY = "Brightness";

    public float SFXVolume => _sfxVolume;
    public float BGMVolume => _bgmVolume;
    public float Brightness => _brightness;

    public float MinBrightness => minBrightness;
    public float MaxBrightness => maxBrightness;

    private void Awake()
    {
    
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"[GlobalSettingsManager] Duplicate instance detected! Destroying {gameObject.name}");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAllSettings();
        Debug.Log($"[GlobalSettingsManager] Initialized - SFX:{_sfxVolume} BGM:{_bgmVolume} Brightness:{_brightness}");
        Debug.Log($"[GlobalSettingsManager] Brightness range: {minBrightness} - {maxBrightness}");
    }

    private void LoadAllSettings()
    {
        _sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, defaultSFXVolume);
        _bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, defaultBGMVolume);
        _brightness = PlayerPrefs.GetFloat(BRIGHTNESS_KEY, defaultBrightness);

        _brightness = Mathf.Clamp(_brightness, minBrightness, maxBrightness);

        Debug.Log($"[GlobalSettingsManager] Loaded settings - SFX: {_sfxVolume}, BGM: {_bgmVolume}, Brightness: {_brightness}");
    }

    public void SetSFXVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, _sfxVolume);
        PlayerPrefs.Save();

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetVolume(_sfxVolume);
        }

        OnSFXVolumeChanged?.Invoke(_sfxVolume);

        Debug.Log($"[GlobalSettingsManager] SFX Volume set to: {_sfxVolume}");
    }

    public void SetBGMVolume(float volume)
    {
        _bgmVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, _bgmVolume);
        PlayerPrefs.Save();

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetBGMVolume(_bgmVolume);
        }

        OnBGMVolumeChanged?.Invoke(_bgmVolume);

        Debug.Log($"[GlobalSettingsManager] BGM Volume set to: {_bgmVolume}");
    }

    public void SetBrightness(float brightness)
    {
        _brightness = Mathf.Clamp(brightness, minBrightness, maxBrightness);
        PlayerPrefs.SetFloat(BRIGHTNESS_KEY, _brightness);
        PlayerPrefs.Save();

        if (BrightnessManager.Instance != null)
        {
            BrightnessManager.Instance.SetBrightness(_brightness);
        }

        OnBrightnessChanged?.Invoke(_brightness);

        Debug.Log($"[GlobalSettingsManager] Brightness set to: {_brightness}");
    }

    public void ApplyAllSettings()
    {
        SetSFXVolume(_sfxVolume);
        SetBGMVolume(_bgmVolume);
        SetBrightness(_brightness);

        Debug.Log("[GlobalSettingsManager] All settings applied");
    }

    public void ResetToDefaults()
    {
        SetSFXVolume(defaultSFXVolume);
        SetBGMVolume(defaultBGMVolume);
        SetBrightness(defaultBrightness);

        Debug.Log("[GlobalSettingsManager] Settings reset to defaults");
    }

    public string GetSettingsSummary()
    {
        return $"SFX: {_sfxVolume:F2}, BGM: {_bgmVolume:F2}, Brightness: {_brightness:F2}";
    }

    [ContextMenu("Clear All Saved Settings")]
    public void ClearAllSettings()
    {
        PlayerPrefs.DeleteKey(SFX_VOLUME_KEY);
        PlayerPrefs.DeleteKey(BGM_VOLUME_KEY);
        PlayerPrefs.DeleteKey(BRIGHTNESS_KEY);
        PlayerPrefs.Save();

        Debug.Log("[GlobalSettingsManager] All saved settings cleared! Will use defaults on next load.");

        LoadAllSettings();
        ApplyAllSettings();
    }
}