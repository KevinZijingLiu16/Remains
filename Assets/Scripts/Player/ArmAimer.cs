using UnityEngine;
using UnityEngine.InputSystem;

public class ArmAimer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform leftArmPivot;
    [SerializeField] private Transform rightArmPivot;
    [SerializeField] private Camera cam;

    [Header("Input Actions")]
    [SerializeField] private InputActionProperty rightStickAction;

    [Header("Mouse Input Mapping (mouse Y -> X rotation)")]
    [SerializeField] private float minX = -60f;
    [SerializeField] private float maxX = 60f;
    [SerializeField] private bool invertMouse = false;

    [Header("Controller Input Mapping (right stick Y -> X rotation)")]
    [SerializeField] private bool invertController = false;
    [SerializeField] private float controllerSensitivity = 1f;
    [SerializeField] private AnimationCurve controllerCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("Per-Arm Options")]
    [SerializeField] private bool mirrorRight = false;

    [Header("Smoothing (deg/sec)")]
    [SerializeField] private float rotateSpeed = 720f;

    [Header("Input Priority")]
    [SerializeField] private bool preferController = false; // If true, controller input overrides mouse when both are active

    private Quaternion _baseLeftRot, _baseRightRot;
    private float _currentTargetOffsetX = 0f;

    // Controller input state
    private Vector2 _rightStickInput;
    private bool _hasControllerInput = false;

    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    void Start()
    {
        if (!leftArmPivot || !rightArmPivot)
        {
            Debug.LogWarning("[ArmAimer] Missing arm pivot references.");
            enabled = false;
            return;
        }

        // Store base rotations
        _baseLeftRot = leftArmPivot.localRotation;
        _baseRightRot = rightArmPivot.localRotation;

        // Enable input
        EnableInput();
    }

    void OnEnable()
    {
        EnableInput();
    }

    void OnDisable()
    {
        DisableInput();
    }

    void OnDestroy()
    {
        DisableInput();
    }

    private void EnableInput()
    {
        if (rightStickAction.action != null)
        {
            rightStickAction.action.Enable();
            rightStickAction.action.performed += OnRightStickInput;
            rightStickAction.action.canceled += OnRightStickCanceled;
        }
    }

    private void DisableInput()
    {
        if (rightStickAction.action != null)
        {
            rightStickAction.action.performed -= OnRightStickInput;
            rightStickAction.action.canceled -= OnRightStickCanceled;
            rightStickAction.action.Disable();
        }
    }

    private void OnRightStickInput(InputAction.CallbackContext context)
    {
        _rightStickInput = context.ReadValue<Vector2>();
        _hasControllerInput = _rightStickInput.magnitude > 0.1f; // Dead zone
    }

    private void OnRightStickCanceled(InputAction.CallbackContext context)
    {
        _rightStickInput = Vector2.zero;
        _hasControllerInput = false;
    }

    void Update()
    {
        float targetOffsetX = 0f;

        // Determine which input to use
        bool useController = _hasControllerInput && (preferController || !HasSignificantMouseMovement());

        if (useController)
        {
            targetOffsetX = GetControllerTargetOffset();
        }
        else
        {
            targetOffsetX = GetMouseTargetOffset();
        }

        _currentTargetOffsetX = targetOffsetX;

        // Apply rotations to both arms
        ApplyLocalX(leftArmPivot, _baseLeftRot, _currentTargetOffsetX, rotateSpeed);

        float rightOffset = mirrorRight ? -_currentTargetOffsetX : _currentTargetOffsetX;
        ApplyLocalX(rightArmPivot, _baseRightRot, rightOffset, rotateSpeed);
    }

    private bool HasSignificantMouseMovement()
    {
        // Check if mouse is actively being moved (you can adjust this logic based on your needs)
        Vector3 mousePos = Input.mousePosition;
        return mousePos.y > 0 && mousePos.y < Screen.height; // Simple check if mouse is in screen bounds
    }

    private float GetMouseTargetOffset()
    {
        float mouseY = Input.mousePosition.y;
        float t = Mathf.Clamp01(mouseY / Mathf.Max(1f, (float)Screen.height)); // 0..1

        if (invertMouse) t = 1f - t;

        return Mathf.Lerp(minX, maxX, t);
    }

    private float GetControllerTargetOffset()
    {
        // Use right stick Y axis for vertical aiming
        float stickY = _rightStickInput.y;

        if (invertController) stickY = -stickY;

        // Apply sensitivity
        stickY *= controllerSensitivity;

        // Apply curve for fine control
        float curveInput = Mathf.Abs(stickY);
        float curveOutput = controllerCurve.Evaluate(curveInput);
        stickY = Mathf.Sign(stickY) * curveOutput;

        // Convert from -1..1 stick range to our min/max X range
        // Stick input of 0 maps to center of our range
        float centerX = (minX + maxX) * 0.5f;
        float rangeX = (maxX - minX) * 0.5f;

        return centerX + (stickY * rangeX);
    }

    static void ApplyLocalX(Transform pivot, Quaternion baseLocalRot, float offsetX, float speed)
    {
        Quaternion target = baseLocalRot * Quaternion.AngleAxis(offsetX, Vector3.right);

        if (speed <= 0f)
        {
            pivot.localRotation = target;
        }
        else
        {
            pivot.localRotation = Quaternion.RotateTowards(
                pivot.localRotation,
                target,
                speed * Time.deltaTime
            );
        }
    }

    // Debug information
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        // Draw current aim direction
        if (leftArmPivot != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(leftArmPivot.position, leftArmPivot.forward * 2f);
        }

        if (rightArmPivot != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(rightArmPivot.position, rightArmPivot.forward * 2f);
        }
    }

    // Public methods for external control
    public void SetTargetOffset(float offsetX)
    {
        _currentTargetOffsetX = Mathf.Clamp(offsetX, minX, maxX);
    }

    public float GetCurrentOffset()
    {
        return _currentTargetOffsetX;
    }

    public bool IsUsingController()
    {
        return _hasControllerInput;
    }

    // Debug methods
    [ContextMenu("Test Controller Input")]
    public void TestControllerInput()
    {
        Debug.Log($"Right Stick: {_rightStickInput}, Has Input: {_hasControllerInput}, Current Offset: {_currentTargetOffsetX}");
    }
}