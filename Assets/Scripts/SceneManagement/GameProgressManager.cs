using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance { get; private set; }

    [Header("Hub Scene Data")]
    public string hubSceneName = "HubScene";

    private Vector3 _savedHubPosition;
    private float _savedSplineT;
    private bool _hasReturnData = false;
    [SerializeField] private Vector3 returnOffset;

    [Header("Level Progress")]
    [SerializeField] private Dictionary<string, LevelProgress> levelProgressDict = new Dictionary<string, LevelProgress>();

    [Header("CheckPoint System")]
    private Dictionary<string, CheckPointData> activeCheckPoints = new Dictionary<string, CheckPointData>();
    private string _currentLevelName = "";
    private float _levelStartTime;

    [Header("Key Management")]
    [SerializeField] private Dictionary<string, KeyData> collectedKeys = new Dictionary<string, KeyData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

           
            SceneManager.sceneLoaded += OnSceneLoaded;
            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.name != hubSceneName)
            {
                SetCurrentLevel(activeScene.name);
                _levelStartTime = Time.time;
                Debug.Log($"[GameProgress] Initial active scene set as current level: {activeScene.name}");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
       
        if (scene.name != hubSceneName)
        {
            SetCurrentLevel(scene.name);
            _levelStartTime = Time.time;
        }
    }

    #region Hub Position Management

    public void SaveHubPosition(Vector3 worldPos, float splineT)
    {
        _savedHubPosition = worldPos;
        _savedSplineT = splineT;
        _hasReturnData = true;
        Debug.Log($"[GameProgress] Saved Hub position: {worldPos}, spline t: {splineT}");
    }

    public bool GetReturnPosition(out Vector3 worldPos, out float splineT)
    {
        worldPos = _savedHubPosition + returnOffset;
        splineT = _savedSplineT;
        return _hasReturnData;
    }

    public void ClearReturnData()
    {
        _hasReturnData = false;
    }

    #endregion

    #region Level Progress Management

    public void SetCurrentLevel(string levelName)
    {
        _currentLevelName = levelName;
        if (!levelProgressDict.ContainsKey(levelName))
        {
            levelProgressDict[levelName] = new LevelProgress();
        }
        Debug.Log($"[GameProgress] Current level set to: {levelName}");
    }

    public string GetCurrentLevelName()
    {
        return _currentLevelName;
    }

    public LevelStatus GetLevelStatus(string levelName)
    {
        if (!levelProgressDict.ContainsKey(levelName))
        {
            return LevelStatus.NotStarted;
        }

        var progress = levelProgressDict[levelName];

        if (progress.isCompleted)
        {
            return LevelStatus.Completed;
        }

      
 

        return LevelStatus.NotStarted;
    }

    public void SetLevelStatus(string levelName, LevelStatus status)
    {
        if (!levelProgressDict.ContainsKey(levelName))
        {
            levelProgressDict[levelName] = new LevelProgress();
        }

        var progress = levelProgressDict[levelName];

        switch (status)
        {
            case LevelStatus.NotStarted:
                progress.isCompleted = false;
                ClearCheckPointsForLevel(levelName);
                break;
          
              
            case LevelStatus.Completed:
                progress.isCompleted = true;
               
                if (_currentLevelName == levelName)
                {
                    float completionTime = Time.time - _levelStartTime;
                    if (completionTime < progress.bestTime)
                    {
                        progress.bestTime = completionTime;
                    }
                }
                break;
        }

        Debug.Log($"[GameProgress] Level {levelName} status set to: {status}");
    }

    public LevelProgress GetLevelProgress(string levelName)
    {
        if (levelProgressDict.ContainsKey(levelName))
        {
            return levelProgressDict[levelName];
        }
        return null;
    }

    public bool IsLevelCompleted(string levelName)
    {
        return GetLevelStatus(levelName) == LevelStatus.Completed;
    }

    public void SetLevelCompleted(string levelName, bool completed = true)
    {
        SetLevelStatus(levelName, completed ? LevelStatus.Completed : LevelStatus.NotStarted);
    }

    public int GetLevelStarRating(string levelName)
    {
        if (levelProgressDict.ContainsKey(levelName))
        {
            return levelProgressDict[levelName].starRating;
        }
        return 0;
    }

    public void SetLevelStarRating(string levelName, int stars)
    {
        if (!levelProgressDict.ContainsKey(levelName))
        {
            levelProgressDict[levelName] = new LevelProgress();
        }
        levelProgressDict[levelName].starRating = Mathf.Clamp(stars, 0, 3);
        Debug.Log($"[GameProgress] Level {levelName} star rating: {stars}");
    }

    public void SetLevelBestTime(string levelName, float time)
    {
        if (!levelProgressDict.ContainsKey(levelName))
        {
            levelProgressDict[levelName] = new LevelProgress();
        }

        if (time < levelProgressDict[levelName].bestTime)
        {
            levelProgressDict[levelName].bestTime = time;
            Debug.Log($"[GameProgress] New best time for {levelName}: {time:F2}s");
        }
    }

    #endregion
    #region Key Management

    public void CollectKey(string keyID, string unlocksLevel)
    {
        if (collectedKeys.ContainsKey(keyID)) return;

        KeyData keyData = new KeyData
        {
            keyID = keyID,
            unlocksLevel = unlocksLevel,
            collectionTime = System.DateTime.Now.Ticks
        };

        collectedKeys[keyID] = keyData;

       
        KeyEvents.OnKeyCollected?.Invoke(keyID, unlocksLevel);

      
    }

    public bool HasKey(string keyID)
    {
        return collectedKeys.ContainsKey(keyID);
    }

    public bool CanAccessLevel(string levelName)
    {
       
        if (levelName == "Level1") return true;

        foreach (var keyData in collectedKeys.Values)
        {
            if (keyData.unlocksLevel == levelName)
            {
                return true;
            }
        }
        return false;
    }

    public string GetKeyForLevel(string levelName)
    {
        foreach (var kvp in collectedKeys)
        {
            if (kvp.Value.unlocksLevel == levelName)
            {
                return kvp.Key;
            }
        }
        return "";
    }

    public List<KeyData> GetAllKeys()
    {
        return new List<KeyData>(collectedKeys.Values);
    }

    public void ClearAllKeys()
    {
        collectedKeys.Clear();
        Debug.Log("[GameProgress] Çå³ýËùÓÐÔ¿³×");
    }

    #endregion

    #region CheckPoint System

    public void ActivateCheckPoint(string checkPointID, Vector3 position, float splineT)
    {
        if (string.IsNullOrEmpty(_currentLevelName)) return;

        string key = $"{_currentLevelName}_{checkPointID}";
        CheckPointData checkPointData = new CheckPointData
        {
            checkPointID = checkPointID,
            levelName = _currentLevelName,
            position = position,
            splineT = splineT,
            activationTime = System.DateTime.Now.Ticks  
        };

        activeCheckPoints[key] = checkPointData;
        Debug.Log($"[GameProgress] CheckPoint {checkPointID} activated in {_currentLevelName}, time: {checkPointData.activationTime}");
    }

    public bool IsCheckPointActivated(string levelName, string checkPointID)
    {
        string key = $"{levelName}_{checkPointID}";
        return activeCheckPoints.ContainsKey(key);
    }

    public bool GetLatestCheckPoint(string levelName, out CheckPointData checkPointData)
    {
        checkPointData = null;
        CheckPointData latestCheckPoint = null;
        long latestTime = 0;

        Debug.Log($"[GameProgress] Searching for latest checkpoint in level: {levelName}");

        foreach (var kvp in activeCheckPoints)
        {
            Debug.Log($"[GameProgress] Checking: {kvp.Value.checkPointID}, time: {kvp.Value.activationTime}");

            if (kvp.Value.levelName == levelName && kvp.Value.activationTime > latestTime)
            {
                latestCheckPoint = kvp.Value;
                latestTime = kvp.Value.activationTime;
            }
        }

        if (latestCheckPoint != null)
        {
            checkPointData = latestCheckPoint;
            Debug.Log($"[GameProgress] Found latest checkpoint: {latestCheckPoint.checkPointID}");
            return true;
        }

        return false;
    }

    public void ClearCheckPointsForLevel(string levelName)
    {
        List<string> keysToRemove = new List<string>();
        foreach (var kvp in activeCheckPoints)
        {
            if (kvp.Value.levelName == levelName)
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (string key in keysToRemove)
        {
            activeCheckPoints.Remove(key);
        }
        Debug.Log($"[GameProgress] Cleared all checkpoints for level {levelName}");
    }

    public List<CheckPointData> GetAllCheckPointsForLevel(string levelName)
    {
        List<CheckPointData> checkPoints = new List<CheckPointData>();
        foreach (var kvp in activeCheckPoints)
        {
            if (kvp.Value.levelName == levelName)
            {
                checkPoints.Add(kvp.Value);
            }
        }
        return checkPoints;
    }

    #endregion

    #region Data Persistence

    [ContextMenu("Save Progress to PlayerPrefs")]
    public void SaveProgressToPlayerPrefs()
    {
       
        foreach (var kvp in levelProgressDict)
        {
            string levelKey = $"Level_{kvp.Key}";
            PlayerPrefs.SetInt($"{levelKey}_Completed", kvp.Value.isCompleted ? 1 : 0);
            PlayerPrefs.SetInt($"{levelKey}_Stars", kvp.Value.starRating);
            PlayerPrefs.SetFloat($"{levelKey}_BestTime", kvp.Value.bestTime);
            PlayerPrefs.SetInt($"{levelKey}_Collectibles", kvp.Value.collectiblesFound);
        }

      
        

      
        if (_hasReturnData)
        {
            PlayerPrefs.SetFloat("Hub_PosX", _savedHubPosition.x);
            PlayerPrefs.SetFloat("Hub_PosY", _savedHubPosition.y);
            PlayerPrefs.SetFloat("Hub_PosZ", _savedHubPosition.z);
            PlayerPrefs.SetFloat("Hub_SplineT", _savedSplineT);
            PlayerPrefs.SetInt("Hub_HasReturnData", 1);
        }
        PlayerPrefs.SetInt("CheckPoint_Count", activeCheckPoints.Count);
        int index = 0;
        foreach (var kvp in activeCheckPoints)
        {
            string prefix = $"CheckPoint_{index}";
            PlayerPrefs.SetString($"{prefix}_Key", kvp.Key);
            PlayerPrefs.SetString($"{prefix}_ID", kvp.Value.checkPointID);
            PlayerPrefs.SetString($"{prefix}_Level", kvp.Value.levelName);
            PlayerPrefs.SetFloat($"{prefix}_PosX", kvp.Value.position.x);
            PlayerPrefs.SetFloat($"{prefix}_PosY", kvp.Value.position.y);
            PlayerPrefs.SetFloat($"{prefix}_PosZ", kvp.Value.position.z);
            PlayerPrefs.SetFloat($"{prefix}_SplineT", kvp.Value.splineT);
            PlayerPrefs.SetString($"{prefix}_Time", kvp.Value.activationTime.ToString());
            index++;
        }
        PlayerPrefs.Save();
        Debug.Log("[GameProgress] Progress saved to PlayerPrefs");
    }

    [ContextMenu("Load Progress from PlayerPrefs")]
    public void LoadProgressFromPlayerPrefs()
    {
       
        int checkPointCount = PlayerPrefs.GetInt("CheckPoint_Count", 0);
        activeCheckPoints.Clear();

        for (int i = 0; i < checkPointCount; i++)
        {
            string prefix = $"CheckPoint_{i}";
            string key = PlayerPrefs.GetString($"{prefix}_Key");

            if (!string.IsNullOrEmpty(key))
            {
                CheckPointData data = new CheckPointData
                {
                    checkPointID = PlayerPrefs.GetString($"{prefix}_ID"),
                    levelName = PlayerPrefs.GetString($"{prefix}_Level"),
                    position = new Vector3(
                        PlayerPrefs.GetFloat($"{prefix}_PosX"),
                        PlayerPrefs.GetFloat($"{prefix}_PosY"),
                        PlayerPrefs.GetFloat($"{prefix}_PosZ")
                    ),
                    splineT = PlayerPrefs.GetFloat($"{prefix}_SplineT"),
                    activationTime = long.Parse(PlayerPrefs.GetString($"{prefix}_Time", "0"))
                };

                activeCheckPoints[key] = data;
            }
        }

      
        if (PlayerPrefs.GetInt("Hub_HasReturnData", 0) == 1)
        {
            _savedHubPosition = new Vector3(
                PlayerPrefs.GetFloat("Hub_PosX"),
                PlayerPrefs.GetFloat("Hub_PosY"),
                PlayerPrefs.GetFloat("Hub_PosZ")
            );
            _savedSplineT = PlayerPrefs.GetFloat("Hub_SplineT");
            _hasReturnData = true;
        }

        Debug.Log("[GameProgress] Progress loaded from PlayerPrefs");

        int keyCount = PlayerPrefs.GetInt("Key_Count", 0);
        collectedKeys.Clear();

        for (int i = 0; i < keyCount; i++)
        {
            string prefix = $"Key_{i}";
            string keyID = PlayerPrefs.GetString($"{prefix}_ID");

            if (!string.IsNullOrEmpty(keyID))
            {
                KeyData keyData = new KeyData
                {
                    keyID = keyID,
                    unlocksLevel = PlayerPrefs.GetString($"{prefix}_UnlocksLevel"),
                    collectionTime = long.Parse(PlayerPrefs.GetString($"{prefix}_CollectionTime", "0"))
                };

                collectedKeys[keyID] = keyData;
            }
        }
    }

    #endregion
}


public enum LevelStatus
{
    NotStarted,  
     
    Completed      
}

[System.Serializable]
public class LevelProgress
{
    public bool isCompleted = false;
    public int starRating = 0; 
    public float bestTime = float.MaxValue;
    public int collectiblesFound = 0;
    public System.DateTime firstCompletionTime;
    public System.DateTime lastPlayedTime;
}

[System.Serializable]
public class CheckPointData
{
    public string checkPointID;
    public string levelName;
    public Vector3 position;
    public float splineT;
    public long activationTime;

 
    public void SetActivationTimeNow()
    {
  
        activationTime = System.DateTime.Now.Ticks;
    }
}