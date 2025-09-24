using UnityEngine;

public interface IHealthUI
{
    void UpdateHealthDisplay(int currentHealth, int maxHealth);
    void PlayDamageEffect(int damageAmount);
    void PlayHealEffect(int healAmount);
}
