using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class WeaponInputProcessor : MonoBehaviour, IWeaponInputProcessor
{
    [Header("Input Actions")]
    public InputActionProperty primaryAttackAction;
    public InputActionProperty secondaryAttackAction;

    [Header("Settings")]
    public bool continuousInput = true;

    private bool _primaryAttackHeld = false;
    private bool _secondaryAttackHeld = false;

    public event System.Action OnPrimaryAttackStart;
    public event System.Action OnPrimaryAttackStop;
    public event System.Action OnSecondaryAttackStart;
    public event System.Action OnSecondaryAttackStop;

    private PauseMenuManager _pauseMenu;
    private PauseManager _pauseManager;
    private WeaponSelectionUI _weaponUI;

    void Awake()
    {
        _pauseMenu = FindFirstObjectByType<PauseMenuManager>();
        _pauseManager = FindFirstObjectByType<PauseManager>();
        _weaponUI = FindFirstObjectByType<WeaponSelectionUI>();
    }

    void OnEnable()
    {
        EnableInput();
    }

    void OnDisable()
    {
        DisableInput();
    }

    private void EnableInput()
    {
        if (primaryAttackAction.action != null)
        {
            primaryAttackAction.action.Enable();
            primaryAttackAction.action.started += OnPrimaryAttackStarted;
            primaryAttackAction.action.canceled += OnPrimaryAttackCanceled;
        }

        if (secondaryAttackAction.action != null)
        {
            secondaryAttackAction.action.Enable();
            secondaryAttackAction.action.started += OnSecondaryAttackStarted;
            secondaryAttackAction.action.canceled += OnSecondaryAttackCanceled;
        }
    }

    private void DisableInput()
    {
        if (primaryAttackAction.action != null)
        {
            primaryAttackAction.action.started -= OnPrimaryAttackStarted;
            primaryAttackAction.action.canceled -= OnPrimaryAttackCanceled;
            primaryAttackAction.action.Disable();
        }

        if (secondaryAttackAction.action != null)
        {
            secondaryAttackAction.action.started -= OnSecondaryAttackStarted;
            secondaryAttackAction.action.canceled -= OnSecondaryAttackCanceled;
            secondaryAttackAction.action.Disable();
        }
    }
    private bool IsGloballyBlocked()
    {

        if ((_pauseMenu != null && _pauseMenu.IsPaused) ||
       (_pauseManager != null && _pauseManager.IsPaused) ||
       Time.timeScale == 0f)
            return true;

        if (_weaponUI != null && _weaponUI.IsVisible)
            return true;


        return false;
    }
    private void OnPrimaryAttackStarted(InputAction.CallbackContext context)
    {
        if (IsGloballyBlocked()) return;
        if (!_primaryAttackHeld)
        {
            _primaryAttackHeld = true;
            OnPrimaryAttackStart?.Invoke();
        }
    }

    private void OnPrimaryAttackCanceled(InputAction.CallbackContext context)
    {
        if (_primaryAttackHeld)
        {
            _primaryAttackHeld = false;
            OnPrimaryAttackStop?.Invoke();
        }
    }

    private void OnSecondaryAttackStarted(InputAction.CallbackContext context)
    {
        if (IsGloballyBlocked()) return;
        if (!_secondaryAttackHeld)
        {
            _secondaryAttackHeld = true;
            OnSecondaryAttackStart?.Invoke();
        }
    }

    private void OnSecondaryAttackCanceled(InputAction.CallbackContext context)
    {
        if (_secondaryAttackHeld)
        {
            _secondaryAttackHeld = false;
            OnSecondaryAttackStop?.Invoke();
        }
    }

    public void ProcessInput(IWeaponAttackBehavior primaryAttack, IWeaponAttackBehavior secondaryAttack)
    {
        
    }
}
