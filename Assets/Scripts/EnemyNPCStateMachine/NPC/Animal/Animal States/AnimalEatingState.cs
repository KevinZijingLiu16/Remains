using UnityEngine;

public class AnimalEatingState : AnimalNPCState
{
    private readonly int eatingHash = Animator.StringToHash("Eating");
    private readonly int speedHash = Animator.StringToHash("Speed");
    private const float crossFadeDuration = 0.2f;

    private float eatingTimer = 0f;
    private float eatingDuration;

    public AnimalEatingState(AnimalNPCStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {

        if (animalStateMachine.Animator != null)
        {
            animalStateMachine.Animator.CrossFadeInFixedTime(eatingHash, crossFadeDuration);
            animalStateMachine.Animator.SetFloat(speedHash, 0f);
        }

        StopMovement();

        eatingTimer = 0f;
        eatingDuration = animalStateMachine.EatingTime + Random.Range(-1f, 1f);
        eatingDuration = Mathf.Max(2f, eatingDuration); // ÖÁÉÙ 2 Ãë

    }

    public override void Tick(float deltaTime)
    {
        eatingTimer += deltaTime;

        if (animalStateMachine.Animator != null)
        {
            animalStateMachine.Animator.SetFloat(speedHash, 0f);
        }

        if (eatingTimer >= eatingDuration)
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