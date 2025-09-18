using UnityEngine;

/// <summary>
/// 可以在spline上移动的通用对象，可以被玩家推动或自主移动
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(SplineObjectMover))]
public class SplineMovableObject : MonoBehaviour
{
    [Header("Auto Movement")]
    [SerializeField] private bool autoMove = false;
    [SerializeField] private float autoMoveSpeed = 2f;
    [SerializeField] private bool autoMoveForward = true;
    [SerializeField] private bool autoMovePingPong = false; // 是否在spline两端往返

    [Header("Push Response")]
    [SerializeField] private bool canBePushed = true;
    [SerializeField] private float pushResistance = 0.5f; // 0 = 无阻力, 1 = 完全阻力
    [SerializeField] private float maxPushSpeed = 10f;

    [Header("Visual")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private bool faceMovementDirection = true;
    [SerializeField] private bool faceHorizontalOnly = true;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent OnReachSplineStart;
    public UnityEngine.Events.UnityEvent OnReachSplineEnd;
    public UnityEngine.Events.UnityEvent<float> OnPushed; // 参数为推动力度

    // 组件引用
    private Rigidbody _rb;
    private SplineObjectMover _splineMover;
    private SplineTracker _splineTracker;

    // 状态
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

        // 配置刚体为kinematic，因为我们手动控制移动
        _rb.isKinematic = false; // 保持动态以便碰撞检测
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Start()
    {
        if (!_splineTracker.IsValid())
        {
            Debug.LogWarning($"[SplineMovableObject] {name} 的 SplineTracker 无效");
            enabled = false;
            return;
        }

        // 快照到spline上
        SnapToSpline();
        _lastPosition = transform.position;
        _movingForward = autoMoveForward;
    }

    void FixedUpdate()
    {
        if (!_splineTracker.IsValid()) return;

        _lastPosition = transform.position;

        // 处理自动移动
        if (autoMove)
        {
            HandleAutoMovement();
        }

        // 更新朝向
        if (faceMovementDirection)
        {
            UpdateOrientation();
        }

        // 检查边界事件
        CheckBoundaryEvents();
    }

    #endregion

    #region Movement Logic

    private void HandleAutoMovement()
    {
        float direction = _movingForward ? 1f : -1f;
        float newT = _splineTracker.MoveAlongSpline(_t, autoMoveSpeed, Time.fixedDeltaTime, direction);

        // 处理ping-pong移动
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

        // 移动到新位置
        var moveResult = _splineMover.MoveToSplinePosition(_rb, newT, _t, direction);
        if (moveResult.success)
        {
            _t = moveResult.finalT;
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

        // 根据移动方向调整朝向
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

        // 触发到达终点事件
        if (atEnd && !_reachedEndLastFrame)
        {
            OnReachSplineEnd?.Invoke();
        }

        // 触发到达起点事件
        if (atStart && !_reachedStartLastFrame)
        {
            OnReachSplineStart?.Invoke();
        }

        _reachedEndLastFrame = atEnd;
        _reachedStartLastFrame = atStart;
    }

    #endregion

    #region Push Interface

    /// <summary>
    /// 被推动时调用
    /// </summary>
    public bool TryPush(float pushDistance, Rigidbody pusher = null)
    {
        if (!canBePushed) return false;

        // 应用阻力
        float effectivePush = pushDistance * (1f - pushResistance);
        effectivePush = Mathf.Clamp(effectivePush, -maxPushSpeed * Time.fixedDeltaTime, maxPushSpeed * Time.fixedDeltaTime);

        if (Mathf.Abs(effectivePush) < 0.001f) return false;

        // 计算新的t值
        float direction = Mathf.Sign(effectivePush);
        float newT = _splineTracker.GetTAfterDistance(_t, effectivePush);

        // 移动
        var moveResult = _splineMover.MoveToSplinePosition(_rb, newT, _t, direction);
        if (moveResult.success)
        {
            _t = moveResult.finalT;

            // 如果有自动移动，更新移动方向
            if (autoMove && autoMovePingPong)
            {
                _movingForward = direction > 0;
            }

            // 触发推动事件
            OnPushed?.Invoke(Mathf.Abs(effectivePush));
            return true;
        }

        return false;
    }

    /// <summary>
    /// 直接设置在spline上的位置
    /// </summary>
    public void SetSplinePosition(float t)
    {
        _t = Mathf.Clamp01(t);
        Vector3 worldPos = _splineTracker.GetWorldPositionAtT(_t);
        Vector3 targetPos = new Vector3(worldPos.x, _rb.position.y, worldPos.z);
        _rb.MovePosition(targetPos);
    }

    /// <summary>
    /// 快照到spline上最近的点
    /// </summary>
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

    /// <summary>
    /// 设置自动移动
    /// </summary>
    public void SetAutoMove(bool enabled, float speed = -1f, bool forward = true, bool pingPong = false)
    {
        autoMove = enabled;
        if (speed >= 0f) autoMoveSpeed = speed;
        autoMoveForward = forward;
        _movingForward = forward;
        autoMovePingPong = pingPong;
    }

    /// <summary>
    /// 设置推动响应
    /// </summary>
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
            // 绘制当前位置
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.2f);

            // 绘制spline上的对应点
            Vector3 splinePos = _splineTracker.GetWorldPositionAtT(_t);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(splinePos, 0.15f);

            // 连线
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, splinePos);
        }
    }
#endif

    #endregion
}