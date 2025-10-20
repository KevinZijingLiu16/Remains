using UnityEngine;
using System.Collections.Generic;

public class PickupSystemController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BatteryInputHandler inputHandler;
    [SerializeField] private BatteryHolder batteryHolder;
    [SerializeField] private PickupUIManager uiManager;

    [Header("Weapon System Integration")]
    [SerializeField] private WeaponSystemController weaponSystemController;
    [SerializeField] private WeaponInputHandler weaponInputHandler;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    private List<BatteryPickup> _nearbyBatteries = new List<BatteryPickup>();
    private List<BatterySlot> _nearbySlots = new List<BatterySlot>();
    private BatteryPickup _currentNearestBattery;
    private BatterySlot _currentNearestSlot;

    void Start()
    {
        InitializeSystem();
        SubscribeToEvents();
        RegisterAllBatteriesAndSlots();
    }

    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void InitializeSystem()
    {
        ValidateDependencies();

        if (enableDebugLogs)
        {
            Debug.Log("[PickupSystemController] System initialized");
        }
    }

    private void ValidateDependencies()
    {
        if (inputHandler == null)
            Debug.LogError("[PickupSystemController] BatteryInputHandler not assigned!");
        if (batteryHolder == null)
            Debug.LogError("[PickupSystemController] BatteryHolder not assigned!");
        if (uiManager == null)
            Debug.LogError("[PickupSystemController] PickupUIManager not assigned!");
    }

    private void RegisterAllBatteriesAndSlots()
    {
  
        var batteries = FindObjectsByType<BatteryPickup>(FindObjectsSortMode.None);
        foreach (var battery in batteries)
        {
            RegisterBattery(battery);
        }

       
        var slots = FindObjectsByType<BatterySlot>(FindObjectsSortMode.None);
        foreach (var slot in slots)
        {
            RegisterSlot(slot);
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[PickupSystemController] Registered {batteries.Length} batteries and {slots.Length} slots");
        }
    }

    private void RegisterBattery(BatteryPickup battery)
    {
        battery.OnPlayerEnterRange += OnBatteryRangeEnter;
        battery.OnPlayerExitRange += OnBatteryRangeExit;
    }

    private void RegisterSlot(BatterySlot slot)
    {
        slot.OnPlayerEnterRange += OnSlotRangeEnter;
        slot.OnPlayerExitRange += OnSlotRangeExit;
    }

    private void SubscribeToEvents()
    {
        if (inputHandler != null)
        {
            inputHandler.OnPickupRequested += HandlePickupInput;
        }

        if (batteryHolder != null)
        {
            batteryHolder.OnBatteryPickedUp += OnBatteryPickedUp;
            batteryHolder.OnBatteryDropped += OnBatteryDropped;
            batteryHolder.OnBatteryInserted += OnBatteryInserted;
        }

 
        if (weaponInputHandler != null)
        {
            weaponInputHandler.OnWeaponSelectionRequested += HandleWeaponSelectionAttempt;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (inputHandler != null)
        {
            inputHandler.OnPickupRequested -= HandlePickupInput;
        }

        if (batteryHolder != null)
        {
            batteryHolder.OnBatteryPickedUp -= OnBatteryPickedUp;
            batteryHolder.OnBatteryDropped -= OnBatteryDropped;
            batteryHolder.OnBatteryInserted -= OnBatteryInserted;
        }

        if (weaponInputHandler != null)
        {
            weaponInputHandler.OnWeaponSelectionRequested -= HandleWeaponSelectionAttempt;
        }

     
        foreach (var battery in _nearbyBatteries)
        {
            if (battery != null)
            {
                battery.OnPlayerEnterRange -= OnBatteryRangeEnter;
                battery.OnPlayerExitRange -= OnBatteryRangeExit;
            }
        }

   
        foreach (var slot in _nearbySlots)
        {
            if (slot != null)
            {
                slot.OnPlayerEnterRange -= OnSlotRangeEnter;
                slot.OnPlayerExitRange -= OnSlotRangeExit;
            }
        }
    }

    private void HandlePickupInput()
    {
        if (enableDebugLogs)
        {
            Debug.Log("[PickupSystemController] Pickup input received");
        }

      
        if (batteryHolder.HasBattery && _currentNearestSlot != null && _currentNearestSlot.CanInsertBattery())
        {
            batteryHolder.InsertBattery(_currentNearestSlot);
            return;
        }

        
        if (batteryHolder.HasBattery)
        {
            batteryHolder.DropBattery();
            return;
        }

   
        if (_currentNearestBattery != null && !_currentNearestBattery.IsInsertedInSlot && batteryHolder.CanPickupBattery())
        {
            batteryHolder.PickupBattery(_currentNearestBattery);
            return;
        }

        if (enableDebugLogs)
        {
            Debug.Log("[PickupSystemController] No valid pickup action available");
        }
    }

    private void HandleWeaponSelectionAttempt()
    {
     
        if (batteryHolder.HasBattery)
        {
            if (enableDebugLogs)
            {
                Debug.Log("[PickupSystemController] Blocked weapon selection - player is holding battery");
            }

           
            if (uiManager != null)
            {
                uiManager.ShowWeaponBlockedPrompt();
               
                StartCoroutine(HidePromptAfterDelay(2f));
            }

           
        }
    }

    private System.Collections.IEnumerator HidePromptAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (uiManager != null)
        {
            uiManager.HidePrompt();
        }
    }

    private void OnBatteryRangeEnter(BatteryPickup battery)
    {
    
        if (battery.IsInsertedInSlot)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[PickupSystemController] Ignoring battery in slot: {battery.PickupName}");
            }
            return;
        }

        if (!_nearbyBatteries.Contains(battery))
        {
            _nearbyBatteries.Add(battery);
        }
        _currentNearestBattery = battery;
        UpdateUI();
    }

    private void OnBatteryRangeExit(BatteryPickup battery)
    {
        _nearbyBatteries.Remove(battery);

        if (_currentNearestBattery == battery)
        {
            _currentNearestBattery = _nearbyBatteries.Count > 0 ? _nearbyBatteries[0] : null;
        }

        UpdateUI();
    }

    private void OnSlotRangeEnter(BatterySlot slot)
    {
        if (!_nearbySlots.Contains(slot))
        {
            _nearbySlots.Add(slot);
        }
        _currentNearestSlot = slot;
        UpdateUI();
    }

    private void OnSlotRangeExit(BatterySlot slot)
    {
        _nearbySlots.Remove(slot);

        if (_currentNearestSlot == slot)
        {
            _currentNearestSlot = _nearbySlots.Count > 0 ? _nearbySlots[0] : null;
        }

        UpdateUI();
    }

    private void OnBatteryPickedUp(BatteryPickup battery)
    {
        UpdateUI();

        if (enableDebugLogs)
        {
            Debug.Log($"[PickupSystemController] Battery picked up: {battery.PickupName}");
        }
    }

    private void OnBatteryDropped(BatteryPickup battery)
    {
        UpdateUI();

        if (enableDebugLogs)
        {
            Debug.Log($"[PickupSystemController] Battery dropped: {battery.PickupName}");
        }
    }

    private void OnBatteryInserted(BatteryPickup battery)
    {
        _currentNearestSlot = null;
        UpdateUI();

        if (enableDebugLogs)
        {
            Debug.Log($"[PickupSystemController] Battery inserted: {battery.PickupName}");
        }
    }

    private void UpdateUI()
    {
        if (uiManager == null) return;

 
        if (batteryHolder.HasBattery && _currentNearestSlot != null && _currentNearestSlot.CanInsertBattery())
        {
            uiManager.ShowInsertPrompt();
            return;
        }

    
        if (batteryHolder.HasBattery)
        {
            uiManager.ShowDropPrompt();
            return;
        }

     
        if (_currentNearestBattery != null && !_currentNearestBattery.IsPickedUp && !_currentNearestBattery.IsInsertedInSlot)
        {
            uiManager.ShowPickupPrompt();
            return;
        }

     
        uiManager.HidePrompt();
    }
}