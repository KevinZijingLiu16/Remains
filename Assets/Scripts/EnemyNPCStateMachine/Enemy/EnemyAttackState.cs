using UnityEngine;

public class EnemyAttackState : EnemyState
{
    private float _attackCooldownTimer = 0f;

    public EnemyAttackState(EnemyStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        if (stateMachine.EnableDebugLogs)
        {
            Debug.Log("[EnemyAttackState] Entered attack state");
        }
        _attackCooldownTimer = 0f;

       
        stateMachine.StopMovement();
    }

    public override void Tick(float deltaTime)
    {
        if (stateMachine.IsStunned) return;

        if (!stateMachine.CanDetectDirtyPlayer())
        {
            stateMachine.SwitchState(stateMachine.CreateStateFromFactory("Patrol"));
            return;
        }

        if (!stateMachine.IsPlayerInAttackRange())
        {
            stateMachine.SwitchState(stateMachine.CreateStateFromFactory("Chasing"));
            return;
        }

        _attackCooldownTimer += deltaTime;
        if (_attackCooldownTimer >= stateMachine.AttackCooldown)
        {
            PerformAttack();
            _attackCooldownTimer = 0f;
        }

      
        if (stateMachine.playerTransform != null && stateMachine.NavAgent != null)
        {
            Vector3 direction = (stateMachine.playerTransform.position - stateMachine.transform.position).normalized;
            direction.y = 0; 

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                stateMachine.transform.rotation = Quaternion.Slerp(
                    stateMachine.transform.rotation,
                    targetRotation,
                    stateMachine.RotationSpeed * deltaTime
                );
            }
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
            Debug.Log("[EnemyAttackState] Exited attack state");
        }
    }
    private void PerformAttack()
    {
        if (stateMachine.EnableDebugLogs)
        {
            Debug.Log("[EnemyAttackState] Performing attack!");
        }

        if (stateMachine.AttackHitbox != null)
        {
            Collider[] hits = Physics.OverlapSphere(
                stateMachine.AttackHitbox.position,
                stateMachine.AttackRange,
                LayerMask.GetMask("Player")
            );

            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    var dirtSystem = hit.GetComponent<PlayerDirtSystem>();
                    if (dirtSystem != null && dirtSystem.IsAnyDirty)
                    {
                    
                        dirtSystem.RemoveDirtFromRandom(2, 1f); //(how many body parts, how much to clean for each part)
                                                                  
                    }

                    var playerHealth = hit.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(1);
                    }

                    if (stateMachine.EnableDebugLogs)
                    {
                        Debug.Log("[EnemyAttackState] Hit player and cleaned dirt!");
                    }
                    break;
                }
            }
        }
    }
}