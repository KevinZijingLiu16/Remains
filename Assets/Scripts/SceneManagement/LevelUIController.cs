using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUIController : MonoBehaviour
{
    [Header("UI Elements - First Time")]
    [SerializeField] private GameObject firstTimePanel; 
    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private Button challengeButton;
    [SerializeField] private Button skipButton;

    [Header("UI Elements - Completed")]
    [SerializeField] private GameObject completedPanel;
    [SerializeField] private TextMeshProUGUI completedLevelNameText;
    [SerializeField] private TextMeshProUGUI completionStatusText;
    [SerializeField] private TextMeshProUGUI bestTimeText;
    [SerializeField] private TextMeshProUGUI starRatingText;
    [SerializeField] private Button rechallengeButton;
    [SerializeField] private Button exitButton;



    private CavePortal _parentPortal;
    private string _levelName;
    private string _levelSceneName;

    public void Initialize(CavePortal portal, string levelName, string sceneName)
    {
        _parentPortal = portal;
        _levelName = levelName;
        _levelSceneName = sceneName;

    
        SetupButtonEvents();

     
        RefreshLevelStatus();
    }

    private void SetupButtonEvents()
    {
       
        if (challengeButton != null)
            challengeButton.onClick.AddListener(OnChallengeClicked);
        if (skipButton != null)
            skipButton.onClick.AddListener(OnSkipClicked);

     
        if (rechallengeButton != null)
            rechallengeButton.onClick.AddListener(OnChallengeClicked);
        if (exitButton != null)
            exitButton.onClick.AddListener(OnSkipClicked);

       
       
    }

    public void RefreshLevelStatus()
    {
        if (GameProgressManager.Instance == null) return;

        LevelStatus status = GameProgressManager.Instance.GetLevelStatus(_levelSceneName);

     
        if (firstTimePanel != null) firstTimePanel.SetActive(false);
        if (completedPanel != null) completedPanel.SetActive(false);
    

        switch (status)
        {
            case LevelStatus.NotStarted:
                ShowFirstTimeUI();
                break;
         
            case LevelStatus.Completed:
                ShowCompletedUI();
                break;
        }
    }

    private void ShowFirstTimeUI()
    {
        if (firstTimePanel != null)
        {
            firstTimePanel.SetActive(true);
            if (levelNameText != null)
                levelNameText.text = _levelName;
        }
    }


    private void ShowCompletedUI()
    {
        if (completedPanel != null)
        {
            completedPanel.SetActive(true);
            if (completedLevelNameText != null)
                completedLevelNameText.text = _levelName;

            var progress = GameProgressManager.Instance.GetLevelProgress(_levelSceneName);
            if (progress != null)
            {
               
                if (completionStatusText != null)
                    completionStatusText.text = "Completed";

          
                if (bestTimeText != null)
                {
                    if (progress.bestTime < float.MaxValue)
                    {
                        bestTimeText.text = $"Best Time: {FormatTime(progress.bestTime)}";
                    }
                    else
                    {
                        bestTimeText.text = "Best Time: --:--";
                    }
                }


                if (starRatingText != null)
                {
                    starRatingText.text = $"Rating: {progress.starRating} Stars";
                }
                }
        }
    }

    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60);
        int secs = Mathf.FloorToInt(seconds % 60);
        return $"{minutes:00}:{secs:00}";
    }

    #region Button Events

    private void OnChallengeClicked()
    {
        _parentPortal?.OnChallengeSelected();
    }

    private void OnSkipClicked()
    {
        _parentPortal?.OnSkipSelected();
    }

    #endregion
}
