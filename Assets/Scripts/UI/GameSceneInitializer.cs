using UnityEngine;
using UnityEngine.UI;

public class GameSceneInitializer : MonoBehaviour
{
    [Header("Brightness Settings")]
    [SerializeField] private Image brightnessOverlay;

    [Header("Auto Find Settings")]
    [SerializeField] private bool autoFindBrightnessOverlay = true;

    private void Start()
    {
        Debug.Log("[GameSceneInitializer] Initializing game scene...");

        InitializeBrightness();
        InitializeSettings();
        InitializeAudio();

        Debug.Log("[GameSceneInitializer] Game scene initialized successfully");
    }

    private void InitializeBrightness()
    {
        if (brightnessOverlay == null && autoFindBrightnessOverlay)
        {
            GameObject overlayObj = GameObject.Find("BrightnessOverlay");
            if (overlayObj != null)
            {
                brightnessOverlay = overlayObj.GetComponent<Image>();
            }
        }

        if (brightnessOverlay != null && BrightnessManager.Instance != null)
        {
            BrightnessManager.Instance.SetOverlay(brightnessOverlay);
            Debug.Log("[GameSceneInitializer] Brightness overlay set");
        }
        else
        {
            if (brightnessOverlay == null)
            {
                Debug.LogWarning("[GameSceneInitializer] Brightness overlay not found!");
            }
            if (BrightnessManager.Instance == null)
            {
                Debug.LogWarning("[GameSceneInitializer] BrightnessManager not available!");
            }
        }
    }

    private void InitializeSettings()
    {
        if (GlobalSettingsManager.Instance != null)
        {
            GlobalSettingsManager.Instance.ApplyAllSettings();
            Debug.Log("[GameSceneInitializer] Settings applied: " + GlobalSettingsManager.Instance.GetSettingsSummary());
        }
        else
        {
            Debug.LogWarning("[GameSceneInitializer] GlobalSettingsManager not found!");
        }
    }

    private void InitializeAudio()
    {
        if (SoundManager.Instance != null)
        {
            Debug.Log("[GameSceneInitializer] SoundManager ready");
        }
        else
        {
            Debug.LogWarning("[GameSceneInitializer] SoundManager not found!");
        }
    }
}