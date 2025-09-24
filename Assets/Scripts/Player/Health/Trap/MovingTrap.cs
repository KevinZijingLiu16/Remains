using UnityEngine;

public class MovingTrap : BaseTrap
{
    [Header("Movement")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private bool startAtPointA = true;

    private bool movingToB = true;
    private Vector3 targetPosition;

    void Start()
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogError("[MovingTrap] Point A or Point B not set!");
            enabled = false;
            return;
        }

        transform.position = startAtPointA ? pointA.position : pointB.position;
        targetPosition = startAtPointA ? pointB.position : pointA.position;
        movingToB = startAtPointA;
    }

    void Update()
    {
        MoveTrap();
    }

    private void MoveTrap()
    {
     
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            
            movingToB = !movingToB;
            targetPosition = movingToB ? pointB.position : pointA.position;
        }
    }
}
