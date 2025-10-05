using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WaterZoneController : MonoBehaviour
{
    [Header("Zone Detection")]
    public LayerMask playerLayer = 1 << 0;
    public string playerTag = "Player";

    [Header("Effects")]
    public WaterZoneEffect zoneEffect;

    [Header("Debug")]
    public bool enableDebugLogs = true;
    public bool showGizmos = true;

    private GameObject _currentPlayer;
    private Collider _zoneTrigger;

    void Awake()
    {
        _zoneTrigger = GetComponent<Collider>();
        _zoneTrigger.isTrigger = true;

        if (!zoneEffect)
        {
            zoneEffect = GetComponent<WaterZoneEffect>();
            if (!zoneEffect) zoneEffect = gameObject.AddComponent<WaterZoneEffect>();
        }
    }

    void Update()
    {
        if (_currentPlayer && zoneEffect) zoneEffect.UpdateEffect(_currentPlayer);
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsPlayer(other.gameObject) && _currentPlayer == null)
        {
            _currentPlayer = other.gameObject;

            EnsurePlayerComponents(_currentPlayer);
            zoneEffect?.OnPlayerEnter(_currentPlayer);

            if (enableDebugLogs)
                Debug.Log($"[WaterZoneController] Player {_currentPlayer.name} entered water.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == _currentPlayer)
        {
            zoneEffect?.OnPlayerExit(_currentPlayer);

            if (enableDebugLogs)
                Debug.Log($"[WaterZoneController] Player {_currentPlayer.name} exited water.");

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
      
        var blower = player.GetComponent<PlayerAirBlowerMovementInWaterController>();
        if (!blower)
        {
            blower = player.AddComponent<PlayerAirBlowerMovementInWaterController>();
            blower.enabled = false;
            if (enableDebugLogs) Debug.Log("[WaterZoneController] Added AirBlowerAimSplineAndVertical to player (disabled).");
        }
        else
        {
            blower.enabled = false;
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        if (!_zoneTrigger) _zoneTrigger = GetComponent<Collider>();
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.25f);

        if (_zoneTrigger is BoxCollider b)
        {
            Gizmos.DrawCube(b.center, b.size);
            Gizmos.color = new Color(0f, 0.5f, 1f, 1f);
            Gizmos.DrawWireCube(b.center, b.size);
        }
        else if (_zoneTrigger is SphereCollider s)
        {
            Gizmos.DrawSphere(s.center, s.radius);
            Gizmos.color = new Color(0f, 0.5f, 1f, 1f);
            Gizmos.DrawWireSphere(s.center, s.radius);
        }
    }
}
