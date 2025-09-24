using UnityEngine;

public interface IHealthEvents
{
    System.Action<int, int> OnHealthChanged { get; set; } // (currentHealth, maxHealth)
    System.Action OnDeath { get; set; }
    System.Action<int> OnDamage { get; set; } 
    System.Action<int> OnHeal { get; set; } 
}
