using UnityEngine;

public class AnimalPatrolState : AnimalNPCState
{
    private readonly int locomotionHash = Animator.StringToHash("Locomotion");
    private readonly int speedHash = Animator.StringToHash("Speed");
    private const float crossFadeDuration = 0.2f;
    private const float animationDampTime = 0.1f;
    private const float arrivalThreshold = 1.2f;

    private Transform currentTarget;
    private bool waitingAtPoint = false;
    private float waitTimer = 0f;
    private float currentWaitTime;
    private int consecutiveFailures = 0;
    private const int maxConsecutiveFailures = 3;

    public AnimalPatrolState(AnimalNPCStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        Debug.Log($"[{animalStateMachine.AnimalType}] 进入巡逻状态");

    
        if (animalStateMachine.Animator != null)
        {
            animalStateMachine.Animator.CrossFadeInFixedTime(locomotionHash, crossFadeDuration);
        }

     
        consecutiveFailures = 0;

       
        SelectNextPatrolPoint();
    }

    public override void Tick(float deltaTime)
    {
        if (waitingAtPoint)
        {
            HandleWaiting(deltaTime);
        }
        else
        {
            HandleMovement(deltaTime);
        }
    }

 
    private void HandleWaiting(float deltaTime)
    {
        waitTimer += deltaTime;

    
        if (animalStateMachine.Animator != null)
        {
            animalStateMachine.Animator.SetFloat(speedHash, 0f, animationDampTime, deltaTime);
        }

        if (waitTimer >= currentWaitTime)
        {
            TransitionToRandomIdleState();
        }
    }

 
    private void HandleMovement(float deltaTime)
    {
     
        if (currentTarget == null)
        {
            Debug.LogWarning($"[{animalStateMachine.AnimalType}] 当前目标为 null，重新选择");
            SelectNextPatrolPoint();
            return;
        }

        if (!animalStateMachine.Agent.isOnNavMesh)
        {
            Debug.LogError($"[{animalStateMachine.AnimalType}] Agent 不在 NavMesh 上！");
            consecutiveFailures++;

            if (consecutiveFailures >= maxConsecutiveFailures)
            {
                Debug.LogError($"[{animalStateMachine.AnimalType}] 连续失败 {maxConsecutiveFailures} 次，切换到 Idle");
                animalStateMachine.SwitchState(new AnimalIdleState(animalStateMachine));
            }
            return;
        }

        consecutiveFailures = 0;

    
        MoveToTarget(deltaTime);

     
        UpdateFacing();

      
        UpdateMovementAnimation(deltaTime);

        if (CheckArrival())
        {
            OnArrival();
        }
    }

   
    private void MoveToTarget(float deltaTime)
    {
      
        MoveToPosition(currentTarget.position, deltaTime);
    }


    private void UpdateFacing()
    {
     
        Vector3 velocity = animalStateMachine.Agent.desiredVelocity;
        if (velocity.sqrMagnitude > 0.01f)
        {
            FaceDirection(velocity);
        }
    }

    private void UpdateMovementAnimation(float deltaTime)
    {
        if (animalStateMachine.Animator == null) return;

   
        float currentSpeed = animalStateMachine.Agent.velocity.magnitude;
        float normalizedSpeed = Mathf.Clamp01(currentSpeed / animalStateMachine.MovementSpeed);

    
        animalStateMachine.Animator.SetFloat(speedHash, normalizedSpeed, animationDampTime, deltaTime);
    }

    private bool CheckArrival()
    {
        if (currentTarget == null) return false;
        if (animalStateMachine.Agent.pathPending) return false;


        float horizontalDistance = GetHorizontalDistanceToTarget(currentTarget.position);

    
        bool nearTarget = horizontalDistance <= arrivalThreshold;
        bool pathComplete = !animalStateMachine.Agent.hasPath ||
                           animalStateMachine.Agent.remainingDistance <= animalStateMachine.Agent.stoppingDistance;

        return nearTarget && pathComplete;
    }

 
    private void OnArrival()
    {
        Debug.Log($"[{animalStateMachine.AnimalType}] 到达巡逻点: {currentTarget.name}");

        waitingAtPoint = true;
        waitTimer = 0f;
        currentWaitTime = Random.Range(1f, 3f);

       
        StopMovement();
    }

    public override void FixedTick(float fixedDeltaTime)
    {
       
    }

    public override void Exit()
    {
        StopMovement();
        Debug.Log($"[{animalStateMachine.AnimalType}] 退出巡逻状态");
    }

    private void SelectNextPatrolPoint()
    {
        currentTarget = animalStateMachine.GetRandomPatrolPoint();
        waitingAtPoint = false;

        if (currentTarget != null)
        {
            Debug.Log($"[{animalStateMachine.AnimalType}] 前往巡逻点: {currentTarget.name}");
        }
        else
        {
            Debug.LogError($"[{animalStateMachine.AnimalType}] 无法获取有效的巡逻点！切换到 Idle");
            animalStateMachine.SwitchState(new AnimalIdleState(animalStateMachine));
        }
    }

 
    private void TransitionToRandomIdleState()
    {
     
        if (Random.value > 0.8f)
        {
            Debug.Log($"[{animalStateMachine.AnimalType}] 切换到 Eating 状态");
            animalStateMachine.SwitchState(new AnimalEatingState(animalStateMachine));
        }
        else
        {
            Debug.Log($"[{animalStateMachine.AnimalType}] 切换到 Idle 状态");
            animalStateMachine.SwitchState(new AnimalIdleState(animalStateMachine));
        }
    }
}