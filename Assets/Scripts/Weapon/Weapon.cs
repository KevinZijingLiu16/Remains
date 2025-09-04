using UnityEngine;

public class Weapon : IWeapon
{
    private readonly WeaponData _data;
    private GameObject _instantiatedWeapon;

    public string WeaponId => _data.weaponId;
    public string WeaponName => _data.weaponName;
    public GameObject WeaponPrefab => _data.weaponPrefab;
    public Sprite WeaponIcon => _data.weaponIcon;

    public Weapon(WeaponData data)
    {
        _data = data ?? throw new System.ArgumentNullException(nameof(data));
        Debug.Log($"[Weapon] Created weapon: {WeaponName} (ID: {WeaponId})");
    }

    public void OnEquip(Transform parent)
    {
        Debug.Log($"[Weapon] OnEquip called for {WeaponName}");
        Debug.Log($"[Weapon] Parent: {(parent != null ? parent.name : "NULL")}");
        Debug.Log($"[Weapon] WeaponPrefab: {(WeaponPrefab != null ? WeaponPrefab.name : "NULL")}");

        if (_instantiatedWeapon != null)
        {
            Debug.Log($"[Weapon] Unequipping existing weapon first");
            OnUnequip();
        }

        if (WeaponPrefab == null)
        {
            Debug.LogError($"[Weapon] Cannot equip {WeaponName} - WeaponPrefab is null!");
            return;
        }

        if (parent == null)
        {
            Debug.LogError($"[Weapon] Cannot equip {WeaponName} - Parent transform is null!");
            return;
        }

        try
        {
            Debug.Log($"[Weapon] Instantiating weapon prefab...");
            _instantiatedWeapon = Object.Instantiate(WeaponPrefab, parent);

            if (_instantiatedWeapon != null)
            {
                
                _instantiatedWeapon.transform.localPosition = _data.equipPosition;
                _instantiatedWeapon.transform.localRotation = Quaternion.Euler(_data.equipRotation);
                _instantiatedWeapon.transform.localScale = _data.equipScale;

                Debug.Log($"[Weapon] ? Successfully equipped {WeaponName}!");
                Debug.Log($"[Weapon] - Position: {_instantiatedWeapon.transform.localPosition}");
                Debug.Log($"[Weapon] - Rotation: {_instantiatedWeapon.transform.localRotation.eulerAngles}");
                Debug.Log($"[Weapon] - Scale: {_instantiatedWeapon.transform.localScale}");
            }
            else
            {
                Debug.LogError($"[Weapon] Failed to instantiate weapon {WeaponName} - Instantiate returned null!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Weapon] Exception while equipping {WeaponName}: {e.Message}");
        }
    }

    public void OnUnequip()
    {
        Debug.Log($"[Weapon] OnUnequip called for {WeaponName}");

        if (_instantiatedWeapon != null)
        {
            Debug.Log($"[Weapon] Destroying weapon instance: {_instantiatedWeapon.name}");
            Object.Destroy(_instantiatedWeapon);
            _instantiatedWeapon = null;
            Debug.Log($"[Weapon] ? Weapon unequipped");
        }
        else
        {
            Debug.Log($"[Weapon] No weapon instance to unequip");
        }
    }
}