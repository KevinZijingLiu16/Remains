using UnityEngine;

public interface IHealth
{
    int MaxHealth { get; }
    int CurrentHealth { get; }
    bool IsDead { get; }

    bool TakeDamage(int damage);
    bool Heal(int amount);
    void ResetToMaxHealth();
}

