using UnityEngine;

public class AnimalRunningState : AnimalNPCState
{
    private readonly int runHash = Animator.StringToHash("Run");
    private readonly int speedHash = Animator.StringToHash("Speed");
    private const float crossFadeDuration = 0.1f;
    private const float animationDampTime = 0.1f;
    private const float arrivalThreshold = 1.5f;

    private Transform fleeTarget;
    private float originalSpeed;

    public AnimalRunningState(AnimalNPCStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {

        if (animalStateMachine.Animator != null)
        {
            animalStateMachine.Animator.CrossFadeInFixedTime(runHash, crossFadeDuration);
        }

        originalSpeed = animalStateMachine.Agent.speed;
        animalStateMachine.Agent.speed = animalStateMachine.FleeSpeed;

        fleeTarget = animalStateMachine.GetNearestFleePoint();

        if (fleeTarget == null)
        {
            TransitionToSafeState();
        }
    }

    public override void Tick(float deltaTime)
    {
        if (fleeTarget == null)
        {
            TransitionToSafeState();
            return;
        }

        if (!animalStateMachine.Agent.isOnNavMesh)
        {
            TransitionToSafeState();
            return;
        }

        MoveToPosition(fleeTarget.position, deltaTime);

        Vector3 velocity = animalStateMachine.Agent.desiredVelocity;
        if (velocity.sqrMagnitude > 0.01f)
        {
            FaceDirection(velocity);
        }

        if (animalStateMachine.Animator != null)
        {
            float currentSpeed = animalStateMachine.Agent.velocity.magnitude;
            float normalizedSpeed = Mathf.Clamp01(currentSpeed / animalStateMachine.FleeSpeed);

            float animationSpeed = normalizedSpeed * 2f;
            animalStateMachine.Animator.SetFloat(speedHash, animationSpeed, animationDampTime, deltaTime);
        }

        
        if (CheckArrival())
        {
            TransitionToSafeState();
        }
    }

    public override void FixedTick(float fixedDeltaTime)
    {
    }

    public override void Exit()
    {
        if (animalStateMachine.Agent != null)
        {
            animalStateMachine.Agent.speed = originalSpeed;
        }

        StopMovement();
    }


    private bool CheckArrival()
    {
        if (fleeTarget == null) return true;
        if (animalStateMachine.Agent.pathPending) return false;

 
        float horizontalDistance = GetHorizontalDistanceToTarget(fleeTarget.position);

    
        bool nearTarget = horizontalDistance <= arrivalThreshold;
        bool pathComplete = !animalStateMachine.Agent.hasPath ||
                           animalStateMachine.Agent.remainingDistance <= animalStateMachine.Agent.stoppingDistance;

        return nearTarget && pathComplete;
    }


    private void TransitionToSafeState()
    {
      
        float random = Random.value;

        if (random > 0.66f)
        {
           
           
            animalStateMachine.SwitchState(new AnimalEatingState(animalStateMachine));
        }
        else if (random > 0.33f)
        {
        
            
            animalStateMachine.SwitchState(new AnimalIdleState(animalStateMachine));
        }
        else
        {
         
         
            animalStateMachine.SwitchState(new AnimalPatrolState(animalStateMachine));
        }
    }
}