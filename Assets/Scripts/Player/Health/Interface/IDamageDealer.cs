using UnityEngine;

public interface IDamageDealer
{
    int DamageAmount { get; }
    void DealDamage(IHealth target);
}