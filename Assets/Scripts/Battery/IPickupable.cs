using UnityEngine;

public interface IPickupable
{
    string PickupId { get; }
    string PickupName { get; }
    bool IsPickedUp { get; }
    void Pickup(Transform holdPoint);
    void Drop(Vector3 dropPosition);
}