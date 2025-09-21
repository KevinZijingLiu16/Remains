using UnityEngine;


[RequireComponent(typeof(Rigidbody), typeof(SplineObjectMover))]
public class SplineMovableObject : MonoBehaviour
{
    [Header("Auto Movement")]
    [SerializeField] private bool autoMove = false;
    [SerializeField] private float autoMoveSpeed = 2f;
    [SerializeField] private bool autoMoveForward = true;
    [SerializeField] private bool autoMovePingPong = false; 

    [Header("Push Response")]
    [SerializeField] private bool canBePushed = true;
    [SerializeField] private float pushResistance = 0.5f; 
    [SerializeField] private float maxPushSpeed = 10f;

    [Header("Visual")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private bool faceMovementDirection = true;
    [SerializeField] private bool faceHorizontalOnly = true;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent OnReachSplineStart;
    public UnityEngine.Events.UnityEvent OnReachSplineEnd;
    public UnityEngine.Events.UnityEvent<float> OnPushed;

 
    private Rigidbody _rb;
    private SplineObjectMover _splineMover;
    private SplineTracker _splineTracker;

  
    private float _t = 0f;
    private bool _movingForward = true;
    private Vector3 _lastPosition;
    private bool _reachedEndLastFrame = false;
    private bool _reachedStartLastFrame = false;

    public float CurrentT => _t;
    public bool IsMoving => Vector3.Distance(transform.position, _lastPosition) > 0.01f;
    public bool CanBePushed => canBePushed;

    #region Unity Lifecycle

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _splineMover = GetComponent<SplineObjectMover>();
        _splineTracker = GetComponent<SplineTracker>();

   
        _rb.isKinematic = false; 
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Start()
    {
        if (!_splineTracker.IsValid())
        {
            Debug.LogWarning($"[SplineMovableObject] {name} µÄ SplineTracker ÎÞÐ§");
            enabled = false;
            return;
        }

      
        SnapToSpline();
        _lastPosition = transform.position;
        _movingForward = autoMoveForward;
    }

    void FixedUpdate()
    {
        if (!_splineTracker.IsValid()) return;

        _lastPosition = transform.position;

      
        if (autoMove)
        {
            HandleAutoMovement();
        }

     
        if (faceMovementDirection)
        {
            UpdateOrientation();
        }

        CheckBoundaryEvents();
        CheckAndCorrectSplineDeviation();
    }

    #endregion

    #region Movement Logic

    private void HandleAutoMovement()
    {
        float direction = _movingForward ? 1f : -1f;
        float newT = _splineTracker.MoveAlongSpline(_t, autoMoveSpeed, Time.fixedDeltaTime, direction);

     
        if (autoMovePingPong)
        {
            if (newT >= 1f && _movingForward)
            {
                _movingForward = false;
                newT = 1f;
            }
            else if (newT <= 0f && !_movingForward)
            {
                _movingForward = true;
                newT = 0f;
            }
        }

       
        var moveResult = _splineMover.MoveToSplinePosition(_rb, newT, _t, direction);
        if (moveResult.success)
        {
            _t = moveResult.finalT;
        }
    }
    private void CheckAndCorrectSplineDeviation()
    {
        var projection = _splineTracker.ProjectWorldPointToSpline(transform.position);
        if (projection.isValid)
        {
            float deviation = Vector3.Distance(transform.position, projection.worldPosition);
            if (deviation > 0.5f) 
            {
                
                Vector3 correctedPos = new Vector3(
                    projection.worldPosition.x,
                    transform.position.y,
                    projection.worldPosition.z
                );
                Vector3 lerpedPos = Vector3.Lerp(transform.position, correctedPos, Time.fixedDeltaTime * 2f);
                _rb.MovePosition(lerpedPos);
                _t = projection.t; 
            }
        }
    }
    private void UpdateOrientation()
    {
        Vector3 tangent = _splineTracker.GetWorldTangentAtT(_t);

        if (faceHorizontalOnly)
        {
            tangent.y = 0f;
            if (tangent.sqrMagnitude < 1e-8f) return;
            tangent.Normalize();
        }

       
        if (!_movingForward)
            tangent = -tangent;

        Quaternion targetRotation = Quaternion.LookRotation(tangent, Vector3.up);

        if (visualRoot != null)
        {
            visualRoot.rotation = Quaternion.Slerp(visualRoot.rotation, targetRotation, Time.fixedDeltaTime * 5f);
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);
        }
    }

    private void CheckBoundaryEvents()
    {
        bool atEnd = _t >= 0.99f;
        bool atStart = _t <= 0.01f;

        if (atEnd && !_reachedEndLastFrame)
        {
            OnReachSplineEnd?.Invoke();
        }

        if (atStart && !_reachedStartLastFrame)
        {
            OnReachSplineStart?.Invoke();
        }

        _reachedEndLastFrame = atEnd;
        _reachedStartLastFrame = atStart;
    }

    #endregion

    #region Push Interface

 
    public bool TryPush(float pushDistance, Rigidbody pusher = null)
    {
        if (!canBePushed) return false;

     
        float effectivePush = pushDistance * (1f - pushResistance);
        effectivePush = Mathf.Clamp(effectivePush, -maxPushSpeed * Time.fixedDeltaTime, maxPushSpeed * Time.fixedDeltaTime);

        if (Mathf.Abs(effectivePush) < 0.001f) return false;

     
        float direction = Mathf.Sign(effectivePush);
        float newT = _splineTracker.GetTAfterDistance(_t, effectivePush);

     
        var moveResult = _splineMover.MoveToSplinePosition(_rb, newT, _t, direction);
        if (moveResult.success)
        {
            _t = moveResult.finalT;

           
            if (autoMove && autoMovePingPong)
            {
                _movingForward = direction > 0;
            }

           
            OnPushed?.Invoke(Mathf.Abs(effectivePush));
            return true;
        }

        return false;
    }

    public void SetSplinePosition(float t)
    {
        _t = Mathf.Clamp01(t);
        Vector3 worldPos = _splineTracker.GetWorldPositionAtT(_t);
        Vector3 targetPos = new Vector3(worldPos.x, _rb.position.y, worldPos.z);
        _rb.MovePosition(targetPos);
    }

  
    public void SnapToSpline()
    {
        if (!_splineTracker.IsValid()) return;

        var projection = _splineTracker.ProjectWorldPointToSpline(transform.position);
        if (projection.isValid)
        {
            _t = projection.t;
            Vector3 snapPos = new Vector3(projection.worldPosition.x, transform.position.y, projection.worldPosition.z);
            _rb.MovePosition(snapPos);
        }
    }

    #endregion

    #region Configuration

  
    public void SetAutoMove(bool enabled, float speed = -1f, bool forward = true, bool pingPong = false)
    {
        autoMove = enabled;
        if (speed >= 0f) autoMoveSpeed = speed;
        autoMoveForward = forward;
        _movingForward = forward;
        autoMovePingPong = pingPong;
    }


    public void SetPushResponse(bool canPush, float resistance = -1f, float maxSpeed = -1f)
    {
        canBePushed = canPush;
        if (resistance >= 0f) pushResistance = Mathf.Clamp01(resistance);
        if (maxSpeed >= 0f) maxPushSpeed = maxSpeed;
    }

    #endregion

    #region Gizmos

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (_splineTracker != null && _splineTracker.IsValid())
        {
           
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.2f);

            Vector3 splinePos = _splineTracker.GetWorldPositionAtT(_t);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(splinePos, 0.15f);

     
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, splinePos);
        }
    }
#endif

    #endregion
}