using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
public class WeaponSelectionUI : MonoBehaviour, IWeaponSelectionUI
{
    [Header("UI References")]
    public GameObject weaponPanel;
    public Transform weaponButtonParent;
    public GameObject weaponButtonPrefab;
    public Button closeButton;

    [Header("Unequip Feature")]
    
    public Button unequipButton;
  
    public GameObject currentWeaponDisplay;
    public Image currentWeaponIcon;
    public TextMeshProUGUI currentWeaponText;

    [Header("Settings")]
    public float panelShowDuration = 0.3f;

    [Header("Debug")]
    public bool enableDebugLogs = true;

    public event System.Action<string> OnWeaponSelected;
    public event System.Action OnWeaponUnequipRequested;

    public bool IsVisible => weaponPanel != null && weaponPanel.activeInHierarchy;

    private List<GameObject> _spawnedButtons = new List<GameObject>();
    private CanvasGroup _panelCanvasGroup;
    private IWeaponEquipmentManager _equipmentManager;

    void Start()
    {
        InitializeUI();

      
        StartCoroutine(DelayedInitializeEquipmentManager());
    }

    private System.Collections.IEnumerator DelayedInitializeEquipmentManager()
    {
      
        yield return null;

        _equipmentManager = FindFirstObjectByType<WeaponEquipmentManager>();
        if (_equipmentManager != null)
        {
            _equipmentManager.OnWeaponEquipped += UpdateCurrentWeaponDisplay;
            _equipmentManager.OnWeaponUnequipped += UpdateCurrentWeaponDisplay;

            if (enableDebugLogs) Debug.Log("[WeaponSelectionUI] Successfully subscribed to equipment manager events");

         
            UpdateCurrentWeaponDisplay(_equipmentManager.CurrentWeapon);
        }
        else
        {
            Debug.LogError("[WeaponSelectionUI] WeaponEquipmentManager not found!");
        }
    }

    void OnDestroy()
    {
        if (_equipmentManager != null)
        {
            _equipmentManager.OnWeaponEquipped -= UpdateCurrentWeaponDisplay;
            _equipmentManager.OnWeaponUnequipped -= UpdateCurrentWeaponDisplay;
        }
    }

