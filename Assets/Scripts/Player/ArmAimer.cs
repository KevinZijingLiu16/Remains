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

        // ��¼��ʼ�ֲ���ת��Ϊ��׼
        _baseLeftRot = leftArmPivot.localRotation;
        _baseRightRot = rightArmPivot.localRotation;
    }

    void Update()
    {
        float mouseY = Input.mousePosition.y;
        float t = Mathf.Clamp01(mouseY / Mathf.Max(1f, (float)Screen.height)); // 0..1
        if (invert) t = 1f - t;

        float offsetX = Mathf.Lerp(minX, maxX, t);

        // ��ۣ����ڳ�ʼ�ֲ���ת���Ʊ��� X ����� offsetX
        ApplyLocalX(leftArmPivot, _baseLeftRot, offsetX, rotateSpeed);

        // �ұۣ��ɾ���
        float rightOffset = mirrorRight ? -offsetX : offsetX;
        ApplyLocalX(rightArmPivot, _baseRightRot, rightOffset, rotateSpeed);
    }

    static void ApplyLocalX(Transform pivot, Quaternion baseLocalRot, float offsetX, float speed)
    {
        // �ڱ��� X ���ϵ�����ת���� base���� AngleAxis���൱���ơ�����X�ᣩ
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
