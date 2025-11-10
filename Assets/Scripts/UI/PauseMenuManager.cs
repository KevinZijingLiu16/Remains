using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class PauseMenuManager : MonoBehaviour
{
    [Header("Pause Menu Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Main Pause Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Settings Panel")]
    [SerializeField] private Button settingsBackButton;

    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "OpeningUI";

    [Header("Input Actions")]
    [SerializeField] private InputActionReference pauseActionRef;
    [SerializeField] private InputActionReference cancelActionRef;

    private bool _isPaused = false;

    private void Awake()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        if (settingsBackButton != null)
            settingsBackButton.onClick.AddListener(OnSettingsBackClicked);

        SetPauseMenuActive(false);
    }

    private void OnEnable()
    {
        if (pauseActionRef != null && pauseActionRef.action != null)
        {
            pauseActionRef.action.Enable();
            pauseActionRef.action.performed += OnPausePerformed;
        }
        else
        {
            Debug.LogWarning("[PauseMenuManager] Pause Action Reference not assigned!");
        }

        if (cancelActionRef != null && cancelActionRef.action != null)
        {
            cancelActionRef.action.Enable();
            cancelActionRef.action.performed += OnCancelPerformed;
        }
        else
        {
            Debug.LogWarning("[PauseMenuManager] Cancel Action Reference not assigned!");
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
        if (!_isPaused)
        {
            Pause();
        }
        else if (pauseMenuPanel != null && pauseMenuPanel.activeSelf)
        {
            Resume();
        }
    }

    private void OnCancelPerformed(InputAction.CallbackContext context)
    {
        if (!_isPaused)
            return;

        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            OnSettingsBackClicked();
        }
        else if (pauseMenuPanel != null && pauseMenuPanel.activeSelf)
        {
            Resume();
        }
    }

    public void Pause()
    {
        _isPaused = true;
        Time.timeScale = 0f;

        ShowPauseMenu();

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.Play("UI_Pause");
        }

        Debug.Log("[PauseMenuManager] Game Paused");
    }

    public void Resume()
    {
        _isPaused = false;
        Time.timeScale = 1f;

        SetPauseMenuActive(false);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.Play("UI_Resume");
        }

        Debug.Log("[PauseMenuManager] Game Resumed");
    }

    private void OnResumeClicked()
    {
        Resume();
    }

    private void OnSettingsClicked()
    {
        ShowSettingsPanel();
    }

    private void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        _isPaused = false;

        Debug.Log($"[PauseMenuManager] Loading main menu: {mainMenuSceneName}");
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnSettingsBackClicked()
    {
        ShowPauseMenu();
    }

    private void ShowPauseMenu()
    {
        SetPanelActive(pauseMenuPanel, true);
        SetPanelActive(settingsPanel, false);

        if (resumeButton != null)
        {
            resumeButton.Select();
        }
    }

    private void ShowSettingsPanel()
    {
        SetPanelActive(pauseMenuPanel, false);
        SetPanelActive(settingsPanel, true);

        if (settingsBackButton != null)
        {
            settingsBackButton.Select();
        }
    }

    private void SetPauseMenuActive(bool active)
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(active);
        }

        if (settingsPanel != null && !active)
        {
            settingsPanel.SetActive(false);
        }
    }

    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }

    public bool IsPaused => _isPaused;

    private void OnDestroy()
    {
        Time.timeScale = 1f;

        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(OnResumeClicked);

        if (settingsButton != null)
            settingsButton.onClick.RemoveListener(OnSettingsClicked);

        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);

        if (settingsBackButton != null)
            settingsBackButton.onClick.RemoveListener(OnSettingsBackClicked);
    }
}