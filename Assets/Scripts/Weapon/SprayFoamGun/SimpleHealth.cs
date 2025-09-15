using UnityEngine;

public class SimpleHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public float maxHealth = 10f;

    private float _currentHealth;

    public float CurrentHealth => _currentHealth;
    public bool IsAlive => _currentHealth > 0;

    void Start()
    {
        _currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        _currentHealth = Mathf.Max(0f, _currentHealth);

        Debug.Log($"[SimpleHealth] {gameObject.name} took {damage} damage, health: {_currentHealth}");

        if (!IsAlive)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"[SimpleHealth] {gameObject.name} died");
        // 可以添加死亡效果、掉落物品等
        Destroy(gameObject, 0.1f);
    }
}