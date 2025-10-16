using UnityEngine;

public class EnemyStateFactory : IStateFactory
{
    private EnemyStateMachine _stateMachine;

    public EnemyStateFactory(EnemyStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
    }

    public State CreateState(string stateName)
    {
        switch (stateName.ToLower())
        {
            case "patrol":
                return new EnemyPatrolState(_stateMachine);
            case "idle":
                return new EnemyIdleState(_stateMachine);
            case "chasing":
                return new EnemyChasingState(_stateMachine);
            case "attack":
                return new EnemyAttackState(_stateMachine);
            default:
                Debug.LogWarning($"[EnemyStateFactory] Unknown state: {stateName}");
                return new EnemyIdleState(_stateMachine);
        }
    }
}