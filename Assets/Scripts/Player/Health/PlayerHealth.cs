using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;

    private HealthSystem healthSystem;
    private IHealthUI healthUI;


    public IHealth Health => healthSystem;
    public IHealthEvents HealthEvents => healthSystem;

    void Awake()
    {
       
        healthSystem = new HealthSystem(maxHealth);

      
        healthSystem.OnDeath += HandleDeath;
    }

    void Start()
    {
      
        healthUI = FindFirstObjectByType<PlayerHealthUI>();
        if (healthUI != null)
        {
            Debug.Log("[PlayerHealth] Found PlayerHealthUI, connecting...");
           
            ConnectToUI();
        }
        else
        {
            Debug.LogWarning("[PlayerHealth] No PlayerHealthUI found in scene!");
         
            StartCoroutine(DelayedUISearch());
        }
    }

    private System.Collections.IEnumerator DelayedUISearch()
    {
      
        for (int i = 0; i < 5; i++)
        {
            yield return null;
            healthUI = FindFirstObjectByType<PlayerHealthUI>();
            if (healthUI != null)
            {
                Debug.Log("[PlayerHealth] Found PlayerHealthUI on delayed search, connecting...");
                ConnectToUI();
                yield break;
            }
        }
        Debug.LogError("[PlayerHealth] Could not find PlayerHealthUI component after delayed search!");
    }

    void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDeath -= HandleDeath;
        }
    }

    private void ConnectToUI()
    {
        Debug.Log("[PlayerHealth] Connecting to UI...");

      
        healthSystem.OnHealthChanged += healthUI.UpdateHealthDisplay;
        healthSystem.OnDamage += healthUI.PlayDamageEffect;
        healthSystem.OnHeal += healthUI.PlayHealEffect;

       
        healthSystem.OnHealthChanged += (current, max) => {
            Debug.Log($"[PlayerHealth] Health changed event fired: {current}/{max}");
        };

        healthSystem.OnHeal += (amount) => {
            Debug.Log($"[PlayerHealth] Heal event fired: +{amount} health");
        };

      
        Debug.Log($"[PlayerHealth] Initializing UI with health: {healthSystem.CurrentHealth}/{healthSystem.MaxHealth}");
        healthUI.UpdateHealthDisplay(healthSystem.CurrentHealth, healthSystem.MaxHealth);

        Debug.Log($"[PlayerHealth] UI connected successfully!");
    }

   
    [ContextMenu("Debug Health Status")]
    public void DebugHealthStatus()
    {
        Debug.Log($"[PlayerHealth] Current Health: {healthSystem.CurrentHealth}/{healthSystem.MaxHealth}");
        Debug.Log($"[PlayerHealth] Is Dead: {healthSystem.IsDead}");

        if (healthUI != null)
        {
            Debug.Log("[PlayerHealth] Forcing UI update...");
            healthUI.UpdateHealthDisplay(healthSystem.CurrentHealth, healthSystem.MaxHealth);
        }
        else
        {
            Debug.LogWarning("[PlayerHealth] UI reference is null!");
        }
    }

    [ContextMenu("Force UI Refresh")]
    public void ForceUIRefresh()
    {
        if (healthUI != null)
        {
         
            for (int i = 0; i < 3; i++)
            {
                bool shouldBeVisible = i < healthSystem.CurrentHealth;
                float targetAlpha = shouldBeVisible ? 1f : 0f;

              
                var playerHealthUI = healthUI as PlayerHealthUI;
                if (playerHealthUI != null)
                {
                  
                    Debug.Log($"[PlayerHealth] Setting heart {i} alpha to {targetAlpha}");
                }
            }

            healthUI.UpdateHealthDisplay(healthSystem.CurrentHealth, healthSystem.MaxHealth);
        }
    }

    [ContextMenu("Test Heal")]
    public void TestHeal()
    {
        bool healed = healthSystem.Heal(1);
        Debug.Log($"[PlayerHealth] Test heal result: {healed}");
    }

    [ContextMenu("Test Damage")]
    public void TestDamage()
    {
        bool damaged = healthSystem.TakeDamage(1);
        Debug.Log($"[PlayerHealth] Test damage result: {damaged}");
    }

    private void HandleDeath()
    {
       
        StartCoroutine(HandleDeathRespawn());
    }

    private System.Collections.IEnumerator HandleDeathRespawn()
    {
       
        yield return new UnityEngine.WaitForSeconds(1f);

        string currentLevel = GameProgressManager.Instance?.GetCurrentLevelName();

        Debug.Log($"[PlayerHealth] Handling death respawn for level: {currentLevel}");

      
        if (GameProgressManager.Instance != null)
        {
            var allCheckPoints = GameProgressManager.Instance.GetAllCheckPointsForLevel(currentLevel);
            Debug.Log($"[PlayerHealth] Found {allCheckPoints.Count} total checkpoints for level {currentLevel}");

            foreach (var cp in allCheckPoints)
            {
                Debug.Log($"[PlayerHealth] CheckPoint: {cp.checkPointID}, Time: {cp.activationTime}");
            }
        }

      
        if (!string.IsNullOrEmpty(currentLevel) &&
            GameProgressManager.Instance.GetLatestCheckPoint(currentLevel, out CheckPointData latestCheckPoint))
        {
            Debug.Log($"[PlayerHealth] Found latest checkpoint: {latestCheckPoint.checkPointID} at {latestCheckPoint.position}");
            RespawnAtCheckPoint(latestCheckPoint);
        }
        else
        {
            Debug.Log("[PlayerHealth] No checkpoints found, returning to Hub");

           
            if (!string.IsNullOrEmpty(currentLevel))
            {
                GameProgressManager.Instance.SetLevelStatus(currentLevel, LevelStatus.NotStarted);
                Debug.Log($"[PlayerHealth] Level {currentLevel} marked as not completed due to death");
            }

          
            ReturnToHub();
        }
    }

    private void RespawnAtCheckPoint(CheckPointData checkPoint)
    {
      
        healthSystem.ResetToMaxHealth();

      
        var splineRunner = GetComponent<SplineRunnerRB>();
        if (splineRunner != null)
        {
            splineRunner.MarkSpawnedBySpawner();
            splineRunner.ResnapToWorldPosition(checkPoint.position);
            Debug.Log($"[PlayerHealth] Player respawned at checkpoint {checkPoint.checkPointID} at position {checkPoint.position}");
        }
        else
        {
       
            transform.position = checkPoint.position;
            Debug.Log($"[PlayerHealth] Player position reset to checkpoint {checkPoint.checkPointID}");
        }
    }

    private void ReturnToHub()
    {
        if (GameProgressManager.Instance != null)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                GameProgressManager.Instance.hubSceneName
            );
        }
        else
        {
            Debug.LogError("[PlayerHealth] GameProgressManager not found, cannot return to hub!");
        }
    }

  
    public bool TakeDamage(int damage) => healthSystem.TakeDamage(damage);
    public bool Heal(int amount) => healthSystem.Heal(amount);
    public void ResetHealth() => healthSystem.ResetToMaxHealth();
}