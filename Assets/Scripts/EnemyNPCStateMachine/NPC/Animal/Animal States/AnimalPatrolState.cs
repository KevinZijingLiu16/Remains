using UnityEngine;

public class AnimalPatrolState : AnimalNPCState
{
    private readonly int locomotionHash = Animator.StringToHash("Locomotion");
    private readonly int speedHash = Animator.StringToHash("Speed");
    private const float crossFadeDuration = 0.2f;
    private const float animationDampTime = 0.1f;

    private Transform currentTarget;
    private bool waitingAtPoint = false;
    private float waitTimer = 0f;
    private float currentWaitTime;

    public AnimalPatrolState(AnimalNPCStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        Debug.Log($"[{animalStateMachine.AnimalType}] Entering Patrol State");

        if (animalStateMachine.Animator != null)
        {
            animalStateMachine.Animator.CrossFadeInFixedTime(locomotionHash, crossFadeDuration);
        }

        SelectNextPatrolPoint();
    }

    public override void Tick(float deltaTime)
    {
        if (waitingAtPoint)
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
        else
        {
         
            if (currentTarget != null)
            {
                MoveToPosition(currentTarget.position, deltaTime);
                FaceDirection(animalStateMachine.Agent.desiredVelocity);

                if (animalStateMachine.Animator != null)
                {
                    float speed = animalStateMachine.Agent.velocity.magnitude / animalStateMachine.MovementSpeed;
                    animalStateMachine.Animator.SetFloat(speedHash, speed, animationDampTime, deltaTime);
                }

          
                if (HasReachedDestination())
                {
                    waitingAtPoint = true;
                    waitTimer = 0f;
                    currentWaitTime = Random.Range(1f, 3f); 
                }
            }
            else
            {
              
                SelectNextPatrolPoint();
            }
        }
    }

    public override void FixedTick(float fixedDeltaTime)
    {
       
    }

    public override void Exit()
    {
        StopMovement();
        Debug.Log($"[{animalStateMachine.AnimalType}] Exiting Patrol State");
    }

    private void SelectNextPatrolPoint()
    {
        currentTarget = animalStateMachine.GetRandomPatrolPoint();
        waitingAtPoint = false;

        if (currentTarget != null)
        {
            Debug.Log($"[{animalStateMachine.AnimalType}] Moving to patrol point: {currentTarget.name}");
        }
    }

    private void TransitionToRandomIdleState()
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