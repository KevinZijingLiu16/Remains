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

        
        if (GameProgressManager.Instance != null &&
            GameProgressManager.Instance.GetReturnPosition(out Vector3 returnPos, out float returnT))
        {
            
            Debug.Log($"[PlayerSpawnOnLoad] Restoring player to return position: {returnPos}");

            
            player.MarkSpawnedBySpawner();

           
            player.ResnapTToWorldPosition(returnPos);
        }
        else if (defaultSpawnPoint != null)
        {
            
            Debug.Log("[PlayerSpawnOnLoad] Using default spawn position");
            player.transform.position = defaultSpawnPoint.position;
        }
    }
}