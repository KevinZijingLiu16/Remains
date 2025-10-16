using UnityEngine;

public class EnemyIdleState : EnemyState
{
    public EnemyIdleState(EnemyStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        if (stateMachine.EnableDebugLogs)
        {
            Debug.Log("[EnemyIdleState] Entered idle state");
        }

    
        stateMachine.StopMovement();
    }

    public override void Tick(float deltaTime)
    {
       
        if (stateMachine.IsStunned) return;

     
        if (stateMachine.CanDetectDirtyPlayer())
        {
            stateMachine.SwitchState(stateMachine.CreateStateFromFactory("Chasing"));
            return;
        }
    }

    public override void FixedTick(float fixedDeltaTime)
    {
       
    }

    public override void Exit()
    {
     
        stateMachine.ResumeMovement();

        if (stateMachine.EnableDebugLogs)
        {
            Debug.Log("[EnemyIdleState] Exited idle state");
        }
    }
}