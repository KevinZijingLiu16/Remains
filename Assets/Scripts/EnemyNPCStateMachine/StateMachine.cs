using UnityEngine;

public abstract class StateMachine : MonoBehaviour
{
    protected State currentState;

    public State CurrentState => currentState;

    public void SwitchState(State newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    protected virtual void Update()
    {
        currentState?.Tick(Time.deltaTime);
    }

    protected virtual void FixedUpdate()
    {
        currentState?.FixedTick(Time.fixedDeltaTime);
    }
}