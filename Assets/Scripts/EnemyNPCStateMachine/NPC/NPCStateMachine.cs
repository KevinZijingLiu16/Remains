using UnityEngine;
using UnityEngine.AI;

public abstract class NPCStateMachine : StateMachine, ITriggerable
{
    [Header("NPC Components")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected CharacterController controller;
    [SerializeField] protected NavMeshAgent agent;

    [Header("NPC Settings")]
    [SerializeField] protected float movementSpeed = 2f;
    [SerializeField] protected float rotationSpeed = 5f;

    public Animator Animator => animator;
    public CharacterController Controller => controller;
    public NavMeshAgent Agent => agent;
    public float MovementSpeed => movementSpeed;
    public float RotationSpeed => rotationSpeed;

    // ITriggerable implementation
    public Transform Transform => transform;

    protected virtual void Start()
    {
        InitializeAgent();
    }

    protected virtual void InitializeAgent()
    {
        if (agent != null)
        {
            agent.updatePosition = false;
            agent.updateRotation = false;
        }
    }

  
    public abstract void OnTriggered(StateTransitionConfig config);

 
    public virtual bool CanBeTriggered(StateTransitionConfig config)
    {
        return true;
    }
}