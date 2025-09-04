using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponDatabase", menuName = "Weapon System/Weapon Database")]
public class WeaponDatabase : ScriptableObject, IWeaponDataProvider
{
    [SerializeField] private WeaponData[] weaponDataArray;

    private Dictionary<string, IWeapon> _weaponCache;

    void OnEnable()
    {
        InitializeWeaponCache();
    }

    private void InitializeWeaponCache()
    {
        _weaponCache = new Dictionary<string, IWeapon>();

        if (weaponDataArray != null)
        {
            foreach (var data in weaponDataArray)
            {
                if (!string.IsNullOrEmpty(data.weaponId))
                {
                    _weaponCache[data.weaponId] = new Weapon(data);
                }
            }
        }
    }

    public IWeapon[] GetAvailableWeapons()
    {
        if (_weaponCache == null) InitializeWeaponCache();
        return _weaponCache.Values.ToArray();
    }

    public IWeapon GetWeapon(string weaponId)
    {
        if (_weaponCache == null) InitializeWeaponCache();
        return _weaponCache.TryGetValue(weaponId, out var weapon) ? weapon : null;
    }
}
