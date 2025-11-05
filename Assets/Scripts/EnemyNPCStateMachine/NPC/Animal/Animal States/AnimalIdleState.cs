using UnityEngine;

public class AnimalIdleState : AnimalNPCState
{
    private readonly int idleHash = Animator.StringToHash("Idle");
    private readonly int speedHash = Animator.StringToHash("Speed");
    private const float crossFadeDuration = 0.2f;

    private float idleTimer = 0f;
    private float idleDuration;

    public AnimalIdleState(AnimalNPCStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {

        if (animalStateMachine.Animator != null)
        {
            animalStateMachine.Animator.CrossFadeInFixedTime(idleHash, crossFadeDuration);
            animalStateMachine.Animator.SetFloat(speedHash, 0f);
        }

        StopMovement();

        idleTimer = 0f;
        idleDuration = animalStateMachine.IdleTime + Random.Range(-0.5f, 0.5f);
        idleDuration = Mathf.Max(1f, idleDuration); // ÖÁÉÙ 1 Ãë

    }

    public override void Tick(float deltaTime)
    {
        idleTimer += deltaTime;

        if (animalStateMachine.Animator != null)
        {
            animalStateMachine.Animator.SetFloat(speedHash, 0f);
        }

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
    }
}