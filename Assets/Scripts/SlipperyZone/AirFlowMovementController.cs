using UnityEngine;

public class AirFlowMovementController : MonoBehaviour, IAirFlowMovement
{
    [Header("Air Flow Movement")]
    public float airFlowForce = 15f;
    public float maxAirFlowSpeed = 8f;

    [Header("Zone Power Settings")]
    public bool requirePowerInZone = true;  
    public float zonePowerCostPerSecond = 2f;  

    [Header("Debug")]
    public bool enableDebugLogs = true;

    [Header("Dependencies")]
    [SerializeField] private SplineRunnerRB _runner;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private WeaponAttackController _weaponController;
    [SerializeField] private WeaponInputProcessor _inputProcessor;
    [SerializeField] private PlayerPower _playerPower;
    [SerializeField] private WeaponEquipmentManager _equipmentManager;

    private bool _isInSlipperyZone = false;
    private bool _airFlowMovementEnabled = false;
    private bool _primaryAttackPressed = false;
    private bool _secondaryAttackPressed = false;

 
    private bool _lastMovementDirectionPositive = true;
    private float _powerCostAccumulator = 0f;

    void Awake()
    {
        _runner = GetComponent<SplineRunnerRB>();
        _rb = GetComponent<Rigidbody>();
        _weaponController = GetComponent<WeaponAttackController>();
        _playerPower = GetComponent<PlayerPower>();
        //_equipmentManager = GetComponent<WeaponEquipmentManager>();  

        

       
       
    }

    void FixedUpdate()
    {
    
        UpdatePlayerDirection();

        if (_airFlowMovementEnabled)
        {
            ProcessAirFlowMovement();
        }
    }

    private void UpdatePlayerDirection()
    {
        if (_runner != null)
        {
           
            _lastMovementDirectionPositive = _runner.IsMovingPositive;

            if (enableDebugLogs && Time.frameCount % 60 == 0) 
            {
                Debug.Log($"[AirFlowMovement] Player facing: {(_lastMovementDirectionPositive ? "Right/Forward" : "Left/Backward")}");
            }
        }
    }

    public void EnableAirFlowMovement(bool enable)
    {
        _isInSlipperyZone = enable;
        _airFlowMovementEnabled = enable;
        _powerCostAccumulator = 0f;  

        if (enable)
        {
            
            if (_inputProcessor != null)
            {
                _inputProcessor.OnPrimaryAttackStart += OnPrimaryAttackStart;
                _inputProcessor.OnPrimaryAttackStop += OnPrimaryAttackStop;
                _inputProcessor.OnSecondaryAttackStart += OnSecondaryAttackStart;
                _inputProcessor.OnSecondaryAttackStop += OnSecondaryAttackStop;

                if (enableDebugLogs)
                    Debug.Log("[AirFlowMovement] Subscribed to input events");
            }
            else if (enableDebugLogs)
            {
                Debug.LogWarning("[AirFlowMovement] InputProcessor not found!");
            }
        }
        else
        {
         
            if (_inputProcessor != null)
            {
                _inputProcessor.OnPrimaryAttackStart -= OnPrimaryAttackStart;
                _inputProcessor.OnPrimaryAttackStop -= OnPrimaryAttackStop;
                _inputProcessor.OnSecondaryAttackStart -= OnSecondaryAttackStart;
                _inputProcessor.OnSecondaryAttackStop -= OnSecondaryAttackStop;
            }

            _primaryAttackPressed = false;
            _secondaryAttackPressed = false;
        }

        if (enableDebugLogs)
            Debug.Log($"[AirFlowMovement] Air flow movement {(enable ? "enabled" : "disabled")}");
    }

    private bool CanUseAirFlow()
    {
       
        if (!requirePowerInZone) return true;

    
        return _playerPower != null && _playerPower.Current > 0;
    }

    private void ConsumePower()
    {
        if (!requirePowerInZone || _playerPower == null) return;

       
        _powerCostAccumulator += zonePowerCostPerSecond * Time.fixedDeltaTime;

       
        if (_powerCostAccumulator >= 1f)
        {
            int cost = Mathf.FloorToInt(_powerCostAccumulator);
            _playerPower.ModifyPower(-cost);
            _powerCostAccumulator -= cost;

            if (enableDebugLogs)
                Debug.Log($"[AirFlowMovement] Consumed {cost} power in zone. Remaining: {_playerPower.Current}");
        }
    }

    private void OnPrimaryAttackStart()
    {
        if (_airFlowMovementEnabled && CanUseAirFlow() && HasAirBlowerEquipped())
        {
            _primaryAttackPressed = true;
            _secondaryAttackPressed = false; 

            if (enableDebugLogs)
            {
                string action = GetAirFlowAction(true);
                Debug.Log($"[AirFlowMovement] Primary attack started - {action}");
            }
        }
        else if (_airFlowMovementEnabled)
        {
            if (!HasAirBlowerEquipped())
            {
                if (enableDebugLogs)
                    Debug.Log("[AirFlowMovement] Cannot use air flow - air blower not equipped");
            }
            else if (!CanUseAirFlow())
            {
                if (enableDebugLogs)
                    Debug.Log("[AirFlowMovement] Cannot use air flow - insufficient power");
            }
        }
    }

    private void OnPrimaryAttackStop()
    {
        if (_airFlowMovementEnabled)
        {
            _primaryAttackPressed = false;

            if (enableDebugLogs)
                Debug.Log("[AirFlowMovement] Primary attack stopped");
        }
    }

