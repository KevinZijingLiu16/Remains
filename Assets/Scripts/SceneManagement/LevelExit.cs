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

        if (completionPrompt != null)
            completionPrompt.SetActive(true);

      
        if (_audioSource != null && completionSound != null)
            _audioSource.PlayOneShot(completionSound);

        Debug.Log($"[LevelExit] Level {currentLevel} completed successfully!");

       
        Invoke(nameof(ReturnToHub), completionDelay);
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