    private void InitializeUI()
    {
        if (weaponPanel != null)
        {
            weaponPanel.SetActive(false);
            _panelCanvasGroup = weaponPanel.GetComponent<CanvasGroup>();
            if (_panelCanvasGroup == null)
            {
                _panelCanvasGroup = weaponPanel.AddComponent<CanvasGroup>();
            }
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() =>
            {
                if (enableDebugLogs) Debug.Log("[WeaponSelectionUI] Close button clicked!");
                HideWeaponPanel();
            });
        }

     
        if (unequipButton != null)
        {
            unequipButton.onClick.RemoveAllListeners();
            unequipButton.onClick.AddListener(() =>
            {
                if (enableDebugLogs) Debug.Log("[WeaponSelectionUI] Unequip button clicked!");
                OnWeaponUnequipRequested?.Invoke();
                HideWeaponPanel();
            });
        }

        
        UpdateCurrentWeaponDisplay(null);
    }

    public void ShowWeaponPanel(IWeapon[] availableWeapons)
    {
        if (weaponPanel == null || availableWeapons == null) return;

        if (enableDebugLogs) Debug.Log($"[WeaponSelectionUI] Showing weapon panel with {availableWeapons.Length} weapons");

      
        ClearWeaponButtons();

     
        foreach (var weapon in availableWeapons)
        {
            CreateWeaponButton(weapon);
        }

     
        UpdateCurrentWeaponDisplay(_equipmentManager?.CurrentWeapon);

      
        weaponPanel.SetActive(true);

        if (_panelCanvasGroup != null)
        {
            _panelCanvasGroup.interactable = true;
            _panelCanvasGroup.blocksRaycasts = true;
            StartCoroutine(FadeIn());
        }

        if (enableDebugLogs) Debug.Log($"[WeaponSelectionUI] Created {_spawnedButtons.Count} weapon buttons");
    }

    public void HideWeaponPanel()
    {
        if (enableDebugLogs) Debug.Log("[WeaponSelectionUI] Hiding weapon panel");

        if (weaponPanel != null)
        {
            if (_panelCanvasGroup != null)
            {
                StartCoroutine(FadeOut());
            }
            else
            {
                weaponPanel.SetActive(false);
            }
        }
    }

    private void UpdateCurrentWeaponDisplay(IWeapon weapon)
    {
        if (enableDebugLogs) Debug.Log($"[WeaponSelectionUI] UpdateCurrentWeaponDisplay called - weapon: {(weapon != null ? weapon.WeaponName : "NULL")}");

        if (currentWeaponDisplay == null)
        {
            if (enableDebugLogs) Debug.Log("[WeaponSelectionUI] currentWeaponDisplay is null, skipping update");
            return;
        }

        bool hasWeapon = weapon != null;

        if (enableDebugLogs) Debug.Log($"[WeaponSelectionUI] Setting currentWeaponDisplay active: {hasWeapon}");

        if (hasWeapon)
        {
            currentWeaponDisplay.SetActive(true);

            if (currentWeaponIcon != null && weapon.WeaponIcon != null)
            {
                currentWeaponIcon.sprite = weapon.WeaponIcon;
                if (enableDebugLogs) Debug.Log("[WeaponSelectionUI] Updated weapon icon");
            }

            if (currentWeaponText != null)
            {
                currentWeaponText.text = $"当前装备: {weapon.WeaponName}";
                if (enableDebugLogs) Debug.Log($"[WeaponSelectionUI] Updated weapon text: {currentWeaponText.text}");
            }
        }
        else
        {
        
            currentWeaponDisplay.SetActive(true); 

            if (currentWeaponIcon != null)
            {
                currentWeaponIcon.sprite = null;
                if (enableDebugLogs) Debug.Log("[WeaponSelectionUI] Cleared weapon icon");
            }

            if (currentWeaponText != null)
            {
                currentWeaponText.text = "无装备武器";
                if (enableDebugLogs) Debug.Log("[WeaponSelectionUI] Set empty weapon text");
            }
        }

      
        if (unequipButton != null)
        {
            unequipButton.interactable = hasWeapon;
            if (enableDebugLogs) Debug.Log($"[WeaponSelectionUI] Updated unequip button interactable: {hasWeapon}");
        }
    }

    private void CreateWeaponButton(IWeapon weapon)
    {
        if (weaponButtonPrefab == null || weaponButtonParent == null) return;

        GameObject buttonObj = Instantiate(weaponButtonPrefab, weaponButtonParent);
        _spawnedButtons.Add(buttonObj);

       
        Image[] images = buttonObj.GetComponentsInChildren<Image>();
        Image weaponIcon = null;

        foreach (var img in images)
        {
            if (img.gameObject != buttonObj)
            {
                weaponIcon = img;
                break;
            }
        }

        if (weaponIcon == null && images.Length > 0)
        {
            weaponIcon = images[0];
        }

        if (weaponIcon != null && weapon.WeaponIcon != null)
        {
            weaponIcon.sprite = weapon.WeaponIcon;
        }

       
        Text[] texts = buttonObj.GetComponentsInChildren<Text>();
        TextMeshProUGUI[] tmpTexts = buttonObj.GetComponentsInChildren<TextMeshProUGUI>();

        if (texts.Length > 0)
        {
            texts[0].text = weapon.WeaponName;
            if (enableDebugLogs) Debug.Log($"[WeaponSelectionUI] Set text '{weapon.WeaponName}' on {texts[0].gameObject.name}");
        }
        else if (tmpTexts.Length > 0)
        {
            tmpTexts[0].text = weapon.WeaponName;
            if (enableDebugLogs) Debug.Log($"[WeaponSelectionUI] Set TMP text '{weapon.WeaponName}' on {tmpTexts[0].gameObject.name}");
        }

   
        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            button.interactable = true;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                if (enableDebugLogs) Debug.Log($"[WeaponSelectionUI] Weapon button clicked: {weapon.WeaponName}");
                OnWeaponSelected?.Invoke(weapon.WeaponId);
                HideWeaponPanel();
            });
        }

    
        bool isCurrentWeapon = _equipmentManager?.CurrentWeapon?.WeaponId == weapon.WeaponId;
        if (isCurrentWeapon && button != null)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = Color.yellow;
            button.colors = colors;
        }
    }

    private void ClearWeaponButtons()
    {
        foreach (var button in _spawnedButtons)
        {
            if (button != null) Destroy(button);
        }
        _spawnedButtons.Clear();
    }

    private System.Collections.IEnumerator FadeIn()
    {
        float elapsed = 0f;
        _panelCanvasGroup.alpha = 0f;

        while (elapsed < panelShowDuration)
        {
            elapsed += Time.deltaTime;
            _panelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / panelShowDuration);
            yield return null;
        }
        _panelCanvasGroup.alpha = 1f;
    }

    private System.Collections.IEnumerator FadeOut()
    {
        float elapsed = 0f;
        _panelCanvasGroup.interactable = false;

        while (elapsed < panelShowDuration)
        {
            elapsed += Time.deltaTime;
            _panelCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / panelShowDuration);
            yield return null;
        }
        _panelCanvasGroup.alpha = 0f;
        weaponPanel.SetActive(false);
    }

    
    [ContextMenu("Test Update Display (Null)")]
    public void TestUpdateDisplayNull()
    {
        UpdateCurrentWeaponDisplay(null);
    }

    [ContextMenu("Force Update Current Weapon Display")]
    public void ForceUpdateCurrentWeaponDisplay()
    {
        if (_equipmentManager != null)
        {
            UpdateCurrentWeaponDisplay(_equipmentManager.CurrentWeapon);
        }
        else
        {
            Debug.Log("Equipment manager is null");
        }
    }
}