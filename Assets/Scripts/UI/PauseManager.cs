using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;


public class PauseManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject gameUI;

    [Header("Pause Panel Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button returnToMenuButton;

    [Header("Settings Panel Buttons")]
    [SerializeField] private Button settingsBackButton;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference pauseActionRef;
    [SerializeField] private InputActionReference cancelActionRef;

    [Header("Scene Settings")]
    [SerializeField] private string openingSceneName = "OpeningUI";

    [Header("Auto Selection Settings")]
    [SerializeField] private bool autoSelectFirstButton = false;

    private bool isPaused = false;

    private void Awake()
    {
        
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);

        if (returnToMenuButton != null)
            returnToMenuButton.onClick.AddListener(OnReturnToMenuClicked);

        if (settingsBackButton != null)
            settingsBackButton.onClick.AddListener(OnSettingsBackClicked);

     
        ShowGameUI();
    }

    private void OnEnable()
    {
 
        if (pauseActionRef != null && pauseActionRef.action != null)
        {
            pauseActionRef.action.Enable();
            pauseActionRef.action.performed += OnPausePerformed;
        }

     
        if (cancelActionRef != null && cancelActionRef.action != null)
        {
            cancelActionRef.action.Enable();
            cancelActionRef.action.performed += OnCancelPerformed;
        }
    }

    private void OnDisable()
    {
     
        if (pauseActionRef != null && pauseActionRef.action != null)
        {
            pauseActionRef.action.performed -= OnPausePerformed;
        }

        if (cancelActionRef != null && cancelActionRef.action != null)
        {
            cancelActionRef.action.performed -= OnCancelPerformed;
        }
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        if (settingsPanel != null && settingsPanel.activeSelf)
        {
          
            OnSettingsBackClicked();
        }
        else
        {
           
            TogglePause();
        }
    }

    private void OnCancelPerformed(InputAction.CallbackContext context)
    {
        if (settingsPanel != null && settingsPanel.activeSelf)
        {
         
            OnSettingsBackClicked();
        }
        else if (isPaused)
        {
         
            ResumeGame();
        }
    }

 
    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }


    public void PauseGame()
    {
        isPaused = true;

     
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PauseGame();
        }
        else
        {
         
            Time.timeScale = 0f;
        }

        ShowPauseMenu();

        Debug.Log("[PauseManager] Game paused");
    }


    public void ResumeGame()
    {
        isPaused = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
        else
        {
         
            Time.timeScale = 1f;
        }

        ShowGameUI();

        Debug.Log("[PauseManager] Game resumed");
    }

    private void OnResumeClicked()
    {
        ResumeGame();
    }

    private void OnSettingsClicked()
    {
        ShowSettingsPanel();
    }

    private void OnReturnToMenuClicked()
    {
  
        Time.timeScale = 1f;

   
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopNamedLoop("BGM_Game");
            SoundManager.Instance.StopNamedLoop("BGM_Level1");
            SoundManager.Instance.StopNamedLoop("BGM_Level2");
            SoundManager.Instance.StopNamedLoop("BGM_Battle");
        }


        SceneManager.LoadScene(openingSceneName);

        Debug.Log($"[PauseManager] Returning to menu: {openingSceneName}");
    }

    private void OnSettingsBackClicked()
    {
        ShowPauseMenu();
    }

    private void ShowGameUI()
    {
        SetPanelActive(gameUI, true);
        SetPanelActive(pauseMenuPanel, false);
        SetPanelActive(settingsPanel, false);

        StartCoroutine(ClearSelectionNextFrame());
    }

    private void ShowPauseMenu()
    {
        SetPanelActive(gameUI, false);
        SetPanelActive(pauseMenuPanel, true);
        SetPanelActive(settingsPanel, false);


        if (autoSelectFirstButton && resumeButton != null)
        {
            StartCoroutine(SelectButtonNextFrame(resumeButton));
        }
        else
        {
            StartCoroutine(ClearSelectionNextFrame());
        }
    }

    private void ShowSettingsPanel()
    {
        SetPanelActive(gameUI, false);
        SetPanelActive(pauseMenuPanel, false);
        SetPanelActive(settingsPanel, true);

      
        StartCoroutine(ClearSelectionNextFrame());
    }

    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }

    private System.Collections.IEnumerator ClearSelectionNextFrame()
    {
        yield return null;
        ClearSelection();
    }

    private System.Collections.IEnumerator SelectButtonNextFrame(Button button)
    {
        yield return null;
        if (button != null)
        {
            button.Select();
        }
    }

    private void ClearSelection()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem != null)
        {
            eventSystem.SetSelectedGameObject(null);
        }
    }

    private void OnDestroy()
    {
       
        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(OnResumeClicked);

        if (settingsButton != null)
            settingsButton.onClick.RemoveListener(OnSettingsClicked);

        if (returnToMenuButton != null)
            returnToMenuButton.onClick.RemoveListener(OnReturnToMenuClicked);

        if (settingsBackButton != null)
            settingsBackButton.onClick.RemoveListener(OnSettingsBackClicked);

      
        Time.timeScale = 1f;
    }
}