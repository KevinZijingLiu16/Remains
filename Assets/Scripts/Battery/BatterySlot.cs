using UnityEngine;

public class BatterySlot : MonoBehaviour
{
    [Header("Slot Settings")]
    [SerializeField] private string slotId = "slot_01";
    [SerializeField] private Transform batteryInsertPoint;

    [Header("Detection Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Visual Settings")]
    [SerializeField] private GameObject slotVisual;
    [SerializeField] private Material emptyMaterial;
    [SerializeField] private Material filledMaterial;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool showGizmos = true;

    private Transform _playerTransform;
    private BatteryPickup _insertedBattery;
    private bool _playerInRange = false;
    private Renderer _slotRenderer;

    public string SlotId => slotId;
    public bool HasBattery => _insertedBattery != null;
    public bool PlayerInRange => _playerInRange;
    public BatteryPickup InsertedBattery => _insertedBattery;

    public event System.Action<BatterySlot, BatteryPickup> OnBatteryInserted;
    public event System.Action<BatterySlot> OnPlayerEnterRange;
    public event System.Action<BatterySlot> OnPlayerExitRange;

    void Start()
    {
        InitializeSlot();
    }

    void Update()
    {
        if (!HasBattery)
        {
            CheckPlayerProximity();
        }
    }

    private void InitializeSlot()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
        }

        if (batteryInsertPoint == null)
        {
            var insertPoint = new GameObject("BatteryInsertPoint");
            insertPoint.transform.SetParent(transform);
            insertPoint.transform.localPosition = Vector3.zero;
            insertPoint.transform.localRotation = Quaternion.identity;
            batteryInsertPoint = insertPoint.transform;
        }

        if (slotVisual != null)
        {
            _slotRenderer = slotVisual.GetComponent<Renderer>();
        }

        UpdateVisuals();

        if (enableDebugLogs)
        {
            Debug.Log($"[BatterySlot] {slotId} initialized at {transform.position}");
        }
    }

    private void CheckPlayerProximity()
    {
        if (_playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, _playerTransform.position);
        bool inRange = distance <= interactionRange;

        if (inRange && !_playerInRange)
        {
            _playerInRange = true;
            OnPlayerEnterRange?.Invoke(this);

            if (enableDebugLogs)
            {
                Debug.Log($"[BatterySlot] Player entered range of slot {slotId}");
            }
        }
        else if (!inRange && _playerInRange)
        {
            _playerInRange = false;
            OnPlayerExitRange?.Invoke(this);

            if (enableDebugLogs)
            {
                Debug.Log($"[BatterySlot] Player exited range of slot {slotId}");
            }
        }
    }

    public bool CanInsertBattery()
    {
        return !HasBattery;
    }

    public void InsertBattery(BatteryPickup battery)
    {
        if (!CanInsertBattery())
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[BatterySlot] Slot {slotId} already has a battery");
            }
            return;
        }

        if (battery == null)
        {
            Debug.LogError("[BatterySlot] Cannot insert null battery!");
            return;
        }

        _insertedBattery = battery;
        battery.InsertIntoSlot(batteryInsertPoint);

        UpdateVisuals();

        OnBatteryInserted?.Invoke(this, battery);

        if (enableDebugLogs)
        {
            Debug.Log($"[BatterySlot] ✓ Battery inserted into slot {slotId}");
        }
    }

    private void UpdateVisuals()
    {
        if (_slotRenderer == null) return;

        if (HasBattery && filledMaterial != null)
        {
            _slotRenderer.material = filledMaterial;
        }
        else if (!HasBattery && emptyMaterial != null)
        {
            _slotRenderer.material = emptyMaterial;
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = HasBattery ? Color.green : (_playerInRange ? Color.cyan : Color.blue);
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        // Draw insert point
        if (batteryInsertPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(batteryInsertPoint.position, Vector3.one * 0.2f);
        }
    }
}