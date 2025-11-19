using UnityEngine;
using UnityEngine.SceneManagement;

public class CavePortal : MonoBehaviour
{
    [Header("Portal Settings")]
    [SerializeField] private string targetSceneName = "Level1";
    [SerializeField] private string levelDisplayName = "";
    [SerializeField] private LayerMask playerLayer = 1;

    [Header("UI References")]
    [SerializeField] private GameObject levelUI; 
    [SerializeField] private Transform uiLookAtTarget; 

    [Header("Visual Effects")]
    [SerializeField] private GameObject portalEffect;
    [SerializeField] private GameObject interactionPrompt;

    private bool _playerInRange = false;
    private SplineRunnerRB _currentPlayer;
    private float _originalMeshOffsetY;
    private LevelUIController _levelUIController;

    void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        if (levelUI != null)
        {
            levelUI.SetActive(false);
            _levelUIController = levelUI.GetComponent<LevelUIController>();
            if (_levelUIController != null)
            {
                _levelUIController.Initialize(this, levelDisplayName, targetSceneName);
            }
        }
    }

    

    void OnTriggerEnter(Collider other)
    {
        if (IsPlayer(other))
        {
            _playerInRange = true;
            _currentPlayer = other.GetComponent<SplineRunnerRB>();

            if (_currentPlayer != null)
            {
               
                _originalMeshOffsetY = _currentPlayer.meshFacingOffsetY;

              
                bool isMovingPositive = _currentPlayer.IsMovingPositive;
                _currentPlayer.meshFacingOffsetY = isMovingPositive ? -90f : 90f;

                Debug.Log($"[CavePortal] Player moving direction: {(isMovingPositive ? "Right" : "Left")}, meshOffset set to: {_currentPlayer.meshFacingOffsetY}");
            }

          
            if (levelUI != null)
            {
                levelUI.SetActive(true);
                
                Vector3 uiPosition = other.transform.position + other.transform.forward * 2f + Vector3.up * 1.5f;
                levelUI.transform.position = uiPosition;

                
                if (_levelUIController != null)
                {
                    _levelUIController.RefreshLevelStatus();
                }
            }

            if (interactionPrompt != null)
                interactionPrompt.SetActive(true);

            Debug.Log($"[CavePortal] Player approached {levelDisplayName}");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IsPlayer(other) && _playerInRange)
        {
            ExitPortal();
        }
    }

    public void OnChallengeSelected()
    {
        if (_currentPlayer == null) return;

        if (GameProgressManager.Instance == null || !GameProgressManager.Instance.CanAccessLevel(targetSceneName))
        {
      
            return;
        }

        Vector3 currentPos = _currentPlayer.transform.position;
        float currentT = _currentPlayer.GetCurrentT();
        GameProgressManager.Instance.SaveHubPosition(currentPos, currentT);

        Debug.Log($"[CavePortal] Entering challenge: {targetSceneName}");
        SceneManager.LoadScene(targetSceneName);
    }
    public void OnSkipSelected()
    {
        Debug.Log($"[CavePortal] Player skipped {levelDisplayName}");
        ExitPortal();
    }

    private void ExitPortal()
    {
        _playerInRange = false;

       
        if (_currentPlayer != null)
        {
            _currentPlayer.meshFacingOffsetY = _originalMeshOffsetY;
            _currentPlayer = null;
        }

       
        if (levelUI != null)
            levelUI.SetActive(false);

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
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