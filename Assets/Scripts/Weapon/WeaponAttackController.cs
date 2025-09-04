using UnityEngine;

public class WeaponAttackController : MonoBehaviour
{
    [Header("Dependencies")]
    public WeaponEquipmentManager equipmentManager;
    public WeaponInputProcessor inputProcessor;
    public PlayerPower playerPower;

    [Header("Debug")]
    public bool enableDebugLogs = true;

    private IWeaponAttackBehavior _currentPrimaryAttack;
    private IWeaponAttackBehavior _currentSecondaryAttack;
    private bool _primaryAttackActive = false;
    private bool _secondaryAttackActive = false;

    void Start()
    {
        InitializeSystem();
    }

    void Update()
    {
        UpdateActiveAttacks();
    }

    private void InitializeSystem()
    {
      
        if (equipmentManager != null)
        {
            equipmentManager.OnWeaponEquipped += OnWeaponEquipped;
            equipmentManager.OnWeaponUnequipped += OnWeaponUnequipped;
        }

      
        if (inputProcessor != null)
        {
            inputProcessor.OnPrimaryAttackStart += StartPrimaryAttack;
            inputProcessor.OnPrimaryAttackStop += StopPrimaryAttack;
            inputProcessor.OnSecondaryAttackStart += StartSecondaryAttack;
            inputProcessor.OnSecondaryAttackStop += StopSecondaryAttack;
        }

        if (playerPower == null)
        {
            playerPower = FindFirstObjectByType<PlayerPower>();
        }

        if (enableDebugLogs) Debug.Log("[WeaponAttackController] System initialized");
    }

    private void OnWeaponEquipped(IWeapon weapon)
    {
       
        SetupWeaponAttacks(weapon);

        if (enableDebugLogs) Debug.Log($"[WeaponAttackController] Setup attacks for weapon: {weapon.WeaponName}");
    }

    private void OnWeaponUnequipped(IWeapon weapon)
    {
        StopAllAttacks();
        _currentPrimaryAttack = null;
        _currentSecondaryAttack = null;

        if (enableDebugLogs) Debug.Log("[WeaponAttackController] Cleared weapon attacks");
    }

    private void SetupWeaponAttacks(IWeapon weapon)
    {
       
        switch (weapon.WeaponId)
        {
            case "foam_spray":
                _currentPrimaryAttack = WeaponAttackFactory.CreateAttackBehavior(AttackType.FoamSpray);
                _currentSecondaryAttack = null;
                break;

            case "air_blower":
                _currentPrimaryAttack = WeaponAttackFactory.CreateAttackBehavior(AttackType.AirBlower);
                _currentSecondaryAttack = WeaponAttackFactory.CreateAttackBehavior(AttackType.AirBlower);

              
                if (_currentSecondaryAttack is AirBlowerAttack secondaryBlower)
                {
                    secondaryBlower.SetReversed(true); 
                }
                break;

            default:
                _currentPrimaryAttack = null;
                _currentSecondaryAttack = null;
                break;
        }
    }

    private void StartPrimaryAttack()
    {
        if (_currentPrimaryAttack != null && !_primaryAttackActive)
        {
            var weaponTransform = GetCurrentWeaponTransform();
            if (weaponTransform != null)
            {
                _currentPrimaryAttack.StartAttack(weaponTransform, playerPower);
                _primaryAttackActive = true;

                if (enableDebugLogs) Debug.Log("[WeaponAttackController] Started primary attack");
            }
        }
    }

    private void StopPrimaryAttack()
    {
        if (_currentPrimaryAttack != null && _primaryAttackActive)
        {
            var weaponTransform = GetCurrentWeaponTransform();
            _currentPrimaryAttack.StopAttack(weaponTransform, playerPower);
            _primaryAttackActive = false;

            if (enableDebugLogs) Debug.Log("[WeaponAttackController] Stopped primary attack");
        }
    }

    private void StartSecondaryAttack()
    {
        if (_currentSecondaryAttack != null && !_secondaryAttackActive)
        {
            var weaponTransform = GetCurrentWeaponTransform();
            if (weaponTransform != null)
            {
                _currentSecondaryAttack.StartAttack(weaponTransform, playerPower);
                _secondaryAttackActive = true;

                if (enableDebugLogs) Debug.Log("[WeaponAttackController] Started secondary attack");
            }
        }
    }

    private void StopSecondaryAttack()
    {
        if (_currentSecondaryAttack != null && _secondaryAttackActive)
        {
            var weaponTransform = GetCurrentWeaponTransform();
            _currentSecondaryAttack.StopAttack(weaponTransform, playerPower);
            _secondaryAttackActive = false;

            if (enableDebugLogs) Debug.Log("[WeaponAttackController] Stopped secondary attack");
        }
    }

    private void UpdateActiveAttacks()
    {
        var weaponTransform = GetCurrentWeaponTransform();

        if (_primaryAttackActive && _currentPrimaryAttack != null)
        {
            _currentPrimaryAttack.UpdateAttack(weaponTransform, playerPower);
        }

        if (_secondaryAttackActive && _currentSecondaryAttack != null)
        {
            _currentSecondaryAttack.UpdateAttack(weaponTransform, playerPower);
        }
    }

    private Transform GetCurrentWeaponTransform()
    {
        if (equipmentManager != null && equipmentManager.weaponEquipPoint != null)
        {
           
            return equipmentManager.weaponEquipPoint.childCount > 0
                ? equipmentManager.weaponEquipPoint.GetChild(0)
                : equipmentManager.weaponEquipPoint;
        }
        return null;
    }

    private void StopAllAttacks()
    {
        if (_primaryAttackActive)
        {
            StopPrimaryAttack();
        }

        if (_secondaryAttackActive)
        {
            StopSecondaryAttack();
        }
    }

    void OnDestroy()
    {
        StopAllAttacks();

        if (equipmentManager != null)
        {
            equipmentManager.OnWeaponEquipped -= OnWeaponEquipped;
            equipmentManager.OnWeaponUnequipped -= OnWeaponUnequipped;
        }

        if (inputProcessor != null)
        {
            inputProcessor.OnPrimaryAttackStart -= StartPrimaryAttack;
            inputProcessor.OnPrimaryAttackStop -= StopPrimaryAttack;
            inputProcessor.OnSecondaryAttackStart -= StartSecondaryAttack;
            inputProcessor.OnSecondaryAttackStop -= StopSecondaryAttack;
        }
    }
}