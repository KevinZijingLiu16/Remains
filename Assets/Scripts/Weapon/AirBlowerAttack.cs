using UnityEngine;



public class AirBlowerAttack : IWeaponAttackBehavior
{
    private bool _isBlowing = false;
    private bool _isReversed = false;
    private GameObject _activeEffect;
    private float _nextPowerCost = 0f;


    private GameObject _cubeFogGO;
    private bool _cubeFogOriginalActive;

    private string CurrentLoopId => _isReversed ? "air_blower_suck" : "air_blower_blow";
    private string CurrentLoopName => _isReversed ? "AirBlowerSuckLoop" : "AirBlowerBlowLoop";

    private string _lastLoopId;

    public int GetPowerCostPerSecond() => 3;

    public bool CanAttack(PlayerPower playerPower) => playerPower != null && playerPower.Current > 0;

    public void StartAttack(Transform weaponTransform, PlayerPower playerPower)
    {
        if (!CanAttack(playerPower) || _isBlowing) return;

        _isBlowing = true;
        _nextPowerCost = 0f;

        CreateAirEffect(weaponTransform);

        DisableCubeFogIfPresent(weaponTransform);

        _lastLoopId = CurrentLoopId;
        SoundManager.Instance?.PlayNamedLoop(CurrentLoopId, CurrentLoopName, 0.6f);

        Debug.Log($"[AirBlowerAttack] Started air {(_isReversed ? "sucking" : "blowing")}");
    }

    public void UpdateAttack(Transform weaponTransform, PlayerPower playerPower)
    {
        if (!_isBlowing) return;

        if (!CanAttack(playerPower))
        {
            StopAttack(weaponTransform, playerPower);
            return;
        }

        _nextPowerCost += Time.deltaTime * GetPowerCostPerSecond();
        if (_nextPowerCost >= 1f)
        {
            int cost = Mathf.FloorToInt(_nextPowerCost);
            playerPower.ModifyPower(-cost);
            _nextPowerCost -= cost;
        }

        PerformAirBlowLogic(weaponTransform);

        if (_cubeFogGO != null && _cubeFogGO.activeSelf)
            _cubeFogGO.SetActive(false);
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

        RestoreCubeFog();

        SoundManager.Instance?.StopNamedLoop(CurrentLoopId);
        if (!string.IsNullOrEmpty(_lastLoopId) && _lastLoopId != CurrentLoopId)
            SoundManager.Instance?.StopNamedLoop(_lastLoopId);

        Debug.Log("[AirBlowerAttack] Stopped air blowing");
    }

    public void SetReversed(bool reversed)
    {
        if (_isReversed == reversed) return;

        _isReversed = reversed;

        if (_isBlowing) SwitchLoopIfNeeded();
    }

    private void SwitchLoopIfNeeded()
    {
        if (!string.IsNullOrEmpty(_lastLoopId))
            SoundManager.Instance?.StopNamedLoop(_lastLoopId);

        SoundManager.Instance?.PlayNamedLoop(CurrentLoopId, CurrentLoopName, 0.6f);

        _lastLoopId = CurrentLoopId;

        Debug.Log($"[AirBlowerAttack] Switched loop to {CurrentLoopName}");
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

                float force = 100f / Mathf.Max(0.01f, Vector3.Distance(weaponTransform.position, rb.position));
                rb.AddForce(direction * force);
            }
        }
    }

    public string GetAttackLoopSoundName() => CurrentLoopName;
    public bool HasLoopSound() => true;


    private void DisableCubeFogIfPresent(Transform weaponTransform)
    {
        if (_cubeFogGO == null)
        {
            if (weaponTransform != null)
            {
                var root = weaponTransform.root;
                _cubeFogGO = FindDeepChildByName(root, "CubeFog")?.gameObject;
            }

            if (_cubeFogGO == null)
            {
                var global = GameObject.Find("CubeFog");
                if (global != null) _cubeFogGO = global;
            }

            if (_cubeFogGO != null)
                _cubeFogOriginalActive = _cubeFogGO.activeSelf;
        }

        if (_cubeFogGO != null)
            _cubeFogGO.SetActive(false);
    }

    private void RestoreCubeFog()
    {
        if (_cubeFogGO != null)
        {
            _cubeFogGO.SetActive(_cubeFogOriginalActive);
          
        }
    }

    private static Transform FindDeepChildByName(Transform parent, string name)
    {
        if (parent == null) return null;
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var result = FindDeepChildByName(child, name);
            if (result != null) return result;
        }
        return null;
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