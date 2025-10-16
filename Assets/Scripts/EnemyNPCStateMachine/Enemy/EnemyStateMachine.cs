using UnityEngine;
using UnityEngine.AI;


public class EnemyStateMachine : StateMachine
{
    [Header("Enemy References")]
    public Transform playerTransform;
    public PlayerDirtSystem playerDirtSystem;

    private NavMeshAgent _navAgent;
    public NavMeshAgent NavAgent => _navAgent;

    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float stoppingDistance = 0.5f;

    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolWaitTime = 2f;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private Transform attackHitbox; 

    [Header("Foam Stun Settings")]
    [SerializeField] private float foamStunDuration = 3f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool showGizmos = true;

   
    private EnemyStateFactory _stateFactory;

   
    public float DetectionRange => detectionRange;
    public float AttackRange => attackRange;
    public float MoveSpeed => moveSpeed;
    public float RotationSpeed => rotationSpeed;
    public Transform[] PatrolPoints => patrolPoints;
    public float PatrolWaitTime => patrolWaitTime;
    public float AttackCooldown => attackCooldown;
    public Transform AttackHitbox => attackHitbox;
    public float FoamStunDuration => foamStunDuration;
    public bool EnableDebugLogs => enableDebugLogs;

   
    private bool _isStunned = false;
    private float _stunTimer = 0f;

    public bool IsStunned => _isStunned;

    void Awake()
    {
        _stateFactory = new EnemyStateFactory(this);

        _navAgent = GetComponent<NavMeshAgent>();
        if (_navAgent != null)
        {
            _navAgent.speed = moveSpeed;
            _navAgent.angularSpeed = rotationSpeed * 50f;
            _navAgent.stoppingDistance = stoppingDistance;
        }

        FindPlayer();
    }

    void Start()
    {
     
        if (_navAgent != null)
        {
            Debug.Log($"[EnemyStateMachine] NavAgent exists: TRUE");
            Debug.Log($"[EnemyStateMachine] NavAgent enabled: {_navAgent.enabled}");
            Debug.Log($"[EnemyStateMachine] NavAgent on NavMesh: {_navAgent.isOnNavMesh}");
            Debug.Log($"[EnemyStateMachine] NavAgent speed: {_navAgent.speed}");
        }
        else
        {
            Debug.LogError("[EnemyStateMachine] NavAgent is NULL!");
        }

   
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogError("[EnemyStateMachine] No patrol points assigned!");
        }
        else
        {
            Debug.Log($"[EnemyStateMachine] Patrol points count: {patrolPoints.Length}");
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Debug.Log($"[EnemyStateMachine] Patrol Point {i}: {patrolPoints[i].position}");
                }
                else
                {
                    Debug.LogWarning($"[EnemyStateMachine] Patrol Point {i} is NULL!");
                }
            }
        }

   
        if (playerTransform != null)
        {
            Debug.Log($"[EnemyStateMachine] Player found at: {playerTransform.position}");
        }
        else
        {
            Debug.LogWarning("[EnemyStateMachine] Player NOT found!");
        }

   
        Debug.Log("[EnemyStateMachine] Starting patrol state...");
        SwitchState(_stateFactory.CreateState("Patrol"));
    }

    protected override void Update()
    {
        base.Update();

        
        Vector3 euler = transform.eulerAngles;
        transform.eulerAngles = new Vector3(0, euler.y, 0);

   
        if (_isStunned)
        {
            _stunTimer -= Time.deltaTime;
            if (_stunTimer <= 0f)
            {
                RecoverFromStun();
            }
        }
    }
    public State CreateStateFromFactory(string stateName)
    {
        return _stateFactory.CreateState(stateName);
    }
    public void StopMovement()
    {
        if (_navAgent != null && _navAgent.enabled && _navAgent.isOnNavMesh)
        {
            _navAgent.isStopped = true;
            _navAgent.ResetPath(); 
        }
    }

    public void ResumeMovement()
    {
        if (_navAgent != null && _navAgent.enabled && _navAgent.isOnNavMesh)
        {
            _navAgent.isStopped = false;
        }
    }
    private void FindPlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerDirtSystem = player.GetComponent<PlayerDirtSystem>();

            if (playerDirtSystem != null)
            {
              
                playerDirtSystem.OnBecameClean += OnPlayerBecameClean;
            }
        }
        else
        {
            Debug.LogWarning("[EnemyStateMachine] Player not found!");
        }
    }

    public bool CanDetectDirtyPlayer()
    {
        if (playerTransform == null || playerDirtSystem == null)
        {
            if (enableDebugLogs)
                Debug.LogWarning("[EnemyStateMachine] Player reference missing!");
            return false;
        }

        if (!playerDirtSystem.IsAnyDirty)
        {
            if (enableDebugLogs)
                Debug.Log("[EnemyStateMachine] Player is clean");
            return false;
        }

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool inRange = distance <= detectionRange;

        if (enableDebugLogs)
        {
            Debug.Log($"[EnemyStateMachine] Player distance: {distance:F2}, Detection range: {detectionRange}, Can detect: {inRange}");
        }

        return inRange;
    }

    public bool IsPlayerInAttackRange()
    {
        if (playerTransform == null) return false;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool inRange = distance <= attackRange;

        if (enableDebugLogs)
        {
            Debug.Log($"[EnemyStateMachine] Attack check - Distance: {distance:F2}, Range: {attackRange}, In range: {inRange}");
        }

        return inRange;
    }

    public void StunByFoam()
    {
        if (_isStunned) return;

        _isStunned = true;
        _stunTimer = foamStunDuration;

        SwitchState(_stateFactory.CreateState("Idle"));

        if (enableDebugLogs)
        {
            Debug.Log("[EnemyStateMachine] Stunned by foam!");
        }
    }

    private void RecoverFromStun()
    {
        _isStunned = false;

       
        SwitchState(_stateFactory.CreateState("Patrol"));

        if (enableDebugLogs)
        {
            Debug.Log("[EnemyStateMachine] Recovered from stun!");
        }
    }

    private void OnPlayerBecameClean()
    {
     
        if (currentState is EnemyChasingState || currentState is EnemyAttackState)
        {
            SwitchState(_stateFactory.CreateState("Patrol"));

            if (enableDebugLogs)
            {
                Debug.Log("[EnemyStateMachine] Player became clean, returning to patrol");
            }
        }
    }

    void OnDestroy()
    {
     
        if (playerDirtSystem != null)
        {
            playerDirtSystem.OnBecameClean -= OnPlayerBecameClean;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
       
        if (collision.gameObject.CompareTag("FoamPlatform"))
        {
            StunByFoam();
        }
    }


    void OnDrawGizmos()
    {
        if (!showGizmos) return;

      
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

     
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

       
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawWireSphere(patrolPoints[i].position, 0.5f);

                
                    if (i < patrolPoints.Length - 1 && patrolPoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                    }
                }
            }
        }
    }
}