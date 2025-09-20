using UnityEngine;

public class PlayerSpawnOnLoad : MonoBehaviour
{
    [Header("Default Spawn")]
    [SerializeField] private Transform defaultSpawnPoint;

    void Start()
    {
        var player = FindFirstObjectByType<SplineRunnerRB>();
        if (player == null)
        {
            Debug.LogError("[PlayerSpawnOnLoad] No SplineRunnerRB found in scene!");
            return;
        }

        if (GameProgressManager.Instance == null)
        {
            Debug.LogWarning("[PlayerSpawnOnLoad] GameProgressManager not found, using default spawn");
            UseDefaultSpawn(player);
            return;
        }

        string currentLevel = GameProgressManager.Instance.GetCurrentLevelName();
        Debug.Log($"[PlayerSpawnOnLoad] Current level: {currentLevel}");

   
        if (GameProgressManager.Instance.GetReturnPosition(out Vector3 returnPos, out float returnT))
        {
            Debug.Log($"[PlayerSpawnOnLoad] Found Hub return position: {returnPos}");

          
            LevelStatus levelStatus = GameProgressManager.Instance.GetLevelStatus(currentLevel);
            Debug.Log($"[PlayerSpawnOnLoad] Level status: {levelStatus}");

    

       
            player.MarkSpawnedBySpawner();
            player.ResnapToWorldPosition(returnPos);
            GameProgressManager.Instance.ClearReturnData();
            return;
        }

        
        if (!string.IsNullOrEmpty(currentLevel))
        {
            if (GameProgressManager.Instance.GetLatestCheckPoint(currentLevel, out CheckPointData checkPoint))
            {
                Debug.Log($"[PlayerSpawnOnLoad] No Hub return data, using checkpoint: {checkPoint.checkPointID} at {checkPoint.position}");
                player.MarkSpawnedBySpawner();
                player.ResnapToWorldPosition(checkPoint.position);
                return;
            }
        }

       
        Debug.Log("[PlayerSpawnOnLoad] No return data or checkpoints found, using default spawn");
        UseDefaultSpawn(player);
    }

    private void UseDefaultSpawn(SplineRunnerRB player)
    {
        if (defaultSpawnPoint != null)
        {
            Debug.Log($"[PlayerSpawnOnLoad] Using default spawn position: {defaultSpawnPoint.position}");
            player.transform.position = defaultSpawnPoint.position;
            player.SnapToNearestOnSpline();
        }
        else
        {
            Debug.LogWarning("[PlayerSpawnOnLoad] No default spawn point set!");
        }
    }
}