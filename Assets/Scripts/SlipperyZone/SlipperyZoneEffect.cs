using UnityEngine;

public class SlipperyZoneEffect : MonoBehaviour, IZoneEffect
{
    [Header("Zone Configuration")]
    public SlipperyMovementModifier movementModifier = new SlipperyMovementModifier();
    public bool enableSwayEffect = true;
    public bool requireAirBlowerWeapon = true;

    [Header("Visual Effects")]
    public ParticleSystem slipperyParticles;
    public AudioSource slipperyAudio;

    [Header("Debug")]
    public bool enableDebugLogs = true;

    private SplineRunnerRB _currentRunner;
    private AirFlowMovementController _airFlowController;
    private bool _effectActive = false;

    public void OnPlayerEnter(GameObject player)
    {
        _currentRunner = player.GetComponent<SplineRunnerRB>();
        _airFlowController = player.GetComponent<AirFlowMovementController>();

        if (_currentRunner == null)
        {
            if (enableDebugLogs) Debug.LogWarning("[SlipperyZone] Player doesn't have SplineRunnerRB component");
            return;
        }

       
        if (requireAirBlowerWeapon && !HasAirBlowerWeapon(player))
        {
            if (enableDebugLogs) Debug.Log("[SlipperyZone] Player doesn't have air blower weapon, normal slippery effect only");
        }

        ApplySlipperyEffect();

        if (enableDebugLogs) Debug.Log("[SlipperyZone] Player entered slippery zone");
    }

    public void OnPlayerExit(GameObject player)
    {
        if (_currentRunner != null)
        {
            RemoveSlipperyEffect();
        }

        _currentRunner = null;
        _airFlowController = null;

        if (enableDebugLogs) Debug.Log("[SlipperyZone] Player exited slippery zone");
    }

    public void UpdateEffect(GameObject player)
    {
        if (_effectActive && _currentRunner != null && enableSwayEffect)
        {
            movementModifier.UpdateSwayEffect(_currentRunner);
        }
    }

    private void ApplySlipperyEffect()
    {
        if (_currentRunner == null) return;

   
        movementModifier.ApplyModification(_currentRunner);

      
        if (_airFlowController != null)
        {
            _airFlowController.EnableAirFlowMovement(true);
        }


        if (slipperyParticles != null)
            slipperyParticles.Play();

        if (slipperyAudio != null)
            slipperyAudio.Play();

        _effectActive = true;
    }

    private void RemoveSlipperyEffect()
    {
        if (_currentRunner == null) return;


        movementModifier.RemoveModification(_currentRunner);

 
        if (_airFlowController != null)
        {
            _airFlowController.EnableAirFlowMovement(false);
        }

     
        if (slipperyParticles != null)
            slipperyParticles.Stop();

        if (slipperyAudio != null)
            slipperyAudio.Stop();

        _effectActive = false;
    }

    private bool HasAirBlowerWeapon(GameObject player)
    {
        var equipmentManager = player.GetComponent<WeaponEquipmentManager>();
        if (equipmentManager != null && equipmentManager.CurrentWeapon != null)
        {
            return equipmentManager.CurrentWeapon.WeaponId == "air_blower";
        }
        return false;
    }
}