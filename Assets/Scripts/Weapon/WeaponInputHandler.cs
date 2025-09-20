using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponInputHandler : MonoBehaviour, IWeaponInputHandler
{
    [Header("Input Settings")]
    public InputActionProperty weaponSelectionAction;

    [Header("Navigation Input")]
    public InputActionProperty moveAction; // For weapon selection navigation
    public InputActionProperty selectAction; // For confirming weapon selection

    [Header("Input Timing")]
    public float navigationInputDelay = 0.2f; // Prevent rapid input

    public event System.Action OnWeaponSelectionRequested;
    public event System.Action<float> OnNavigationInput; // -1 for up, 1 for down
    public event System.Action OnSelectionConfirmed;

    private float _lastNavigationTime;
    private WeaponSelectionUI _weaponUI;

    void Start()
    {
        // Find the weapon UI component
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
        // Enable weapon selection action
        weaponSelectionAction.action?.Enable();
        if (weaponSelectionAction.action != null)
        {
            weaponSelectionAction.action.performed += OnWeaponSelectionInput;
        }

        // Enable navigation actions
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
        // Disable weapon selection action
        if (weaponSelectionAction.action != null)
        {
            weaponSelectionAction.action.performed -= OnWeaponSelectionInput;
        }
        weaponSelectionAction.action?.Disable();

        // Disable navigation actions
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
        OnWeaponSelectionRequested?.Invoke();
    }

    private void OnMoveInput(InputAction.CallbackContext context)
    {
        // Only process navigation input if weapon UI is visible
        if (_weaponUI != null && _weaponUI.IsVisible)
        {
            // Prevent rapid input
            if (Time.unscaledTime - _lastNavigationTime < navigationInputDelay)
                return;

            float moveValue = context.ReadValue<float>();

            // Convert to discrete up/down commands
            if (Mathf.Abs(moveValue) > 0.5f)
            {
                _lastNavigationTime = Time.unscaledTime;
                OnNavigationInput?.Invoke(moveValue);
            }
        }
    }

    private void OnSelectInput(InputAction.CallbackContext context)
    {
        // Only process selection input if weapon UI is visible
        if (_weaponUI != null && _weaponUI.IsVisible)
        {
            OnSelectionConfirmed?.Invoke();
        }
    }
}