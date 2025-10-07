using UnityEngine;

public class AnimalRunningState : AnimalNPCState
{
    private readonly int runHash = Animator.StringToHash("Run");
    private readonly int speedHash = Animator.StringToHash("Speed");
    private const float crossFadeDuration = 0.1f;
    private const float animationDampTime = 0.1f;

    private Transform fleeTarget;
    private float originalSpeed;

    public AnimalRunningState(AnimalNPCStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        Debug.Log($"[{animalStateMachine.AnimalType}] Entering Running State");

        if (animalStateMachine.Animator != null)
        {
            animalStateMachine.Animator.CrossFadeInFixedTime(runHash, crossFadeDuration);
        }

     
        originalSpeed = animalStateMachine.Agent.speed;
        animalStateMachine.Agent.speed = animalStateMachine.FleeSpeed;

 
        fleeTarget = animalStateMachine.GetNearestFleePoint();

        if (fleeTarget != null)
        {
            Debug.Log($"[{animalStateMachine.AnimalType}] Fleeing to: {fleeTarget.name}");
        }
        else
        {
            Debug.LogWarning($"[{animalStateMachine.AnimalType}] No flee point found!");
        }
    }

    public override void Tick(float deltaTime)
    {
        if (fleeTarget != null)
        {
            MoveToPosition(fleeTarget.position, deltaTime);
            FaceDirection(animalStateMachine.Agent.desiredVelocity);

            if (animalStateMachine.Animator != null)
            {
               
                float speed = animalStateMachine.Agent.velocity.magnitude / animalStateMachine.FleeSpeed;
                animalStateMachine.Animator.SetFloat(speedHash, speed * 2f, animationDampTime, deltaTime);
            }

         
            if (HasReachedDestination())
            {
                Debug.Log($"[{animalStateMachine.AnimalType}] Reached flee point");
              
                TransitionToSafeState();
            }
        }
        else
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
        Debug.Log($"[{animalStateMachine.AnimalType}] Exiting Running State");
    }

    private void TransitionToSafeState()
    {
     
        if (Random.value > 0.5f)
        {
            animalStateMachine.SwitchState(new AnimalEatingState(animalStateMachine));
        }
        else
        {
            animalStateMachine.SwitchState(new AnimalIdleState(animalStateMachine));
        }
    }
}