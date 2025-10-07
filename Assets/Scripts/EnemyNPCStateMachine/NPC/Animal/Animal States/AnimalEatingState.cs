using UnityEngine;

public class AnimalEatingState : AnimalNPCState
{
    private readonly int eatingHash = Animator.StringToHash("Eating");
    private const float crossFadeDuration = 0.2f;

    private float eatingTimer = 0f;
    private float eatingDuration;

    public AnimalEatingState(AnimalNPCStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        Debug.Log($"[{animalStateMachine.AnimalType}] Entering Eating State");

        if (animalStateMachine.Animator != null)
        {
            animalStateMachine.Animator.CrossFadeInFixedTime(eatingHash, crossFadeDuration);
        }

        eatingTimer = 0f;
        eatingDuration = animalStateMachine.EatingTime;
    }

    public override void Tick(float deltaTime)
    {
        eatingTimer += deltaTime;

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
        Debug.Log($"[{animalStateMachine.AnimalType}] Exiting Eating State");
    }
}