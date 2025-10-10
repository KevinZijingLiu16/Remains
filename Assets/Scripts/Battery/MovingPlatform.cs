using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Platform Settings")]
    [SerializeField] private bool requiresBattery = true;
    [SerializeField] private BatterySlot linkedBatterySlot;

    [Header("Movement Settings")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float waitTimeAtPoints = 1f;
    [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Auto Setup")]
    [SerializeField] private bool autoCreatePoints = true;
    [SerializeField] private Vector3 pointAOffset = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 pointBOffset = new Vector3(0, 5, 0);

    [Header("Player Attachment")]
    [SerializeField] private bool attachPlayer = true;
    [SerializeField] private LayerMask playerLayer;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool showGizmos = true;

    private bool _isMoving = false;
    private bool _isActivated = false;
    private Vector3 _targetPosition;
    private Vector3 _startPosition;
    private float _moveProgress = 0f;
    private float _waitTimer = 0f;
    private bool _movingToB = true;
    private Transform _attachedPlayer;

    public bool IsActivated => _isActivated;
    public bool RequiresBattery => requiresBattery;

    void Start()
    {
        InitializePlatform();
    }

    void Update()
    {
        if (!_isActivated) return;

        if (_isMoving)
        {
            MovePlatform();
        }
        else
        {
            WaitAtPoint();
        }
    }

    void LateUpdate()
    {
        if (attachPlayer && _attachedPlayer != null)
        {
        }
    }

    private void InitializePlatform()
    {
        // Auto create movement points if needed
        if (autoCreatePoints)
        {
            if (pointA == null)
            {
                var pointAObj = new GameObject("PointA");
                pointAObj.transform.SetParent(transform);
                pointAObj.transform.position = transform.position + pointAOffset;
                pointA = pointAObj.transform;
            }

            if (pointB == null)
            {
                var pointBObj = new GameObject("PointB");
                pointBObj.transform.SetParent(transform);
                pointBObj.transform.position = transform.position + pointBOffset;
                pointB = pointBObj.transform;
            }
        }

       
        if (requiresBattery)
        {
            if (linkedBatterySlot != null)
            {
                linkedBatterySlot.OnBatteryInserted += OnBatteryInserted;

                if (enableDebugLogs)
                {
                    Debug.Log($"[MovingPlatform] Linked to battery slot: {linkedBatterySlot.SlotId}");
                }
            }
            else
            {
                Debug.LogWarning("[MovingPlatform] Requires battery but no slot is linked!");
            }
        }
        else
        {
           
            ActivatePlatform();
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[MovingPlatform] Initialized - Requires Battery: {requiresBattery}");
        }
    }

    private void OnBatteryInserted(BatterySlot slot, BatteryPickup battery)
    {
        if (slot != linkedBatterySlot) return;

        if (enableDebugLogs)
        {
            Debug.Log("[MovingPlatform] Battery inserted, activating platform!");
        }

        ActivatePlatform();
    }

    public void ActivatePlatform()
    {
        if (_isActivated)
        {
            if (enableDebugLogs)
            {
                Debug.Log("[MovingPlatform] Platform already activated");
            }
            return;
        }

        _isActivated = true;
        _isMoving = true;
        _startPosition = transform.position;
        _targetPosition = pointB.position;
        _moveProgress = 0f;

        if (enableDebugLogs)
        {
            Debug.Log("[MovingPlatform] ✓ Platform activated and moving!");
        }
    }

    private void MovePlatform()
    {
        _moveProgress += Time.deltaTime * moveSpeed;
        float curveValue = movementCurve.Evaluate(Mathf.Clamp01(_moveProgress));

        transform.position = Vector3.Lerp(_startPosition, _targetPosition, curveValue);

       
        if (_moveProgress >= 1f)
        {
            transform.position = _targetPosition;
            _isMoving = false;
            _waitTimer = 0f;

            if (enableDebugLogs)
            {
                Debug.Log($"[MovingPlatform] Reached {(_movingToB ? "Point B" : "Point A")}");
            }
        }
    }

    private void WaitAtPoint()
    {
        _waitTimer += Time.deltaTime;

        if (_waitTimer >= waitTimeAtPoints)
        {
         
            _movingToB = !_movingToB;
            _startPosition = transform.position;
            _targetPosition = _movingToB ? pointB.position : pointA.position;
            _moveProgress = 0f;
            _isMoving = true;

            if (enableDebugLogs)
            {
                Debug.Log($"[MovingPlatform] Moving to {(_movingToB ? "Point B" : "Point A")}");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!attachPlayer) return;

        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            _attachedPlayer = other.transform;
            _attachedPlayer.SetParent(transform);

            if (enableDebugLogs)
            {
                Debug.Log("[MovingPlatform] Player attached to platform");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!attachPlayer) return;

        if (other.transform == _attachedPlayer)
        {
            _attachedPlayer.SetParent(null);
            _attachedPlayer = null;

            if (enableDebugLogs)
            {
                Debug.Log("[MovingPlatform] Player detached from platform");
            }
        }
    }

    void OnDestroy()
    {
      
        if (requiresBattery && linkedBatterySlot != null)
        {
            linkedBatterySlot.OnBatteryInserted -= OnBatteryInserted;
        }

      
        if (_attachedPlayer != null)
        {
            _attachedPlayer.SetParent(null);
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        if (pointA != null && pointB != null)
        {
          
            Gizmos.color = _isActivated ? Color.green : Color.red;
            Gizmos.DrawLine(pointA.position, pointB.position);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(pointA.position, 0.3f);
            Gizmos.DrawWireSphere(pointB.position, 0.3f);

        
            if (Application.isPlaying)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
            }
        }
    }

  
    [ContextMenu("Activate Platform (Force)")]
    public void ForceActivate()
    {
        ActivatePlatform();
    }

    [ContextMenu("Toggle Requires Battery")]
    public void ToggleRequiresBattery()
    {
        requiresBattery = !requiresBattery;
        Debug.Log($"[MovingPlatform] Requires Battery: {requiresBattery}");

        if (!requiresBattery && !_isActivated)
        {
            ActivatePlatform();
        }
    }
}