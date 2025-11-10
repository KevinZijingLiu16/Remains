using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "DemoV1New";

    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject exitConfirmPanel;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button settingsBackButton;
    [SerializeField] private Button exitYesButton;
    [SerializeField] private Button exitNoButton;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference cancelActionRef;

    [Header("Auto Selection Settings")]
    [SerializeField] private bool autoSelectFirstButton = false;

    private void Awake()
    {
        Time.timeScale = 1f;
        Debug.Log($"[MainMenuManager] Awake - Time.timeScale reset to {Time.timeScale}");

        if (playButton != null)
            playButton.onClick.AddListener(OnPlayButtonClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitButtonClicked);

        if (settingsBackButton != null)
            settingsBackButton.onClick.AddListener(OnSettingsBackClicked);

        if (exitYesButton != null)
            exitYesButton.onClick.AddListener(OnExitYesClicked);

        if (exitNoButton != null)
            exitNoButton.onClick.AddListener(OnExitNoClicked);

        ShowMainMenu();
    }

    private void Start()
    {
        Time.timeScale = 1f;

        if (GlobalSettingsManager.Instance != null)
        {
            GlobalSettingsManager.Instance.ApplyAllSettings();
            Debug.Log("[MainMenuManager] Applied all settings from GlobalSettingsManager");
        }

        Debug.Log("[MainMenuManager] BGM will be handled by BGMSceneManager");
    }

    private void OnEnable()
    {
        Time.timeScale = 1f;

        if (cancelActionRef != null && cancelActionRef.action != null)
        {
            cancelActionRef.action.Enable();
            cancelActionRef.action.performed += OnCancelPerformed;
        }
        else
        {
            Debug.LogWarning("[MainMenuManager] Cancel Action Reference not assigned!");
        }
    }

    private void OnDisable()
    {
        if (cancelActionRef != null && cancelActionRef.action != null)
        {
            cancelActionRef.action.performed -= OnCancelPerformed;
        }
    }

    private void OnCancelPerformed(InputAction.CallbackContext context)
    {
        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            OnSettingsBackClicked();
        }
        else if (exitConfirmPanel != null && exitConfirmPanel.activeSelf)
        {
            OnExitNoClicked();
        }
    }

    private void OnPlayButtonClicked()
    {
        Debug.Log($"[MainMenuManager] Loading scene: {gameSceneName}");
        SceneManager.LoadScene(gameSceneName);
    }

    private void OnSettingsButtonClicked()
    {
        ShowSettingsPanel();
    }

    private void OnExitButtonClicked()
    {
        ShowExitConfirmPanel();
    }

    private void OnSettingsBackClicked()
    {
        ShowMainMenu();
    }

    private void OnExitYesClicked()
    {
        Debug.Log("[MainMenuManager] Quitting application");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnExitNoClicked()
    {
        ShowMainMenu();
    }

    private void ShowMainMenu()
    {
        SetPanelActive(mainMenuPanel, true);
        SetPanelActive(settingsPanel, false);
        SetPanelActive(exitConfirmPanel, false);

        if (!autoSelectFirstButton)
        {
            StartCoroutine(ClearSelectionNextFrame());
        }
        else if (playButton != null)
        {
            playButton.Select();
        }
    }

    private void ShowSettingsPanel()
    {
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(settingsPanel, true);
        SetPanelActive(exitConfirmPanel, false);

        if (autoSelectFirstButton && settingsBackButton != null)
        {
            settingsBackButton.Select();
        }
        else
        {
            StartCoroutine(ClearSelectionNextFrame());
        }
    }

    private void ShowExitConfirmPanel()
    {
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(settingsPanel, false);
        SetPanelActive(exitConfirmPanel, true);

        if (exitNoButton != null)
        {
            StartCoroutine(SelectButtonNextFrame(exitNoButton));
        }
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
        if (playButton != null)
            playButton.onClick.RemoveListener(OnPlayButtonClicked);

        if (settingsButton != null)
            settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);

        if (exitButton != null)
            exitButton.onClick.RemoveListener(OnExitButtonClicked);

        if (settingsBackButton != null)
            settingsBackButton.onClick.RemoveListener(OnSettingsBackClicked);

        if (exitYesButton != null)
            exitYesButton.onClick.RemoveListener(OnExitYesClicked);

        if (exitNoButton != null)
            exitNoButton.onClick.RemoveListener(OnExitNoClicked);
    }
}