using UnityEngine;

public interface IWeapon
{
    string WeaponId { get; }
    string WeaponName { get; }
    GameObject WeaponPrefab { get; }
    Sprite WeaponIcon { get; }
    void OnEquip(Transform parent);
    void OnUnequip();
}