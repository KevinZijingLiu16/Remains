using UnityEngine;

public class WeaponAttackData
{
    [Header("Attack Settings")]
    public AttackType primaryAttackType = AttackType.None;
    public AttackType secondaryAttackType = AttackType.None;

    [Header("Power Consumption")]
    public int primaryPowerCostPerSecond = 5;
    public int secondaryPowerCostPerSecond = 5;

    [Header("Attack Parameters")]
    public float attackRange = 10f;
    public float attackForce = 100f;
    public LayerMask attackLayers = ~0;

    [Header("Effects")]
    public GameObject attackEffectPrefab;
    public Transform attackOrigin; 
}
