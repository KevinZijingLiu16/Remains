using UnityEngine;

public class WeaponEquipmentManager : MonoBehaviour, IWeaponEquipmentManager
{
    [Header("Equipment Settings")]
    public Transform weaponEquipPoint;

    [Header("Dependencies")]
    [SerializeField] private WeaponDatabase weaponDatabase;

    [Header("Debug")]
    public bool enableDebugLogs = true;
    public bool showEquipPointGizmo = true;

    private IWeapon _currentWeapon;
    private IWeaponDataProvider _dataProvider;

    public IWeapon CurrentWeapon => _currentWeapon;

    public event System.Action<IWeapon> OnWeaponEquipped;
    public event System.Action<IWeapon> OnWeaponUnequipped;

    void Start()
    {
        InitializeDataProvider();
        ValidateSetup();
    }

    private void InitializeDataProvider()
    {
        _dataProvider = weaponDatabase;
        if (_dataProvider == null)
        {
            Debug.LogError("[WeaponEquipmentManager] WeaponDatabase not assigned!");
        }
        else if (enableDebugLogs)
        {
            Debug.Log("[WeaponEquipmentManager] WeaponDatabase initialized");
        }
    }

    private void ValidateSetup()
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[WeaponEquipmentManager] Validating setup...");
            Debug.Log($"- weaponEquipPoint: {(weaponEquipPoint != null ? weaponEquipPoint.name : "NULL")}");
            Debug.Log($"- weaponDatabase: {(weaponDatabase != null ? weaponDatabase.name : "NULL")}");

            if (weaponEquipPoint != null)
            {
                Debug.Log($"- equipPoint position: {weaponEquipPoint.position}");
                Debug.Log($"- equipPoint parent: {(weaponEquipPoint.parent != null ? weaponEquipPoint.parent.name : "NULL")}");
            }
        }

        if (weaponEquipPoint == null)
        {
            Debug.LogError("[WeaponEquipmentManager] weaponEquipPoint is not assigned!");
        }

        if (weaponDatabase == null)
        {
            Debug.LogError("[WeaponEquipmentManager] weaponDatabase is not assigned!");
        }

      
        if (_dataProvider != null && enableDebugLogs)
        {
            var weapons = _dataProvider.GetAvailableWeapons();
            Debug.Log($"[WeaponEquipmentManager] Found {weapons.Length} weapons in database:");
            foreach (var weapon in weapons)
            {
                Debug.Log($"- {weapon.WeaponName} (ID: {weapon.WeaponId}) - Prefab: {(weapon.WeaponPrefab != null ? "✓" : "✗")}");
            }
        }
    }

    public void EquipWeapon(string weaponId)
    {
        if (enableDebugLogs) Debug.Log($"[WeaponEquipmentManager] EquipWeapon called with ID: {weaponId}");

        if (_dataProvider == null)
        {
            Debug.LogError("[WeaponEquipmentManager] Cannot equip weapon - data provider is null!");
            return;
        }

        if (string.IsNullOrEmpty(weaponId))
        {
            Debug.LogError("[WeaponEquipmentManager] Cannot equip weapon - weaponId is null or empty!");
            return;
        }

        var weaponToEquip = _dataProvider.GetWeapon(weaponId);
        if (weaponToEquip == null)
        {
            Debug.LogError($"[WeaponEquipmentManager] Weapon not found in database: {weaponId}");

            
            if (enableDebugLogs)
            {
                var availableWeapons = _dataProvider.GetAvailableWeapons();
                Debug.Log("Available weapon IDs:");
                foreach (var w in availableWeapons)
                {
                    Debug.Log($"- {w.WeaponId}");
                }
            }
            return;
        }

        if (enableDebugLogs) Debug.Log($"[WeaponEquipmentManager] Found weapon: {weaponToEquip.WeaponName}");

     
        UnequipCurrentWeapon();

      
        if (weaponEquipPoint == null)
        {
            Debug.LogError($"[WeaponEquipmentManager] Cannot equip weapon - weaponEquipPoint is null!");
            return;
        }

        if (enableDebugLogs) Debug.Log($"[WeaponEquipmentManager] Equipping weapon to: {weaponEquipPoint.name}");

        weaponToEquip.OnEquip(weaponEquipPoint);
        _currentWeapon = weaponToEquip;

        OnWeaponEquipped?.Invoke(_currentWeapon);

        if (enableDebugLogs) Debug.Log($"[WeaponEquipmentManager] ✓ Weapon equipped successfully: {_currentWeapon.WeaponName}");
    }

    public void UnequipCurrentWeapon()
    {
        if (_currentWeapon != null)
        {
            if (enableDebugLogs) Debug.Log($"[WeaponEquipmentManager] Unequipping current weapon: {_currentWeapon.WeaponName}");

            _currentWeapon.OnUnequip();
            var unequippedWeapon = _currentWeapon;
            _currentWeapon = null;

            OnWeaponUnequipped?.Invoke(unequippedWeapon);

            if (enableDebugLogs) Debug.Log("[WeaponEquipmentManager] ✓ Weapon unequipped");
        }
        else if (enableDebugLogs)
        {
            Debug.Log("[WeaponEquipmentManager] No current weapon to unequip");
        }
    }

 
    [ContextMenu("Test Equip First Weapon")]
    public void TestEquipFirstWeapon()
    {
        if (_dataProvider != null)
        {
            var weapons = _dataProvider.GetAvailableWeapons();
            if (weapons.Length > 0)
            {
                Debug.Log($"[WeaponEquipmentManager] Testing equip first weapon: {weapons[0].WeaponId}");
                EquipWeapon(weapons[0].WeaponId);
            }
            else
            {
                Debug.LogError("[WeaponEquipmentManager] No weapons available for testing!");
            }
        }
    }

    [ContextMenu("Print Current Weapon Info")]
    public void PrintCurrentWeaponInfo()
    {
        if (_currentWeapon != null)
        {
            Debug.Log($"[WeaponEquipmentManager] Current weapon: {_currentWeapon.WeaponName} (ID: {_currentWeapon.WeaponId})");
        }
        else
        {
            Debug.Log("[WeaponEquipmentManager] No current weapon equipped");
        }
    }


    void OnDrawGizmos()
    {
        if (showEquipPointGizmo && weaponEquipPoint != null)
        {
            Gizmos.color = _currentWeapon != null ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(weaponEquipPoint.position, 0.1f);

          
            Gizmos.color = Color.red;
            Gizmos.DrawRay(weaponEquipPoint.position, weaponEquipPoint.right * 0.2f);
            Gizmos.color = Color.green;
            Gizmos.DrawRay(weaponEquipPoint.position, weaponEquipPoint.up * 0.2f);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(weaponEquipPoint.position, weaponEquipPoint.forward * 0.2f);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (weaponEquipPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(weaponEquipPoint.position, 0.2f);
        }
    }
}