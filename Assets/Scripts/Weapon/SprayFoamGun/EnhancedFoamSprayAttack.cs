using UnityEngine;

public class EnhancedFoamSprayAttack : IWeaponAttackBehavior
{
    private bool _isAttacking = false;
    private GameObject _activeEffect;
    private float _nextPowerCost = 0f;
    private float _nextFoamSpawn = 0f;

    [Header("Foam Spawn Settings")]
    public GameObject foamPlatformPrefab; 
    public float foamSpawnInterval = 0.1f; 
    public int foamBurstCount = 100; 
    public float burstSpread = 0f;

    [Header("Foam Launch Settings")]
    public float minLaunchSpeed = 3f; 
    public float maxLaunchSpeed = 10f;
    public float upwardForce = 0.5f; // 0 is horizaontal, 1 is vertical
    public bool useRandomSpeed = true; 

    [Header("Enemy Effects")]
    public float slowAmount = 1f; 
    public float slowDuration = 10f; 

    public int GetPowerCostPerSecond() => 20;

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
        Debug.Log("[EnhancedFoamSprayAttack] Started enhanced foam spray");
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

        
        PerformEnhancedFoamLogic(weaponTransform);

        
        _nextFoamSpawn += Time.deltaTime;
        if (_nextFoamSpawn >= foamSpawnInterval)
        {
            SpawnFoamBurst(weaponTransform);
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

        Debug.Log("[EnhancedFoamSprayAttack] Stopped enhanced foam spray");
    }

    private void CreateFoamEffect(Transform weaponTransform)
    {
        GameObject foamPrefab = Resources.Load<GameObject>("FoamSprayEffect");
        if (foamPrefab != null && weaponTransform != null)
        {
            _activeEffect = Object.Instantiate(foamPrefab, weaponTransform);
        }
    }

    private void PerformEnhancedFoamLogic(Transform weaponTransform)
    {
        if (weaponTransform == null) return;

      
        Ray ray = new Ray(weaponTransform.position, weaponTransform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, 10f);

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

    
    private void SpawnFoamBurst(Transform weaponTransform)
    {
        if (weaponTransform == null) return;

        GameObject platformPrefab = foamPlatformPrefab ?? Resources.Load<GameObject>("FoamPlatform");
        if (platformPrefab == null)
        {
            Debug.LogWarning("[EnhancedFoamSprayAttack] No foam platform prefab found");
            return;
        }

        
        for (int i = 0; i < foamBurstCount; i++)
        {
            SpawnSingleFoam(weaponTransform, platformPrefab, i);
        }
    }

    private void SpawnSingleFoam(Transform weaponTransform, GameObject platformPrefab, int burstIndex)
    {
      
        Vector3 baseSpawnPos = weaponTransform.position + weaponTransform.forward * 1.5f;

       
        Vector3 spawnPosition = baseSpawnPos;
        Vector3 shootDirection = weaponTransform.forward;

        if (foamBurstCount > 1)
        {
           
            float spreadAngle = (burstIndex - (foamBurstCount - 1) * 0.5f) * burstSpread;

            
            shootDirection = Quaternion.AngleAxis(spreadAngle, weaponTransform.up) * weaponTransform.forward;

           
            Vector3 sideOffset = weaponTransform.right * (spreadAngle * 0.01f); 
            spawnPosition = baseSpawnPos + sideOffset;
        }

       
        Vector3 randomOffset = new Vector3(
            Random.Range(-0.1f, 0.1f),
            Random.Range(-0.05f, 0.05f),
            Random.Range(-0.1f, 0.1f)
        );
        spawnPosition += randomOffset;

      
        GameObject foamPlatform = Object.Instantiate(platformPrefab, spawnPosition, Random.rotation);

     
        float launchSpeed = useRandomSpeed
            ? Random.Range(minLaunchSpeed, maxLaunchSpeed)
            : (minLaunchSpeed + maxLaunchSpeed) * 0.5f;

       
        Rigidbody platformRb = foamPlatform.GetComponent<Rigidbody>();
        if (platformRb != null)
        {
           
            Vector3 finalDirection = (shootDirection + Vector3.up * upwardForce).normalized;
            platformRb.linearVelocity = finalDirection * launchSpeed;

           
            platformRb.angularVelocity = Random.insideUnitSphere * 2f;
        }

        //Debug.Log($"[EnhancedFoamSprayAttack] Spawned foam #{burstIndex + 1} with speed {launchSpeed:F1}");
    }

   
    public void SetSpawnRate(float interval)
    {
        foamSpawnInterval = Mathf.Max(0.05f, interval);
    }

    public void SetBurstSettings(int count, float spread)
    {
        foamBurstCount = Mathf.Clamp(count, 1, 10); 
        burstSpread = Mathf.Clamp(spread, 0f, 45f); 
    }

    public void SetLaunchSpeed(float min, float max)
    {
        minLaunchSpeed = Mathf.Max(0.5f, min);
        maxLaunchSpeed = Mathf.Max(minLaunchSpeed + 0.5f, max);
    }

    public void SetUpwardForce(float force)
    {
        upwardForce = Mathf.Clamp01(force);
    }
}