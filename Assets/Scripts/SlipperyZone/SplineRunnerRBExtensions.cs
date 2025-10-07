public static class SplineRunnerRBExtensions
{
    public static float GetMoveSpeed(this SplineRunnerRB runner)
    {
        var field = typeof(SplineRunnerRB).GetField("moveSpeed",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field != null ? (float)field.GetValue(runner) : 6f;
    }

    public static void SetMoveSpeed(this SplineRunnerRB runner, float speed)
    {
        var field = typeof(SplineRunnerRB).GetField("moveSpeed",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(runner, speed);
    }

    public static void SetCurrentT(this SplineRunnerRB runner, float t)
    {
        var field = typeof(SplineRunnerRB).GetField("_t",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(runner, t);
    }
}