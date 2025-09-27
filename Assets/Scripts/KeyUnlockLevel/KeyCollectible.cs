using UnityEngine;

public class KeyCollectible : MonoBehaviour
{
    [Header("Key Settings")]
    [SerializeField] private string keyID;
    [SerializeField] private string targetLevelName; 
    [SerializeField] private LayerMask playerLayer = 1;
    [SerializeField] private LayerMask obstacleLayerMask = -1; 

    [Header("Visual Settings")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private GameObject keyVisual;
    [SerializeField] private GameObject collectEffect;
    [SerializeField] private AudioClip collectSound;

    [Header("UI")]
    [SerializeField] private GameObject collectPrompt;
    [SerializeField] private Canvas worldSpaceCanvas;

    private bool isCollected = false;
    private bool playerInRange = false;
    private Transform playerTransform;
    private Camera playerCamera;

    void Start()
    {
   
        if (GameProgressManager.Instance != null && GameProgressManager.Instance.HasKey(keyID))
        {
            isCollected = true;
            gameObject.SetActive(false);
            return;
        }

        if (collectPrompt != null)
            collectPrompt.SetActive(false);

 
        if (string.IsNullOrEmpty(keyID))
        {
            keyID = $"Key_{targetLevelName}_{gameObject.name}";
        }
    }

    void Update()
    {
        if (isCollected || !playerInRange) return;

        UpdateUILookAt();
    }

    private void UpdateUILookAt()
    {
        if (worldSpaceCanvas != null && playerCamera != null)
        {
            Vector3 direction = playerCamera.transform.position - worldSpaceCanvas.transform.position;
            worldSpaceCanvas.transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isCollected || !IsPlayer(other)) return;

        playerInRange = true;
        playerTransform = other.transform;

       
        playerCamera = Camera.main;

        if (playerCamera == null)
        {
       
            playerCamera = FindFirstObjectByType<Camera>();
        }

        if (playerCamera != null)
        {
            Debug.Log($"[KeyCollectible] Found camera: {playerCamera.name}");
        }
        else
        {
            Debug.LogError("[KeyCollectible] Could not find any camera!");
            return;
        }

        if (collectPrompt != null)
            collectPrompt.SetActive(true);

       
        if (IsVisibleToPlayer())
        {
            CollectKey();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IsPlayer(other))
        {
            playerInRange = false;
            playerTransform = null;
            playerCamera = null;

            if (collectPrompt != null)
                collectPrompt.SetActive(false);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (isCollected || !IsPlayer(other) || !playerInRange) return;

     
        if (IsVisibleToPlayer())
        {
            CollectKey();
        }
    }

    private bool IsVisibleToPlayer()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("[KeyCollectible] Player camera is null!");
            return false;
        }

     
        Vector3 viewportPoint = playerCamera.WorldToViewportPoint(transform.position);
        bool isInView = viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
                       viewportPoint.y >= 0 && viewportPoint.y <= 1 &&
                       viewportPoint.z > 0;

        Debug.Log($"[KeyCollectible] Viewport point: {viewportPoint}, Is in view: {isInView}");

        if (!isInView)
        {
            Debug.Log("[KeyCollectible] Key not in camera view");
            return false;
        }

      
        Vector3 directionToKey = transform.position - playerCamera.transform.position;
        float distance = directionToKey.magnitude;

        Ray ray = new Ray(playerCamera.transform.position, directionToKey.normalized);
        if (Physics.Raycast(ray, out RaycastHit hit, distance, obstacleLayerMask))
        {
            Debug.Log($"[KeyCollectible] Raycast hit: {hit.transform.name}");
            
            bool isKeyHit = hit.transform == transform || hit.transform.IsChildOf(transform);
            Debug.Log($"[KeyCollectible] Is key hit: {isKeyHit}");
            return isKeyHit;
        }

        Debug.Log("[KeyCollectible] No obstacles detected, key is visible");
        return true; 
    }
    private void CollectKey()
    {
        if (isCollected) return;

        isCollected = true;

        
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.CollectKey(keyID, targetLevelName);
        }

     
        PlayCollectionEffects();

   
        if (keyVisual != null)
            keyVisual.SetActive(false);

        if (collectPrompt != null)
            collectPrompt.SetActive(false);

 
        Invoke(nameof(DestroyKey), 2f);

        Debug.Log($"[KeyCollectible] 收集了钥匙: {keyID} (解锁: {targetLevelName})");
    }

    private void PlayCollectionEffects()
    {
        if (collectEffect != null)
        {
            GameObject effect = Instantiate(collectEffect, transform.position, transform.rotation);
            Destroy(effect, 3f);
        }

        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
    }

    private void DestroyKey()
    {
        gameObject.SetActive(false);
    }

    private bool IsPlayer(Collider other)
    {
        return ((1 << other.gameObject.layer) & playerLayer) != 0;
    }

    public void SetKeyData(string id, string targetLevel)
    {
        keyID = id;
        targetLevelName = targetLevel;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (playerCamera != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(playerCamera.transform.position, transform.position);
        }
    }
}