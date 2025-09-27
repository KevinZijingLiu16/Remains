using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class KeyInventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform keyContainer; 
    [SerializeField] private GameObject keyIconPrefab;
    [SerializeField] private float iconSpacing = 60f; 

    [Header("Animation Settings")]
    [SerializeField] private float popInScale = 1.2f; 
    [SerializeField] private float animationDuration = 0.5f; 
    [SerializeField] private AnimationCurve popCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); 

    [Header("Key Visual Settings")]
    [SerializeField] private List<KeyVisualData> keyVisuals = new List<KeyVisualData>(); 

    private Dictionary<string, GameObject> activeKeyIcons = new Dictionary<string, GameObject>();
    private bool isInitialized = false;

    void Start()
    {
      
        InitializeUI();

       
        KeyEvents.OnKeyCollected += OnKeyCollected;
    }

    void OnDestroy()
    {
        KeyEvents.OnKeyCollected -= OnKeyCollected;
    }

    private void InitializeUI()
    {
        if (GameProgressManager.Instance == null)
        {
            Debug.LogWarning("[KeyInventoryUI] GameProgressManager not found, will retry later");
            Invoke(nameof(InitializeUI), 0.5f);
            return;
        }

        
        ClearAllIcons();

     
        var allKeys = GameProgressManager.Instance.GetAllKeys();
        foreach (var keyData in allKeys)
        {
            CreateKeyIcon(keyData.keyID, keyData.unlocksLevel, false); 
        }

        isInitialized = true;
        Debug.Log($"[KeyInventoryUI] Initialized with {allKeys.Count} keys");
    }

    private void OnKeyCollected(string keyID, string unlocksLevel)
    {
        if (!isInitialized) return;

        Debug.Log($"[KeyInventoryUI] Key collected: {keyID} for level: {unlocksLevel}");
        CreateKeyIcon(keyID, unlocksLevel, true); 
    }

    private void CreateKeyIcon(string keyID, string unlocksLevel, bool playAnimation = true)
    {
        if (activeKeyIcons.ContainsKey(keyID))
        {
            Debug.LogWarning($"[KeyInventoryUI] Key icon already exists: {keyID}");
            return;
        }

        if (keyIconPrefab == null || keyContainer == null)
        {
            Debug.LogError("[KeyInventoryUI] KeyIconPrefab or KeyContainer is null!");
            return;
        }

  
        GameObject keyIcon = Instantiate(keyIconPrefab, keyContainer);
        activeKeyIcons[keyID] = keyIcon;

       
        SetupKeyIcon(keyIcon, keyID, unlocksLevel);

    
        ArrangeKeyIcons();

      
        if (playAnimation)
        {
            StartCoroutine(PlayKeyCollectedAnimation(keyIcon));
        }
    }

    private void SetupKeyIcon(GameObject keyIcon, string keyID, string unlocksLevel)
    {
        
        KeyVisualData visualData = GetKeyVisualData(keyID, unlocksLevel);

        if (visualData != null)
        {
            Image iconImage = keyIcon.GetComponent<Image>();
            if (iconImage != null && visualData.keySprite != null)
            {
                iconImage.sprite = visualData.keySprite;
                iconImage.color = visualData.keyColor;
            }
        }

     
        KeyIconTooltip tooltip = keyIcon.GetComponent<KeyIconTooltip>();
        if (tooltip == null)
        {
            tooltip = keyIcon.AddComponent<KeyIconTooltip>();
        }
        tooltip.SetKeyInfo(keyID, unlocksLevel);
    }

    private KeyVisualData GetKeyVisualData(string keyID, string unlocksLevel)
    {
       
        foreach (var visual in keyVisuals)
        {
            if (visual.keyID == keyID) return visual;
        }

      
        foreach (var visual in keyVisuals)
        {
            if (visual.unlocksLevel == unlocksLevel) return visual;
        }

       
        return keyVisuals.Count > 0 ? keyVisuals[0] : null;
    }

    private void ArrangeKeyIcons()
    {
        int index = 0;
        foreach (var kvp in activeKeyIcons)
        {
            RectTransform rectTransform = kvp.Value.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
           
                float xPosition = index * iconSpacing;
                rectTransform.anchoredPosition = new Vector2(xPosition, 0);
            }
            index++;
        }
    }

    private IEnumerator PlayKeyCollectedAnimation(GameObject keyIcon)
    {
        RectTransform rectTransform = keyIcon.GetComponent<RectTransform>();
        if (rectTransform == null) yield break;

        Vector3 originalScale = rectTransform.localScale;
        Vector3 targetScale = originalScale * popInScale;

        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; 
            float progress = elapsedTime / animationDuration;
            float curveValue = popCurve.Evaluate(progress);

            if (progress <= 0.5f)
            {
              
                rectTransform.localScale = Vector3.Lerp(originalScale, targetScale, curveValue * 2f);
            }
            else
            {
                
                rectTransform.localScale = Vector3.Lerp(targetScale, originalScale, (curveValue - 0.5f) * 2f);
            }

            yield return null;
        }

        rectTransform.localScale = originalScale;


        StartCoroutine(PlayGlowEffect(keyIcon));
    }

    private IEnumerator PlayGlowEffect(GameObject keyIcon)
    {
        Image iconImage = keyIcon.GetComponent<Image>();
        if (iconImage == null) yield break;

        Color originalColor = iconImage.color;

        if (originalColor.a < 0.5f)
        {
            originalColor.a = 1f;
            iconImage.color = originalColor;
        }

        Color glowColor = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);

        for (int i = 0; i < 3; i++)
        {
        
            float glowDuration = 0.2f;
            float elapsedTime = 0f;
            while (elapsedTime < glowDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(originalColor.a, 1f, elapsedTime / glowDuration);
                iconImage.color = new Color(glowColor.r, glowColor.g, glowColor.b, alpha);
                yield return null;
            }

     
            elapsedTime = 0f;
            while (elapsedTime < glowDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(1f, originalColor.a, elapsedTime / glowDuration);
                iconImage.color = new Color(glowColor.r, glowColor.g, glowColor.b, alpha);
                yield return null;
            }
        }

      
        iconImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
    }
    private void ClearAllIcons()
    {
        foreach (var kvp in activeKeyIcons)
        {
            if (kvp.Value != null)
            {
                Destroy(kvp.Value);
            }
        }
        activeKeyIcons.Clear();
    }

 
    [ContextMenu("Refresh Key UI")]
    public void RefreshKeyUI()
    {
        InitializeUI();
    }
}
[System.Serializable]
public class KeyVisualData
{
    public string keyID;        
    public string unlocksLevel;
    public Sprite keySprite;   
    public Color keyColor = Color.white; 
    public string displayName; 
}


public class KeyIconTooltip : MonoBehaviour, UnityEngine.EventSystems.IPointerEnterHandler, UnityEngine.EventSystems.IPointerExitHandler
{
    private string keyInfo;
    private GameObject tooltipObject;

    public void SetKeyInfo(string keyID, string unlocksLevel)
    {
        keyInfo = $"Key: {keyID}\nUnlocks: {unlocksLevel}";
    }

    public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        
        Debug.Log($"[KeyTooltip] {keyInfo}");
   
    }

    public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
      
    }
}