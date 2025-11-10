using UnityEngine;
using System;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    [SerializeField] private GameState currentState = GameState.Playing;


    public event Action<GameState> OnGameStateChanged;
    public event Action OnGamePaused;
    public event Action OnGameResumed;
    public event Action OnGameOver;
    public event Action OnGameWin;

    public GameState CurrentState => currentState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Debug.Log("[GameManager] Initialized");
    }

    private void Start()
    {
      
        SetGameState(GameState.Playing);

        Debug.Log("[GameManager] Game started");
    }


    public void SetGameState(GameState newState)
    {
        if (currentState == newState) return;

        GameState oldState = currentState;
        currentState = newState;

        Debug.Log($"[GameManager] Game state changed: {oldState} ¡ú {newState}");

     
        switch (newState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                OnGameResumed?.Invoke();
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                OnGamePaused?.Invoke();
                break;

            case GameState.GameOver:
                Time.timeScale = 0f;
                OnGameOver?.Invoke();
                break;

            case GameState.Win:
                Time.timeScale = 0f;
                OnGameWin?.Invoke();
                break;
        }

        OnGameStateChanged?.Invoke(newState);
    }


    public void PauseGame()
    {
        SetGameState(GameState.Paused);
    }


    public void ResumeGame()
    {
        SetGameState(GameState.Playing);
    }


    public void TriggerGameOver()
    {
        SetGameState(GameState.GameOver);
    }


    public void TriggerWin()
    {
        SetGameState(GameState.Win);
    }


    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }


    public void ReturnToMainMenu(string menuSceneName = "OpeningUI")
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(menuSceneName);
    }

    public bool IsGamePaused()
    {
        return currentState == GameState.Paused;
    }


    public bool IsGamePlaying()
    {
        return currentState == GameState.Playing;
    }

    private void OnDestroy()
    {
   
        Time.timeScale = 1f;
    }

    private void OnApplicationQuit()
    {
        Time.timeScale = 1f;
    }
}


public enum GameState
{
    Playing,    
    Paused,     
    GameOver,  
    Win        
}