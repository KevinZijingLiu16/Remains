using UnityEngine;

public class DamageTrap : BaseTrap
{
    [Header("Trap Type")]
    [SerializeField] private TrapType trapType = TrapType.Light;

    void Awake()
    {
    
        damageAmount = (int)trapType;
    }

    protected override void OnDamageDealt()
    {
        base.OnDamageDealt();
        Debug.Log($"[DamageTrap] {trapType} trap activated!");
    }
}


