using UnityEngine;

public class AnimalHearingState : AnimalNPCState
{
    private readonly int alertHash = Animator.StringToHash("Alert");
    private const float crossFadeDuration = 0.1f;

    private float hearingTimer = 0f;
    private float hearingDuration;

    public AnimalHearingState(AnimalNPCStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        Debug.Log($"[{animalStateMachine.AnimalType}] Entering Hearing State");

        StopMovement();

        if (animalStateMachine.Animator != null)
        {
            animalStateMachine.Animator.CrossFadeInFixedTime(alertHash, crossFadeDuration);
        }

        hearingTimer = 0f;
        hearingDuration = animalStateMachine.HearingDuration;
    }

    public override void Tick(float deltaTime)
    {
        hearingTimer += deltaTime;

        if (hearingTimer >= hearingDuration)
        {
           
            animalStateMachine.SwitchState(new AnimalRunningState(animalStateMachine));
        }
    }

    public override void FixedTick(float fixedDeltaTime)
    {
    }

    public override void Exit()
    {
        Debug.Log($"[{animalStateMachine.AnimalType}] Exiting Hearing State");
    }
}