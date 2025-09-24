
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealthUI : MonoBehaviour, IHealthUI
{
    [Header("UI References")]
    [SerializeField] private Image[] heartImages = new Image[3];
    [SerializeField] private bool useAnimations = true; 
    [SerializeField] private bool useHealEffect = false; 

    [Header("Animation Settings")]
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [SerializeField] private AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Effects")]
    [SerializeField] private float damageShakeIntensity = 10f;
    [SerializeField] private float damageShakeDuration = 0.2f;
    [SerializeField] private Color healColor = Color.green;
    [SerializeField] private float healEffectDuration = 0.5f;

    private Coroutine[] heartAnimations = new Coroutine[3];

    void Start()
    {
     
        if (heartImages[0] == null)
        {
            AutoFindHeartImages();
        }

       
        ValidateUISetup();

      
        InitializeHeartStates();
    }

    private void InitializeHeartStates()
    {
      
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] != null)
            {
                Color color = heartImages[i].color;
                color.a = 1f; 
                heartImages[i].color = color;
                Debug.Log($"[PlayerHealthUI] Initialized heart {i} with alpha: {color.a}");
            }
        }
    }

    private void AutoFindHeartImages()
    {
        Image[] foundImages = GetComponentsInChildren<Image>();
        for (int i = 0; i < Mathf.Min(foundImages.Length, heartImages.Length); i++)
        {
            heartImages[i] = foundImages[i];
        }
    }

    private void ValidateUISetup()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null)
            {
                Debug.LogError($"[PlayerHealthUI] Heart image {i} is not assigned!");
            }
        }
    }

    public void UpdateHealthDisplay(int currentHealth, int maxHealth)
    {
        Debug.Log($"[PlayerHealthUI] Updating health display: {currentHealth}/{maxHealth}, useAnimations: {useAnimations}");

       
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null) continue;

            bool shouldBeVisible = i < currentHealth;
            bool isCurrentlyVisible = heartImages[i].color.a > 0.5f;

            Debug.Log($"[PlayerHealthUI] Heart {i}: shouldBeVisible={shouldBeVisible}, currentAlpha={heartImages[i].color.a}");

            if (shouldBeVisible && !isCurrentlyVisible)
            {
             
                if (useAnimations)
                {
                    Debug.Log($"[PlayerHealthUI] Fading in heart {i}");
                    FadeInHeart(i);
                }
                else
                {
                   
                    SetHeartAlpha(i, 1f);
                    Debug.Log($"[PlayerHealthUI] Immediately showing heart {i}");
                }
            }
            else if (!shouldBeVisible && isCurrentlyVisible)
            {
               
                if (useAnimations)
                {
                    Debug.Log($"[PlayerHealthUI] Fading out heart {i}");
                    FadeOutHeart(i);
                }
                else
                {
                   
                    SetHeartAlpha(i, 0f);
                    Debug.Log($"[PlayerHealthUI] Immediately hiding heart {i}");
                }
            }
        }
    }

    private void SetHeartAlpha(int index, float alpha)
    {
        if (heartImages[index] != null)
        {
            Color color = heartImages[index].color;
            color.a = alpha;
            heartImages[index].color = color;
        }
    }

    public void PlayDamageEffect(int damageAmount)
    {
        Debug.Log($"[PlayerHealthUI] Playing damage effect for {damageAmount} damage");

    
        StartCoroutine(ShakeEffect());
    }

    public void PlayHealEffect(int healAmount)
    {
        Debug.Log($"[PlayerHealthUI] Playing heal effect for {healAmount} heal, useHealEffect: {useHealEffect}");

       
        if (useHealEffect)
        {
            StartCoroutine(HealGlowEffect());
        }
    }
    [ContextMenu("Force Show All Hearts")]
    public void ForceShowAllHearts()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            SetHeartAlpha(i, 1f);
            Debug.Log($"[PlayerHealthUI] Forced heart {i} to alpha 1");
        }
    }

    [ContextMenu("Debug Heart States")]
    public void DebugHeartStates()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] != null)
            {
                Debug.Log($"[PlayerHealthUI] Heart {i} alpha: {heartImages[i].color.a}");
            }
        }
    }
    private void FadeOutHeart(int index)
    {
        if (heartAnimations[index] != null)
        {
            StopCoroutine(heartAnimations[index]);
        }

        heartAnimations[index] = StartCoroutine(FadeHeartCoroutine(index, false));
    }

    private void FadeInHeart(int index)
    {
        if (heartAnimations[index] != null)
        {
            StopCoroutine(heartAnimations[index]);
        }

        heartAnimations[index] = StartCoroutine(FadeHeartCoroutine(index, true));
    }

    private IEnumerator FadeHeartCoroutine(int index, bool fadeIn)
    {
        if (heartImages[index] == null)
        {
            Debug.LogWarning($"[PlayerHealthUI] Heart image {index} is null!");
            yield break;
        }

        float duration = fadeIn ? fadeInDuration : fadeOutDuration;
        AnimationCurve curve = fadeIn ? fadeInCurve : fadeOutCurve;

        Color startColor = heartImages[index].color;
        float startAlpha = startColor.a;
        float targetAlpha = fadeIn ? 1f : 0f;

        Debug.Log($"[PlayerHealthUI] Starting {(fadeIn ? "fade in" : "fade out")} animation for heart {index}. StartAlpha: {startAlpha}, TargetAlpha: {targetAlpha}");

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / duration;
            float curveValue = curve.Evaluate(normalizedTime);

           
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, curveValue);

            Color currentColor = heartImages[index].color;
            currentColor.a = currentAlpha;
            heartImages[index].color = currentColor;

            yield return null;
        }

      
        Color finalColor = heartImages[index].color;
        finalColor.a = targetAlpha;
        heartImages[index].color = finalColor;

        heartAnimations[index] = null;

        Debug.Log($"[PlayerHealthUI] Completed {(fadeIn ? "fade in" : "fade out")} animation for heart {index}. Final alpha: {finalColor.a}");
    }

    private IEnumerator ShakeEffect()
    {
        Vector3 originalPosition = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < damageShakeDuration)
        {
            elapsedTime += Time.deltaTime;

            Vector3 randomOffset = Random.insideUnitCircle * damageShakeIntensity;
            transform.position = originalPosition + randomOffset;

            yield return null;
        }

        transform.position = originalPosition;
    }

    private IEnumerator HealGlowEffect()
    {
       
        Color[] originalColors = new Color[heartImages.Length];
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] != null)
            {
                originalColors[i] = heartImages[i].color;
            }
        }

        float elapsedTime = 0f;

        while (elapsedTime < healEffectDuration)
        {
            elapsedTime += Time.deltaTime;
            float intensity = Mathf.Sin((elapsedTime / healEffectDuration) * Mathf.PI);

            for (int i = 0; i < heartImages.Length; i++)
            {
                if (heartImages[i] != null)
                {
                    Color glowColor = Color.Lerp(originalColors[i], healColor, intensity * 0.5f);
                    heartImages[i].color = glowColor;
                }
            }

            yield return null;
        }

       
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] != null)
            {
                heartImages[i].color = originalColors[i];
            }
        }
    }
}


[System.Serializable]
public struct HeartUIData
{
    public Image heartImage;
    public bool isVisible;
    public float currentAlpha;
}