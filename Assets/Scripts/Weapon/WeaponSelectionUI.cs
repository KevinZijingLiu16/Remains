using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;

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

    [Header("Input Actions")]
    public InputActionProperty moveAction;
    public InputActionProperty selectAction;

    [Header("Navigation")]
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

    // Navigation state
    private int _selectedIndex = 0;
    private bool _navigationMode = false;
    private float _originalTimeScale;
    private float _lastNavigationTime = 0f;
    private float _navigationInputDelay = 0.2f;

    void Start()
    {
        InitializeUI();
        InitializeInputActions();
        StartCoroutine(DelayedInitializeEquipmentManager());
    }

    private void InitializeInputActions()
    {
        // Enable input actions
        if (moveAction.action != null)
        {
            moveAction.action.Enable();
            moveAction.action.performed += OnMoveInputAction;
        }

        if (selectAction.action != null)
        {
            selectAction.action.Enable();
            selectAction.action.performed += OnSelectInputAction;
        }
    }
    private void OnMoveInputAction(InputAction.CallbackContext context)
    {
        // Only process navigation input if weapon UI is visible
        if (!_navigationMode || !IsVisible) return;

        // Prevent rapid input
        if (Time.unscaledTime - _lastNavigationTime < _navigationInputDelay) return;

        float moveValue = context.ReadValue<float>();

        if (enableDebugLogs) Debug.Log($"[WeaponSelectionUI] Move input received: {moveValue}");

        // Handle navigation based on your 1D Axis setup
        if (moveValue > 0.5f) // Positive = D key
        {
            NavigateDown(); // Move down the list
            _lastNavigationTime = Time.unscaledTime;
        }
        else if (moveValue < -0.5f) // Negative = A key  
        {
            NavigateUp(); // Move up the list
            _lastNavigationTime = Time.unscaledTime;
        }
    }

    private void OnSelectInputAction(InputAction.CallbackContext context)
    {
        // Only process selection input if weapon UI is visible
        if (!_navigationMode || !IsVisible) return;

        if (enableDebugLogs) Debug.Log("[WeaponSelectionUI] Select input received");
        SelectCurrentWeapon();
    }
    void OnDestroy()
    {
        if (_equipmentManager != null)
        {
            _equipmentManager.OnWeaponEquipped -= UpdateCurrentWeaponDisplay;
            _equipmentManager.OnWeaponUnequipped -= UpdateCurrentWeaponDisplay;
        }

        // Disable input actions
        if (moveAction.action != null)
        {
            moveAction.action.performed -= OnMoveInputAction;
            moveAction.action.Disable();
        }

        if (selectAction.action != null)
        {
            selectAction.action.performed -= OnSelectInputAction;
            selectAction.action.Disable();
        }

        // Restore time scale if destroyed while panel is open
        if (_navigationMode)
        {
            Time.timeScale = _originalTimeScale;
        }
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

   

    void Update()
    {
      
    }

    private void HandleNavigationInput()
    {
        // Get move input from Input Manager (configured in your input system)
        float moveInput = Input.GetAxis("Move"); // This should map to your 1D Axis positive/negative

        // Alternative: Use Input System if you prefer
        // You can also check for specific key presses:
        bool moveUp = Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
        bool moveDown = Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow);

        // Handle vertical navigation
        if (moveUp || moveInput > 0.5f)
        {
            NavigateUp();
        }
        else if (moveDown || moveInput < -0.5f)
        {
            NavigateDown();
        }

        // Handle selection (you can map this to your TurnOnWeapon input or Enter key)
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            SelectCurrentWeapon();
        }
    }

    private void NavigateUp()
    {
        if (_spawnedButtons.Count == 0) return;

        _selectedIndex = (_selectedIndex - 1 + _spawnedButtons.Count) % _spawnedButtons.Count;
        UpdateButtonVisuals();

        if (enableDebugLogs) Debug.Log($"[WeaponSelectionUI] Navigated up to index: {_selectedIndex}");
    }

    private void NavigateDown()
    {
        if (_spawnedButtons.Count == 0) return;

        _selectedIndex = (_selectedIndex + 1) % _spawnedButtons.Count;
        UpdateButtonVisuals();

        if (enableDebugLogs) Debug.Log($"[WeaponSelectionUI] Navigated down to index: {_selectedIndex}");
    }

    private void SelectCurrentWeapon()
    {
        if (_selectedIndex >= 0 && _selectedIndex < _currentWeapons.Count)
        {
            var selectedWeapon = _currentWeapons[_selectedIndex];
            if (enableDebugLogs) Debug.Log($"[WeaponSelectionUI] Selected weapon via navigation: {selectedWeapon.WeaponName}");

            OnWeaponSelected?.Invoke(selectedWeapon.WeaponId);
            HideWeaponPanel();
        }
    }

    private void UpdateButtonVisuals()
    {
        for (int i = 0; i < _spawnedButtons.Count; i++)
        {
            var button = _spawnedButtons[i].GetComponent<Button>();
            if (button != null)
            {
                ColorBlock colors = button.colors;

                if (i == _selectedIndex)
                {
                    
                    colors.normalColor = selectedButtonColor;
                }
                else
                {
                  
                    colors.normalColor = normalButtonColor;
                }

                button.colors = colors;
            }
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

        // Store original time scale and pause game
        _originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        _navigationMode = true;

        // Clear previous weapons and buttons
        ClearWeaponButtons();
        _currentWeapons.Clear();

        // Create new buttons and store weapon references
        _selectedIndex = 0;
        for (int i = 0; i < availableWeapons.Length; i++)
        {
            var weapon = availableWeapons[i];
            _currentWeapons.Add(weapon);
            CreateWeaponButton(weapon, i);

            // Set selected index to currently equipped weapon
            if (_equipmentManager?.CurrentWeapon?.WeaponId == weapon.WeaponId)
            {
                _selectedIndex = i;
            }
        }

        // Update current weapon display
        UpdateCurrentWeaponDisplay(_equipmentManager?.CurrentWeapon);

        // Show panel
        weaponPanel.SetActive(true);

        if (_panelCanvasGroup != null)
        {
            _panelCanvasGroup.interactable = true;
            _panelCanvasGroup.blocksRaycasts = true;
            StartCoroutine(FadeIn());
        }

        // Update button visuals after everything is set up
        UpdateButtonVisuals();

        if (enableDebugLogs) Debug.Log($"[WeaponSelectionUI] Created {_spawnedButtons.Count} weapon buttons, selected index: {_selectedIndex}");
    }

    public void HideWeaponPanel()
    {
        if (enableDebugLogs) Debug.Log("[WeaponSelectionUI] Hiding weapon panel");

        // Restore time scale
        if (_navigationMode)
        {
            Time.timeScale = _originalTimeScale;
            _navigationMode = false;
        }

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
                currentWeaponText.text = $"current weapon: {weapon.WeaponName}";
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
                currentWeaponText.text = "no weapon equipped";
                if (enableDebugLogs) Debug.Log("[WeaponSelectionUI] Set empty weapon text");
            }
        }

        if (unequipButton != null)
        {
            unequipButton.interactable = hasWeapon;
            if (enableDebugLogs) Debug.Log($"[WeaponSelectionUI] Updated unequip button interactable: {hasWeapon}");
        }

        // Update button visuals if in navigation mode
        if (_navigationMode)
        {
            UpdateButtonVisuals();
        }
    }

    private void CreateWeaponButton(IWeapon weapon, int index)
    {
        if (weaponButtonPrefab == null || weaponButtonParent == null) return;

        GameObject buttonObj = Instantiate(weaponButtonPrefab, weaponButtonParent);
        _spawnedButtons.Add(buttonObj);

        // Set up icon
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

        // Set up text
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

        // Set up button click (still works for mouse/touch input)
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
            elapsed += Time.unscaledDeltaTime; // Use unscaled time since game is paused
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
            elapsed += Time.unscaledDeltaTime; // Use unscaled time since game is paused
            _panelCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / panelShowDuration);
            yield return null;
        }
        _panelCanvasGroup.alpha = 0f;
        weaponPanel.SetActive(false);
    }

    // Debug methods
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