using UnityEngine;
using System.Collections.Generic;


public class WeaponAccessoryManager : MonoBehaviour
{
    [Header("Accessory Settings")]
    [SerializeField] private Transform playerBackAttachPoint; 
    [SerializeField] private bool autoCreateBackAttachPoint = true;
    [SerializeField] private Vector3 backAttachPointOffset = new Vector3(0, 0.5f, -0.3f); 

    [Header("Accessory Prefabs")]
    [SerializeField] private GameObject airBlowerTankPrefab; 
    [SerializeField] private GameObject foamSprayTankPrefab; 

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
  //  [SerializeField] private bool showGizmos = true;

    private Dictionary<string, GameObject> _activeAccessories = new Dictionary<string, GameObject>();
    private WeaponEquipmentManager _equipmentManager;

    public Transform PlayerBackAttachPoint => playerBackAttachPoint;

    void Start()
    {
        InitializeSystem();
    }

    private void InitializeSystem()
    {
       
        _equipmentManager = FindFirstObjectByType<WeaponEquipmentManager>();
        if (_equipmentManager == null)
        {
            Debug.LogWarning("[WeaponAccessoryManager] WeaponEquipmentManager not found!");
            return;
        }

     
        _equipmentManager.OnWeaponEquipped += OnWeaponEquipped;
        _equipmentManager.OnWeaponUnequipped += OnWeaponUnequipped;

   
        if (playerBackAttachPoint == null && autoCreateBackAttachPoint)
        {
            CreateBackAttachPoint();
        }

        if (enableDebugLogs)
        {
            Debug.Log("[WeaponAccessoryManager] System initialized");
            Debug.Log($"- Back attach point: {(playerBackAttachPoint != null ? playerBackAttachPoint.name : "NULL")}");
        }
    }

    private void CreateBackAttachPoint()
    {
        var playerTransform = GetComponent<Transform>();
        if (playerTransform == null)
        {
            Debug.LogError("[WeaponAccessoryManager] Cannot create back attach point - no player transform!");
            return;
        }

  
        var backPoint = new GameObject("PlayerBackAttachPoint");
        backPoint.transform.SetParent(playerTransform);
        backPoint.transform.localPosition = backAttachPointOffset;
        backPoint.transform.localRotation = Quaternion.identity;

        playerBackAttachPoint = backPoint.transform;

        if (enableDebugLogs)
        {
            Debug.Log("[WeaponAccessoryManager] Created back attach point automatically");
        }
    }

    private void OnWeaponEquipped(IWeapon weapon)
    {
        if (weapon == null) return;

        switch (weapon.WeaponId.ToLower())
        {
            case "air_blower":
                CreateAccessory("air_blower_tank", airBlowerTankPrefab);
                break;
            case "foam_spray":
                CreateAccessory("foam_spray_tank", foamSprayTankPrefab);
                break;
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[WeaponAccessoryManager] Processed weapon equip: {weapon.WeaponName}");
        }
    }

    private void OnWeaponUnequipped(IWeapon weapon)
    {
        if (weapon == null) return;

  
        switch (weapon.WeaponId.ToLower())
        {
            case "air_blower":
                RemoveAccessory("air_blower_tank");
                break;
            case "foam_spray":
                RemoveAccessory("foam_spray_tank");
                break;
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[WeaponAccessoryManager] Processed weapon unequip: {weapon.WeaponName}");
        }
    }

    private void CreateAccessory(string accessoryId, GameObject prefab)
    {
        if (prefab == null)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning($"[WeaponAccessoryManager] No prefab assigned for accessory: {accessoryId}");
            }
            return;
        }

        if (playerBackAttachPoint == null)
        {
            Debug.LogError("[WeaponAccessoryManager] Cannot create accessory - no back attach point!");
            return;
        }

   
        RemoveAccessory(accessoryId);

    
        var accessoryInstance = Instantiate(prefab, playerBackAttachPoint);
        accessoryInstance.name = $"{prefab.name}_Instance";

  
        accessoryInstance.transform.localPosition = Vector3.zero;
        accessoryInstance.transform.localRotation = Quaternion.identity;
        accessoryInstance.transform.localScale = Vector3.one;

        _activeAccessories[accessoryId] = accessoryInstance;

        if (enableDebugLogs)
        {
            Debug.Log($"[WeaponAccessoryManager] ✓ Created accessory: {accessoryId}");
        }
    }

    private void RemoveAccessory(string accessoryId)
    {
        if (_activeAccessories.TryGetValue(accessoryId, out GameObject accessory))
        {
            if (accessory != null)
            {
                if (enableDebugLogs)
                {
                    Debug.Log($"[WeaponAccessoryManager] Removing accessory: {accessoryId}");
                }

                DestroyImmediate(accessory);
            }

            _activeAccessories.Remove(accessoryId);

            if (enableDebugLogs)
            {
                Debug.Log($"[WeaponAccessoryManager] ✓ Removed accessory: {accessoryId}");
            }
        }
    }


    [ContextMenu("Clear All Accessories")]
    public void ClearAllAccessories()
    {
        var accessoryIds = new List<string>(_activeAccessories.Keys);
        foreach (var id in accessoryIds)
        {
            RemoveAccessory(id);
        }

        if (enableDebugLogs)
        {
            Debug.Log("[WeaponAccessoryManager] ✓ Cleared all accessories");
        }
    }


    [ContextMenu("Test Create Air Blower Tank")]
    public void TestCreateAirBlowerTank()
    {
        CreateAccessory("air_blower_tank", airBlowerTankPrefab);
    }


    public GameObject GetActiveAccessory(string accessoryId)
    {
        _activeAccessories.TryGetValue(accessoryId, out GameObject accessory);
        return accessory;
    }


    public bool HasAccessory(string accessoryId)
    {
        return _activeAccessories.ContainsKey(accessoryId) && _activeAccessories[accessoryId] != null;
    }

   
    public void SetBackAttachPointOffset(Vector3 offset)
    {
        backAttachPointOffset = offset;
        if (playerBackAttachPoint != null)
        {
            playerBackAttachPoint.localPosition = offset;
        }
    }

    void OnDestroy()
    {
       
        if (_equipmentManager != null)
        {
            _equipmentManager.OnWeaponEquipped -= OnWeaponEquipped;
            _equipmentManager.OnWeaponUnequipped -= OnWeaponUnequipped;
        }

      
        ClearAllAccessories();
    }

   

 
}