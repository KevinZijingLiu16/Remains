using UnityEngine;
using UnityEngine.UI;
using TMPro;

public abstract class SettingController : MonoBehaviour
{
    [SerializeField] protected Slider settingSlider;
    [SerializeField] protected TextMeshProUGUI valueText;

    protected string settingKey;
    protected float defaultValue = 0.5f;

    protected virtual void Awake()
    {
        LoadSetting();
    }

    protected virtual void OnEnable()
    {
        if (settingSlider != null)
            settingSlider.onValueChanged.AddListener(OnValueChanged);
    }

    protected virtual void OnDisable()
    {
        if (settingSlider != null)
            settingSlider.onValueChanged.RemoveListener(OnValueChanged);
    }

    protected virtual void LoadSetting()
    {
        float value = PlayerPrefs.GetFloat(settingKey, defaultValue);
        if (settingSlider != null)
            settingSlider.value = value;

        ApplySetting(value);
        UpdateValueText(value);
    }

    protected virtual void OnValueChanged(float value)
    {
        ApplySetting(value);
        PlayerPrefs.SetFloat(settingKey, value);
        UpdateValueText(value);
    }

    public virtual void SaveSetting()
    {
        PlayerPrefs.Save();
    }

    private void UpdateValueText(float value)
    {
        if (valueText != null)
        {
            valueText.text = Mathf.RoundToInt(value * 100).ToString();

        }
    }

    protected abstract void ApplySetting(float value);
}