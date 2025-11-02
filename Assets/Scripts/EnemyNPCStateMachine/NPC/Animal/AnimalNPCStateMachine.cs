using UnityEngine;
using UnityEngine.AI;

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

      
        ValidateAnimalConfiguration();

     
        SwitchState(new AnimalPatrolState(this));
    }


    private void ValidateAnimalConfiguration()
    {
     
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogError($"[{animalType}] Patrol Points is null");
        }
        else
        {
          
            int validCount = 0;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] == null)
                {
                    Debug.LogError($"[{animalType}] Patrol Point[{i}] is null£¡");
                }
                else
                {
                    validCount++;

               
                    NavMeshHit hit;
                   
                }
            }
        }

        if (fleePoints == null || fleePoints.Length == 0)
        {
        }
        else
        {
            int validCount = 0;
            for (int i = 0; i < fleePoints.Length; i++)
            {
                if (fleePoints[i] != null)
                {
                    validCount++;

                    
                    NavMeshHit hit;
                    if (!NavMesh.SamplePosition(fleePoints[i].position, out hit, 2f, NavMesh.AllAreas))
                    {
                        Debug.LogWarning($"[{animalType}] Flee Point[{i}] ({fleePoints[i].name}) ²»ÔÚ NavMesh ÉÏ£¡");
                    }
                }
            }
        }
    }

 
    public Transform GetNearestFleePoint()
    {
        if (fleePoints == null || fleePoints.Length == 0)
        {
            return null;
        }

   
        Transform nearest = null;
        float minDistance = float.MaxValue;

        foreach (Transform point in fleePoints)
        {
            if (point == null) continue;

            float distance = Vector3.Distance(transform.position, point.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = point;
            }
        }

     
      

        return nearest;
    }


    public Transform GetRandomPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            return null;
        }

  
        Transform[] validPoints = System.Array.FindAll(patrolPoints, point => point != null);

        if (validPoints.Length == 0)
        {
            return null;
        }

        Transform selectedPoint = validPoints[Random.Range(0, validPoints.Length)];

        return selectedPoint;
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
                return new AnimalIdleState(this);
        }
    }

    public override void OnTriggered(StateTransitionConfig config)
    {
        if (!CanBeTriggered(config))
        {
            return;
        }

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


    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

     
        if (patrolPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (Transform point in patrolPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.5f);

                 
#if UNITY_EDITOR
                    UnityEditor.Handles.Label(point.position + Vector3.up * 1f, point.name);
#endif
                }
            }

         
            if (Application.isPlaying && currentState is AnimalPatrolState)
            {
                Gizmos.color = Color.yellow;
                foreach (Transform point in patrolPoints)
                {
                    if (point != null)
                    {
                        Gizmos.DrawLine(transform.position, point.position);
                    }
                }
            }
        }

        if (fleePoints != null)
        {
            Gizmos.color = Color.red;
            foreach (Transform point in fleePoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireCube(point.position, Vector3.one * 0.5f);

#if UNITY_EDITOR
                    UnityEditor.Handles.Label(point.position + Vector3.up * 1f, point.name);
#endif
                }
            }

         
            if (Application.isPlaying && currentState is AnimalRunningState)
            {
                Transform nearest = GetNearestFleePoint();
                if (nearest != null)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(transform.position, nearest.position);
                }
            }
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

      
        if (Application.isPlaying && currentState != null)
        {
#if UNITY_EDITOR
            string stateInfo = $"State: {currentState.GetType().Name}\n";
            stateInfo += $"Animal: {animalType}\n";
            if (Agent != null)
            {
                stateInfo += $"On NavMesh: {Agent.isOnNavMesh}\n";
                stateInfo += $"Speed: {Agent.velocity.magnitude:F2}";
            }
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, stateInfo);
#endif
        }
    }
}