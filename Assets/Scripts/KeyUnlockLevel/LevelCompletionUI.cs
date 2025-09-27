using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelCompletionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject completionPanel;
    [SerializeField] private TextMeshProUGUI levelCompleteText;
    [SerializeField] private TextMeshProUGUI keyFoundText;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button returnToHubButton;
    [SerializeField] private GameObject keyFoundPanel;

    private string nextLevelKey;
    private string nextLevelScene;
    private bool hasFoundKey;

    void Start()
    {
        if (completionPanel != null)
            completionPanel.SetActive(false);

        SetupButtons();
    }

    private void SetupButtons()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueToNextLevel);

        if (returnToHubButton != null)
            returnToHubButton.onClick.AddListener(OnReturnToHub);
    }

    public void ShowCompletionUI(string currentLevel, string foundKey = "", string nextLevel = "")
    {
        if (completionPanel == null) return;

        nextLevelKey = foundKey;
        nextLevelScene = nextLevel;
        hasFoundKey = !string.IsNullOrEmpty(foundKey);

        completionPanel.SetActive(true);

        if (levelCompleteText != null)
            levelCompleteText.text = $"{currentLevel} Completed!";

 
        if (hasFoundKey)
        {
            if (keyFoundPanel != null)
                keyFoundPanel.SetActive(true);

            if (keyFoundText != null)
                keyFoundText.text = $"Found {nextLevel} key!";

            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(true);
                continueButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Continue {nextLevel}";
            }
        }
        else
        {
            if (keyFoundPanel != null)
                keyFoundPanel.SetActive(false);

            if (continueButton != null)
                continueButton.gameObject.SetActive(false);
        }

      
        Time.timeScale = 0f;
    }

    private void OnContinueToNextLevel()
    {
        Time.timeScale = 1f;

        if (GameProgressManager.Instance != null && !string.IsNullOrEmpty(nextLevelScene))
        {
           
            Vector3 currentPos = Vector3.zero;
            float currentT = 0f;

            var player = FindFirstObjectByType<SplineRunnerRB>();
            if (player != null)
            {
                currentPos = player.transform.position;
                currentT = player.GetCurrentT();
            }

            GameProgressManager.Instance.SaveHubPosition(currentPos, currentT);
            SceneManager.LoadScene(nextLevelScene);
        }
    }

    private void OnReturnToHub()
    {
        Time.timeScale = 1f;

        if (GameProgressManager.Instance != null)
        {
            SceneManager.LoadScene(GameProgressManager.Instance.hubSceneName);
        }
    }
}