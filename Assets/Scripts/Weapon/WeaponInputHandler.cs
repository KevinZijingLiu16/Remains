using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponInputHandler : MonoBehaviour, IWeaponInputHandler
{
    [Header("Input Settings")]
    public InputActionProperty weaponSelectionAction;

    [Header("New: Quick Cycle and Hotkeys")]
    public InputActionProperty quickCycleAction; // Q key
    public InputActionProperty hotkey1Action; // 1 key - No Weapon
    public InputActionProperty hotkey2Action; // 2 key - Foam Spray
    public InputActionProperty hotkey3Action; // 3 key - Air Blower

    [Header("Navigation Input")]
    public InputActionProperty moveAction;
    public InputActionProperty selectAction;

    [Header("Input Timing")]
    public float navigationInputDelay = 0.2f;

    public event System.Action OnWeaponSelectionRequested;
    public event System.Action<float> OnNavigationInput;
    public event System.Action OnSelectionConfirmed;

    // New events
    public event System.Action OnQuickCyclePressed;
    public event System.Action<int> OnHotkeyPressed; // 1, 2, or 3

    private float _lastNavigationTime;
    private WeaponSelectionUI _weaponUI;

    void Start()
    {
        _weaponUI = FindFirstObjectByType<WeaponSelectionUI>();
    }

    void OnEnable()
    {
        Enable();
    }

    void OnDisable()
    {
        Disable();
    }

    public void Enable()
    {
        // Original weapon selection
        weaponSelectionAction.action?.Enable();
        if (weaponSelectionAction.action != null)
        {
            weaponSelectionAction.action.performed += OnWeaponSelectionInput;
        }

        // Quick cycle (Q key)
        quickCycleAction.action?.Enable();
        if (quickCycleAction.action != null)
        {
            quickCycleAction.action.performed += OnQuickCycleInput;
        }

        // Hotkeys (1, 2, 3)
        hotkey1Action.action?.Enable();
        if (hotkey1Action.action != null)
        {
            hotkey1Action.action.performed += ctx => OnHotkeyInput(1);
        }

        hotkey2Action.action?.Enable();
        if (hotkey2Action.action != null)
        {
            hotkey2Action.action.performed += ctx => OnHotkeyInput(2);
        }

        hotkey3Action.action?.Enable();
        if (hotkey3Action.action != null)
        {
            hotkey3Action.action.performed += ctx => OnHotkeyInput(3);
        }

        // Navigation actions
        moveAction.action?.Enable();
        if (moveAction.action != null)
        {
            moveAction.action.performed += OnMoveInput;
        }

        selectAction.action?.Enable();
        if (selectAction.action != null)
        {
            selectAction.action.performed += OnSelectInput;
        }
    }

    public void Disable()
    {
        // Original weapon selection
        if (weaponSelectionAction.action != null)
        {
            weaponSelectionAction.action.performed -= OnWeaponSelectionInput;
        }
        weaponSelectionAction.action?.Disable();

        // Quick cycle
        if (quickCycleAction.action != null)
        {
            quickCycleAction.action.performed -= OnQuickCycleInput;
        }
        quickCycleAction.action?.Disable();

        // Hotkeys
        if (hotkey1Action.action != null)
        {
            hotkey1Action.action.performed -= ctx => OnHotkeyInput(1);
        }
        hotkey1Action.action?.Disable();

        if (hotkey2Action.action != null)
        {
            hotkey2Action.action.performed -= ctx => OnHotkeyInput(2);
        }
        hotkey2Action.action?.Disable();

        if (hotkey3Action.action != null)
        {
            hotkey3Action.action.performed -= ctx => OnHotkeyInput(3);
        }
        hotkey3Action.action?.Disable();

        // Navigation
        if (moveAction.action != null)
        {
            moveAction.action.performed -= OnMoveInput;
        }
        moveAction.action?.Disable();

        if (selectAction.action != null)
        {
            selectAction.action.performed -= OnSelectInput;
        }
        selectAction.action?.Disable();
    }

    private void OnWeaponSelectionInput(InputAction.CallbackContext context)
    {
        Debug.Log("[WeaponInputHandler] Tab/WeaponSelection input received");
        OnWeaponSelectionRequested?.Invoke();
    }

    private void OnQuickCycleInput(InputAction.CallbackContext context)
    {
        Debug.Log("[WeaponInputHandler] Q/QuickCycle input received");
        OnQuickCyclePressed?.Invoke();
    }

    private void OnHotkeyInput(int hotkeyNumber)
    {
        Debug.Log($"[WeaponInputHandler] Hotkey {hotkeyNumber} input received");
        OnHotkeyPressed?.Invoke(hotkeyNumber);
    }

    private void OnMoveInput(InputAction.CallbackContext context)
    {
        if (_weaponUI != null && _weaponUI.IsVisible)
        {
            if (Time.unscaledTime - _lastNavigationTime < navigationInputDelay)
                return;

            float moveValue = context.ReadValue<float>();
            if (Mathf.Abs(moveValue) > 0.5f)
            {
                _lastNavigationTime = Time.unscaledTime;
                OnNavigationInput?.Invoke(moveValue);
            }
        }
    }

    private void OnSelectInput(InputAction.CallbackContext context)
    {
        if (_weaponUI != null && _weaponUI.IsVisible)
        {
            OnSelectionConfirmed?.Invoke();
        }
    }
}