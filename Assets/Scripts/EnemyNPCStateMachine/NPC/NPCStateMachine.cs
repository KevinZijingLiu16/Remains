using UnityEngine;
using UnityEngine.AI;

public abstract class NPCStateMachine : StateMachine, ITriggerable
{
    [Header("NPC Components")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected NavMeshAgent agent;

    [Header("NPC Settings")]
    [SerializeField] protected float movementSpeed = 2f;
    [SerializeField] protected float rotationSpeed = 5f;

    [Header("Ground Check Settings")]
    [SerializeField] protected bool enableGroundCheck = true;
    [SerializeField] protected float maxGroundCheckDistance = 2f;
    [SerializeField] protected float groundSnapDistance = 0.5f;

    public Animator Animator => animator;
    public NavMeshAgent Agent => agent;
    public float MovementSpeed => movementSpeed;
    public float RotationSpeed => rotationSpeed;

  
    public Transform Transform => transform;

    protected virtual void Start()
    {
        InitializeAgent();
        ValidateComponents();
    }

    protected virtual void FixedUpdate()
    {
        base.FixedUpdate();

        if (enableGroundCheck && agent != null)
        {
            EnsureOnNavMesh();
        }
    }

    protected virtual void InitializeAgent()
    {
        if (agent != null)
        {
         
            agent.updatePosition = true;  
            agent.updateRotation = false; 
            agent.speed = movementSpeed;
            agent.autoBraking = true;

          
            if (agent.baseOffset == 0)
            {
                agent.baseOffset = 0.1f;
            }

        
            if (!agent.isOnNavMesh)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(transform.position, out hit, 10f, NavMesh.AllAreas))
                {
                    transform.position = hit.position;
                    Debug.Log($"[{gameObject.name}] 初始化位置调整到 NavMesh: {hit.position}");
                }
                else
                {
                    Debug.LogError($"[{gameObject.name}] 无法找到有效的 NavMesh 位置！");
                }
            }
        }
    }

  
    protected virtual void ValidateComponents()
    {
  
        if (agent == null)
        {
            Debug.LogError($"[{gameObject.name}] 缺少 NavMesh Agent 组件！");
            return;
        }

    
        if (animator == null)
        {
            Debug.LogWarning($"[{gameObject.name}] 缺少 Animator 组件！");
        }
        else
        {
            if (animator.applyRootMotion)
            {
                Debug.LogWarning($"[{gameObject.name}] Animator 的 Apply Root Motion 应该关闭！");
                animator.applyRootMotion = false;
            }
        }

      
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            Debug.LogError($"[{gameObject.name}] 检测到 Character Controller！这会与 NavMesh Agent 冲突。");
            Debug.LogError($"请移除 Character Controller 组件！NavMesh Agent 会处理所有移动。");

          
            cc.enabled = false;
        }

       
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            if (!rb.isKinematic)
            {
                Debug.LogWarning($"[{gameObject.name}] Rigidbody 应该设置为 Is Kinematic！");
                rb.isKinematic = true;
            }
            if (rb.useGravity)
            {
                Debug.LogWarning($"[{gameObject.name}] Rigidbody 不应该使用重力（NavMesh Agent 会处理）！");
                rb.useGravity = false;
            }
        }
    }

    protected virtual void EnsureOnNavMesh()
    {
        if (!agent.enabled || !agent.isOnNavMesh)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, maxGroundCheckDistance, NavMesh.AllAreas))
            {
                if (Vector3.Distance(transform.position, hit.position) <= groundSnapDistance)
                {
                    transform.position = hit.position;
                    Debug.LogWarning($"[{gameObject.name}] 被吸附回 NavMesh: {hit.position}");
                }
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] 找不到附近的 NavMesh！位置: {transform.position}");
            }
        }
    }

 
    public abstract void OnTriggered(StateTransitionConfig config);

    public virtual bool CanBeTriggered(StateTransitionConfig config)
    {
        return true;
    }

 
    protected virtual void OnDrawGizmos()
    {
      
        if (Application.isPlaying && agent != null && agent.hasPath)
        {
            Gizmos.color = Color.yellow;
            Vector3[] corners = agent.path.corners;
            for (int i = 0; i < corners.Length - 1; i++)
            {
                Gizmos.DrawLine(corners[i], corners[i + 1]);
            }
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        
        if (enableGroundCheck)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, maxGroundCheckDistance);
        }
    }
}