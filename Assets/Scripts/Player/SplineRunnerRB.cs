using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(Rigidbody), typeof(SplineObjectMover))]
public class SplineRunnerRB : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [Tooltip("Air control factor (0~1). 0 = no control in air, 1 = same as on ground.")]
    [Range(0f, 1f)] public float airControl = 0.6f;
    [Tooltip("Reserved bank angle in degrees. Can be used for visual tilt effects.")]
    public float GetMoveSpeed() => moveSpeed;
    public void SetMoveSpeed(float speed) => moveSpeed = speed;
    public void SetCurrentT(float t) => _t = t;
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
    [SerializeField] private float _meshFacingOffsetY = 0f;

  

    public float meshFacingOffsetY
    {
        get { return _meshFacingOffsetY; }
        set { _meshFacingOffsetY = value; }
    }
    [Tooltip("Flip interpolation speed (0 = instant, 1 = very slow).")]
    [Range(0f, 1f)] public float meshFlipLerp = 0.25f;
    [SerializeField] private bool faceHorizontalOnly = true;

    [Header("Audio")]
    [SerializeField] private string moveSound = "motor";
    [Tooltip("Volume of movement sound (0-1)")]
    [SerializeField, Range(0f, 1f)] private float moveSoundVolume = 0.8f;
    [Tooltip("Minimum input to start playing sound")]
    [SerializeField, Range(0f, 1f)] private float minSpeedForSound = 0.1f;
    [Tooltip("Fade speed when starting/stopping")]
    [SerializeField] private float volumeFadeSpeed = 5f;
    private float _currentSoundVolume = 0f;
    private bool _isSoundPlaying = false;




    [Header("VFX")]
    [SerializeField] private ParticleSystem moveDust;
    [SerializeField, Range(0f, 1f)] private float minInputForDust = 0.1f;
    [SerializeField] private bool requireGroundedForDust = true;



    private Rigidbody _rb;
    private SplineObjectMover _splineMover;
    private SplineTracker _splineTracker;


    private float _t = 0f;
    private float _cachedMove;
    private bool _cachedJump;
    private bool _grounded;
    private bool _lastMovePositive = true;
    public bool IsCurrentlyGrounded
    {
        get { return _grounded; }
    }
    public bool IsMovingPositive
    {
        get { return _lastMovePositive; }
    }


    private int _forceHoldFrames = 0;
    private Vector3 _forcedPosOnce;
    private bool _skipInitialSnapOnce = false;


    #region Unity Lifecycle

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _splineMover = GetComponent<SplineObjectMover>();
        _splineTracker = GetComponent<SplineTracker>();


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

            enabled = false;
            return;
        }


        _splineTracker.RecomputeLength();


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

        _cachedMove = moveAction.action != null ? moveAction.action.ReadValue<float>() : 0f;
        if (jumpAction.action != null && jumpAction.action.WasPressedThisFrame())
            _cachedJump = true;
    }

    void FixedUpdate()
    {


        if (_forceHoldFrames > 0)
        {
            _rb.MovePosition(_forcedPosOnce);
            _forceHoldFrames--;
            return;
        }

        if (!_splineTracker.IsValid()) return;

        _grounded = IsGrounded();


        float control = _grounded ? 1f : Mathf.Clamp01(airControl);


        HandleSplineMovement(control);


        HandleJump();


        UpdateOrientationWithStabilization();


        UpdateVisualEffects();
        UpdateMovementSound();
    }

    #endregion

    #region Movement Logic

    private void HandleSplineMovement(float controlFactor)
    {
        float dt = Time.fixedDeltaTime;
        float direction = Mathf.Sign(_cachedMove);
        float speed = Mathf.Abs(_cachedMove) * moveSpeed * controlFactor;

        float newT = _splineTracker.MoveAlongSpline(_t, speed, dt, direction);


        var moveResult = _splineMover.MoveToSplinePosition(_rb, newT, _t, direction);

        if (moveResult.success)
        {
            _t = moveResult.finalT;
        }


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

            float angleDiff = Quaternion.Angle(_rb.rotation, targetRotation);

            float lerpSpeed;
            if (angleDiff > maxRotationDeviation)
            {

                lerpSpeed = correctionRotationSpeed;


                _rb.angularVelocity = Vector3.zero;
            }
            else
            {

                lerpSpeed = _grounded ? normalRotationSpeed : normalRotationSpeed * 0.5f;
            }

            _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRotation, lerpSpeed));
        }
        else
        {

            float lerpSpeed = _grounded ? 0.2f : 0.1f;
            _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRotation, lerpSpeed));
        }
    }

    private void UpdateVisualEffects()
    {
        if (meshRoot != null)
        {

            Vector2 mousePos = Input.mousePosition;


            bool mouseOnLeft = mousePos.x < Screen.width * 0.5f;


            float targetAngle = (mouseOnLeft ? -180f : 0f) + _meshFacingOffsetY;

            Quaternion targetLocal = Quaternion.Euler(0f, 0f, targetAngle);  // ZÖá

            meshRoot.localRotation = (meshFlipLerp <= 0f)
                ? targetLocal
                : Quaternion.Slerp(meshRoot.localRotation, targetLocal, meshFlipLerp);
        }


        if (moveDust != null)
        {
            float dustYaw = _lastMovePositive ? 180f : 0f;
            moveDust.transform.localRotation = Quaternion.Euler(0f, dustYaw, 0f);
        }

        bool inputMoving = Mathf.Abs(_cachedMove) > minInputForDust;
        bool shouldPlayDust = inputMoving && (!requireGroundedForDust || _grounded);
        SetMoveDust(shouldPlayDust);
    }
    #endregion

    #region Collision Handling


    void OnCollisionEnter(Collision collision)
    {
        if (resetAngularVelocityOnCollision && collision.rigidbody != null)
        {

            _rb.angularVelocity = Vector3.zero;


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


    void OnCollisionStay(Collision collision)
    {
        if (resetAngularVelocityOnCollision && collision.rigidbody != null)
        {

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


    public float GetCurrentT()
    {
        return _t;
    }


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
            _forceHoldFrames = 3;
        }
    }

    public void MarkSpawnedBySpawner()
    {
        _skipInitialSnapOnce = true;
    }


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



    //public void EnterFreeMode()
    //{
    //    FreeMode = true;
    //}

    //public void ExitFreeModeAndResnap()
    //{
    //    FreeMode = false;

    //    ResnapToWorldPosition(transform.position);
    //}

    #endregion

    #region Audio
    private void UpdateMovementSound()
    {
        bool isMoving = Mathf.Abs(_cachedMove) > minSpeedForSound;
        bool shouldPlay = isMoving && _grounded;

        if (shouldPlay && !_isSoundPlaying)
        {
          
            SoundManager.Instance?.PlayLoop(moveSound, moveSoundVolume);
            _isSoundPlaying = true;
        }
        else if (!shouldPlay && _isSoundPlaying)
        {
          
            SoundManager.Instance?.StopLoop();
            _isSoundPlaying = false;
        }

      
        if (_isSoundPlaying)
        {
            float speedFactor = Mathf.Abs(_cachedMove);
            SoundManager.Instance?.SetLoopVolume(moveSoundVolume * speedFactor);
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


            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * 2f);


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