using UnityEngine;

public class SFXVolumeSetting : SettingController
{
    private void Reset()
    {
        settingKey = "SFXVolume"; // Default key
        defaultValue = 0.5f;
    }

    protected override void ApplySetting(float value)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetVolume(value);
        }
    }
}