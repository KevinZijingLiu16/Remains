using UnityEngine;

public class StickyFoamSprayAttack : IWeaponAttackBehavior
{
    private bool _isAttacking = false;
    private GameObject _activeEffect;
    private float _nextPowerCost = 0f;
    private float _nextFoamSpawn = 0f;

    private const string LOOP_SOUND_ID = "foam_spray_primary";
    private const string LOOP_SOUND_NAME = "FoamSprayLoop";

    [Header("Foam Spawn Settings")]
    public GameObject stickyFoamPrefab; 
    public float foamSpawnInterval = 0.3f; 
    public int foamBurstCount = 1; 
    public float burstSpread = 1f; 

    [Header("Foam Launch Settings")]
    public float minLaunchSpeed = 15f; 
    public float maxLaunchSpeed = 15f;
    public float upwardForce = 0.1f; 
    public bool useRandomSpeed = true;

    [Header("Targeting")]
    public float maxRange = 20f;
    public LayerMask targetLayers = ~0;
    public bool aimAssist = true; 

    [Header("Enemy Effects")]
    public float slowAmount = 0.4f;
    public float slowDuration = 5f;



    public int GetPowerCostPerSecond() => 8;

    public bool CanAttack(PlayerPower playerPower)
    {
        return playerPower != null && playerPower.Current > 0;
    }

    public void StartAttack(Transform weaponTransform, PlayerPower playerPower)
    {
        if (!CanAttack(playerPower) || _isAttacking) return;

        _isAttacking = true;
        _nextPowerCost = 0f;
        _nextFoamSpawn = 0f;

        CreateFoamEffect(weaponTransform);
        SoundManager.Instance?.PlayNamedLoop(LOOP_SOUND_ID, LOOP_SOUND_NAME, 0.7f);
        Debug.Log("[StickyFoamSprayAttack] Started sticky foam spray");
    }

    public void UpdateAttack(Transform weaponTransform, PlayerPower playerPower)
    {
        if (!_isAttacking) return;

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

      
        PerformEnemySlowLogic(weaponTransform);

       
        _nextFoamSpawn += Time.deltaTime;
        if (_nextFoamSpawn >= foamSpawnInterval)
        {
            SpawnStickyFoamBurst(weaponTransform);
            _nextFoamSpawn = 0f;
        }
    }

    public void StopAttack(Transform weaponTransform, PlayerPower playerPower)
    {
        if (!_isAttacking) return;

        _isAttacking = false;

        if (_activeEffect != null)
        {
            Object.Destroy(_activeEffect);
            _activeEffect = null;
        }
        SoundManager.Instance?.StopNamedLoop(LOOP_SOUND_ID);
        Debug.Log("[StickyFoamSprayAttack] Stopped sticky foam spray");
    }

    private void CreateFoamEffect(Transform weaponTransform)
    {
        GameObject foamPrefab = Resources.Load<GameObject>("FoamSprayEffect");
        if (foamPrefab != null && weaponTransform != null)
        {
            _activeEffect = Object.Instantiate(foamPrefab, weaponTransform);
        }
    }

    private void PerformEnemySlowLogic(Transform weaponTransform)
    {
        if (weaponTransform == null) return;

       
        Ray ray = new Ray(weaponTransform.position, weaponTransform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, maxRange);

        foreach (var hit in hits)
        {
            var foamAffectable = hit.collider.GetComponent<IFoamAffectable>();
            if (foamAffectable != null)
            {
                foamAffectable.ApplyFoamSlow(slowAmount, slowDuration);
            }

            var damageable = hit.collider.GetComponent<IDamageable>();
            damageable?.TakeDamage(1);
        }
    }

    private void SpawnStickyFoamBurst(Transform weaponTransform)
    {
        if (weaponTransform == null) return;

        GameObject platformPrefab = stickyFoamPrefab ?? Resources.Load<GameObject>("StickyFoamPlatform");
        if (platformPrefab == null)
        {
            Debug.LogWarning("[StickyFoamSprayAttack] No sticky foam platform prefab found");
            return;
        }

        for (int i = 0; i < foamBurstCount; i++)
        {
            SpawnSingleStickyFoam(weaponTransform, platformPrefab, i);
        }
    }

    private void SpawnSingleStickyFoam(Transform weaponTransform, GameObject platformPrefab, int burstIndex)
    {
      
        Vector3 spawnPosition = weaponTransform.position + weaponTransform.forward * 1f + weaponTransform.up * 0.5f;

      
        Vector3 shootDirection = CalculateShootDirection(weaponTransform, burstIndex);

        
        //Vector3 randomOffset = Random.insideUnitSphere * 0.05f;
        //spawnPosition += randomOffset;

        
        GameObject stickyFoam = Object.Instantiate(platformPrefab, spawnPosition, Quaternion.identity);

       
        float launchSpeed = useRandomSpeed
            ? Random.Range(minLaunchSpeed, maxLaunchSpeed)
            : (minLaunchSpeed + maxLaunchSpeed) * 0.5f;

        Rigidbody foamRb = stickyFoam.GetComponent<Rigidbody>();
        if (foamRb != null)
        {
           
            Vector3 finalDirection = (shootDirection + Vector3.up * upwardForce).normalized;
            foamRb.linearVelocity = finalDirection * launchSpeed;

          
            foamRb.angularVelocity = Vector3.zero;
        }

        Debug.Log($"[StickyFoamSprayAttack] Spawned sticky foam #{burstIndex + 1}");
    }

    private Vector3 CalculateShootDirection(Transform weaponTransform, int burstIndex)
    {
        Vector3 baseDirection = weaponTransform.forward;

        
        if (foamBurstCount > 1)
        {
            float spreadAngle = (burstIndex - (foamBurstCount - 1) * 0.5f) * burstSpread;
            baseDirection = Quaternion.AngleAxis(spreadAngle, weaponTransform.up) * baseDirection;
        }

        
        if (aimAssist)
        {
            Ray aimRay = new Ray(weaponTransform.position, baseDirection);
            if (Physics.Raycast(aimRay, out RaycastHit hit, maxRange, targetLayers))
            {
               
                Vector3 directionToTarget = (hit.point - weaponTransform.position).normalized;
                baseDirection = Vector3.Slerp(baseDirection, directionToTarget, 0.3f); 
            }
        }

        return baseDirection;
    }

 
    public void SetStickyFoamSettings(float spawnInterval, int burstCount, float spread)
    {
        foamSpawnInterval = Mathf.Max(0.05f, spawnInterval);
        foamBurstCount = Mathf.Clamp(burstCount, 1, 5); 
        burstSpread = Mathf.Clamp(spread, 0f, 30f);
    }

    public void SetLaunchParameters(float minSpeed, float maxSpeed, float upward)
    {
        minLaunchSpeed = Mathf.Max(1f, minSpeed);
        maxLaunchSpeed = Mathf.Max(minLaunchSpeed + 1f, maxSpeed);
        upwardForce = Mathf.Clamp01(upward);
    }

    public void SetTargetingSettings(float range, bool assist)
    {
        maxRange = Mathf.Max(5f, range);
        aimAssist = assist;
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