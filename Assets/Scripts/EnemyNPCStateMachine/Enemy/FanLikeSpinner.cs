using UnityEngine;

public class FanLikeSpinner : MonoBehaviour
{
    public enum AxisOption { X, Y, Z, Custom }
    public enum Mode { Continuous, Intermittent }

    [Header("Axis")]
    public AxisOption axis = AxisOption.Y;
    public Vector3 customAxis = Vector3.up;     // 选择 Custom 时使用
    public bool useLocalAxis = true;            // true=围绕自身轴旋转，false=世界轴

    [Header("Rotation")]
    [Tooltip("峰值角速度(度/秒)")]
    public float maxSpeedDegPerSec = 360f;
    [Tooltip("true=顺时针(沿轴的右手系负方向), false=逆时针")]
    public bool clockwise = true;

    [Header("Mode")]
    public Mode mode = Mode.Intermittent;
    public bool playOnAwake = true;
    public bool useUnscaledTime = false;

    [Header("Intermittent Cycle (加速→保持→减速→停顿)")]
    [Tooltip("从0到最高速度所用时间")]
    public float rampUpTime = 1.0f;
    [Tooltip("保持最高速度的时间")]
    public float holdTime = 2.0f;
    [Tooltip("从最高速度降为0的时间")]
    public float rampDownTime = 1.0f;
    [Tooltip("完全停止的停顿时间")]
    public float idleTime = 0.5f;

    [Header("Speed Profile")]
    [Tooltip("用于加速/减速的速度曲线（0→1）。减速会使用反向曲线。")]
    public AnimationCurve rampCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // ---- runtime ----
    private float _currentSpeed;   // 当前角速度(度/秒，可为正负)
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

        // 计算目标轴向
        Vector3 axisVec = GetAxisVector();
        if (axisVec.sqrMagnitude < 1e-6f) axisVec = Vector3.up; // 兜底
        axisVec.Normalize();

        // 计算当前目标速度并更新状态机
        float sign = clockwise ? -1f : 1f; // Unity 的 Rotate 顺时针通常是负角度
        float peak = Mathf.Max(0f, maxSpeedDegPerSec) * sign;

        if (mode == Mode.Continuous)
        {
            // 连续模式：可选做一个平滑启停
            if (_spinning)
            {
                // 以 rampUpTime 作为“加速常数”让速度平滑逼近峰值
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
                            // 反向使用曲线：从1->0
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
                // 被 StopSpin() 后平滑降速到 0
                float decel = (rampDownTime > 0f) ? (Mathf.Abs(peak) / rampDownTime) : Mathf.Infinity;
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0f, decel * dt);
            }
        }

        // 应用旋转
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

    // -------- Public controls --------
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
        _spinning = false; // 连续模式会平滑降速；间歇模式走“被动减速到0”
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
