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

    [Header("Input Actions")]
    public InputActionProperty moveAction;

    [Header("Navigation")]
    public Color normalButtonColor = Color.white;
    public Color selectedButtonColor = Color.yellow;

    [Header("Auto-Confirm Settings")]
    public float autoConfirmDelay = 1.0f;
    public float inputCooldown = 0.15f;

    [Header("Settings")]
    public float panelShowDuration = 0.3f;

    [Header("Mouse / Pointer")]
    public bool allowMousePointer = false;

    [Header("Debug")]
    public bool enableDebugLogs = true;

    public event System.Action<string> OnWeaponSelected;
    public event System.Action OnWeaponUnequipRequested;

    public bool IsVisible => weaponPanel != null && weaponPanel.activeInHierarchy;

    private List<GameObject> _spawnedButtons = new List<GameObject>();
    private List<IWeapon> _currentWeapons = new List<IWeapon>();
    private CanvasGroup _panelCanvasGroup;
    private IWeaponEquipmentManager _equipmentManager;

    private int _selectedIndex = 0;
    private bool _navigationMode = false;
    private float _originalTimeScale;
    private float _lastInputTime = 0f;
    private Coroutine _autoConfirmCoroutine;
    private bool _hasNoWeaponOption = false;

    void Start()
    {
        InitializeUI();
        InitializeInputActions();
        StartCoroutine(DelayedInitializeEquipmentManager());
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (weaponPanel != null)
            ApplyMousePointerSetting();
    }
