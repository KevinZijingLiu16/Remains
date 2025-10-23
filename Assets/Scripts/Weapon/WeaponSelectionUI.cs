using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class WeaponSelectionUI : MonoBehaviour, IWeaponSelectionUI
{
    [Header("UI References")]
    public GameObject weaponPanel;
    public Transform weaponButtonParent;
    public GameObject weaponButtonPrefab;
    public Button closeButton;

    [Header("No Weapon Option")]
    public Sprite noWeaponIcon;

    [Header("Current Weapon Display")]
    public GameObject currentWeaponDisplay;
    public Image currentWeaponIcon;
    public TextMeshProUGUI currentWeaponText;

    [Header("Input Settings")]
    public bool allowMoveInput = false;  //set for controller
    public bool allowMouseInput = true;  

    [Header("Navigation Colors")]
    public Color normalButtonColor = Color.white;
    public Color selectedButtonColor = Color.yellow;

    [Header("Settings")]
    public float panelShowDuration = 0.3f;

    [Header("Debug")]
    public bool enableDebugLogs = true;

    public event System.Action<string> OnWeaponSelected;
    public event System.Action OnWeaponUnequipRequested;

    public bool IsVisible => weaponPanel != null && weaponPanel.activeInHierarchy;

    private List<GameObject> _spawnedButtons = new List<GameObject>();
    private List<IWeapon> _currentWeapons = new List<IWeapon>();
    private CanvasGroup _panelCanvasGroup;
    private IWeaponEquipmentManager _equipmentManager;

    private int _currentEquippedIndex = -1;  
    private int _hoveredIndex = -1;          
    private float _originalTimeScale;
    private bool _hasNoWeaponOption = false;

    void Start()
    {
        InitializeUI();
        StartCoroutine(DelayedInitializeEquipmentManager());
    }

    void OnDestroy()
    {
        if (_equipmentManager != null)
        {
            _equipmentManager.OnWeaponEquipped -= UpdateCurrentWeaponDisplay;
            _equipmentManager.OnWeaponUnequipped -= UpdateCurrentWeaponDisplay;
        }
    }

    private IEnumerator DelayedInitializeEquipmentManager()
    {
        yield return null;
        _equipmentManager = FindFirstObjectByType<WeaponEquipmentManager>();
        if (_equipmentManager != null)
        {
            _equipmentManager.OnWeaponEquipped += UpdateCurrentWeaponDisplay;
            _equipmentManager.OnWeaponUnequipped += UpdateCurrentWeaponDisplay;
            UpdateCurrentWeaponDisplay(_equipmentManager.CurrentWeapon);
        }
    }

    private void InitializeUI()
    {
        if (weaponPanel != null)
        {
            weaponPanel.SetActive(false);
            _panelCanvasGroup = weaponPanel.GetComponent<CanvasGroup>();
            if (_panelCanvasGroup == null) _panelCanvasGroup = weaponPanel.AddComponent<CanvasGroup>();
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => HideWeaponPanel());
        }

        UpdateCurrentWeaponDisplay(null);
    }

    public void ShowWeaponPanel(IWeapon[] availableWeapons)
    {
        if (weaponPanel == null || availableWeapons == null) return;

        _originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        ClearWeaponButtons();
        _currentWeapons.Clear();
        _currentWeapons.AddRange(availableWeapons);

        _hasNoWeaponOption = true;

       
        _currentEquippedIndex = GetCurrentEquippedIndex();
        _hoveredIndex = -1;  

      
        CreateNoWeaponButton();
        for (int i = 0; i < availableWeapons.Length; i++)
            CreateWeaponButton(availableWeapons[i], i + 1);

        UpdateCurrentWeaponDisplay(_equipmentManager?.CurrentWeapon);

        weaponPanel.SetActive(true);

        if (_panelCanvasGroup != null)
        {
            _panelCanvasGroup.interactable = true;
            _panelCanvasGroup.blocksRaycasts = true;
            StartCoroutine(FadeIn());
        }

     
        UpdateAllButtonVisuals();
    }

    private int GetCurrentEquippedIndex()
    {
        if (_equipmentManager == null || _equipmentManager.CurrentWeapon == null)
        {
            return 0; // No Weapon 
        }

     
        for (int i = 0; i < _currentWeapons.Count; i++)
        {
            if (_currentWeapons[i].WeaponId == _equipmentManager.CurrentWeapon.WeaponId)
            {
                return i + 1; 
            }
        }

        return 0; 
    }

    private void CreateNoWeaponButton()
    {
        if (weaponButtonPrefab == null || weaponButtonParent == null) return;

        GameObject buttonObj = Instantiate(weaponButtonPrefab, weaponButtonParent);
        buttonObj.name = "Button_NoWeapon";
        _spawnedButtons.Add(buttonObj);

        ForceSetButtonIcon(buttonObj, noWeaponIcon, "No Weapon");

        var button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                if (enableDebugLogs) Debug.Log("[WeaponSelectionUI] No Weapon button clicked!");
                OnWeaponUnequipRequested?.Invoke();
                HideWeaponPanel();
            });
        }

     
        if (allowMouseInput)
        {
            AddHoverEvents(buttonObj, 0);
        }
    }

    private void CreateWeaponButton(IWeapon weapon, int buttonIndex)
    {
        if (weaponButtonPrefab == null || weaponButtonParent == null || weapon == null) return;

        GameObject buttonObj = Instantiate(weaponButtonPrefab, weaponButtonParent);
        buttonObj.name = $"Button_{weapon.WeaponName}";
        _spawnedButtons.Add(buttonObj);

        ForceSetButtonIcon(buttonObj, weapon.WeaponIcon, weapon.WeaponName);

        var button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            string weaponId = weapon.WeaponId;
            string weaponName = weapon.WeaponName;

            button.onClick.AddListener(() =>
            {
                if (enableDebugLogs) Debug.Log($"[WeaponSelectionUI] Weapon button clicked: {weaponName} (ID: {weaponId})");
                OnWeaponSelected?.Invoke(weaponId);
                HideWeaponPanel();
            });
        }

     
        if (allowMouseInput)
        {
            AddHoverEvents(buttonObj, buttonIndex);
        }
    }

    private void AddHoverEvents(GameObject buttonObj, int buttonIndex)
    {
        var eventTrigger = buttonObj.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (eventTrigger == null)
            eventTrigger = buttonObj.AddComponent<UnityEngine.EventSystems.EventTrigger>();

      
        var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry
        {
            eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter
        };
        pointerEnter.callback.AddListener((data) => OnButtonHover(buttonIndex));
        eventTrigger.triggers.Add(pointerEnter);

    
        var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry
        {
            eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit
        };
        pointerExit.callback.AddListener((data) => OnButtonExit(buttonIndex));
        eventTrigger.triggers.Add(pointerExit);
    }

    private void OnButtonHover(int buttonIndex)
    {
        _hoveredIndex = buttonIndex;
        UpdateAllButtonVisuals();

        if (enableDebugLogs)
            Debug.Log($"[WeaponSelectionUI] Hover on button index: {buttonIndex}");
    }

    private void OnButtonExit(int buttonIndex)
    {
        if (_hoveredIndex == buttonIndex)
        {
            _hoveredIndex = -1;
            UpdateAllButtonVisuals();
        }
    }

    private void UpdateAllButtonVisuals()
    {
        for (int i = 0; i < _spawnedButtons.Count; i++)
        {
            bool shouldHighlight = false;

            if (_hoveredIndex >= 0)
            {
                
                shouldHighlight = (i == _hoveredIndex);
            }
            else
            {
             
                shouldHighlight = (i == _currentEquippedIndex);
            }

            SetButtonVisual(_spawnedButtons[i], shouldHighlight);
        }
    }

    private void SetButtonVisual(GameObject buttonObj, bool highlighted)
    {
        if (buttonObj == null) return;

        var button = buttonObj.GetComponent<Button>();
        if (!button) return;

        var colors = button.colors;
        colors.normalColor = (highlighted) ? selectedButtonColor : normalButtonColor;
        button.colors = colors;

        var rt = buttonObj.GetComponent<RectTransform>();
        if (rt) rt.localScale = (highlighted) ? Vector3.one * 1.1f : Vector3.one;
    }

    private Image GetButtonIconImage(GameObject buttonObj)
    {
        if (buttonObj == null) return null;

      
        string[] possibleNames = { "Icon", "WeaponIcon", "ItemIcon", "Image", "icon", "weapon_icon" };
        foreach (string name in possibleNames)
        {
            Transform found = buttonObj.transform.Find(name);
            if (found != null)
            {
                Image img = found.GetComponent<Image>();
                if (img != null) return img;
            }
        }

   
        Image[] images = buttonObj.GetComponentsInChildren<Image>();
        foreach (var img in images)
        {
            if (img.gameObject != buttonObj)
                return img;
        }

        return null;
    }

    private void ForceSetButtonIcon(GameObject buttonObj, Sprite iconSprite, string itemName)
    {
        if (buttonObj == null) return;
        Image[] allImages = buttonObj.GetComponentsInChildren<Image>(true);

        Image targetIcon = null;
        string[] possibleNames = { "Icon", "WeaponIcon", "ItemIcon", "Image", "icon", "weapon_icon" };
        foreach (string name in possibleNames)
        {
            Transform found = buttonObj.transform.Find(name);
            if (found != null)
            {
                targetIcon = found.GetComponent<Image>();
                if (targetIcon != null) break;
            }
        }
        if (targetIcon == null)
        {
            foreach (Transform child in buttonObj.transform)
            {
                Image img = child.GetComponent<Image>();
                if (img != null && child.gameObject != buttonObj) { targetIcon = img; break; }
            }
        }
        if (targetIcon == null && allImages.Length > 0) targetIcon = allImages[0];

        if (targetIcon != null)
        {
            targetIcon.sprite = null;
            targetIcon.overrideSprite = null;
            if (iconSprite != null)
            {
                targetIcon.sprite = iconSprite;
                targetIcon.color = Color.white;  
                targetIcon.preserveAspect = true;
                Canvas.ForceUpdateCanvases();
            }
            else
            {
                targetIcon.color = new Color(1, 1, 1, 0.3f);
            }

            if (enableDebugLogs)
                Debug.Log($"[WeaponSelectionUI] Set icon for {itemName}: {(iconSprite != null ? iconSprite.name : "NULL")}");
        }
    }

    public void HideWeaponPanel()
    {
        Time.timeScale = _originalTimeScale;

        if (weaponPanel != null)
        {
            if (_panelCanvasGroup != null)
                StartCoroutine(FadeOut());
            else
                weaponPanel.SetActive(false);
        }

        _hoveredIndex = -1;
    }

    private void UpdateCurrentWeaponDisplay(IWeapon weapon)
    {
        if (currentWeaponDisplay == null) return;

        currentWeaponDisplay.SetActive(true);

        if (weapon != null)
        {
            if (currentWeaponIcon != null && weapon.WeaponIcon != null)
            {
                currentWeaponIcon.sprite = weapon.WeaponIcon;
                currentWeaponIcon.color = Color.white;
            }
            if (currentWeaponText != null)
                currentWeaponText.text = $"Current: {weapon.WeaponName}";
        }
        else
        {
            if (currentWeaponIcon != null)
            {
                currentWeaponIcon.sprite = noWeaponIcon;
                if (noWeaponIcon == null)
                    currentWeaponIcon.color = new Color(1, 1, 1, 0.3f);
            }
            if (currentWeaponText != null)
                currentWeaponText.text = "Current: No Weapon";
        }
    }

    private void ClearWeaponButtons()
    {
        foreach (var button in _spawnedButtons)
            if (button != null)
                Destroy(button);

        _spawnedButtons.Clear();
        _hasNoWeaponOption = false;
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        _panelCanvasGroup.alpha = 0f;
        while (elapsed < panelShowDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            _panelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / panelShowDuration);
            yield return null;
        }
        _panelCanvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        _panelCanvasGroup.interactable = false;
        while (elapsed < panelShowDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            _panelCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / panelShowDuration);
            yield return null;
        }
        _panelCanvasGroup.alpha = 0f;
        weaponPanel.SetActive(false);
    }
}