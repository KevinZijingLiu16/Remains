using UnityEngine;

public interface IPhysicsBody
{
    Vector3 Position { get; }
    Quaternion Rotation { get; }
    Vector3 Velocity { get; set; }

    void MovePosition(Vector3 worldPos);
    void MoveRotation(Quaternion worldRot);
}
