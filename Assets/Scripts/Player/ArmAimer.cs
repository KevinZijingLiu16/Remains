using UnityEngine;
using UnityEngine.InputSystem;

public class ArmAimer : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform leftArmPivot;
    [SerializeField] private Transform rightArmPivot;
    [SerializeField] private Camera cam;

    [Header("Mapping (mouse Y -> X)")]
    [SerializeField] private float minX = -60f;
    [SerializeField] private float maxX = 60f;

    [SerializeField] private bool invert = false;

    [Header("Per-Arm Options")]
    [SerializeField] private bool mirrorRight = false;

    [Header("Smoothing (deg/sec)")]
    [SerializeField] private float rotateSpeed = 720f;

    Quaternion _baseLeftRot, _baseRightRot;

    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    void Start()
    {
        if (!leftArmPivot || !rightArmPivot)
        {
            Debug.LogWarning("[ArmAimer] Missing references.");
            enabled = false;
            return;
        }

        // 记录初始局部旋转作为基准
        _baseLeftRot = leftArmPivot.localRotation;
        _baseRightRot = rightArmPivot.localRotation;
    }

    void Update()
    {
        float mouseY = Input.mousePosition.y;
        float t = Mathf.Clamp01(mouseY / Mathf.Max(1f, (float)Screen.height)); // 0..1
        if (invert) t = 1f - t;

        float offsetX = Mathf.Lerp(minX, maxX, t);

        // 左臂：基于初始局部旋转，绕本地 X 轴叠加 offsetX
        ApplyLocalX(leftArmPivot, _baseLeftRot, offsetX, rotateSpeed);

        // 右臂：可镜像
        float rightOffset = mirrorRight ? -offsetX : offsetX;
        ApplyLocalX(rightArmPivot, _baseRightRot, rightOffset, rotateSpeed);
    }

    static void ApplyLocalX(Transform pivot, Quaternion baseLocalRot, float offsetX, float speed)
    {
        // 在本地 X 轴上叠加旋转：先 base，再 AngleAxis（相当于绕“自身”X轴）
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
}