#endif

    private void InitializeInputActions()
    {
        if (moveAction.action != null)
        {
            moveAction.action.Enable();
            moveAction.action.performed += OnMoveInputAction;
        }
    }

    private void OnMoveInputAction(InputAction.CallbackContext context)
    {
        if (!_navigation_mode_or_not()) return;

        float moveValue = context.ReadValue<float>();
        if (Mathf.Abs(moveValue) <= 0.5f) return;

        _lastInputTime = Time.unscaledTime;
        RestartAutoConfirm();

        if (moveValue > 0.5f) NavigateDown();
        else NavigateUp();
    }

    private bool _navigation_mode_or_not()
    {
        if (!_navigationMode || !IsVisible) return false;
        if (Time.unscaledTime - _lastInputTime < inputCooldown) return false;
        return true;
    }

    void OnDestroy()
    {
        if (_equipmentManager != null)
        {
            _equipmentManager.OnWeaponEquipped -= UpdateCurrentWeaponDisplay;
            _equipmentManager.OnWeaponUnequipped -= UpdateCurrentWeaponDisplay;
        }
        if (moveAction.action != null)
        {
            moveAction.action.performed -= OnMoveInputAction;
            moveAction.action.Disable();
        }
        if (_navigationMode) Time.timeScale = _originalTimeScale;
        if (_autoConfirmCoroutine != null) StopCoroutine(_autoConfirmCoroutine);
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

    private void NavigateUp()
    {
        if (_spawnedButtons.Count == 0) return;
        _selectedIndex = (_selectedIndex - 1 + _spawnedButtons.Count) % _spawnedButtons.Count;
        UpdateButtonVisuals();
    }

    private void NavigateDown()
    {
        if (_spawnedButtons.Count == 0) return;
        _selectedIndex = (_selectedIndex + 1) % _spawnedButtons.Count;
        UpdateButtonVisuals();
    }

    private void RestartAutoConfirm()
    {
        if (_autoConfirmCoroutine != null) StopCoroutine(_autoConfirmCoroutine);
        _autoConfirmCoroutine = StartCoroutine(AutoConfirmSelection());
    }

    private IEnumerator AutoConfirmSelection()
    {
        yield return new WaitForSecondsRealtime(autoConfirmDelay);
        ConfirmCurrentSelection();
    }

    private void ConfirmCurrentSelection()
    {
        if (_hasNoWeaponOption && _selectedIndex == 0)
            OnWeaponUnequipRequested?.Invoke();
        else
        {
            int weaponIndex = _hasNoWeaponOption ? _selectedIndex - 1 : _selectedIndex;
            if (weaponIndex >= 0 && weaponIndex < _currentWeapons.Count)
                OnWeaponSelected?.Invoke(_currentWeapons[weaponIndex].WeaponId);
        }
        HideWeaponPanel();
    }

    private void UpdateButtonVisuals()
    {
        for (int i = 0; i < _spawnedButtons.Count; i++)
        {
            var button = _spawnedButtons[i].GetComponent<Button>();
            if (!button) continue;

            var colors = button.colors;
            colors.normalColor = (i == _selectedIndex) ? selectedButtonColor : normalButtonColor;
            button.colors = colors;

            var rt = _spawnedButtons[i].GetComponent<RectTransform>();
            if (rt) rt.localScale = (i == _selectedIndex) ? Vector3.one * 1.1f : Vector3.one;
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
            closeButton.onClick.AddListener(() =>
            {
                if (_autoConfirmCoroutine != null) StopCoroutine(_autoConfirmCoroutine);
                HideWeaponPanel();
            });
        }

        ApplyMousePointerSetting();
        UpdateCurrentWeaponDisplay(null);
    }

    public void ShowWeaponPanel(IWeapon[] availableWeapons)
    {
        if (weaponPanel == null || availableWeapons == null) return;

        _originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        _navigationMode = true;

        ClearWeaponButtons();
        _currentWeapons.Clear();
        _currentWeapons.AddRange(availableWeapons);

        _hasNoWeaponOption = true;
        CreateNoWeaponButton();
        for (int i = 0; i < availableWeapons.Length; i++)
            CreateWeaponButton(availableWeapons[i], i + 1);

        _selectedIndex = 0;
        UpdateCurrentWeaponDisplay(_equipmentManager?.CurrentWeapon);

        weaponPanel.SetActive(true);

        if (_panelCanvasGroup != null)
        {
            _panelCanvasGroup.interactable = true;
            _panelCanvasGroup.blocksRaycasts = true;
            StartCoroutine(FadeIn());
        }

        ApplyMousePointerSetting(); 
        UpdateButtonVisuals();
        RestartAutoConfirm();
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
                if (_autoConfirmCoroutine != null) StopCoroutine(_autoConfirmCoroutine);
                OnWeaponUnequipRequested?.Invoke();
                HideWeaponPanel();
            });
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
                if (_autoConfirmCoroutine != null) StopCoroutine(_autoConfirmCoroutine);
                OnWeaponSelected?.Invoke(weaponId);
                HideWeaponPanel();
            });
        }
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
        }
    }

    public void HideWeaponPanel()
    {
        if (_autoConfirmCoroutine != null)
        {
            StopCoroutine(_autoConfirmCoroutine);
            _autoConfirmCoroutine = null;
        }
        if (_navigationMode)
        {
            Time.timeScale = _originalTimeScale;
            _navigationMode = false;
        }

        if (weaponPanel != null)
        {
            if (_panelCanvasGroup != null) StartCoroutine(FadeOut());
            else weaponPanel.SetActive(false);
        }
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
            if (currentWeaponText != null) currentWeaponText.text = $"Current: {weapon.WeaponName}";
        }
        else
        {
            if (currentWeaponIcon != null)
            {
                currentWeaponIcon.sprite = noWeaponIcon;
                if (noWeaponIcon == null) currentWeaponIcon.color = new Color(1, 1, 1, 0.3f);
            }
            if (currentWeaponText != null) currentWeaponText.text = "Current: No Weapon";
        }
    }

    private void ClearWeaponButtons()
    {
        foreach (var button in _spawnedButtons) if (button != null) Destroy(button);
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


    private void ApplyMousePointerSetting()
    {
        if (weaponPanel == null) return;
        var graphics = weaponPanel.GetComponentsInChildren<Graphic>(true);
        foreach (var g in graphics)
            g.raycastTarget = allowMousePointer;
    }

    public void SetAllowMousePointer(bool enabled)
    {
        allowMousePointer = enabled;
        ApplyMousePointerSetting();
    }
}
