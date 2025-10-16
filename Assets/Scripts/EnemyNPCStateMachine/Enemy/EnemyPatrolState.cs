using UnityEngine;

public class EnemyPatrolState : EnemyState
{
    private int _currentPatrolIndex = 0;
    private float _waitTimer = 0f;
    private bool _waiting = false;
    private bool _hasSetDestination = false; 

    public EnemyPatrolState(EnemyStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        if (stateMachine.EnableDebugLogs)
        {
            Debug.Log("[EnemyPatrolState] Entered patrol state");
        }

        _currentPatrolIndex = FindNearestPatrolPoint();
        _waiting = false;
        _hasSetDestination = false; 

    
        if (stateMachine.NavAgent != null)
        {
            stateMachine.NavAgent.isStopped = false;
        }
    }

    public override void Tick(float deltaTime)
    {
       
        if (stateMachine.IsStunned) return;

       
        if (stateMachine.CanDetectDirtyPlayer())
        {
            stateMachine.SwitchState(stateMachine.CreateStateFromFactory("Chasing"));
            return;
        }

        
        if (stateMachine.PatrolPoints == null || stateMachine.PatrolPoints.Length == 0)
        {
           
            if (stateMachine.EnableDebugLogs)
            {
                Debug.LogWarning("[EnemyPatrolState] No patrol points set, switching to Idle");
            }
            stateMachine.SwitchState(stateMachine.CreateStateFromFactory("Idle"));
            return;
        }

        Transform targetPoint = stateMachine.PatrolPoints[_currentPatrolIndex];
        if (targetPoint == null)
        {
            if (stateMachine.EnableDebugLogs)
            {
                Debug.LogWarning($"[EnemyPatrolState] Patrol point {_currentPatrolIndex} is null");
            }
            return;
        }

     
        bool hasReached = false;
        if (stateMachine.NavAgent != null && stateMachine.NavAgent.enabled && stateMachine.NavAgent.isOnNavMesh)
        {
        
            if (_hasSetDestination && !stateMachine.NavAgent.pathPending)
            {
                hasReached = stateMachine.NavAgent.remainingDistance <= stateMachine.NavAgent.stoppingDistance;

                if (stateMachine.EnableDebugLogs)
                {
                    Debug.Log($"[EnemyPatrolState] Remaining distance: {stateMachine.NavAgent.remainingDistance:F2}, Stopping distance: {stateMachine.NavAgent.stoppingDistance:F2}");
                }
            }
        }
        else
        {
           
            float distance = Vector3.Distance(stateMachine.transform.position, targetPoint.position);
            hasReached = distance <= 0.5f;

            if (stateMachine.EnableDebugLogs && stateMachine.NavAgent != null && !stateMachine.NavAgent.isOnNavMesh)
            {
                Debug.LogWarning("[EnemyPatrolState] NavAgent is not on NavMesh!");
            }
        }

        if (!hasReached && !_waiting)
        {
         
            MoveTowards(targetPoint.position, deltaTime);
        }
        else if (!_waiting)
        {
        
            _waiting = true;
            _waitTimer = 0f;

            if (stateMachine.EnableDebugLogs)
            {
                Debug.Log($"[EnemyPatrolState] Reached patrol point {_currentPatrolIndex}, waiting...");
            }
        }

        if (_waiting)
        {
            _waitTimer += deltaTime;
            if (_waitTimer >= stateMachine.PatrolWaitTime)
            {
  
                _currentPatrolIndex = (_currentPatrolIndex + 1) % stateMachine.PatrolPoints.Length;
                _waiting = false;
                _hasSetDestination = false; 

                if (stateMachine.EnableDebugLogs)
                {
                    Debug.Log($"[EnemyPatrolState] Moving to next patrol point: {_currentPatrolIndex}");
                }
            }
        }
    }

    public override void FixedTick(float fixedDeltaTime)
    {
       
    }

    public override void Exit()
    {
       
        if (stateMachine.NavAgent != null && stateMachine.NavAgent.enabled && stateMachine.NavAgent.isOnNavMesh)
        {
            stateMachine.NavAgent.ResetPath();
        }

        if (stateMachine.EnableDebugLogs)
        {
            Debug.Log("[EnemyPatrolState] Exited patrol state");
        }
    }

    private int FindNearestPatrolPoint()
    {
        if (stateMachine.PatrolPoints == null || stateMachine.PatrolPoints.Length == 0)
            return 0;

        int nearestIndex = 0;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < stateMachine.PatrolPoints.Length; i++)
        {
            if (stateMachine.PatrolPoints[i] == null) continue;

            float distance = Vector3.Distance(stateMachine.transform.position, stateMachine.PatrolPoints[i].position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }

        if (stateMachine.EnableDebugLogs)
        {
            Debug.Log($"[EnemyPatrolState] Nearest patrol point: {nearestIndex}");
        }

        return nearestIndex;
    }

    private void MoveTowards(Vector3 target, float deltaTime)
    {
        if (stateMachine.NavAgent != null && stateMachine.NavAgent.enabled && stateMachine.NavAgent.isOnNavMesh)
        {
            stateMachine.NavAgent.SetDestination(target);
            _hasSetDestination = true; 

            if (stateMachine.EnableDebugLogs)
            {
                Debug.Log($"[EnemyPatrolState] Setting NavAgent destination to {target}");
            }
        }
        else
        {
           
            Vector3 direction = (target - stateMachine.transform.position).normalized;
            stateMachine.transform.position += direction * stateMachine.MoveSpeed * deltaTime;

            if (stateMachine.EnableDebugLogs)
            {
                Debug.LogWarning("[EnemyPatrolState] Using fallback movement (NavAgent unavailable)");
            }
        }
    }
}