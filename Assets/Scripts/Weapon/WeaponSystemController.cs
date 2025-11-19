using UnityEngine;

public class WeaponSystemController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private WeaponDatabase weaponDatabase;
    [SerializeField] private WeaponInputHandler inputHandler;
    [SerializeField] private WeaponSelectionUI selectionUI;
    [SerializeField] private WeaponEquipmentManager equipmentManager;

    [Header("Hotkey Weapon IDs")]
    [SerializeField] private string hotkey2WeaponId = "foam_spray"; // Key 2
    [SerializeField] private string hotkey3WeaponId = "air_blower";  // Key 3

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
        {
            _inputHandler.OnWeaponSelectionRequested += HandleWeaponSelectionRequest;
            _inputHandler.OnQuickCyclePressed += HandleQuickCyclePressed;
            _inputHandler.OnHotkeyPressed += HandleHotkeyPressed;
        }

        if (_selectionUI != null)
        {
            _selectionUI.OnWeaponSelected += HandleWeaponSelection;
        }

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
        {
            _inputHandler.OnWeaponSelectionRequested -= HandleWeaponSelectionRequest;
            _inputHandler.OnQuickCyclePressed -= HandleQuickCyclePressed;
            _inputHandler.OnHotkeyPressed -= HandleHotkeyPressed;
        }

        if (_selectionUI != null)
        {
            _selectionUI.OnWeaponSelected -= HandleWeaponSelection;
        }

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

        var enhancedInputHandler = inputHandler as WeaponInputHandler;
        if (enhancedInputHandler != null)
        {
            enhancedInputHandler.OnNavigationInput -= HandleNavigationInput;
            enhancedInputHandler.OnSelectionConfirmed -= HandleSelectionConfirmed;
        }
    }

    private void HandleNavigationInput(float direction)
    {
        Debug.Log($"[WeaponSystemController] Navigation input: {direction}");
    }

    private void HandleSelectionConfirmed()
    {
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

    // Original Tab key behavior - toggle weapon panel
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

    // NEW: Q key quick cycle behavior
    // NEW: Q key quick cycle behavior
    private void HandleQuickCyclePressed()
    {
        var batteryHolder = FindFirstObjectByType<BatteryHolder>();
        if (batteryHolder != null && batteryHolder.HasBattery)
        {
            Debug.Log("[WeaponSystemController] Cannot cycle weapons - player is holding battery");
            return;
        }

        if (_dataProvider == null) return;

        var weaponUI = _selectionUI as WeaponSelectionUI;
        if (weaponUI == null) return;

        // If panel is already open (either from Tab or previous Q), cycle to next weapon
        if (weaponUI.IsVisible)
        {
            // Convert to quick cycle mode if it was opened via Tab
            weaponUI.ConvertToQuickCycleMode();
            weaponUI.CycleToNextWeapon();
        }
        else
        {
            // Open panel in quick cycle mode with next weapon highlighted
            var availableWeapons = _dataProvider.GetAvailableWeapons();
            weaponUI.ShowWeaponPanelQuickCycle(availableWeapons);
        }

        Debug.Log("[WeaponSystemController] Quick cycle pressed");
    }

    // NEW: 1/2/3 hotkey direct weapon switching
    private void HandleHotkeyPressed(int hotkeyNumber)
    {
        var batteryHolder = FindFirstObjectByType<BatteryHolder>();
        if (batteryHolder != null && batteryHolder.HasBattery)
        {
            Debug.Log("[WeaponSystemController] Cannot switch weapons - player is holding battery");
            return;
        }

        Debug.Log($"[WeaponSystemController] Hotkey {hotkeyNumber} pressed");

        switch (hotkeyNumber)
        {
            case 1: // Unequip
                _equipmentManager?.UnequipCurrentWeapon();
                Debug.Log("[WeaponSystemController] Hotkey 1: Unequipped weapon");
                break;

            case 2: // Foam Spray
                _equipmentManager?.EquipWeapon(hotkey2WeaponId);
                Debug.Log($"[WeaponSystemController] Hotkey 2: Equipped {hotkey2WeaponId}");
                break;

            case 3: // Air Blower
                _equipmentManager?.EquipWeapon(hotkey3WeaponId);
                Debug.Log($"[WeaponSystemController] Hotkey 3: Equipped {hotkey3WeaponId}");
                break;
        }
    }

    private void HandleWeaponSelection(string weaponId)
    {
        _equipmentManager?.EquipWeapon(weaponId);
    }

    private void HandleWeaponEquipped(IWeapon weapon)
    {
        Debug.Log($"[WeaponSystemController] Weapon equipped: {weapon.WeaponName}");
    }

    private void HandleWeaponUnequipped(IWeapon weapon)
    {
        Debug.Log($"[WeaponSystemController] Weapon unequipped: {weapon.WeaponName}");
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