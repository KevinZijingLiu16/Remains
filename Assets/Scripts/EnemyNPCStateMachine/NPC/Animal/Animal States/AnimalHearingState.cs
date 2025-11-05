using UnityEngine;

public class AnimalHearingState : AnimalNPCState
{
    private readonly int alertHash = Animator.StringToHash("Alert");
    private readonly int speedHash = Animator.StringToHash("Speed");
    private const float crossFadeDuration = 0.1f;

    private float hearingTimer = 0f;
    private float hearingDuration;

    public AnimalHearingState(AnimalNPCStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {

        StopMovement();

        if (animalStateMachine.Animator != null)
        {
            animalStateMachine.Animator.CrossFadeInFixedTime(alertHash, crossFadeDuration);
            animalStateMachine.Animator.SetFloat(speedHash, 0f);
        }

        hearingTimer = 0f;
        hearingDuration = animalStateMachine.HearingDuration;

    }

    public override void Tick(float deltaTime)
    {
        hearingTimer += deltaTime;

        if (animalStateMachine.Animator != null)
        {
            animalStateMachine.Animator.SetFloat(speedHash, 0f);
        }

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
    }
}