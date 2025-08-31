using UnityEngine;
using UnityEngine.InputSystem;

public class ArmAimer : MonoBehaviour
{
    [Header("Refs")]
    public Transform leftArmPivot;
    public Transform rightArmPivot;
    public Camera cam;

    [Header("Mapping (mouse Y -> Z)")]
    [Tooltip("������Ļ�ײ�������ӳ�䵽 [MinZ, MaxZ]���ȣ�")]
    public float minZ = -60f;
    public float maxZ = 60f;
    [Tooltip("����ӳ�䣺���������Խ�߽Ƕ�ԽС")]
    public bool invert = false;

    [Header("Per-Arm Options")]
    [Tooltip("�ұ��Ƿ��񣨺ܶ�ģ�����ҹ��������෴ʱ��Ҫ���ϣ�")]
    public bool mirrorRight = false;

    [Header("Smoothing")]
    [Tooltip("��/�룻0 ��ʾ˲ʱ����")]
    public float rotateSpeed = 720f;

    // ��¼��ʼ�ı���Z�ǣ�ӳ�������ڴ˻�����ƫ��
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

        // ӳ�䵽Ŀ��Ƕȣ���Գ�ʼZ�ǵ�ƫ�ƣ�
        float offsetZ = Mathf.Lerp(minZ, maxZ, t);

        // ���
        ApplyZ(leftArmPivot, _baseLeftZ + offsetZ, rotateSpeed);

        // �ұۣ��ɾ���
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
