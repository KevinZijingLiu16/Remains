using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 重构后的玩家控制器，专注于输入处理和游戏逻辑协调
/// 修复了碰撞时被推转的问题
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(SplineObjectMover))]
public class SplineRunnerRB : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [Tooltip("Air control factor (0~1). 0 = no control in air, 1 = same as on ground.")]
    [Range(0f, 1f)] public float airControl = 0.6f;
    [Tooltip("Reserved bank angle in degrees. Can be used for visual tilt effects.")]
    [SerializeField] private float bankDegrees = 0f;

    [Header("Jump & Ground")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField] private float groundCheckRadius = 0.25f;
    [Tooltip("Offset for the ground check sphere (world space). Typically slightly upward to avoid false negatives.")]
    [SerializeField] private Vector3 groundCheckOffset = new Vector3(0f, 0.1f, 0f);

    [Header("Rotation Stability")]
    [SerializeField] private bool useStrongRotationCorrection = true;
    [SerializeField] private float maxRotationDeviation = 45f;
    [SerializeField] private float normalRotationSpeed = 0.3f;
    [SerializeField] private float correctionRotationSpeed = 1f;
    [SerializeField] private bool resetAngularVelocityOnCollision = true;

    [Header("Input (New Input System)")]
    public InputActionProperty moveAction;
    public InputActionProperty jumpAction;

    [Header("Visual (mesh only)")]
    [SerializeField] private Transform meshRoot;
    [SerializeField] private float meshFacingOffsetY = 0f;
    [Tooltip("Flip interpolation speed (0 = instant, 1 = very slow).")]
    [Range(0f, 1f)] public float meshFlipLerp = 0.25f;
    [SerializeField] private bool faceHorizontalOnly = true;

    [Header("VFX")]
    [SerializeField] private ParticleSystem moveDust;
    [SerializeField, Range(0f, 1f)] private float minInputForDust = 0.1f;
    [SerializeField] private bool requireGroundedForDust = true;

    // 组件引用
    private Rigidbody _rb;
    private SplineObjectMover _splineMover;
    private SplineTracker _splineTracker;

    // 状态变量
    private float _t = 0f;                  // 当前在spline上的位置 (0..1)
    private float _cachedMove;
    private bool _cachedJump;
    private bool _grounded;
    private bool _lastMovePositive = true;

    // 强制位置保持机制 (用于spawner等特殊情况)
    private int _forceHoldFrames = 0;
    private Vector3 _forcedPosOnce;
    private bool _skipInitialSnapOnce = false;

    #region Unity Lifecycle

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _splineMover = GetComponent<SplineObjectMover>();
        _splineTracker = GetComponent<SplineTracker>();

        // 配置刚体 - 只锁定X和Z轴旋转，保留Y轴用于转向
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void OnEnable()
    {
        moveAction.action?.Enable();
        jumpAction.action?.Enable();

        if (!_splineTracker.IsValid())
        {
            Debug.LogWarning("[SplineRunnerRB] SplineTracker 无效，组件已禁用。");
            enabled = false;
            return;
        }

        // 重新计算spline长度
        _splineTracker.RecomputeLength();

        // 根据需要进行初始快照
        if (_skipInitialSnapOnce)
        {
            _skipInitialSnapOnce = false;
        }
        else
        {
            SnapToNearestOnSpline();
        }
    }

    void OnDisable()
    {
        moveAction.action?.Disable();
        jumpAction.action?.Disable();
    }

    void Update()
    {
        // 缓存输入
        _cachedMove = moveAction.action != null ? moveAction.action.ReadValue<float>() : 0f;
        if (jumpAction.action != null && jumpAction.action.WasPressedThisFrame())
            _cachedJump = true;
    }

    void FixedUpdate()
    {
        // 处理强制位置保持
        if (_forceHoldFrames > 0)
        {
            _rb.MovePosition(_forcedPosOnce);
            _forceHoldFrames--;
            return;
        }

        if (!_splineTracker.IsValid()) return;

        // 更新地面检测
        _grounded = IsGrounded();

        // 计算控制系数（空中控制）
        float control = _grounded ? 1f : Mathf.Clamp01(airControl);

        // 处理spline移动
        HandleSplineMovement(control);

        // 处理跳跃
        HandleJump();

        // 更新朝向（重点修改的部分）
        UpdateOrientationWithStabilization();

        // 更新视觉效果
        UpdateVisualEffects();
    }

    #endregion

    #region Movement Logic

    private void HandleSplineMovement(float controlFactor)
    {
        float dt = Time.fixedDeltaTime;
        float direction = Mathf.Sign(_cachedMove);
        float speed = Mathf.Abs(_cachedMove) * moveSpeed * controlFactor;

        // 计算新的t值
        float newT = _splineTracker.MoveAlongSpline(_t, speed, dt, direction);

        // 尝试移动到新位置
        var moveResult = _splineMover.MoveToSplinePosition(_rb, newT, _t, direction);

        if (moveResult.success)
        {
            _t = moveResult.finalT;
        }

        // 记录移动方向用于视觉更新
        if (Mathf.Abs(_cachedMove) > 0.001f)
            _lastMovePositive = _cachedMove > 0f;
    }

    private void HandleJump()
    {
        if (_cachedJump && _grounded)
        {
            float g = -Physics.gravity.y;
            float vy = Mathf.Sqrt(2f * g * Mathf.Max(0.01f, jumpHeight));
            var v = _rb.linearVelocity;
            v.y = vy;
            _rb.linearVelocity = v;
        }
        _cachedJump = false;
    }

    /// <summary>
    /// 带有稳定化功能的旋转更新 - 防止碰撞时被推转
    /// </summary>
    private void UpdateOrientationWithStabilization()
    {
        Vector3 splineTangent = _splineTracker.GetWorldTangentAtT(_t);

        if (faceHorizontalOnly)
        {
            splineTangent.y = 0f;
            if (splineTangent.sqrMagnitude < 1e-8f)
                splineTangent = transform.forward;
            else
                splineTangent.Normalize();
        }

        Quaternion targetRotation = Quaternion.LookRotation(splineTangent, Vector3.up);

        if (useStrongRotationCorrection)
        {
            // 计算当前旋转与目标旋转的角度差
            float angleDiff = Quaternion.Angle(_rb.rotation, targetRotation);

            float lerpSpeed;
            if (angleDiff > maxRotationDeviation)
            {
                // 偏差太大，强制快速纠正
                lerpSpeed = correctionRotationSpeed;

                // 清除角速度以防止继续旋转
                _rb.angularVelocity = Vector3.zero;
            }
            else
            {
                // 正常情况下的平滑旋转
                lerpSpeed = _grounded ? normalRotationSpeed : normalRotationSpeed * 0.5f;
            }

            _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRotation, lerpSpeed));
        }
        else
        {
            // 原来的逻辑
            float lerpSpeed = _grounded ? 0.2f : 0.1f;
            _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRotation, lerpSpeed));
        }
    }

    private void UpdateVisualEffects()
    {
        // 更新mesh朝向
        if (meshRoot != null)
        {
            float targetYaw = (_lastMovePositive ? 0f : -180f) + meshFacingOffsetY;
            Quaternion targetLocal = Quaternion.Euler(0f, targetYaw, 0f);
            meshRoot.localRotation = (meshFlipLerp <= 0f)
                ? targetLocal
                : Quaternion.Slerp(meshRoot.localRotation, targetLocal, meshFlipLerp);
        }

        // 更新粒子效果
        bool inputMoving = Mathf.Abs(_cachedMove) > minInputForDust;
        bool shouldPlayDust = inputMoving && (!requireGroundedForDust || _grounded);
        SetMoveDust(shouldPlayDust);
    }

    #endregion

    #region Collision Handling

    /// <summary>
    /// 碰撞开始时重置旋转状态
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        if (resetAngularVelocityOnCollision && collision.rigidbody != null)
        {
            // 立即停止角速度
            _rb.angularVelocity = Vector3.zero;

            // 如果偏差很大，立即纠正旋转
            Vector3 splineTangent = _splineTracker.GetWorldTangentAtT(_t);
            if (faceHorizontalOnly)
            {
                splineTangent.y = 0f;
                if (splineTangent.sqrMagnitude > 1e-8f) splineTangent.Normalize();
            }

            if (splineTangent.sqrMagnitude > 0.1f)
            {
                Quaternion correctRotation = Quaternion.LookRotation(splineTangent, Vector3.up);
                float angleDiff = Quaternion.Angle(_rb.rotation, correctRotation);

                if (angleDiff > maxRotationDeviation * 0.5f)
                {
                    _rb.MoveRotation(correctRotation);
                }
            }
        }
    }

    /// <summary>
    /// 持续碰撞时保持旋转稳定
    /// </summary>
    void OnCollisionStay(Collision collision)
    {
        if (resetAngularVelocityOnCollision && collision.rigidbody != null)
        {
            // 在持续接触期间保持角速度为零
            _rb.angularVelocity = Vector3.zero;
        }
    }

    #endregion

    #region Ground Detection

    private bool IsGrounded()
    {
        Vector3 origin = _rb.worldCenterOfMass + groundCheckOffset;
        return Physics.CheckSphere(origin, groundCheckRadius, groundLayers, QueryTriggerInteraction.Ignore);
    }

    #endregion

    #region Public Interface

    /// <summary>
    /// 获取当前在spline上的t值
    /// </summary>
    public float GetCurrentT()
    {
        return _t;
    }

    /// <summary>
    /// 重新将对象快照到指定世界坐标对应的spline位置
    /// </summary>
    public void ResnapToWorldPosition(Vector3 worldPos)
    {
        if (!_splineTracker.IsValid()) return;

        var projection = _splineTracker.ProjectWorldPointToSpline(worldPos);
        if (!projection.isValid) return;

        _t = projection.t;

        Vector3 snapPos = _splineTracker.SnapToNearestPoint(_rb.position, true);
        if (!float.IsNaN(snapPos.x))
        {
            _rb.position = snapPos;
            _forcedPosOnce = _rb.position;
            _forceHoldFrames = 3; // 保持3帧
        }
    }

    /// <summary>
    /// 标记此对象由spawner生成，跳过初始快照
    /// </summary>
    public void MarkSpawnedBySpawner()
    {
        _skipInitialSnapOnce = true;
    }

    /// <summary>
    /// 手动快照到spline上最近的点
    /// </summary>
    public void SnapToNearestOnSpline()
    {
        if (!_splineTracker.IsValid()) return;

        var projection = _splineTracker.ProjectWorldPointToSpline(_rb.position);
        if (!projection.isValid) return;

        _t = projection.t;
        Vector3 snapPos = _splineTracker.SnapToNearestPoint(_rb.position, true);
        if (!float.IsNaN(snapPos.x))
        {
            _rb.position = snapPos;
        }
    }

    /// <summary>
    /// 强制重置旋转到正确方向（调试用）
    /// </summary>
    [ContextMenu("Force Correct Rotation")]
    public void ForceCorrectRotation()
    {
        if (!_splineTracker.IsValid()) return;

        Vector3 splineTangent = _splineTracker.GetWorldTangentAtT(_t);
        if (faceHorizontalOnly)
        {
            splineTangent.y = 0f;
            splineTangent.Normalize();
        }

        if (splineTangent.sqrMagnitude > 0.1f)
        {
            Quaternion correctRotation = Quaternion.LookRotation(splineTangent, Vector3.up);
            _rb.MoveRotation(correctRotation);
            _rb.angularVelocity = Vector3.zero;
        }
    }

    #endregion

    #region Utility

    private void SetMoveDust(bool active)
    {
        if (!moveDust) return;
        moveDust.gameObject.SetActive(active);
    }

    #endregion

    #region Gizmos

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (_rb)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(_rb.worldCenterOfMass + groundCheckOffset, groundCheckRadius);

            // 显示当前朝向
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * 2f);

            // 如果有splineTracker，显示应该朝向的方向
            if (_splineTracker != null && _splineTracker.IsValid())
            {
                Vector3 splineDir = _splineTracker.GetWorldTangentAtT(_t);
                if (faceHorizontalOnly) splineDir.y = 0f;
                splineDir.Normalize();

                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, splineDir * 2f);
            }
        }
    }
#endif

    #endregion
}