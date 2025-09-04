using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    [Header("Exit Settings")]
    
    [SerializeField] private LayerMask playerLayer = 1;

    [Header("Visual Effects")]

    [SerializeField] private GameObject exitEffect;


    [SerializeField] private GameObject completionPrompt;

    void Start()
    {
        if (completionPrompt != null)
            completionPrompt.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsPlayer(other))
        {
            if (completionPrompt != null)
                completionPrompt.SetActive(true);

            
            Debug.Log("[LevelExit] Level completed! Returning to Hub...");
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (IsPlayer(other))
        {
            
            ReturnToHub();
        }
    }

    public void ReturnToHub()
    {
        
        if (GameProgressManager.Instance != null)
        {
            SceneManager.LoadScene(GameProgressManager.Instance.hubSceneName);
        }
        else
        {
            Debug.LogError("[LevelExit] GameProgressManager not found!");
        }
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
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;

            if (collider is BoxCollider box)
                Gizmos.DrawCube(box.center, box.size);
            else if (collider is SphereCollider sphere)
                Gizmos.DrawSphere(sphere.center, sphere.radius);
        }
    }

    
}
