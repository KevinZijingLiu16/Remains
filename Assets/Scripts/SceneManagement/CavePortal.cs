using UnityEngine;
using UnityEngine.SceneManagement;

public class CavePortal : MonoBehaviour
{
    [Header("Portal Settings")]

    [SerializeField] private string targetSceneName = "Level1";


    [SerializeField] private LayerMask playerLayer = 1;

    [Header("Visual Effects")]

    [SerializeField] private GameObject portalEffect;


    [SerializeField] private GameObject interactionPrompt;

    private bool _playerNearby = false;

    void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsPlayer(other))
        {
            _playerNearby = true;
            if (interactionPrompt != null)
                interactionPrompt.SetActive(true);

            // can play audio or vfx here
            Debug.Log($"[CavePortal] Player approached cave entrance to {targetSceneName}");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IsPlayer(other))
        {
            _playerNearby = false;
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (IsPlayer(other) && _playerNearby)
        {
            // now is enter the level automatically, can change to UI button toggle
            EnterCave(other);
        }
    }

    void EnterCave(Collider playerCollider)
    {
        
        var splineRunner = playerCollider.GetComponent<SplineRunnerRB>();
        if (splineRunner == null)
        {
            Debug.LogError("[CavePortal] Player doesn't have SplineRunnerRB component!");
            return;
        }

        // save current pos in GameProgressManager
        Vector3 currentPos = playerCollider.transform.position;
        float currentT = splineRunner.GetCurrentT();

        GameProgressManager.Instance.SaveHubPosition(currentPos, currentT);

       
        Debug.Log($"[CavePortal] Entering cave: {targetSceneName}");
        SceneManager.LoadScene(targetSceneName);
    }

    bool IsPlayer(Collider other)
    {
        return ((1 << other.gameObject.layer) & playerLayer) != 0;
    }

   
    void OnDrawGizmosSelected()
    {
        var collider = GetComponent<Collider>();
        if (collider != null)
        {
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;

            if (collider is BoxCollider box)
                Gizmos.DrawCube(box.center, box.size);
            else if (collider is SphereCollider sphere)
                Gizmos.DrawSphere(sphere.center, sphere.radius);
            else if (collider is CapsuleCollider capsule)
                Gizmos.DrawWireCube(capsule.center, new Vector3(capsule.radius * 2, capsule.height, capsule.radius * 2));
        }
    }
}