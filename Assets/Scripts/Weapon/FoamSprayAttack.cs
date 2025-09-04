using UnityEngine;

public class FoamSprayAttack : IWeaponAttackBehavior
{
    private bool _isAttacking = false;
    private GameObject _activeEffect;
    private float _nextPowerCost = 0f;

    public int GetPowerCostPerSecond() => 5; 

    public bool CanAttack(PlayerPower playerPower)
    {
        return playerPower != null && playerPower.Current > 0;
    }

    public void StartAttack(Transform weaponTransform, PlayerPower playerPower)
    {
        if (!CanAttack(playerPower) || _isAttacking) return;

        _isAttacking = true;
        _nextPowerCost = 0f;

      
        CreateFoamEffect(weaponTransform);

        Debug.Log("[FoamSprayAttack] Started foam spray");
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

       
        PerformFoamSprayLogic(weaponTransform);
    }

    public void StopAttack(Transform weaponTransform, PlayerPower playerPower)
    {
        if (!_isAttacking) return;

        _isAttacking = false;

        // 清理特效
        if (_activeEffect != null)
        {
            Object.Destroy(_activeEffect);
            _activeEffect = null;
        }

        Debug.Log("[FoamSprayAttack] Stopped foam spray");
    }

    private void CreateFoamEffect(Transform weaponTransform)
    {
        
        GameObject foamPrefab = Resources.Load<GameObject>("FoamSprayEffect");
        if (foamPrefab != null && weaponTransform != null)
        {
            _activeEffect = Object.Instantiate(foamPrefab, weaponTransform);
        }
    }

    private void PerformFoamSprayLogic(Transform weaponTransform)
    {
        if (weaponTransform == null) return;

        
        Ray ray = new Ray(weaponTransform.position, weaponTransform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, 10f);

        foreach (var hit in hits)
        {
            
            var damageable = hit.collider.GetComponent<IDamageable>();
            damageable?.TakeDamage(1); 
        }
    }
}