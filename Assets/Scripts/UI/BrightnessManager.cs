using UnityEngine;
using UnityEngine.UI;

public class BrightnessManager : MonoBehaviour
{
    public static BrightnessManager Instance { get; private set; }

    [SerializeField] private Image brightnessOverlay;
    private float _currentBrightness = 1f;

    private void Awake()
    {
        //singleton
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"[BrightnessManager] Duplicate instance detected! Destroying {gameObject.name}");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadBrightness();
        Debug.Log($"[BrightnessManager] Initialized on {gameObject.name}");
    }

    private void LoadBrightness()
    {
        _currentBrightness = PlayerPrefs.GetFloat("Brightness", 1f);
        ApplyBrightness(_currentBrightness);
        Debug.Log($"[BrightnessManager] Loaded brightness: {_currentBrightness}");
    }

    public void SetBrightness(float value)
    {
        _currentBrightness = Mathf.Clamp01(value);
        ApplyBrightness(_currentBrightness);
        PlayerPrefs.SetFloat("Brightness", _currentBrightness);

        Debug.Log($"[BrightnessManager] Brightness set to: {_currentBrightness}");
    }

    public float GetBrightness()
    {
        return _currentBrightness;
    }

    private void ApplyBrightness(float value)
    {
        if (brightnessOverlay != null)
        {
            Color overlayColor = brightnessOverlay.color;
            overlayColor.a = 1f - value;
            brightnessOverlay.color = overlayColor;
        }
    }

    public void SetOverlay(Image overlay)
    {
        brightnessOverlay = overlay;
        ApplyBrightness(_currentBrightness);
        Debug.Log($"[BrightnessManager] Overlay set: {(overlay != null ? overlay.name : "NULL")}");
    }
}