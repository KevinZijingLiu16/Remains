using UnityEngine;

public abstract class NPCBaseState : State
{
    protected NPCStateMachine stateMachine;

    protected NPCBaseState(NPCStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }


    protected void MoveToPosition(Vector3 targetPosition, float deltaTime)
    {
        if (stateMachine.Agent != null)
        {
            stateMachine.Agent.SetDestination(targetPosition);
            Vector3 movement = stateMachine.Agent.desiredVelocity;
            stateMachine.Controller.Move(movement * deltaTime);
        }
    }

    protected void StopMovement()
    {
        if (stateMachine.Agent != null)
        {
            stateMachine.Agent.ResetPath();
            stateMachine.Agent.velocity = Vector3.zero;
        }
    }

    protected void FaceDirection(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            direction.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            stateMachine.transform.rotation = Quaternion.Slerp(
                stateMachine.transform.rotation,
                targetRotation,
                Time.deltaTime * stateMachine.RotationSpeed
            );
        }
    }

    protected bool HasReachedDestination()
    {
        if (stateMachine.Agent == null) return false;

        if (stateMachine.Agent.pathPending) return false;

        if (stateMachine.Agent.remainingDistance <= stateMachine.Agent.stoppingDistance)
        {
            if (!stateMachine.Agent.hasPath || stateMachine.Agent.velocity.sqrMagnitude == 0f)
            {
                return true;
            }
        }
        return false;
    }

   
    protected float GetNormalizedAnimationTime(string tag)
    {
        if (stateMachine.Animator == null) return 0f;

        AnimatorStateInfo currentInfo = stateMachine.Animator.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo nextInfo = stateMachine.Animator.GetNextAnimatorStateInfo(0);

        if (stateMachine.Animator.IsInTransition(0) && nextInfo.IsTag(tag))
        {
            return nextInfo.normalizedTime;
        }
        else if (!stateMachine.Animator.IsInTransition(0) && currentInfo.IsTag(tag))
        {
            return currentInfo.normalizedTime;
        }
        return 0f;
    }
}