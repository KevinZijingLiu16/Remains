using UnityEngine;
using UnityEngine.InputSystem;

public class ArmAimer : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform leftArmPivot;
    [SerializeField] private Transform rightArmPivot;
    [SerializeField] private Camera cam;

    [Header("Mapping (mouse Y -> Z)")]
  
    [SerializeField] private float minZ = -60f;
    [SerializeField] private float maxZ = 60f;

    [SerializeField] private bool invert = false;

    [Header("Per-Arm Options")]

    [SerializeField] private bool mirrorRight = false;

    [Header("Smoothing")]

    [SerializeField] private float rotateSpeed = 720f;

 
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

       
        float offsetZ = Mathf.Lerp(minZ, maxZ, t);

     
        ApplyZ(leftArmPivot, _baseLeftZ + offsetZ, rotateSpeed);

     
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
