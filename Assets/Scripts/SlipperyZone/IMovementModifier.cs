using UnityEngine;

public interface IMovementModifier 
{
    void ApplyModification(SplineRunnerRB runner);
    void RemoveModification(SplineRunnerRB runner);
}
