using UnityEngine;
using UnityEngine.UI;

public class OpeningSceneInitializer : MonoBehaviour
{
    [Header("Brightness Settings")]
    [SerializeField] private Image brightnessOverlay;

    [Header("Auto Find Settings")]
    [SerializeField] private bool autoFindBrightnessOverlay = true;

    private void Start()
    {
        Debug.Log("[OpeningSceneInitializer] Initializing opening scene...");

        InitializeBrightness();
        InitializeSettings();
        InitializeAudio();

        Debug.Log("[OpeningSceneInitializer] Opening scene initialized successfully");
    }

 
    private void InitializeBrightness()
    {
     
        if (brightnessOverlay == null && autoFindBrightnessOverlay)
        {
            GameObject overlayObj = GameObject.Find("BrightnessOverlay");
            if (overlayObj != null)
            {
                brightnessOverlay = overlayObj.GetComponent<Image>();
                Debug.Log($"[OpeningSceneInitializer] Auto-found BrightnessOverlay");
            }
        }

        if (brightnessOverlay != null && BrightnessManager.Instance != null)
        {
            BrightnessManager.Instance.SetOverlay(brightnessOverlay);
            Debug.Log($"[OpeningSceneInitializer] Brightness overlay set: {brightnessOverlay.name}");
        }
        else
        {
            if (brightnessOverlay == null)
            {
                Debug.LogWarning("[OpeningSceneInitializer] Brightness overlay not found!");
            }
            if (BrightnessManager.Instance == null)
            {
                Debug.LogWarning("[OpeningSceneInitializer] BrightnessManager not available!");
            }
        }
    }


    private void InitializeSettings()
    {
        if (GlobalSettingsManager.Instance != null)
        {
           
            GlobalSettingsManager.Instance.ApplyAllSettings();
            Debug.Log("[OpeningSceneInitializer] Settings applied: " +
                     GlobalSettingsManager.Instance.GetSettingsSummary());
        }
        else
        {
            Debug.LogWarning("[OpeningSceneInitializer] GlobalSettingsManager not found!");
        }
    }

 
    private void InitializeAudio()
    {
        if (SoundManager.Instance != null)
        {
            Debug.Log("[OpeningSceneInitializer] SoundManager ready");

           
        }
        else
        {
            Debug.LogWarning("[OpeningSceneInitializer] SoundManager not found!");
        }
    }
}