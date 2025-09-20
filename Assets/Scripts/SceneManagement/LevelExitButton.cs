using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelExitButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button exitButton;
    [SerializeField] private GameObject exitUI;
    [SerializeField] private Button confirmExitButton;
    [SerializeField] private Button cancelExitButton;
    [SerializeField] private TextMeshProUGUI warningText;

   

    private bool _hasCheckPoint = false;

    void Start()
    {
       
        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitButtonClicked);

        if (confirmExitButton != null)
            confirmExitButton.onClick.AddListener(OnConfirmExit);

        if (cancelExitButton != null)
            cancelExitButton.onClick.AddListener(OnCancelExit);

        if (exitUI != null)
            exitUI.SetActive(false);

      
        CheckForExistingCheckPoint();

      

    }





    private void CheckForExistingCheckPoint()
    {
        if (GameProgressManager.Instance != null)
        {
            string currentLevel = GameProgressManager.Instance.GetCurrentLevelName();
            if (!string.IsNullOrEmpty(currentLevel))
            {
                var checkPoints = GameProgressManager.Instance.GetAllCheckPointsForLevel(currentLevel);
                _hasCheckPoint = checkPoints.Count > 0;
                UpdateUI();
            }
        }
    }

  
    private void UpdateUI()
    {
        
        if (exitButton != null)
        {
            exitButton.interactable = true; 

          
            var buttonText = exitButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = _hasCheckPoint ? "Exit Level" : "Give up Level";
            }
        }
    }

    public void OnExitButtonClicked()
    {
        if (exitUI != null)
        {
            exitUI.SetActive(true);

         
            if (warningText != null)
            {
                warningText.text = "Level progression will lose, are you sure to exit?";
            }

       
            Time.timeScale = 0f;
        }
    }

    public void OnConfirmExit()
    {
        
        Time.timeScale = 1f;

        if (GameProgressManager.Instance == null)
        {
            Debug.LogError("[LevelExitButton] GameProgressManager not found!");
            return;
        }

        string currentLevel = GameProgressManager.Instance.GetCurrentLevelName();
        if (string.IsNullOrEmpty(currentLevel))
        {
            Debug.LogError("[LevelExitButton] Current level name is empty!");
            return;
        }

        else
        {
         
            GameProgressManager.Instance.SetLevelStatus(currentLevel, LevelStatus.NotStarted);
            GameProgressManager.Instance.ClearCheckPointsForLevel(currentLevel);
            Debug.Log($"[LevelExitButton] Exiting without progress for {currentLevel}");
        }

      
        SceneManager.LoadScene(GameProgressManager.Instance.hubSceneName);
    }

    public void OnCancelExit()
    {
    
        Time.timeScale = 1f;

        if (exitUI != null)
            exitUI.SetActive(false);
    }
}


public static class CheckPointEvents
{
    public static System.Action<string> OnCheckPointActivated;
}