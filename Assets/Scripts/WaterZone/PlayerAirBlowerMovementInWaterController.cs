using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class PlayerAirBlowerMovementInWaterController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private SplineRunnerRB runner;
    [SerializeField] private SplineTracker tracker;
    [SerializeField] private SplineObjectMover mover;
    [SerializeField] private WeaponEquipmentManager equipmentManager;
    [SerializeField] private WeaponInputProcessor inputProcessor;
    [SerializeField] private ArmAimer armAimer;
    [SerializeField] private PlayerPower playerPower;


    [SerializeField] private Transform forwardBasis;

    [Header("Along-Spline (T)")]
  
    public float maxAlongSpeed = 8f;
    public bool smoothAlong = true;
    public float alongLerp = 12f;             
    [Range(0f, 1f)] public float alongDeadzone = 0.08f; 

    [Header("Vertical (Y)")]
  
    public float verticalForce = 15f;
    public bool smoothVertical = true;
    public float verticalLerp = 16f;
  
    public float maxVerticalSpeed = 6f;
    [Range(0f, 1f)] public float verticalDeadzone = 0.05f;

    [Header("Power Gate")]
  
    public bool requirePower = true;
 
    public float powerCostPerSecond = 0f;

    [Header("Gating")]

    public bool onlyWhenAirBlowerEquipped = true;

    [Header("Debug")]
    public bool enableDebugLogs = true;
    public int logEveryNFrames = 20;


    private Rigidbody _rb;
    private bool _primaryHeld, _secondaryHeld, _airBlowerEquipped;
    private float _alongAbs;         
    private float _verticalFactor;  
    private float _powerAccu;        
    void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        if (!runner) runner = GetComponent<SplineRunnerRB>();
        if (!tracker) tracker = GetComponent<SplineTracker>();
        if (!mover) mover = GetComponent<SplineObjectMover>();
        if (!equipmentManager) equipmentManager = GetComponent<WeaponEquipmentManager>();
        if (!inputProcessor) inputProcessor = GetComponent<WeaponInputProcessor>();
        if (!armAimer) armAimer = GetComponentInChildren<ArmAimer>();
        if (!playerPower) playerPower = GetComponent<PlayerPower>();

    
        if (!forwardBasis && equipmentManager && equipmentManager.weaponEquipPoint)
            forwardBasis = equipmentManager.weaponEquipPoint;
        if (!forwardBasis)
            forwardBasis = transform;
    }

    void OnEnable()
    {
        if (equipmentManager)
        {
            equipmentManager.OnWeaponEquipped += OnWeaponEquipped;
            equipmentManager.OnWeaponUnequipped += OnWeaponUnequipped;
            _airBlowerEquipped = equipmentManager.CurrentWeapon != null &&
                                 equipmentManager.CurrentWeapon.WeaponId == "air_blower";
        }
        if (inputProcessor)
        {
            inputProcessor.OnPrimaryAttackStart += OnPrimaryStart;
            inputProcessor.OnPrimaryAttackStop += OnPrimaryStop;
            inputProcessor.OnSecondaryAttackStart += OnSecondaryStart;
            inputProcessor.OnSecondaryAttackStop += OnSecondaryStop;
        }
    }

    void OnDisable()
    {
        if (equipmentManager)
        {
            equipmentManager.OnWeaponEquipped -= OnWeaponEquipped;
            equipmentManager.OnWeaponUnequipped -= OnWeaponUnequipped;
        }
        if (inputProcessor)
        {
            inputProcessor.OnPrimaryAttackStart -= OnPrimaryStart;
            inputProcessor.OnPrimaryAttackStop -= OnPrimaryStop;
            inputProcessor.OnSecondaryAttackStart -= OnSecondaryStart;
            inputProcessor.OnSecondaryAttackStop -= OnSecondaryStop;
        }

        _primaryHeld = _secondaryHeld = false;
        _alongAbs = 0f;
        _verticalFactor = 0f;
        _powerAccu = 0f;
    }

    void FixedUpdate()
    {
        if (!runner || !tracker || !mover || !tracker.IsValid()) return;

        if (onlyWhenAirBlowerEquipped && !_airBlowerEquipped)
        {
            DecayToZero();
            ApplyAlong(0f);
            ApplyVertical();
            return;
        }

        bool hasPower = !requirePower || (playerPower != null && playerPower.Current > 0);
        if (!hasPower)
        {
     
            DecayToZero();
            ApplyAlong(0f);
            ApplyVertical();
            _powerAccu = 0f;
            return;
        }

        if (!_primaryHeld && !_secondaryHeld)
        {
       
            DecayToZero();
            ApplyAlong(0f);
            ApplyVertical();
            _powerAccu = 0f;
            return;
        }

      
        float pitch = armAimer ? armAimer.GetCurrentOffset() : -90f; 
        float alpha = pitch + 90f;    
        float rad = alpha * Mathf.Deg2Rad;
        float cosA = Mathf.Cos(rad); 
        float sinA = Mathf.Sin(rad);  

        float alongRel =
            (_primaryHeld && !_secondaryHeld) ? -cosA :
            (_secondaryHeld && !_primaryHeld) ? cosA : 0f;


        float vertRaw =
            (_primaryHeld && !_secondaryHeld) ? sinA :
            (_secondaryHeld && !_primaryHeld) ? -sinA : 0f;

    
        if (Mathf.Abs(alongRel) < alongDeadzone) alongRel = 0f;
        if (Mathf.Abs(vertRaw) < verticalDeadzone) vertRaw = 0f;

  
        float targetAlongAbs = Mathf.Clamp01(Mathf.Abs(alongRel)) * maxAlongSpeed;
        _alongAbs = smoothAlong
            ? Mathf.Lerp(_alongAbs, targetAlongAbs, 1f - Mathf.Exp(-alongLerp * Time.fixedDeltaTime))
            : targetAlongAbs;

        float targetVert = Mathf.Clamp(vertRaw, -1f, 1f);
        _verticalFactor = smoothVertical
            ? Mathf.Lerp(_verticalFactor, targetVert, 1f - Mathf.Exp(-verticalLerp * Time.fixedDeltaTime))
            : targetVert;

   
        ApplyAlong(alongRel);
        ApplyVertical();

 
        if (powerCostPerSecond > 0f && playerPower != null)
        {
            _powerAccu += powerCostPerSecond * Time.fixedDeltaTime;
            if (_powerAccu >= 1f)
            {
                int cost = Mathf.FloorToInt(_powerAccu);
                if (cost > 0) playerPower.ModifyPower(-cost);
                _powerAccu -= cost;
            }
        }
        else
        {
            _powerAccu = 0f;
        }

        if (enableDebugLogs && Time.frameCount % Mathf.Max(1, logEveryNFrames) == 0)
        {
            Debug.Log($"[AirBlowerAimSplineAndVertical] pitch={pitch:F1}¡ã, cosA={cosA:F2}, sinA={sinA:F2}, " +
                      $"alongRel={alongRel:F2}, alongAbs={_alongAbs:F2}, vert={_verticalFactor:F2}, " +
                      $"power={(playerPower ? playerPower.Current : -1)}");
        }
    }


    private void ApplyAlong(float alongRelSign)
    {

        Vector3 fwd = forwardBasis.forward; fwd.y = 0f; if (fwd.sqrMagnitude > 1e-6f) fwd.Normalize();
        Vector3 tan = tracker.GetWorldTangentAtT(runner.GetCurrentT()); tan.y = 0f; if (tan.sqrMagnitude > 1e-6f) tan.Normalize();

        float forwardVsSplineSign = Vector3.Dot(fwd, tan) >= 0f ? 1f : -1f;
        float sign = Mathf.Sign(alongRelSign);
        float signedSpeed = _alongAbs * sign * forwardVsSplineSign; // Ã×/Ãë

        if (Mathf.Abs(signedSpeed) < 1e-5f) return;

        float t0 = runner.GetCurrentT();
        float t1 = tracker.MoveAlongSpline(t0, Mathf.Abs(signedSpeed), Time.fixedDeltaTime, Mathf.Sign(signedSpeed));
        var move = mover.MoveToSplinePosition(_rb, t1, t0, Mathf.Sign(signedSpeed));
        if (move.success) runner.SetCurrentT(move.finalT);
    }


    private void ApplyVertical()
    {
        if (Mathf.Abs(_verticalFactor) > 1e-5f)
        {
            _rb.AddForce(Vector3.up * (_verticalFactor * verticalForce), ForceMode.Force);

       
            var v = _rb.linearVelocity;
            v.y = Mathf.Clamp(v.y, -maxVerticalSpeed, maxVerticalSpeed);
            _rb.linearVelocity = v;
        }
    }

    private void DecayToZero()
    {
        _alongAbs = smoothAlong
            ? Mathf.Lerp(_alongAbs, 0f, 1f - Mathf.Exp(-alongLerp * Time.fixedDeltaTime))
            : 0f;

        _verticalFactor = smoothVertical
            ? Mathf.Lerp(_verticalFactor, 0f, 1f - Mathf.Exp(-verticalLerp * Time.fixedDeltaTime))
            : 0f;
    }

    #region Events
    private void OnPrimaryStart() { _primaryHeld = true; _secondaryHeld = false; }
    private void OnPrimaryStop() { _primaryHeld = false; }
    private void OnSecondaryStart() { _secondaryHeld = true; _primaryHeld = false; }
    private void OnSecondaryStop() { _secondaryHeld = false; }

    private void OnWeaponEquipped(IWeapon w)
    {
        _airBlowerEquipped = (w != null && w.WeaponId == "air_blower");
    }
    private void OnWeaponUnequipped(IWeapon w)
    {
        _airBlowerEquipped = false;
        _primaryHeld = _secondaryHeld = false;
        _alongAbs = 0f;
        _verticalFactor = 0f;
        _powerAccu = 0f;
    }
    #endregion

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!enabled) return;
        float pitch = armAimer ? armAimer.GetCurrentOffset() : -90f;
        float alpha = pitch + 90f;
        float rad = alpha * Mathf.Deg2Rad;
        Vector3 fwd = (forwardBasis ? forwardBasis : transform).forward; fwd.y = 0f; fwd.Normalize();
        Vector3 aim = (fwd * Mathf.Cos(rad)) + (Vector3.up * -Mathf.Sin(rad));
        Vector3 origin = transform.position + Vector3.up * 1.2f;
        Gizmos.color = Color.cyan; Gizmos.DrawRay(origin, aim.normalized * 2f);   
        Gizmos.color = Color.yellow; Gizmos.DrawRay(origin, -aim.normalized * 2f);  
    }
#endif
}
