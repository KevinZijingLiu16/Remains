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
        if (stateMachine.Agent == null) return;

        if (!stateMachine.Agent.isOnNavMesh)
        {
            Debug.LogWarning($"[{stateMachine.gameObject.name}] Agent ²»ÔÚ NavMesh ÉÏ£¡");
            return;
        }

        if (!stateMachine.Agent.pathPending)
        {
            stateMachine.Agent.SetDestination(targetPosition);
        }
    }


    protected void StopMovement()
    {
        if (stateMachine.Agent != null && stateMachine.Agent.isOnNavMesh)
        {
            stateMachine.Agent.ResetPath();
            stateMachine.Agent.velocity = Vector3.zero;
        }
    }


    protected void FaceDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.01f) return;

       
        direction.y = 0;
        direction.Normalize();

        if (direction != Vector3.zero)
        {
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
        if (!stateMachine.Agent.isOnNavMesh) return false;
        if (stateMachine.Agent.pathPending) return false;

      
        if (!stateMachine.Agent.hasPath)
        {
            return true;
        }


        float remainingDistance = stateMachine.Agent.remainingDistance;
        float stoppingDistance = stateMachine.Agent.stoppingDistance;

        if (remainingDistance <= stoppingDistance)
        {
            if (stateMachine.Agent.velocity.sqrMagnitude < 0.01f)
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


    protected float GetHorizontalDistanceToTarget(Vector3 targetPosition)
    {
        Vector3 horizontalPosition = new Vector3(
            stateMachine.transform.position.x,
            0,
            stateMachine.transform.position.z
        );

        Vector3 horizontalTarget = new Vector3(
            targetPosition.x,
            0,
            targetPosition.z
        );

        return Vector3.Distance(horizontalPosition, horizontalTarget);
    }
}