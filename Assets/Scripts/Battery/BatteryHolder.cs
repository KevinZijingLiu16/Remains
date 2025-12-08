using UnityEngine;

public class BatteryHolder : MonoBehaviour
{
    [Header("Hold Settings")]
    [SerializeField] private Transform batteryHoldPoint;
    [SerializeField] private bool autoCreateHoldPoint = true;
    [SerializeField] private Vector3 holdPointOffset = new Vector3(0.5f, 0, 0.5f);

    [Header("Drop Settings")]
    [SerializeField] private Vector3 dropOffsetLeft = new Vector3(2, 0, -1);   
    [SerializeField] private Vector3 dropOffsetRight = new Vector3(-2, 0, 1); 

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    private BatteryPickup _currentBattery;
    private WeaponEquipmentManager _weaponEquipmentManager;

    public bool HasBattery => _currentBattery != null;
    public BatteryPickup CurrentBattery => _currentBattery;
    public Transform BatteryHoldPoint => batteryHoldPoint;

    public event System.Action<BatteryPickup> OnBatteryPickedUp;
    public event System.Action<BatteryPickup> OnBatteryDropped;
    public event System.Action<BatteryPickup> OnBatteryInserted;

    void Start()
    {
        InitializeSystem();
    }

    private void InitializeSystem()
    {
        _weaponEquipmentManager = FindFirstObjectByType<WeaponEquipmentManager>();

        if (batteryHoldPoint == null && autoCreateHoldPoint)
        {
            CreateBatteryHoldPoint();
        }

        if (enableDebugLogs)
        {
            Debug.Log("[BatteryHolder] System initialized");
            //test
        }
    }

    private void CreateBatteryHoldPoint()
    {
        var holdPoint = new GameObject("BatteryHoldPoint");
        holdPoint.transform.SetParent(transform);
        holdPoint.transform.localPosition = holdPointOffset;
        holdPoint.transform.localRotation = Quaternion.identity;
        batteryHoldPoint = holdPoint.transform;
    }

    public bool CanPickupBattery()
    {
        if (HasBattery) return false;
        if (_weaponEquipmentManager != null && _weaponEquipmentManager.CurrentWeapon != null) return false;
        return true;
    }

    public void PickupBattery(BatteryPickup battery)
    {
        if (!CanPickupBattery() || battery == null || battery.IsInsertedInSlot) return;

        _currentBattery = battery;
        battery.Pickup(batteryHoldPoint);

        OnBatteryPickedUp?.Invoke(battery);

        if (enableDebugLogs)
        {
            Debug.Log($"[BatteryHolder]  Picked up: {battery.PickupName}");
        }
    }

    public void DropBattery()
    {
        if (!HasBattery) return;

      
        Vector3 dropOffset = GetDropOffsetBasedOnMouse();

   
        Vector3 localOffset = transform.TransformDirection(dropOffset);
        Vector3 dropPosition = transform.position + localOffset;

        var droppedBattery = _currentBattery;
        _currentBattery.Drop(dropPosition);
        _currentBattery = null;

        OnBatteryDropped?.Invoke(droppedBattery);

        if (enableDebugLogs)
        {
            Debug.Log($"[BatteryHolder] ✓ Dropped: {droppedBattery.PickupName} (Mouse side: {(IsMouseOnLeftSide() ? "Left" : "Right")})");
        }
    }

    private Vector3 GetDropOffsetBasedOnMouse()
    {
        return IsMouseOnLeftSide() ? dropOffsetLeft : dropOffsetRight;
    }


    private bool IsMouseOnLeftSide()
    {
        Vector2 mousePos = Input.mousePosition;
        return mousePos.x < Screen.width * 0.5f;
    }

    public void InsertBattery(BatterySlot slot)
    {
        if (!HasBattery || slot == null) return;

        var insertedBattery = _currentBattery;
        slot.InsertBattery(_currentBattery);
        _currentBattery = null;

        OnBatteryInserted?.Invoke(insertedBattery);

        if (enableDebugLogs)
        {
            Debug.Log($"[BatteryHolder] ✓ Inserted into slot");
        }
    }
}