using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    [Header("Input System - Direct Action References")]
    public InputActionReference navigateActionRef;
    public InputActionReference confirmActionRef;

    [Header("Navigation Settings")]
    public float gamepadNavigationDelay = 0.3f; 
    public float mouseMovementThreshold = 5f;     

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
    private int _highlightedIndex = 0; 
    private float _originalTimeScale;
    private bool _hasNoWeaponOption = false;

    private bool _isUsingMouse = true;
    private Vector2 _lastMousePosition;
    private float _lastGamepadNavigationTime;
    private Camera _uiCamera;

    void Start()
    {
        InitializeUI();
        InitializeInputSystem();
        StartCoroutine(DelayedInitializeEquipmentManager());

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            _uiCamera = canvas.worldCamera;
        }
    }

    private void InitializeInputSystem()
    {
        if (navigateActionRef == null || confirmActionRef == null)
        {
            Debug.LogError("[WeaponSelectionUI] Input Action References not assigned!");
            return;
        }

        if (navigateActionRef.action != null)
        {
            navigateActionRef.action.performed += OnNavigate;
            navigateActionRef.action.started += OnNavigate;
            navigateActionRef.action.canceled += OnNavigateCanceled; 
        }

        if (confirmActionRef.action != null)
        {
            confirmActionRef.action.performed += OnConfirm;
        }
    }

    private void OnNavigateCanceled(InputAction.CallbackContext context)
    {
        if (enableDebugLogs)
            Debug.Log($"[WeaponSelectionUI] Navigate canceled from: {context.control.path}");
    }
    void OnEnable()
    {
        navigateActionRef?.action?.Enable();
        confirmActionRef?.action?.Enable();
    }

    void OnDisable()
    {
        navigateActionRef?.action?.Disable();
        confirmActionRef?.action?.Disable();
    }

    void OnDestroy()
    {
        if (navigateActionRef?.action != null)
        {
            navigateActionRef.action.performed -= OnNavigate;
            navigateActionRef.action.started -= OnNavigate;
        }

        if (confirmActionRef?.action != null)
        {
            confirmActionRef.action.performed -= OnConfirm;
        }

        if (_equipmentManager != null)
        {
            _equipmentManager.OnWeaponEquipped -= UpdateCurrentWeaponDisplay;
            _equipmentManager.OnWeaponUnequipped -= UpdateCurrentWeaponDisplay;
        }
    }

    private void OnNavigate(InputAction.CallbackContext context)
    {
        if (!IsVisible) return;

        string controlPath = context.control.path;

        if (enableDebugLogs)
            Debug.Log($"[WeaponSelectionUI] Navigate input from: {controlPath}");

        if (controlPath.Contains("Mouse") || controlPath.Contains("Position"))
        {
            Vector2 mousePos = context.ReadValue<Vector2>();
            HandleMouseNavigation(mousePos);
        }
        else if (controlPath.Contains("Stick") || controlPath.Contains("Gamepad") || controlPath.Contains("DPad"))
        {
            Vector2 stickInput = context.ReadValue<Vector2>();

            if (context.phase == InputActionPhase.Performed)
            {
                HandleGamepadNavigation(stickInput);
            }
        }
    }

    private void HandleMouseNavigation(Vector2 mousePosition)
    {
        if (Vector2.Distance(mousePosition, _lastMousePosition) < mouseMovementThreshold)
            return;

        _isUsingMouse = true;
        _lastMousePosition = mousePosition;

        int hoveredIndex = GetButtonIndexAtPosition(mousePosition);

        if (hoveredIndex >= 0 && hoveredIndex != _highlightedIndex)
        {
            _highlightedIndex = hoveredIndex;
            UpdateButtonVisuals();

            if (enableDebugLogs)
                Debug.Log($"[WeaponSelectionUI] Mouse navigate to index: {_highlightedIndex}");
        }
    }

    private void HandleGamepadNavigation(Vector2 stickInput)
    {
        if (Time.unscaledTime - _lastGamepadNavigationTime < gamepadNavigationDelay)
            return;

        float deadZone = 0.7f; 

        if (Mathf.Abs(stickInput.y) < deadZone && Mathf.Abs(stickInput.x) < deadZone)
            return;

        _isUsingMouse = false;
        _lastGamepadNavigationTime = Time.unscaledTime;

        if (Mathf.Abs(stickInput.y) > Mathf.Abs(stickInput.x))
        {
            if (stickInput.y > deadZone)
            {
                _highlightedIndex--;
                if (_highlightedIndex < 0)
                    _highlightedIndex = _spawnedButtons.Count - 1;
            }
            else if (stickInput.y < -deadZone) 
            {
                _highlightedIndex++;
                if (_highlightedIndex >= _spawnedButtons.Count)
                    _highlightedIndex = 0;
            }
        }

        UpdateButtonVisuals();

        if (enableDebugLogs)
            Debug.Log($"[WeaponSelectionUI] Gamepad navigate to index: {_highlightedIndex}, stick value: {stickInput}");
    }

    private int GetButtonIndexAtPosition(Vector2 screenPosition)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = screenPosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            GameObject hitObject = result.gameObject;

            for (int i = 0; i < _spawnedButtons.Count; i++)
            {
                if (_spawnedButtons[i] == hitObject ||
                    hitObject.transform.IsChildOf(_spawnedButtons[i].transform))
                {
                    return i;
                }
            }
        }

        return -1;
    }

    private void OnConfirm(InputAction.CallbackContext context)
    {
        if (!IsVisible) return;

        SelectWeaponAtIndex(_highlightedIndex);

        if (enableDebugLogs)
        {
            string device = context.control.device is Mouse ? "Mouse" : "Gamepad";
            Debug.Log($"[WeaponSelectionUI] Confirm from {device} at index: {_highlightedIndex}");
        }
    }

    private void SelectWeaponAtIndex(int index)
    {
        if (index < 0 || index >= _spawnedButtons.Count) return;

        if (index == 0 && _hasNoWeaponOption)
        {
            if (enableDebugLogs)
                Debug.Log("[WeaponSelectionUI] No Weapon selected!");
            OnWeaponUnequipRequested?.Invoke();
        }
        else
        {
            int weaponIndex = _hasNoWeaponOption ? index - 1 : index;
            if (weaponIndex >= 0 && weaponIndex < _currentWeapons.Count)
            {
                var weapon = _currentWeapons[weaponIndex];
                if (enableDebugLogs)
                    Debug.Log($"[WeaponSelectionUI] Weapon selected: {weapon.WeaponName}");
                OnWeaponSelected?.Invoke(weapon.WeaponId);
            }
        }

        HideWeaponPanel();
    }

    private void UpdateButtonVisuals()
    {
        for (int i = 0; i < _spawnedButtons.Count; i++)
        {
            bool shouldHighlight = (i == _highlightedIndex);
            SetButtonVisual(_spawnedButtons[i], shouldHighlight);
        }
    }

    private void SetButtonVisual(GameObject buttonObj, bool highlighted)
    {
        if (buttonObj == null) return;

        var image = buttonObj.GetComponent<Image>();
        if (image != null)
        {
            image.color = highlighted ? selectedButtonColor : normalButtonColor;
        }

        var rt = buttonObj.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = highlighted ? Vector3.one * 1.15f : Vector3.one;
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

        _highlightedIndex = _currentEquippedIndex >= 0 ? _currentEquippedIndex : 0;

        CreateNoWeaponButton();
        for (int i = 0; i < availableWeapons.Length; i++)
        {
            CreateWeaponButton(availableWeapons[i], i + 1);
        }

        UpdateCurrentWeaponDisplay(_equipmentManager?.CurrentWeapon);

        weaponPanel.SetActive(true);

        if (_panelCanvasGroup != null)
        {
            _panelCanvasGroup.interactable = true;
            _panelCanvasGroup.blocksRaycasts = true;
            StartCoroutine(FadeIn());
        }

        UpdateButtonVisuals();
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
        }
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

        _highlightedIndex = -1;
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
        {
            if (button != null)
                Destroy(button);
        }
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