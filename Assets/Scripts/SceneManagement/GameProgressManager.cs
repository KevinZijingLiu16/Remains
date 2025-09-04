using UnityEngine;

public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance { get; private set; }

    [Header("Hub Scene Data")]
    public string hubSceneName = "HubScene";

  
    private Vector3 _savedHubPosition;
    private float _savedSplineT;
    private bool _hasReturnData = false;
    [SerializeField] private Vector3 returnOffset;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

   
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
}