using UnityEngine;

public class BatteryPickup : MonoBehaviour, IPickupable
{
    [Header("Battery Settings")]
    [SerializeField] private string batteryId = "battery_01";
    [SerializeField] private string batteryName = "Battery";

    [Header("Detection Settings")]
    [SerializeField] private float pickupRange = 2f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Visual Settings")]
    [SerializeField] private GameObject visualModel;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool showGizmos = true;

    private Transform _playerTransform;
    private bool _isPickedUp = false;
    private bool _playerInRange = false;
    private bool _isInsertedInSlot = false;

    public string PickupId => batteryId;
    public string PickupName => batteryName;
    public bool IsPickedUp => _isPickedUp;
    public bool PlayerInRange => _playerInRange;
    public bool IsInsertedInSlot => _isInsertedInSlot;

    public event System.Action<BatteryPickup> OnPlayerEnterRange;
    public event System.Action<BatteryPickup> OnPlayerExitRange;

    void Start()
    {
        FindPlayer();

        if (visualModel == null)
        {
            visualModel = transform.GetChild(0)?.gameObject;
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[BatteryPickup] {batteryName} initialized at {transform.position}");
        }
    }

    void Update()
    {
        if (_isPickedUp || _isInsertedInSlot) return;

        CheckPlayerProximity();
    }

    private void FindPlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("[BatteryPickup] Player not found! Make sure player has 'Player' tag.");
        }
    }

    private void CheckPlayerProximity()
    {
        if (_playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, _playerTransform.position);
        bool inRange = distance <= pickupRange;

        if (inRange && !_playerInRange)
        {
            _playerInRange = true;
            OnPlayerEnterRange?.Invoke(this);

            if (enableDebugLogs)
            {
                Debug.Log($"[BatteryPickup] Player entered range of {batteryName}");
            }
        }
        else if (!inRange && _playerInRange)
        {
            _playerInRange = false;
            OnPlayerExitRange?.Invoke(this);

            if (enableDebugLogs)
            {
                Debug.Log($"[BatteryPickup] Player exited range of {batteryName}");
            }
        }
    }

    public void Pickup(Transform holdPoint)
    {
        if (_isPickedUp) return;

        _isPickedUp = true;

  
        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

      
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        var collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[BatteryPickup] {batteryName} picked up");
        }
    }

    public void Drop(Vector3 dropPosition)
    {
        if (!_isPickedUp) return;

        _isPickedUp = false;

      
        transform.SetParent(null);
        transform.position = dropPosition;

      
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        var collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = true;
        }

        _playerInRange = false;

        if (enableDebugLogs)
        {
            Debug.Log($"[BatteryPickup] {batteryName} dropped at {dropPosition}");
        }
    }

    public void InsertIntoSlot(Transform slotTransform)
    {
        if (!_isPickedUp) return;

        _isPickedUp = false;
        _isInsertedInSlot = true;

    
        transform.SetParent(slotTransform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        
        var collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

      
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        

        if (enableDebugLogs)
        {
            Debug.Log($"[BatteryPickup] {batteryName} inserted into slot - pickup permanently disabled");
        }
    }
    public void SetVisibility(bool visible)
    {
        if (visualModel != null)
        {
            visualModel.SetActive(visible);
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = _playerInRange ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}