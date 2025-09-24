
using UnityEditor;
using UnityEngine;

public class CheckPointSystem : MonoBehaviour
{
    [Header("CheckPoint Settings")]
    [SerializeField] private string checkPointID;
    [SerializeField] private bool autoGenerateID = true;
    [SerializeField] private bool activateOnTrigger = true;
    [SerializeField] private bool saveProgress = true;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject inactiveVisual;
    [SerializeField] private GameObject activeVisual;
    [SerializeField] private ParticleSystem activationEffect;
    [SerializeField] private AudioClip activationSound;
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private Color activeColor = Color.green;

    [Header("Spline Integration")]
    [SerializeField] private bool useSplinePosition = true;
    [SerializeField] private float splineT = 0f; 

    private bool isActivated = false;
    private Renderer checkPointRenderer;
    private Light checkPointLight;

    public string CheckPointID => checkPointID;
    public bool IsActivated => isActivated;
    public Vector3 RespawnPosition => transform.position;

    void Awake()
    {
      
        if (autoGenerateID && string.IsNullOrEmpty(checkPointID))
        {
            checkPointID = $"CP_{gameObject.name}_{transform.position.GetHashCode()}";
        }

      
        checkPointRenderer = GetComponent<Renderer>();
        checkPointLight = GetComponentInChildren<Light>();

     
        UpdateVisuals();
    }

    void Start()
    {
      
        string currentLevel = GameProgressManager.Instance?.GetCurrentLevelName();
        if (!string.IsNullOrEmpty(currentLevel))
        {
            bool wasActivated = GameProgressManager.Instance.IsCheckPointActivated(currentLevel, checkPointID);
            if (wasActivated && !isActivated)
            {
                ActivateCheckPoint(false);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!activateOnTrigger || isActivated) return;

      
        if (other.GetComponent<PlayerHealth>() != null)
        {
            ActivateCheckPoint(true);
        }
    }

    public void ActivateCheckPoint(bool playEffects = true)
    {
        if (isActivated) return;

        isActivated = true;

       
        float currentSplineT = CalculateSplineT();

       
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.ActivateCheckPoint(checkPointID, transform.position, currentSplineT);

         
            if (saveProgress)
            {
                GameProgressManager.Instance.SaveProgressToPlayerPrefs();
            }
        }

       
        UpdateVisuals();

       
        if (playEffects)
        {
            PlayActivationEffects();
        }

        Debug.Log($"[CheckPoint] CheckPoint {checkPointID} activated at position {transform.position}");
    }

    private float CalculateSplineT()
    {
        if (!useSplinePosition)
        {
            return splineT;
        }

       
        var splineRunner = FindFirstObjectByType<SplineRunnerRB>();
        if (splineRunner != null)
        {
           
            return 0f; 
        }

        return splineT;
    }

    private void UpdateVisuals()
    {
       
        if (inactiveVisual != null)
        {
            inactiveVisual.SetActive(!isActivated);
        }

        if (activeVisual != null)
        {
            activeVisual.SetActive(isActivated);
        }

      
        if (checkPointRenderer != null)
        {
            Color targetColor = isActivated ? activeColor : inactiveColor;
            checkPointRenderer.material.color = targetColor;
        }

    
        if (checkPointLight != null)
        {
            checkPointLight.color = isActivated ? activeColor : inactiveColor;
            checkPointLight.intensity = isActivated ? 1f : 0.3f;
        }
    }

    private void PlayActivationEffects()
    {
        
        if (activationEffect != null)
        {
            activationEffect.Play();
        }

       
        if (activationSound != null)
        {
            AudioSource.PlayClipAtPoint(activationSound, transform.position);
        }

     
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Activate");
        }
    }

 
    [ContextMenu("Reset CheckPoint")]
    public void ResetCheckPoint()
    {
        isActivated = false;
        UpdateVisuals();

       
        string currentLevel = GameProgressManager.Instance?.GetCurrentLevelName();
        if (!string.IsNullOrEmpty(currentLevel))
        {
         
            Debug.Log($"[CheckPoint] CheckPoint {checkPointID} reset (may need manual cleanup in GameProgressManager)");
        }
    }

  
    public float GetDistanceToPlayer()
    {
        var player = FindFirstObjectByType<PlayerHealth>();
        if (player != null)
        {
            return Vector3.Distance(transform.position, player.transform.position);
        }
        return float.MaxValue;
    }

    
    void OnDrawGizmos()
    {
        Gizmos.color = isActivated ? activeColor : inactiveColor;
        Gizmos.DrawWireSphere(transform.position, 1f);

       
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, checkPointID);
#endif
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 1.5f);

       
        var triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            Gizmos.color = Color.green;
            if (triggerCollider is SphereCollider sphere)
            {
                Gizmos.DrawWireSphere(transform.position, sphere.radius);
            }
            else if (triggerCollider is BoxCollider box)
            {
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                Gizmos.DrawWireCube(Vector3.zero, box.size);
                Gizmos.matrix = Matrix4x4.identity;
            }
        }
    }
}


