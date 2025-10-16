using UnityEngine;

public class EnemyChasingState : EnemyState
{
    public EnemyChasingState(EnemyStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        if (stateMachine.EnableDebugLogs)
        {
            Debug.Log("[EnemyChasingState] Entered chasing state");
        }
    }

    public override void Tick(float deltaTime)
    {
        if (stateMachine.IsStunned) return;

        if (!stateMachine.CanDetectDirtyPlayer())
        {
            stateMachine.SwitchState(stateMachine.CreateStateFromFactory("Patrol"));
            return;
        }

        if (stateMachine.IsPlayerInAttackRange())
        {
            stateMachine.SwitchState(stateMachine.CreateStateFromFactory("Attack"));
            return;
        }

       
        if (stateMachine.playerTransform != null && stateMachine.NavAgent != null)
        {
            stateMachine.NavAgent.SetDestination(stateMachine.playerTransform.position);
        }
    }

    public override void FixedTick(float fixedDeltaTime)
    {
        
    }


    public override void Exit()
    {
       
        if (stateMachine.NavAgent != null && stateMachine.NavAgent.enabled)
        {
            stateMachine.NavAgent.ResetPath();
        }

        if (stateMachine.EnableDebugLogs)
        {
            Debug.Log("[EnemyChasingState] Exited chasing state");
        }
    }
}