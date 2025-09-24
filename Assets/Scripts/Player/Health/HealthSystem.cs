
using UnityEngine;

[System.Serializable]
public class HealthSystem : IHealth, IHealthEvents
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int currentHealth;

    // Events
    public System.Action<int, int> OnHealthChanged { get; set; }
    public System.Action OnDeath { get; set; }
    public System.Action<int> OnDamage { get; set; }
    public System.Action<int> OnHeal { get; set; }

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0;

    public HealthSystem(int maxHealth = 3)
    {
        this.maxHealth = maxHealth;
        currentHealth = maxHealth;
    }

    public bool TakeDamage(int damage)
    {
        if (damage <= 0 || IsDead) return false;

        int previousHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - damage);

        Debug.Log($"[HealthSystem] Took {damage} damage. Health: {previousHealth} -> {currentHealth}");

        OnDamage?.Invoke(damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0 && previousHealth > 0)
        {
            OnDeath?.Invoke();
            Debug.Log("[HealthSystem] Player died!");
        }

        return true;
    }

    public bool Heal(int amount)
    {
        if (amount <= 0)
        {
            Debug.Log($"[HealthSystem] Invalid heal amount: {amount}");
            return false;
        }

        if (currentHealth >= maxHealth)
        {
            Debug.Log("[HealthSystem] Already at max health, cannot heal");
            return false;
        }

        int previousHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        int actualHealAmount = currentHealth - previousHealth;

        Debug.Log($"[HealthSystem] Healed {actualHealAmount}. Health: {previousHealth} -> {currentHealth}");

        if (actualHealAmount > 0)
        {
            OnHeal?.Invoke(actualHealAmount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            return true;
        }

        return false;
    }

    public void ResetToMaxHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"[HealthSystem] Health reset to max: {maxHealth}");
    }
}