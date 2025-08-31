using UnityEngine;
using UnityEngine.InputSystem;

public class ArmAimer : MonoBehaviour
{
    [Header("Refs")]
    public Transform leftArmPivot;
    public Transform rightArmPivot;
    public Camera cam;

    [Header("Mapping (mouse Y -> Z)")]
    [Tooltip("鼠标从屏幕底部到顶部映射到 [MinZ, MaxZ]（度）")]
    public float minZ = -60f;
    public float maxZ = 60f;
    [Tooltip("反向映射：勾上则鼠标越高角度越小")]
    public bool invert = false;

    [Header("Per-Arm Options")]
    [Tooltip("右臂是否镜像（很多模型左右骨骼方向相反时需要勾上）")]
    public bool mirrorRight = false;

    [Header("Smoothing")]
    [Tooltip("度/秒；0 表示瞬时对齐")]
    public float rotateSpeed = 720f;

    // 记录初始的本地Z角，映射结果会在此基础上偏移
    float _baseLeftZ, _baseRightZ;

    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    void Start()
    {
        if (!leftArmPivot || !rightArmPivot || !cam)
        {
            Debug.LogWarning("[ArmPivotsZByMouseY] Missing references.");
            enabled = false;
            return;
        }

        _baseLeftZ = leftArmPivot.localEulerAngles.z;
        _baseRightZ = rightArmPivot.localEulerAngles.z;
    }

    void Update()
    {
        float mouseY = Input.mousePosition.y;
        float t = Mathf.Clamp01(mouseY / Mathf.Max(1f, (float)Screen.height)); // 0..1

        if (invert) t = 1f - t;

        // 映射到目标角度（相对初始Z角的偏移）
        float offsetZ = Mathf.Lerp(minZ, maxZ, t);

        // 左臂
        ApplyZ(leftArmPivot, _baseLeftZ + offsetZ, rotateSpeed);

        // 右臂（可镜像）
        float rightTarget = _baseRightZ + (mirrorRight ? -offsetZ : offsetZ);
        ApplyZ(rightArmPivot, rightTarget, rotateSpeed);
    }

    static void ApplyZ(Transform t, float targetZ, float speed)
    {
        Vector3 e = t.localEulerAngles;
        float curZ = e.z;

        float newZ = (speed <= 0f)
            ? targetZ
            : Mathf.MoveTowardsAngle(curZ, targetZ, speed * Time.deltaTime);

        e.z = newZ;
        t.localEulerAngles = e;
    }
}
