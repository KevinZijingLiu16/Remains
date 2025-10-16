using UnityEngine;

public abstract class EnemyState : State
{
    protected EnemyStateMachine stateMachine;

    public EnemyState(EnemyStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }
}
