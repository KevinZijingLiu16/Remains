using UnityEngine;

public class AnimalIdleState : AnimalNPCState
{
    private readonly int idleHash = Animator.StringToHash("Idle");
    private const float crossFadeDuration = 0.2f;

    private float idleTimer = 0f;
    private float idleDuration;

    public AnimalIdleState(AnimalNPCStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        Debug.Log($"[{animalStateMachine.AnimalType}] Entering Idle State");

        if (animalStateMachine.Animator != null)
        {
            animalStateMachine.Animator.CrossFadeInFixedTime(idleHash, crossFadeDuration);
        }

        idleTimer = 0f;
        idleDuration = animalStateMachine.IdleTime;
    }

    public override void Tick(float deltaTime)
    {
        idleTimer += deltaTime;

        if (idleTimer >= idleDuration)
        {
           
            animalStateMachine.SwitchState(new AnimalPatrolState(animalStateMachine));
        }
    }

    public override void FixedTick(float fixedDeltaTime)
    {
    }

    public override void Exit()
    {
        Debug.Log($"[{animalStateMachine.AnimalType}] Exiting Idle State");
    }
}