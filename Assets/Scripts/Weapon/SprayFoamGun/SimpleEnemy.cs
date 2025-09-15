using UnityEngine;

public class SimpleEnemy : MonoBehaviour
{
    [Header("Movement")]
    public float normalSpeed = 3f;
    public Transform target; 

    private UnityEngine.AI.NavMeshAgent _agent;

    void Start()
    {
        _agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (_agent != null)
        {
            _agent.speed = normalSpeed;
        }

        
        if (GetComponent<EnemyFoamSlow>() == null)
        {
            gameObject.AddComponent<EnemyFoamSlow>();
        }
    }

    void Update()
    {
        if (target != null && _agent != null && _agent.isActiveAndEnabled)
        {
            _agent.SetDestination(target.position);
        }
    }
}
