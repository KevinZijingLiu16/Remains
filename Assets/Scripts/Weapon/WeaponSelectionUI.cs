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
    public float gamepadNavigationDelay = 0.3f;  // 手柄导航延迟
    public float mouseMovementThreshold = 5f;     // 鼠标移动检测阈值

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
    private int _highlightedIndex = 0;  // 当前高亮的索引
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

        // 订阅输入事件
        if (navigateActionRef.action != null)
        {
            // 对于鼠标，需要started和performed
            // 对于手柄，只需要performed
            navigateActionRef.action.performed += OnNavigate;
            navigateActionRef.action.started += OnNavigate;
            navigateActionRef.action.canceled += OnNavigateCanceled; // 添加取消事件
        }

        if (confirmActionRef.action != null)
        {
            confirmActionRef.action.performed += OnConfirm;
        }
    }

    // 添加取消处理
    private void OnNavigateCanceled(InputAction.CallbackContext context)
    {
        // 可以在这里重置一些状态
        if (enableDebugLogs)
            Debug.Log($"[WeaponSelectionUI] Navigate canceled from: {context.control.path}");
    }
    void OnEnable()
    {
        // 启用Actions
        navigateActionRef?.action?.Enable();
        confirmActionRef?.action?.Enable();
    }

    void OnDisable()
    {
        // 禁用Actions
        navigateActionRef?.action?.Disable();
        confirmActionRef?.action?.Disable();
    }

    void OnDestroy()
    {
        // 取消订阅
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

        // 更精确地判断输入来源
        string controlPath = context.control.path;

        if (enableDebugLogs)
            Debug.Log($"[WeaponSelectionUI] Navigate input from: {controlPath}");

        // 判断输入类型
        if (controlPath.Contains("Mouse") || controlPath.Contains("Position"))
        {
            Vector2 mousePos = context.ReadValue<Vector2>();
            HandleMouseNavigation(mousePos);
        }
        else if (controlPath.Contains("Stick") || controlPath.Contains("Gamepad") || controlPath.Contains("DPad"))
        {
            Vector2 stickInput = context.ReadValue<Vector2>();

            // 只在performed阶段处理手柄输入，避免重复
            if (context.phase == InputActionPhase.Performed)
            {
                HandleGamepadNavigation(stickInput);
            }
        }
    }

    private void HandleMouseNavigation(Vector2 mousePosition)
    {
        // 检测鼠标是否真的在移动
        if (Vector2.Distance(mousePosition, _lastMousePosition) < mouseMovementThreshold)
            return;

        _isUsingMouse = true;
        _lastMousePosition = mousePosition;

        // 检测鼠标在哪个按钮上
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
        // 防止过快导航
        if (Time.unscaledTime - _lastGamepadNavigationTime < gamepadNavigationDelay)
            return;

        // 增加死区，减少误触发
        float deadZone = 0.7f; // 提高死区值

        // 只处理明显的输入
        if (Mathf.Abs(stickInput.y) < deadZone && Mathf.Abs(stickInput.x) < deadZone)
            return;

        _isUsingMouse = false;
        _lastGamepadNavigationTime = Time.unscaledTime;

        // 优先处理垂直方向
        if (Mathf.Abs(stickInput.y) > Mathf.Abs(stickInput.x))
        {
            // 上下导航
            if (stickInput.y > deadZone)
            {
                _highlightedIndex--;
                if (_highlightedIndex < 0)
                    _highlightedIndex = _spawnedButtons.Count - 1;
            }
            else if (stickInput.y < -deadZone) // 注意这里改为负值
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
        // 使用光线投射检测鼠标在哪个按钮上
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = screenPosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            GameObject hitObject = result.gameObject;

            // 检查是否是我们的武器按钮
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

        // 无论是鼠标还是手柄，都选择当前高亮的项
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
            // 选择了"无武器"
            if (enableDebugLogs)
                Debug.Log("[WeaponSelectionUI] No Weapon selected!");
            OnWeaponUnequipRequested?.Invoke();
        }
        else
        {
            // 选择了具体武器
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

        // 改变颜色
        var image = buttonObj.GetComponent<Image>();
        if (image != null)
        {
            image.color = highlighted ? selectedButtonColor : normalButtonColor;
        }

        // 改变缩放
        var rt = buttonObj.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = highlighted ? Vector3.one * 1.15f : Vector3.one;
        }
    }

    // ... 保留所有其他方法不变 ...

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

        // 获取当前装备的武器索引
        _currentEquippedIndex = GetCurrentEquippedIndex();

        // 初始高亮当前装备的武器（如果有的话）
        _highlightedIndex = _currentEquippedIndex >= 0 ? _currentEquippedIndex : 0;

        // 创建按钮
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

        // 更新视觉效果
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

        // 移除Button组件的onClick监听（我们用Input System处理）
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

        // 移除Button组件的onClick监听（我们用Input System处理）
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