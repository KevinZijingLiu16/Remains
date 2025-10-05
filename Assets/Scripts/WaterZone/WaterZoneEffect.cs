using UnityEngine;

public class WaterZoneEffect : MonoBehaviour, IZoneEffect
{
    [Header("Movement Overrides")]

    public float moveSpeedMultiplier = 0.4f;

  
    public float jumpHeightMultiplier = 1.6f;

 
    public bool overrideAirControl = true;
    [Range(0f, 1f)] public float airControlInWater = 0.85f;

    [Header("Optional Rigidbody Damping")]
    public bool applyWaterDamping = true;
    public float waterLinearDamping = 2.0f;
    public float waterAngularDamping = 2.0f;

    [Header("Thrust")]
   
    public bool enableBlowerThrustInWater = true;

    [Header("Debug")]
    public bool enableDebugLogs = true;

 
    private SplineRunnerRB _runner;
    private Rigidbody _rb;
    private PlayerAirBlowerMovementInWaterController _blower;

  
    private float _origMoveSpeed;
    private float _origJumpHeight;
    private float _origAirControl;
    private float _origLinearDamping;
    private float _origAngularDamping;

    private bool _active;

    public void OnPlayerEnter(GameObject player)
    {
        _runner = player.GetComponent<SplineRunnerRB>();
        _rb = player.GetComponent<Rigidbody>();
        _blower = player.GetComponent<PlayerAirBlowerMovementInWaterController>();

        if (_runner == null || _rb == null)
        {
            if (enableDebugLogs) Debug.LogWarning("[WaterZone] Missing SplineRunnerRB or Rigidbody on player.");
            return;
        }

        
        _origMoveSpeed = _runner.GetMoveSpeed();
        _origJumpHeight = _runner.GetJumpHeight();
        _origAirControl = _runner.airControl;
        _origLinearDamping = _rb.linearDamping;
        _origAngularDamping = _rb.angularDamping;

   
        _runner.SetMoveSpeed(_origMoveSpeed * moveSpeedMultiplier);
        _runner.SetJumpHeight(_origJumpHeight * jumpHeightMultiplier);
        if (overrideAirControl) _runner.airControl = airControlInWater;

        if (applyWaterDamping)
        {
            _rb.linearDamping = waterLinearDamping;
            _rb.angularDamping = waterAngularDamping;
        }

        if (enableBlowerThrustInWater && _blower != null)
            _blower.enabled = true;  

        _active = true;
        if (enableDebugLogs) Debug.Log("[WaterZone] Applied water movement overrides.");
    }

    public void OnPlayerExit(GameObject player)
    {
        if (!_active || _runner == null || _rb == null) return;


        _runner.SetMoveSpeed(_origMoveSpeed);
        _runner.SetJumpHeight(_origJumpHeight);
        _runner.airControl = _origAirControl;

        if (applyWaterDamping)
        {
            _rb.linearDamping = _origLinearDamping;
            _rb.angularDamping = _origAngularDamping;
        }

        if (_blower != null)
            _blower.enabled = false;  

        _active = false;
        _runner = null; _rb = null; _blower = null;

        if (enableDebugLogs) Debug.Log("[WaterZone] Restored movement & disabled blower.");
    }

    public void UpdateEffect(GameObject player)
    {
      
    }
}
