using UnityEngine;

public class SlipperyZoneController : MonoBehaviour
{
    [Header("Zone Detection")]
    public LayerMask playerLayer = 1 << 0;
    public string playerTag = "Player";

    [Header("Effects")]
    public SlipperyZoneEffect zoneEffect;

    [Header("Debug")]
    public bool enableDebugLogs = true;
    public bool showGizmos = true;

    private GameObject _currentPlayer;
    private Collider _zoneTrigger;

    void Awake()
    {
        _zoneTrigger = GetComponent<Collider>();
        if (_zoneTrigger == null)
        {
            Debug.LogError("[SlipperyZoneController] No Collider found! Adding BoxCollider as trigger.");
            _zoneTrigger = gameObject.AddComponent<BoxCollider>();
        }

        _zoneTrigger.isTrigger = true;

      
        if (zoneEffect == null)
        {
            zoneEffect = GetComponent<SlipperyZoneEffect>();
            if (zoneEffect == null)
            {
                zoneEffect = gameObject.AddComponent<SlipperyZoneEffect>();
            }
        }
    }

    void Update()
    {
        if (_currentPlayer != null && zoneEffect != null)
        {
            zoneEffect.UpdateEffect(_currentPlayer);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsPlayer(other.gameObject) && _currentPlayer == null)
        {
            _currentPlayer = other.gameObject;

     
            EnsurePlayerComponents(_currentPlayer);

            zoneEffect?.OnPlayerEnter(_currentPlayer);

            if (enableDebugLogs)
                Debug.Log($"[SlipperyZoneController] Player {_currentPlayer.name} entered zone");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == _currentPlayer)
        {
            zoneEffect?.OnPlayerExit(_currentPlayer);

            if (enableDebugLogs)
                Debug.Log($"[SlipperyZoneController] Player {_currentPlayer.name} exited zone");

            _currentPlayer = null;
        }
    }

    private bool IsPlayer(GameObject obj)
    {
        return obj.CompareTag(playerTag) &&
               ((1 << obj.layer) & playerLayer.value) != 0;
    }

    private void EnsurePlayerComponents(GameObject player)
    {
       
        if (player.GetComponent<AirFlowMovementController>() == null)
        {
            player.AddComponent<AirFlowMovementController>();
            if (enableDebugLogs)
                Debug.Log("[SlipperyZoneController] Added AirFlowMovementController to player");
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = Color.cyan;
        Gizmos.matrix = transform.localToWorldMatrix;

        if (_zoneTrigger is BoxCollider box)
        {
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else if (_zoneTrigger is SphereCollider sphere)
        {
            Gizmos.DrawWireSphere(sphere.center, sphere.radius);
        }
    }
}