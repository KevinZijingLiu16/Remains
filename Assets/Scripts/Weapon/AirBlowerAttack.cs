using UnityEngine;

public class AirBlowerAttack : IWeaponAttackBehavior
{
    private bool _isBlowing = false;
    private bool _isReversed = false; 
    private GameObject _activeEffect;
    private float _nextPowerCost = 0f;

    private string LoopSoundId => _isReversed ? "air_blower_suck" : "air_blower_blow";
    private string LoopSoundName => _isReversed ? "AirBlowerSuckLoop" : "AirBlowerBlowLoop";

    public int GetPowerCostPerSecond() => 3;

    public bool CanAttack(PlayerPower playerPower)
    {
        return playerPower != null && playerPower.Current > 0;
    }

    public void StartAttack(Transform weaponTransform, PlayerPower playerPower)
    {
        if (!CanAttack(playerPower) || _isBlowing) return;

        _isBlowing = true;
        _nextPowerCost = 0f;

        CreateAirEffect(weaponTransform);

        string mode = _isReversed ? "sucking" : "blowing";

        //SoundManager.Instance?.PlayNamedLoop(LoopSoundId, LoopSoundName, 0.6f);

        Debug.Log($"[AirBlowerAttack] Started air {mode}");
    }

    public void UpdateAttack(Transform weaponTransform, PlayerPower playerPower)
    {
        if (!_isBlowing) return;

        if (!CanAttack(playerPower))
        {
            StopAttack(weaponTransform, playerPower);
            return;
        }

        // Consume Power
        _nextPowerCost += Time.deltaTime * GetPowerCostPerSecond();
        if (_nextPowerCost >= 1f)
        {
            int cost = Mathf.FloorToInt(_nextPowerCost);
            playerPower.ModifyPower(-cost);
            _nextPowerCost -= cost;
        }

        
        PerformAirBlowLogic(weaponTransform);
    }

    public void StopAttack(Transform weaponTransform, PlayerPower playerPower)
    {
        if (!_isBlowing) return;

        _isBlowing = false;

        if (_activeEffect != null)
        {
            Object.Destroy(_activeEffect);
            _activeEffect = null;
        }
      //  SoundManager.Instance?.StopNamedLoop(LoopSoundId);
        Debug.Log("[AirBlowerAttack] Stopped air blowing");
    }

    public void SetReversed(bool reversed)
    {
        _isReversed = reversed;
    }

    private void CreateAirEffect(Transform weaponTransform)
    {
        string prefabName = _isReversed ? "AirSuckEffect" : "AirBlowEffect";
        GameObject airPrefab = Resources.Load<GameObject>(prefabName);
        if (airPrefab != null && weaponTransform != null)
        {
            _activeEffect = Object.Instantiate(airPrefab, weaponTransform);
        }
    }

    private void PerformAirBlowLogic(Transform weaponTransform)
    {
        if (weaponTransform == null) return;

        
        Collider[] colliders = Physics.OverlapSphere(weaponTransform.position, 10f);

        foreach (var col in colliders)
        {
            Rigidbody rb = col.attachedRigidbody;
            if (rb != null && rb.gameObject != weaponTransform.root.gameObject)
            {
                Vector3 direction = _isReversed
                    ? (weaponTransform.position - rb.position).normalized
                    : (rb.position - weaponTransform.position).normalized;

                float force = 100f / Vector3.Distance(weaponTransform.position, rb.position);
                rb.AddForce(direction * force);
            }
        }
    }

    public string GetAttackLoopSoundName()
    {
        throw new System.NotImplementedException();
    }

    public bool HasLoopSound()
    {
        throw new System.NotImplementedException();
    }
}



public static class WeaponAttackFactory
{
    public static IWeaponAttackBehavior CreateAttackBehavior(AttackType attackType)
    {
        return attackType switch
        {
            AttackType.FoamSpray => new FoamSprayAttack(),
            AttackType.AirBlower => new AirBlowerAttack(),
            AttackType.None => null,
            _ => null
        };
    }
}