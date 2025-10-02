using UnityEngine;

[System.Serializable]
public class SlipperyMovementModifier : IMovementModifier
{
    [Header("Slippery Parameters")]
    public float moveSpeedMultiplier = 0.1f;
    public float airControlModifier = 0.05f;
    public float swayAmplitude = 5f;
    public float swayFrequency = 2f;

    private float _originalMoveSpeed;
    private float _originalAirControl;
    private float _originalMeshOffset;

    public void ApplyModification(SplineRunnerRB runner)
    {
      
        _originalMoveSpeed = runner.GetMoveSpeed();
        _originalAirControl = runner.airControl;
        _originalMeshOffset = runner.meshFacingOffsetY;

       
        runner.SetMoveSpeed(_originalMoveSpeed * moveSpeedMultiplier);
        runner.airControl = airControlModifier;

        Debug.Log("[SlipperyZone] Applied slippery movement modifier");
    }

    public void RemoveModification(SplineRunnerRB runner)
    {
    
        runner.SetMoveSpeed(_originalMoveSpeed);
        runner.airControl = _originalAirControl;
        runner.meshFacingOffsetY = _originalMeshOffset;

        Debug.Log("[SlipperyZone] Removed slippery movement modifier");
    }

    public void UpdateSwayEffect(SplineRunnerRB runner)
    {
        float sway = Mathf.Sin(Time.time * swayFrequency) * swayAmplitude;
        runner.meshFacingOffsetY = _originalMeshOffset + sway;
    }
}