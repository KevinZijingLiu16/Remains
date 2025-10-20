using UnityEngine;

public interface ITriggerable
{
    Transform Transform { get; }
    void OnTriggered(StateTransitionConfig config);
    bool CanBeTriggered(StateTransitionConfig config);
}