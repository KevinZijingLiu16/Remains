using UnityEngine;

public class FanLikeSpinner : MonoBehaviour
{
    public enum AxisOption { X, Y, Z, Custom }
    public enum Mode { Continuous, Intermittent }

    [Header("Axis")]
    public AxisOption axis = AxisOption.Y;
    public Vector3 customAxis = Vector3.up;     
    public bool useLocalAxis = true;            

    [Header("Rotation")]
 
    public float maxSpeedDegPerSec = 360f;

    public bool clockwise = true;

    [Header("Mode")]
    public Mode mode = Mode.Intermittent;
    public bool playOnAwake = true;
    public bool useUnscaledTime = false;


   
    public float rampUpTime = 1.0f;

    public float holdTime = 2.0f;
  
    public float rampDownTime = 1.0f;
  
    public float idleTime = 0.5f;

    [Header("Speed Profile")]
    [Tooltip("用于加速/减速的速度曲线（0→1）。减速会使用反向曲线。")]
    public AnimationCurve rampCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // ---- runtime ----
    private float _currentSpeed;   
    private bool _spinning;
    private Phase _phase = Phase.Idle;
    private float _phaseTimer;

    private enum Phase { Idle, RampUp, Hold, RampDown }

    void OnEnable()
    {
        if (playOnAwake) StartSpin();
    }

    void Update()
    {
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        if (dt <= 0f) return;

        Vector3 axisVec = GetAxisVector();
        if (axisVec.sqrMagnitude < 1e-6f) axisVec = Vector3.up; // 兜底
        axisVec.Normalize();

  
        float sign = clockwise ? -1f : 1f; 
        float peak = Mathf.Max(0f, maxSpeedDegPerSec) * sign;

        if (mode == Mode.Continuous)
        {
            
            if (_spinning)
            {
                
                float accel = (rampUpTime > 0f) ? (Mathf.Abs(peak) / rampUpTime) : Mathf.Infinity;
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, peak, accel * dt);
            }
            else
            {
                float decel = (rampDownTime > 0f) ? (Mathf.Abs(peak) / rampDownTime) : Mathf.Infinity;
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0f, decel * dt);
            }
        }
        else // Intermittent
        {
            if (_spinning)
            {
                _phaseTimer += dt;
                switch (_phase)
                {
                    case Phase.RampUp:
                        {
                            float t = (rampUpTime <= 0f) ? 1f : Mathf.Clamp01(_phaseTimer / rampUpTime);
                            float k = rampCurve.Evaluate(t);
                            _currentSpeed = Mathf.Lerp(0f, Mathf.Abs(peak), k) * Mathf.Sign(peak);
                            if (t >= 1f) { _phase = Phase.Hold; _phaseTimer = 0f; }
                        }
                        break;

                    case Phase.Hold:
                        _currentSpeed = peak;
                        if (_phaseTimer >= holdTime)
                        {
                            _phase = Phase.RampDown;
                            _phaseTimer = 0f;
                        }
                        break;

                    case Phase.RampDown:
                        {
                            float t = (rampDownTime <= 0f) ? 1f : Mathf.Clamp01(_phaseTimer / rampDownTime);
                         
                            float k = rampCurve.Evaluate(1f - t);
                            _currentSpeed = Mathf.Lerp(0f, Mathf.Abs(peak), k) * Mathf.Sign(peak);
                            if (t >= 1f) { _phase = Phase.Idle; _phaseTimer = 0f; _currentSpeed = 0f; }
                        }
                        break;

                    case Phase.Idle:
                        _currentSpeed = 0f;
                        if (_phaseTimer >= idleTime)
                        {
                            _phase = Phase.RampUp;
                            _phaseTimer = 0f;
                        }
                        break;
                }
            }
            else
            {
             
                float decel = (rampDownTime > 0f) ? (Mathf.Abs(peak) / rampDownTime) : Mathf.Infinity;
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0f, decel * dt);
            }
        }

        float deltaAngle = _currentSpeed * dt;
        if (useLocalAxis) transform.Rotate(axisVec, deltaAngle, Space.Self);
        else transform.Rotate(axisVec, deltaAngle, Space.World);
    }

    Vector3 GetAxisVector()
    {
        switch (axis)
        {
            case AxisOption.X: return Vector3.right;
            case AxisOption.Y: return Vector3.up;
            case AxisOption.Z: return Vector3.forward;
            case AxisOption.Custom: return customAxis;
            default: return Vector3.up;
        }
    }

   
    public void StartSpin()
    {
        _spinning = true;
        if (mode == Mode.Intermittent)
        {
            _phase = Phase.RampUp;
            _phaseTimer = 0f;
        }
    }

    public void StopSpin()
    {
        _spinning = false; 
    }

    public void ToggleDirection() => clockwise = !clockwise;

    public void SetDirection(bool cw) => clockwise = cw;

    public void SetContinuous(bool continuous)
    {
        mode = continuous ? Mode.Continuous : Mode.Intermittent;
        if (mode == Mode.Intermittent)
        {
            _phase = Phase.RampUp;
            _phaseTimer = 0f;
        }
    }
}
