using UnityEngine;

public class WeaponSystemController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private WeaponDatabase weaponDatabase;
    [SerializeField] private WeaponInputHandler inputHandler;
    [SerializeField] private WeaponSelectionUI selectionUI;
    [SerializeField] private WeaponEquipmentManager equipmentManager;

    private IWeaponDataProvider _dataProvider;
    private IWeaponInputHandler _inputHandler;
    private IWeaponSelectionUI _selectionUI;
    private IWeaponEquipmentManager _equipmentManager;

    void Start()
    {
        InitializeDependencies();
        SubscribeToEvents();
    }

    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void InitializeDependencies()
    {
        _dataProvider = weaponDatabase;
        _inputHandler = inputHandler;
        _selectionUI = selectionUI;
        _equipmentManager = equipmentManager;

        ValidateDependencies();
    }

    private void ValidateDependencies()
    {
        if (_dataProvider == null)
            Debug.LogError("[WeaponSystemController] IWeaponDataProvider not assigned!");
        if (_inputHandler == null)
            Debug.LogError("[WeaponSystemController] IWeaponInputHandler not assigned!");
        if (_selectionUI == null)
            Debug.LogError("[WeaponSystemController] IWeaponSelectionUI not assigned!");
        if (_equipmentManager == null)
            Debug.LogError("[WeaponSystemController] IWeaponEquipmentManager not assigned!");
    }

    private void SubscribeToEvents()
    {
        if (_inputHandler != null)
            _inputHandler.OnWeaponSelectionRequested += HandleWeaponSelectionRequest;

        if (_selectionUI != null)
            _selectionUI.OnWeaponSelected += HandleWeaponSelection;

        if (_equipmentManager != null)
        {
            _equipmentManager.OnWeaponEquipped += HandleWeaponEquipped;
            _equipmentManager.OnWeaponUnequipped += HandleWeaponUnequipped;
        }

        var weaponUI = _selectionUI as WeaponSelectionUI;
        if (weaponUI != null)
        {
            weaponUI.OnWeaponUnequipRequested += HandleWeaponUnequipRequest;
        }

        // Subscribe to navigation events if using enhanced input handler
        var enhancedInputHandler = inputHandler as WeaponInputHandler;
        if (enhancedInputHandler != null)
        {
            enhancedInputHandler.OnNavigationInput += HandleNavigationInput;
            enhancedInputHandler.OnSelectionConfirmed += HandleSelectionConfirmed;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (_inputHandler != null)
            _inputHandler.OnWeaponSelectionRequested -= HandleWeaponSelectionRequest;

        if (_selectionUI != null)
            _selectionUI.OnWeaponSelected -= HandleWeaponSelection;

        if (_equipmentManager != null)
        {
            _equipmentManager.OnWeaponEquipped -= HandleWeaponEquipped;
            _equipmentManager.OnWeaponUnequipped -= HandleWeaponUnequipped;
        }

        var weaponUI = _selectionUI as WeaponSelectionUI;
        if (weaponUI != null)
        {
            weaponUI.OnWeaponUnequipRequested -= HandleWeaponUnequipRequest;
        }

        // Unsubscribe from navigation events
        var enhancedInputHandler = inputHandler as WeaponInputHandler;
        if (enhancedInputHandler != null)
        {
            enhancedInputHandler.OnNavigationInput -= HandleNavigationInput;
            enhancedInputHandler.OnSelectionConfirmed -= HandleSelectionConfirmed;
        }
    }

    private void HandleNavigationInput(float direction)
    {
        // This is handled directly in WeaponSelectionUI now
        // But we could add additional logic here if needed
        Debug.Log($"[WeaponSystemController] Navigation input: {direction}");
    }

    private void HandleSelectionConfirmed()
    {
        // This is handled directly in WeaponSelectionUI now
        // But we could add additional logic here if needed
        Debug.Log("[WeaponSystemController] Selection confirmed via input");
    }

    private void HandleWeaponUnequipRequest()
    {
        if (_equipmentManager != null)
        {
            var currentWeapon = _equipmentManager.CurrentWeapon;
            if (currentWeapon != null)
            {
                _equipmentManager.UnequipCurrentWeapon();
            }
        }
        else
        {
            Debug.LogError("[WeaponSystemController] Equipment manager is null!");
        }
    }

    private void HandleWeaponSelectionRequest()
    {
      
        var batteryHolder = FindFirstObjectByType<BatteryHolder>();
        if (batteryHolder != null && batteryHolder.HasBattery)
        {
            Debug.Log("[WeaponSystemController] Cannot open weapon UI - player is holding battery");
            return; 
        }
        if (_selectionUI == null || _dataProvider == null) return;

        if (_selectionUI.IsVisible)
        {
            _selectionUI.HideWeaponPanel();
        }
        else
        {
            var availableWeapons = _dataProvider.GetAvailableWeapons();
            _selectionUI.ShowWeaponPanel(availableWeapons);
        }
    }

    private void HandleWeaponSelection(string weaponId)
    {
        _equipmentManager?.EquipWeapon(weaponId);
    }

    private void HandleWeaponEquipped(IWeapon weapon)
    {
        Debug.Log($"[WeaponSystemController] Weapon equipped: {weapon.WeaponName}");
        // TODO: Add audio for equipping weapon, effects
    }

    private void HandleWeaponUnequipped(IWeapon weapon)
    {
        Debug.Log($"[WeaponSystemController] Weapon unequipped: {weapon.WeaponName}");
        // TODO: Add audio for unequipping weapon, effects
    }

    // Public API methods
    public void EquipWeapon(string weaponId)
    {
        _equipmentManager?.EquipWeapon(weaponId);
    }

    public void UnequipCurrentWeapon()
    {
        _equipmentManager?.UnequipCurrentWeapon();
    }

    public IWeapon GetCurrentWeapon()
    {
        return _equipmentManager?.CurrentWeapon;
    }
}