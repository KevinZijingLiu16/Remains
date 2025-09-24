using UnityEngine;

public class CheckPointManager : MonoBehaviour
{
    [Header("CheckPoint Management")]
    [SerializeField] private CheckPointSystem[] checkPoints;
    [SerializeField] private bool autoFindCheckPoints = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;

    private CheckPointSystem lastActivatedCheckPoint;

    public CheckPointSystem[] AllCheckPoints => checkPoints;
    public CheckPointSystem LastActivatedCheckPoint => lastActivatedCheckPoint;

    void Awake()
    {
        if (autoFindCheckPoints || checkPoints == null || checkPoints.Length == 0)
        {
            FindAllCheckPoints();
        }
    }

    void Start()
    {
      
        foreach (var checkPoint in checkPoints)
        {
            if (checkPoint != null)
            {
                //can add some event hearing here
            }
        }

      
        FindLastActivatedCheckPoint();
    }

    private void FindAllCheckPoints()
    {
        checkPoints = FindObjectsByType<CheckPointSystem>(FindObjectsSortMode.None);
        Debug.Log($"[CheckPointManager] Found {checkPoints.Length} checkpoints in scene");
    }

    private void FindLastActivatedCheckPoint()
    {
        string currentLevel = GameProgressManager.Instance?.GetCurrentLevelName();
        if (string.IsNullOrEmpty(currentLevel)) return;

        if (GameProgressManager.Instance.GetLatestCheckPoint(currentLevel, out CheckPointData latestData))
        {
         
            foreach (var checkPoint in checkPoints)
            {
                if (checkPoint != null && checkPoint.CheckPointID == latestData.checkPointID)
                {
                    lastActivatedCheckPoint = checkPoint;
                    Debug.Log($"[CheckPointManager] Last activated checkpoint: {latestData.checkPointID}");
                    break;
                }
            }
        }
    }

   
    [ContextMenu("Reset All CheckPoints")]
    public void ResetAllCheckPoints()
    {
        foreach (var checkPoint in checkPoints)
        {
            if (checkPoint != null)
            {
                checkPoint.ResetCheckPoint();
            }
        }

        lastActivatedCheckPoint = null;

       
        string currentLevel = GameProgressManager.Instance?.GetCurrentLevelName();
        if (!string.IsNullOrEmpty(currentLevel))
        {
            GameProgressManager.Instance.ClearCheckPointsForLevel(currentLevel);
        }

        Debug.Log("[CheckPointManager] All checkpoints reset");
    }

    
    public int GetActivatedCheckPointCount()
    {
        int count = 0;
        foreach (var checkPoint in checkPoints)
        {
            if (checkPoint != null && checkPoint.IsActivated)
            {
                count++;
            }
        }
        return count;
    }

  
    public CheckPointSystem GetNearestCheckPoint(Vector3 position)
    {
        CheckPointSystem nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (var checkPoint in checkPoints)
        {
            if (checkPoint != null)
            {
                float distance = Vector3.Distance(position, checkPoint.RespawnPosition);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = checkPoint;
                }
            }
        }

        return nearest;
    }

    //void OnGUI()
    //{
    //    if (!showDebugInfo) return;

    //    GUILayout.BeginArea(new Rect(10, 100, 300, 200));
    //    //GUILayout.Label("CheckPoint Debug Info:", EditorGUIUtility.boldLabel);

    //    GUILayout.Label($"Total CheckPoints: {checkPoints.Length}");
    //    GUILayout.Label($"Activated: {GetActivatedCheckPointCount()}");

    //    if (lastActivatedCheckPoint != null)
    //    {
    //        GUILayout.Label($"Last Activated: {lastActivatedCheckPoint.CheckPointID}");
    //    }

    //    if (GUILayout.Button("Reset All CheckPoints"))
    //    {
    //        ResetAllCheckPoints();
    //    }

    //    GUILayout.EndArea();
    //}
}