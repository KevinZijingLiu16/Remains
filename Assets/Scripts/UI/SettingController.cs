using UnityEngine;
using UnityEngine.UI;
using TMPro;


public abstract class SettingController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] protected Slider settingSlider;
    [SerializeField] protected TextMeshProUGUI valueText;

    [Header("Settings")]
    [SerializeField] protected string settingKey;
    [SerializeField][Range(0f, 1f)] protected float defaultValue = 0.5f;

    private bool _isInitialized = false;
    private bool _isSubscribed = false; 

    protected virtual void Awake()
    {
        SubscribeToGlobalEvents();
    }

    protected virtual void Start()
    {
        LoadSetting();
        _isInitialized = true;

        Debug.Log($"[{GetType().Name}] Initialized for slider: {(settingSlider != null ? settingSlider.name : "NULL")}");
    }

    protected virtual void OnEnable()
    {
        if (settingSlider != null && !_isSubscribed)
        {
            settingSlider.onValueChanged.AddListener(OnValueChanged);
            _isSubscribed = true;
            Debug.Log($"[{GetType().Name}] Subscribed to slider: {settingSlider.name}");
        }
    }

    protected virtual void OnDisable()
    {
        if (settingSlider != null && _isSubscribed)
        {
            settingSlider.onValueChanged.RemoveListener(OnValueChanged);
            _isSubscribed = false;
            Debug.Log($"[{GetType().Name}] Unsubscribed from slider: {settingSlider.name}");
        }
    }

    protected virtual void OnDestroy()
    {
        UnsubscribeFromGlobalEvents();

        if (settingSlider != null && _isSubscribed)
        {
            settingSlider.onValueChanged.RemoveListener(OnValueChanged);
            _isSubscribed = false;
        }
    }

    protected virtual void SubscribeToGlobalEvents()
    {
    }

    protected virtual void UnsubscribeFromGlobalEvents()
    {
    }

    protected virtual void LoadSetting()
    {
        float value;

        if (GlobalSettingsManager.Instance != null)
        {
            value = GetValueFromGlobalSettings();
            Debug.Log($"[{GetType().Name}] Loading from GlobalSettings: {settingKey} = {value}");
        }
        else
        {
            value = PlayerPrefs.GetFloat(settingKey, defaultValue);
            Debug.Log($"[{GetType().Name}] Loading from PlayerPrefs: {settingKey} = {value}");
        }

        if (settingSlider != null)
        {
            if (_isSubscribed)
            {
                settingSlider.onValueChanged.RemoveListener(OnValueChanged);
            }

            settingSlider.value = value;

            if (_isSubscribed)
            {
                settingSlider.onValueChanged.AddListener(OnValueChanged);
            }
        }

        UpdateValueText(value);
    }

    protected virtual float GetValueFromGlobalSettings()
    {
        return PlayerPrefs.GetFloat(settingKey, defaultValue);
    }

    protected virtual void OnValueChanged(float value)
    {
        Debug.Log($"[{GetType().Name}] OnValueChanged called: {value} on slider: {settingSlider.name}");

        ApplySetting(value);
        PlayerPrefs.SetFloat(settingKey, value);
        UpdateValueText(value);
    }

    public virtual void SaveSetting()
    {
        PlayerPrefs.Save();
    }

    protected void UpdateSliderSilently(float value)
    {
        if (settingSlider == null) return;

        if (_isSubscribed)
        {
            settingSlider.onValueChanged.RemoveListener(OnValueChanged);
        }

        settingSlider.value = value;
        UpdateValueText(value);


        if (_isSubscribed)
        {
            settingSlider.onValueChanged.AddListener(OnValueChanged);
        }
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