using UnityEngine;

[DisallowMultipleComponent]
public class RigidbodyAdapter : MonoBehaviour, IPhysicsBody
{
    [SerializeField] private Rigidbody rb;

    void Reset()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
    }

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (rb == null) Debug.LogError("[RigidbodyAdapter] Missing Rigidbody.");
    }

    public Vector3 Position => rb ? rb.position : transform.position;
    public Quaternion Rotation => rb ? rb.rotation : transform.rotation;

    public Vector3 Velocity
    {
        get => rb ? rb.linearVelocity : Vector3.zero;
        set { if (rb) rb.linearVelocity = value; }
    }

    public void MovePosition(Vector3 worldPos)
    {
        if (rb) rb.MovePosition(worldPos);
        else transform.position = worldPos;
    }

    public void MoveRotation(Quaternion worldRot)
    {
        if (rb) rb.MoveRotation(worldRot);
        else transform.rotation = worldRot;
    }
}
