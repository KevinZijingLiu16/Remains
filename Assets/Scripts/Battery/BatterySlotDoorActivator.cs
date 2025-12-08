using UnityEngine;

public class BatteryDoorOpener : MonoBehaviour
{
    public Transform door;            // Drag your door here!
    public Transform openPosition;    // Empty object = final open spot
    public float speed = 2f;          // Speed of opening

    bool playerNearby = false;
    bool doorOpening = false;

    void Update()
    {
        // Player presses E
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            doorOpening = true;
        }

        // Move the door towards the open position
        if (doorOpening)
        {
            door.position = Vector3.Lerp(door.position, openPosition.position, Time.deltaTime * speed);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerNearby = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerNearby = false;
    }
}

