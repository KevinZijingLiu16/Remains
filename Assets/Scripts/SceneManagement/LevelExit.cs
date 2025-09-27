using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    [Header("Exit Settings")]
    [SerializeField] private LayerMask playerLayer = 1;
    [SerializeField] private float completionDelay = 1f;

    [Header("Completion Rewards")]
    [SerializeField] private int baseStarRating = 1;
    [SerializeField] private float threeStarTime = 60f;
    [SerializeField] private float twoStarTime = 90f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject exitEffect;
    [SerializeField] private GameObject completionPrompt;

    [Header("Audio")]
    [SerializeField] private AudioClip completionSound;



    [Header("Level Progression")]
    [SerializeField] private LevelCompletionUI levelCompletionUI;

    [Header("Level Key Mapping")]
    [SerializeField] private List<LevelKeyMapping> levelKeyMappings = new List<LevelKeyMapping>();

    [System.Serializable]
    public class LevelKeyMapping
    {
        public string currentLevelName;
        public string nextLevelKey;
        public string nextLevelScene;
    }

    private AudioSource _audioSource;
    private bool _hasCompleted = false;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        if (completionPrompt != null)
            completionPrompt.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsPlayer(other) && !_hasCompleted)
        {
            CompleteLevel();
        }
    }

    private void CompleteLevel()
    {
        _hasCompleted = true;
        if (GameProgressManager.Instance == null)
        {
            Debug.LogError("[LevelExit] GameProgressManager not found!");
            return;
        }

        string currentLevel = GameProgressManager.Instance.GetCurrentLevelName();
        if (string.IsNullOrEmpty(currentLevel))
        {
            Debug.LogError("[LevelExit] Current level name is empty!");
            return;
        }

      
        GameProgressManager.Instance.SetLevelCompleted(currentLevel, true);
        GameProgressManager.Instance.SetLevelStarRating(currentLevel, 3);
        GameProgressManager.Instance.SetLevelStatus(currentLevel, LevelStatus.Completed);

      
        GameProgressManager.Instance.ClearCheckPointsForLevel(currentLevel);

       
        if (_audioSource != null && completionSound != null)
            _audioSource.PlayOneShot(completionSound);

        if (completionPrompt != null)
            completionPrompt.SetActive(true);

        Debug.Log($"[LevelExit] Level {currentLevel} completed successfully!");

    
        CheckForNextLevelKey(currentLevel);
    }

    private void CheckForNextLevelKey(string currentLevel)
    {
        Debug.Log($"[LevelExit] Looking for mappings for current level: '{currentLevel}'");
        Debug.Log($"[LevelExit] Available mappings count: {levelKeyMappings.Count}");
        string nextLevelKey = "";
        string nextLevelScene = "";

     
        foreach (var mapping in levelKeyMappings)
        {
            if (mapping.currentLevelName == currentLevel)
            {
                nextLevelKey = mapping.nextLevelKey;
                nextLevelScene = mapping.nextLevelScene;
                break;
            }
        }

        if (!string.IsNullOrEmpty(nextLevelScene) && !string.IsNullOrEmpty(nextLevelKey))
        {
         
            bool hasNextLevelKey = GameProgressManager.Instance.HasKey(nextLevelKey);

            Debug.Log($"[LevelExit] Checking for key: {nextLevelKey}, hasKey: {hasNextLevelKey}");

    
            if (hasNextLevelKey)
            {
                ShowCompletionUI(currentLevel, nextLevelKey, nextLevelScene);
            }
            else
            {
                ShowCompletionUI(currentLevel, "", "");
            }
        }
        else
        {
            ShowCompletionUI(currentLevel, "", "");
        }
    }

    private void ShowCompletionUI(string currentLevel, string foundKey, string nextLevel)
    {
     
        if (levelCompletionUI == null)
        {
            levelCompletionUI = FindFirstObjectByType<LevelCompletionUI>();
        }

        if (levelCompletionUI == null)
        {
            Debug.LogWarning("[LevelExit] No LevelCompletionUI found in scene!");
           
            Invoke(nameof(ReturnToHub), completionDelay);
            return;
        }

        levelCompletionUI.ShowCompletionUI(currentLevel, foundKey, nextLevel);
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
}