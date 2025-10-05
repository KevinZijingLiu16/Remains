using System.Reflection;
using UnityEngine;

public static class SplineRunnerRBWaterExtensions
{
    static FieldInfo _jumpHeightFI;
    static FieldInfo JumpHeightFI
    {
        get
        {
            if (_jumpHeightFI == null)
                _jumpHeightFI = typeof(SplineRunnerRB)
                    .GetField("jumpHeight", BindingFlags.NonPublic | BindingFlags.Instance);
            return _jumpHeightFI;
        }
    }

    public static float GetJumpHeight(this SplineRunnerRB runner)
    {
        return (JumpHeightFI != null) ? (float)JumpHeightFI.GetValue(runner) : 2f;
    }

    public static void SetJumpHeight(this SplineRunnerRB runner, float h)
    {
        JumpHeightFI?.SetValue(runner, h);
    }
}
