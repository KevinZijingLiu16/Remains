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

    [Header("UI Elements - Locked")]
    [SerializeField] private GameObject lockedPanel;
    [SerializeField] private TextMeshProUGUI lockedLevelNameText;
    [SerializeField] private TextMeshProUGUI lockedMessageText;
    [SerializeField] private Button lockedExitButton;



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

        // 锁定状态按钮
        if (lockedExitButton != null)
            lockedExitButton.onClick.AddListener(OnSkipClicked);

    }

    public void RefreshLevelStatus()
    {
        if (GameProgressManager.Instance == null) return;

        LevelStatus status = GameProgressManager.Instance.GetLevelStatus(_levelSceneName);
        bool hasKey = GameProgressManager.Instance.CanAccessLevel(_levelSceneName);

        // 隐藏所有面板
        if (firstTimePanel != null) firstTimePanel.SetActive(false);
        if (completedPanel != null) completedPanel.SetActive(false);
        if (lockedPanel != null) lockedPanel.SetActive(false);

        switch (status)
        {
            case LevelStatus.NotStarted:
                if (hasKey)
                {
                    ShowFirstTimeUI();
                }
                else
                {
                    ShowLockedUI();
                }
                break;
            case LevelStatus.Completed:
                if (hasKey)
                {
                    ShowCompletedUI();
                }
                else
                {
                    ShowLockedUI(); // 理论上不会发生，但保险起见
                }
                break;
        }
    }

    private void ShowLockedUI()
    {
        if (lockedPanel != null)
        {
            lockedPanel.SetActive(true);
            if (lockedLevelNameText != null)
                lockedLevelNameText.text = _levelName;
            if (lockedMessageText != null)
                lockedMessageText.text = "Missing key for this level";
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
