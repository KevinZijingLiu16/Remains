using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;

public class AnyInputToNextScene : MonoBehaviour
{
    [Header("UI Settings")]
    public CanvasGroup textGroup;
    public float fadeDuration = 1f;

    private bool isTransitioning = false;

    private void Start()
    {
        if (textGroup != null)
            textGroup.alpha = 0; 

        StartCoroutine(FadeInText());
    }

    private IEnumerator FadeInText()
    {
        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            textGroup.alpha = Mathf.Lerp(0, 1, time / fadeDuration);
            yield return null;
        }
        textGroup.alpha = 1;
    }

    private void Update()
    {
        if (isTransitioning) return;
        if (HasAnyInput())
        {
            StartCoroutine(FadeOutAndLoadScene());
        }
    }

    private bool HasAnyInput()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) return true;
        if (Mouse.current != null &&
            (Mouse.current.leftButton.wasPressedThisFrame ||
             Mouse.current.rightButton.wasPressedThisFrame ||
             Mouse.current.middleButton.wasPressedThisFrame)) return true;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) return true;
        if (Gamepad.current != null && Gamepad.current.allControls.Any(c => c is ButtonControl btn && btn.wasPressedThisFrame)) return true;

        return false;
    }

    private IEnumerator FadeOutAndLoadScene()
    {
        isTransitioning = true;
        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            textGroup.alpha = Mathf.Lerp(1, 0, time / fadeDuration);
            yield return null;
        }
        textGroup.alpha = 0;

        // 加载下一场景
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextSceneIndex);
        else
            Debug.LogWarning("No Next Scene");
    }
}
