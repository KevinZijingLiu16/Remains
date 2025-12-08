using UnityEngine;
using System.Collections;

public class FuseBoxActivator : MonoBehaviour
{
    public Transform TopObstacle;   // Drag the TopObstacle platform here
    public Vector3 moveOffset = new Vector3(0, -5, 0); // Move distance (down 5 units)
    public float moveTime = 2f;     // Time to move

    private bool playerNear = false;
    private bool isMoving = false;
    private Vector3 startPos;
    private Vector3 targetPos;

    private void Start()
    {
        startPos = TopObstacle.position;
        targetPos = startPos + moveOffset;
    }

    private void Update()
    {
        if (playerNear && !isMoving && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(MovePlatformDown());
        }
    }

    private IEnumerator MovePlatformDown()
    {
        isMoving = true;
        float elapsed = 0f;

        Vector3 initial = TopObstacle.position;

        while (elapsed < moveTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveTime);
            TopObstacle.position = Vector3.Lerp(initial, targetPos, t);
            yield return null;
        }

        TopObstacle.position = targetPos;
        isMoving = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;
            // OPTIONAL: show UI "Press E to activate fusebox"
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
            // OPTIONAL: hide UI
        }
    }
}
