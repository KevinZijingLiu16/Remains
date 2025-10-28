using UnityEngine;
using TMPro;
using System.Collections;


public class InteractionPromptManager : MonoBehaviour
{
    public static InteractionPromptManager Instance { get; private set; }


    public TextMeshProUGUI promptUI;


    public float fadeSpeed = 5f;


    public bool useFadeEffect = true;

   
    public bool usePrioritySystem = true;

    private CanvasGroup canvasGroup;
    private float targetAlpha = 0f;
    private string currentText = "";
    private int currentPriority = 0;
    private Coroutine hideCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeUI();
    }

    private void InitializeUI()
    {
        if (promptUI != null)
        {
            canvasGroup = promptUI.GetComponent<CanvasGroup>();
            if (canvasGroup == null && useFadeEffect)
            {
                canvasGroup = promptUI.gameObject.AddComponent<CanvasGroup>();
            }

            if (useFadeEffect && canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
            else
            {
                promptUI.gameObject.SetActive(false);
            }
        }
       
    }

    private void Update()
    {
        if (useFadeEffect && canvasGroup != null && promptUI != null)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);

            if (canvasGroup.alpha < 0.01f && targetAlpha < 0.5f)
            {
                promptUI.gameObject.SetActive(false);
            }
        }
    }


    public void ShowPrompt(string text, int priority = 0)
    {
        if (promptUI == null) return;

        
        if (usePrioritySystem && priority < currentPriority)
        {
            return;
        }

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        currentText = text;
        currentPriority = priority;
        promptUI.text = text;

        
        if (useFadeEffect && canvasGroup != null)
        {
            promptUI.gameObject.SetActive(true);
            targetAlpha = 1f;
        }
        else
        {
            promptUI.gameObject.SetActive(true);
        }

    }

 
    public void HidePrompt(int priority = 0)
    {
        if (promptUI == null) return;

     
        if (usePrioritySystem && priority < currentPriority)
        {
            return;
        }

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

     
        currentPriority = 0;
        currentText = "";

      
        if (useFadeEffect && canvasGroup != null)
        {
            targetAlpha = 0f;
        }
        else
        {
            promptUI.gameObject.SetActive(false);
        }

     
    }


    public void ShowPromptTimed(string text, float duration, int priority = 0)
    {
        ShowPrompt(text, priority);
        hideCoroutine = StartCoroutine(HideAfterDelay(duration, priority));
    }

    private IEnumerator HideAfterDelay(float delay, int priority)
    {
        yield return new WaitForSeconds(delay);
        HidePrompt(priority);
        hideCoroutine = null;
    }

    public void ForceHide()
    {
        currentPriority = 0;
        currentText = "";

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        if (useFadeEffect && canvasGroup != null)
        {
            targetAlpha = 0f;
        }
        else if (promptUI != null)
        {
            promptUI.gameObject.SetActive(false);
        }
    }


    public string GetCurrentText()
    {
        return currentText;
    }

    public int GetCurrentPriority()
    {
        return currentPriority;
    }

    public bool IsShowingPrompt()
    {
        return !string.IsNullOrEmpty(currentText) && currentPriority > 0;
    }
}