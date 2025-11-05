using UnityEngine;
using UnityEngine.UI;

public class BrightnessSetting : SettingController
{
    [Header("Brightness Specific")]
    [SerializeField] private Image brightnessOverlay;

 
    private void OnValidate()
    {
        settingKey = "Brightness";
        defaultValue = 1f;
    }


    protected override void Awake()
    {
        settingKey = "Brightness";
        defaultValue = 1f;

      
        base.Awake();
    }

    protected override void Start()
    {
      
        ConfigureSliderRange();
        base.Start();
    }

    private void ConfigureSliderRange()
    {
        if (settingSlider != null && GlobalSettingsManager.Instance != null)
        {
            settingSlider.minValue = GlobalSettingsManager.Instance.MinBrightness;
            settingSlider.maxValue = GlobalSettingsManager.Instance.MaxBrightness;
            Debug.Log($"[BrightnessSetting] Slider range set to {settingSlider.minValue} - {settingSlider.maxValue}");
        }
    }

    protected override float GetValueFromGlobalSettings()
    {
        if (GlobalSettingsManager.Instance != null)
        {
            return GlobalSettingsManager.Instance.Brightness;
        }
        return defaultValue;
    }

    protected override void SubscribeToGlobalEvents()
    {
        if (GlobalSettingsManager.Instance != null)
        {
            GlobalSettingsManager.Instance.OnBrightnessChanged += OnGlobalBrightnessChanged;
            Debug.Log("[BrightnessSetting] Subscribed to global brightness changed event");
        }
    }

    protected override void UnsubscribeFromGlobalEvents()
    {
        if (GlobalSettingsManager.Instance != null)
        {
            GlobalSettingsManager.Instance.OnBrightnessChanged -= OnGlobalBrightnessChanged;
            Debug.Log("[BrightnessSetting] Unsubscribed from global brightness changed event");
        }
    }

    private void OnGlobalBrightnessChanged(float value)
    {
        Debug.Log($"[BrightnessSetting] Global brightness changed to: {value}");
        UpdateSliderSilently(value);
    }

    protected override void ApplySetting(float value)
    {
        Debug.Log($"[BrightnessSetting] Applying brightness: {value}");

        if (GlobalSettingsManager.Instance != null)
        {
            GlobalSettingsManager.Instance.SetBrightness(value);
        }
        else
        {
           
            if (BrightnessManager.Instance != null)
            {
                BrightnessManager.Instance.SetBrightness(value);
            }
            ApplyToOverlay(value);
        }
    }

    private void ApplyToOverlay(float value)
    {
        if (brightnessOverlay != null)
        {
            Color overlayColor = brightnessOverlay.color;
            overlayColor.a = 1f - value;
            brightnessOverlay.color = overlayColor;
            Debug.Log($"[BrightnessSetting] Applied brightness to local overlay: {value}");
        }
    }
}