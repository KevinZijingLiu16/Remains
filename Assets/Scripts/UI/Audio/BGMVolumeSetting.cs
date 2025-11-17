using UnityEngine;

public class BGMVolumeSetting : SettingController
{

    private void OnValidate()
    {
        settingKey = "BGMVolume";
        defaultValue = 0.5f;
    }

   
    protected override void Awake()
    {
        settingKey = "BGMVolume";
        defaultValue = 0.5f;

        
        base.Awake();
    }

    protected override float GetValueFromGlobalSettings()
    {
        if (GlobalSettingsManager.Instance != null)
        {
            return GlobalSettingsManager.Instance.BGMVolume;
        }
        return defaultValue;
    }

    protected override void SubscribeToGlobalEvents()
    {
        if (GlobalSettingsManager.Instance != null)
        {
            GlobalSettingsManager.Instance.OnBGMVolumeChanged += OnGlobalBGMVolumeChanged;
            Debug.Log("[BGMVolumeSetting] Subscribed to global BGM volume changed event");
        }
    }

    protected override void UnsubscribeFromGlobalEvents()
    {
        if (GlobalSettingsManager.Instance != null)
        {
            GlobalSettingsManager.Instance.OnBGMVolumeChanged -= OnGlobalBGMVolumeChanged;
            Debug.Log("[BGMVolumeSetting] Unsubscribed from global BGM volume changed event");
        }
    }

    private void OnGlobalBGMVolumeChanged(float value)
    {
        Debug.Log($"[BGMVolumeSetting] Global BGM volume changed to: {value}");
        UpdateSliderSilently(value);
    }

    protected override void ApplySetting(float value)
    {
        Debug.Log($"[BGMVolumeSetting] Applying BGM volume: {value}");

        if (GlobalSettingsManager.Instance != null)
        {
            GlobalSettingsManager.Instance.SetBGMVolume(value);
        }
        else if (SoundManager.Instance != null)
        {
        
            SoundManager.Instance.SetNamedLoopVolume("BGM_Opening", value);
            SoundManager.Instance.SetNamedLoopVolume("BGM_Game", value);
            SoundManager.Instance.SetNamedLoopVolume("BGM_Level1", value);
        }
    }
}