    private void OnSecondaryAttackStart()
    {
        if (_airFlowMovementEnabled && CanUseAirFlow() && HasAirBlowerEquipped())
        {
            _secondaryAttackPressed = true;
            _primaryAttackPressed = false; 

            if (enableDebugLogs)
            {
                string action = GetAirFlowAction(false);
                Debug.Log($"[AirFlowMovement] Secondary attack started - {action}");
            }
        }
        else if (_airFlowMovementEnabled)
        {
            if (!HasAirBlowerEquipped())
            {
                if (enableDebugLogs)
                    Debug.Log("[AirFlowMovement] Cannot use air flow - air blower not equipped");
            }
            else if (!CanUseAirFlow())
            {
                if (enableDebugLogs)
                    Debug.Log("[AirFlowMovement] Cannot use air flow - insufficient power");
            }
        }
    }

    private void OnSecondaryAttackStop()
    {
        if (_airFlowMovementEnabled)
        {
            _secondaryAttackPressed = false;

            if (enableDebugLogs)
                Debug.Log("[AirFlowMovement] Secondary attack stopped");
        }
    }

    private string GetAirFlowAction(bool isPrimary)
    {
    
        if (_lastMovementDirectionPositive)
        {
    
            return isPrimary ? "blowing backward (T-)" : "sucking forward (T+)";
        }
        else
        {
     
            return isPrimary ? "blowing backward (T+)" : "sucking forward (T-)";
        }
    }

    private void ProcessAirFlowMovement()
    {
        if (!_airFlowMovementEnabled || _runner == null) return;

  
        if (!HasAirBlowerEquipped())
        {
        
            if (_primaryAttackPressed || _secondaryAttackPressed)
            {
                _primaryAttackPressed = false;
                _secondaryAttackPressed = false;
                if (enableDebugLogs)
                    Debug.Log("[AirFlowMovement] Air flow stopped - air blower not equipped");
            }
            return;
        }

  
        if (!CanUseAirFlow())
        {
      
            if (_primaryAttackPressed || _secondaryAttackPressed)
            {
                _primaryAttackPressed = false;
                _secondaryAttackPressed = false;
                if (enableDebugLogs)
                    Debug.Log("[AirFlowMovement] Air flow stopped - no power");
            }
            return;
        }

        float intensity = 1f;

        if (_primaryAttackPressed)
        {
     
            bool needReverse = _lastMovementDirectionPositive;
            HandleAirFlowMovement(true, needReverse, intensity);
            ConsumePower();  
        }
        else if (_secondaryAttackPressed)
        {
            bool needReverse = !_lastMovementDirectionPositive;
            HandleAirFlowMovement(true, needReverse, intensity);
            ConsumePower(); 
        }
    }

    public void HandleAirFlowMovement(bool isBlowing, bool isReversed, float intensity)
    {
        if (!_airFlowMovementEnabled || _runner == null) return;

        float direction = isReversed ? -1f : 1f;

    
        float moveDistance = direction * airFlowForce * intensity * Time.fixedDeltaTime;

      
        ApplySplineMovement(moveDistance);
    }

    private void ApplySplineMovement(float distance)
    {
        if (_runner == null) return;

     
        var splineTracker = _runner.GetComponent<SplineTracker>();
        var splineMover = _runner.GetComponent<SplineObjectMover>();
        if (splineTracker == null || !splineTracker.IsValid() || splineMover == null) return;

        float currentT = _runner.GetCurrentT();
        float speed = Mathf.Abs(distance);
        float direction = Mathf.Sign(distance);

     
        float newT = splineTracker.MoveAlongSpline(currentT, speed, 1f, direction);

       
        var moveResult = splineMover.MoveToSplinePosition(_rb, newT, currentT, direction);

        if (moveResult.success)
        {
         
            _runner.SetCurrentT(moveResult.finalT);

            if (enableDebugLogs)
                Debug.Log($"[AirFlowMovement] Moved from T={currentT:F3} to T={moveResult.finalT:F3}");
        }
        else if (enableDebugLogs)
        {
            Debug.LogWarning("[AirFlowMovement] SplineMover failed to move position");
        }
    }

   
    public void TriggerAirFlow(bool forward)
    {
        if (_airFlowMovementEnabled && CanUseAirFlow() && HasAirBlowerEquipped())
        {
        
            if (forward)
            {
              
                bool needReverse = !_lastMovementDirectionPositive;
                HandleAirFlowMovement(true, needReverse, 1f);
            }
            else
            {
           
                bool needReverse = _lastMovementDirectionPositive;
                HandleAirFlowMovement(true, needReverse, 1f);
            }

            ConsumePower();  
        }
    }

  
    private bool HasAirBlowerEquipped()
    {
        if (_equipmentManager != null && _equipmentManager.CurrentWeapon != null)
        {
            bool hasAirBlower = _equipmentManager.CurrentWeapon.WeaponId == "air_blower";

            //if (!hasAirBlower && enableDebugLogs && Time.frameCount % 60 == 0)  
            //{
            //    Debug.Log($"[AirFlowMovement] Current weapon is {_equipmentManager.CurrentWeapon.WeaponId}, air blower required");
            //}

            return hasAirBlower;
        }

        //if (enableDebugLogs && Time.frameCount % 60 == 0)
        //    Debug.Log("[AirFlowMovement] No weapon equipped");

        return false;
    }

    void OnDestroy()
    {
        EnableAirFlowMovement(false);
    }
}