using UnityEngine;
using UnityEngine.InputSystem;

public class ArmAimer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform leftArmPivot;
    [SerializeField] private Transform rightArmPivot;
    [SerializeField] private Camera cam;

    [Header("Input Actions (New Input System)")]
    [SerializeField] private InputActionProperty rightStickAction;   // <Gamepad>/rightStick (Vector2)
    [SerializeField] private InputActionProperty pointerPositionAction; // <Mouse>/position or <Pointer>/position (Vector2)

    [Header("Mouse/Pointer Mapping (pointer Y -> X rotation)")]
    [SerializeField] private float minX = -60f;
    [SerializeField] private float maxX = 60f;
    [SerializeField] private bool invertPointer = false;

    [Header("Controller Mapping (right stick Y -> X rotation)")]
    [SerializeField] private bool invertController = false;
    [SerializeField] private float controllerSensitivity = 1f;
    [SerializeField] private AnimationCurve controllerCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("Per-Arm Options")]
    [SerializeField] private bool mirrorRight = false;

    [Header("Smoothing (deg/sec)")]
    [SerializeField] private float rotateSpeed = 720f;

    [Header("Input Priority")]
    [SerializeField] private bool preferController = false; // 若为 true，右摇杆优先

    [Header("Debug")]
    [SerializeField] private bool logAngleEachFrame = false;
    [SerializeField] private int logEveryNFrames = 10;

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

        _baseLeftRot = leftArmPivot.localRotation;
        _baseRightRot = rightArmPivot.localRotation;

        EnableInput();
    }

    void OnEnable() => EnableInput();
    void OnDisable() => DisableInput();
    void OnDestroy() => DisableInput();

    private void EnableInput()
    {
        if (rightStickAction.action != null)
        {
            rightStickAction.action.Enable();
            rightStickAction.action.performed += OnRightStickInput;
            rightStickAction.action.canceled += OnRightStickCanceled;
        }

        if (pointerPositionAction.action != null)
        {
            pointerPositionAction.action.Enable();
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

        if (pointerPositionAction.action != null)
        {
            pointerPositionAction.action.Disable();
        }
    }

    private void OnRightStickInput(InputAction.CallbackContext ctx)
    {
        _rightStickInput = ctx.ReadValue<Vector2>();
        _hasControllerInput = _rightStickInput.magnitude > 0.1f; // deadzone
    }

    private void OnRightStickCanceled(InputAction.CallbackContext ctx)
    {
        _rightStickInput = Vector2.zero;
        _hasControllerInput = false;
    }

    void Update()
    {
        // 选择输入来源
        bool useController = _hasControllerInput && (preferController || !HasPointerInside());

        float targetOffsetX = useController
            ? GetControllerTargetOffset()
            : GetPointerTargetOffset(); // 使用 pointer position（新输入系统）

        _currentTargetOffsetX = Mathf.Clamp(targetOffsetX, minX, maxX);

        if (logAngleEachFrame && (Time.frameCount % logEveryNFrames == 0))
        {
            Debug.Log($"[ArmAimer] Current Aim Pitch: {_currentTargetOffsetX:F1}° (useController={useController})");
        }

        // 应用到左右臂
        ApplyLocalX(leftArmPivot, _baseLeftRot, _currentTargetOffsetX, rotateSpeed);
        float rightOffset = mirrorRight ? -_currentTargetOffsetX : _currentTargetOffsetX;
        ApplyLocalX(rightArmPivot, _baseRightRot, rightOffset, rotateSpeed);
    }

    // 判断指针是否在屏幕内（用新输入系统的 pointer position）
    private bool HasPointerInside()
    {
        if (pointerPositionAction.action == null) return false;

        Vector2 pos = pointerPositionAction.action.ReadValue<Vector2>();
        return (pos.x >= 0f && pos.x <= Screen.width && pos.y >= 0f && pos.y <= Screen.height);
    }

    // 用 pointer 的 Y 位置映射到 X 旋转（保持你原先手感）
    private float GetPointerTargetOffset()
    {
        Vector2 pos = Vector2.zero;

        if (pointerPositionAction.action != null)
        {
            pos = pointerPositionAction.action.ReadValue<Vector2>();
        }
        else
        {
            // 兜底：若没绑定，尽量从新系统 Mouse 取
            if (Mouse.current != null) pos = Mouse.current.position.ReadValue();
        }

        float t = Mathf.Clamp01(pos.y / Mathf.Max(1f, (float)Screen.height)); // 0..1
        if (invertPointer) t = 1f - t;

        return Mathf.Lerp(minX, maxX, t);
    }

    // 右摇杆 Y -> X 旋转
    private float GetControllerTargetOffset()
    {
        float stickY = _rightStickInput.y;
        if (invertController) stickY = -stickY;

        stickY *= controllerSensitivity;

        float curveInput = Mathf.Clamp01(Mathf.Abs(stickY));
        float curveOutput = controllerCurve.Evaluate(curveInput);
        stickY = Mathf.Sign(stickY) * curveOutput;

        float centerX = (minX + maxX) * 0.5f;
        float rangeX = (maxX - minX) * 0.5f;
        return centerX + (stickY * rangeX);
    }

    static void ApplyLocalX(Transform pivot, Quaternion baseLocalRot, float offsetX, float speed)
    {
        Quaternion target = baseLocalRot * Quaternion.AngleAxis(offsetX, Vector3.right);
        if (speed <= 0f) pivot.localRotation = target;
        else pivot.localRotation = Quaternion.RotateTowards(pivot.localRotation, target, speed * Time.deltaTime);
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        if (leftArmPivot != null) { Gizmos.color = Color.red; Gizmos.DrawRay(leftArmPivot.position, leftArmPivot.forward * 2f); }
        if (rightArmPivot != null) { Gizmos.color = Color.blue; Gizmos.DrawRay(rightArmPivot.position, rightArmPivot.forward * 2f); }
    }

    // Public
    public void SetTargetOffset(float offsetX) => _currentTargetOffsetX = Mathf.Clamp(offsetX, minX, maxX);
    public float GetCurrentOffset() => _currentTargetOffsetX;
    public bool IsUsingController() => _hasControllerInput;
}
