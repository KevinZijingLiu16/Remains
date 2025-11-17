using UnityEngine;

public class SFXVolumeSetting : SettingController
{

    private void OnValidate()
    {
        settingKey = "SFXVolume";
        defaultValue = 0.5f;
    }

 
    protected override void Awake()
    {
        settingKey = "SFXVolume";
        defaultValue = 0.5f;

      
        base.Awake();
    }

    protected override float GetValueFromGlobalSettings()
    {
        if (GlobalSettingsManager.Instance != null)
        {
            return GlobalSettingsManager.Instance.SFXVolume;
        }
        return defaultValue;
    }

    protected override void SubscribeToGlobalEvents()
    {
        if (GlobalSettingsManager.Instance != null)
        {
            GlobalSettingsManager.Instance.OnSFXVolumeChanged += OnGlobalSFXVolumeChanged;
            Debug.Log("[SFXVolumeSetting] Subscribed to global SFX volume changed event");
        }
    }

    protected override void UnsubscribeFromGlobalEvents()
    {
        if (GlobalSettingsManager.Instance != null)
        {
            GlobalSettingsManager.Instance.OnSFXVolumeChanged -= OnGlobalSFXVolumeChanged;
            Debug.Log("[SFXVolumeSetting] Unsubscribed from global SFX volume changed event");
        }
    }

    private void OnGlobalSFXVolumeChanged(float value)
    {
        Debug.Log($"[SFXVolumeSetting] Global SFX volume changed to: {value}");
        UpdateSliderSilently(value);
    }

    protected override void ApplySetting(float value)
    {
        Debug.Log($"[SFXVolumeSetting] Applying SFX volume: {value}");

        if (GlobalSettingsManager.Instance != null)
        {
            GlobalSettingsManager.Instance.SetSFXVolume(value);
        }
        else if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetVolume(value);
        }
    }
}