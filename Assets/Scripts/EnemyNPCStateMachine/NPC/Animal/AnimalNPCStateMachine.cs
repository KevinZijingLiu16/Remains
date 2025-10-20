using UnityEngine;

public class AnimalNPCStateMachine : NPCStateMachine, IStateFactory
{
    [Header("Animal Settings")]
    [SerializeField] private AnimalType animalType;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private Transform[] fleePoints;
    [SerializeField] private float fleeSpeed = 5f;

    [Header("Behavior Settings")]
    [SerializeField] private float idleTime = 3f;
    [SerializeField] private float eatingTime = 5f;
    [SerializeField] private float hearingDuration = 1f;

    public AnimalType AnimalType => animalType;
    public Transform[] PatrolPoints => patrolPoints;
    public Transform[] FleePoints => fleePoints;
    public float FleeSpeed => fleeSpeed;
    public float IdleTime => idleTime;
    public float EatingTime => eatingTime;
    public float HearingDuration => hearingDuration;

    protected override void Start()
    {
        base.Start();

   
        SwitchState(new AnimalPatrolState(this));
    }


    public Transform GetNearestFleePoint()
    {
        if (fleePoints == null || fleePoints.Length == 0) return null;

        Transform nearest = fleePoints[0];
        float minDistance = Vector3.Distance(transform.position, nearest.position);

        for (int i = 1; i < fleePoints.Length; i++)
        {
            float distance = Vector3.Distance(transform.position, fleePoints[i].position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = fleePoints[i];
            }
        }

        return nearest;
    }

   
    public Transform GetRandomPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return null;
        return patrolPoints[Random.Range(0, patrolPoints.Length)];
    }


    public State CreateState(string stateName)
    {
        switch (stateName)
        {
            case "Patrol":
                return new AnimalPatrolState(this);
            case "Idle":
                return new AnimalIdleState(this);
            case "Eating":
                return new AnimalEatingState(this);
            case "Hearing":
                return new AnimalHearingState(this);
            case "Running":
                return new AnimalRunningState(this);
            default:
                Debug.LogWarning($"Unknown state: {stateName}");
                return new AnimalIdleState(this);
        }
    }


    public override void OnTriggered(StateTransitionConfig config)
    {
        if (!CanBeTriggered(config)) return;

        State newState = CreateState(config.targetStateName);
        if (newState != null)
        {
            SwitchState(newState);
        }
    }


    public override bool CanBeTriggered(StateTransitionConfig config)
    {

        if (currentState is AnimalRunningState)
        {
            return config.canInterruptRunning;
        }
        return base.CanBeTriggered(config);
    }
}