using UnityEngine;


public abstract class BaseTrap : MonoBehaviour, IDamageDealer
{
    [Header("Damage Settings")]
    [SerializeField] protected int damageAmount;
    [SerializeField] protected bool destroyAfterHit = false;
    [SerializeField] protected bool hasInvulnerabilityPeriod = true;
    [SerializeField] protected float invulnerabilityDuration = 1f;

    [Header("Effects")]
    [SerializeField] protected GameObject hitEffect;
    [SerializeField] protected AudioClip hitSound;

    private static float lastDamageTime;

    public int DamageAmount => damageAmount;

    protected virtual void OnTriggerEnter(Collider other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            DealDamage(playerHealth.Health);
        }
    }

    public virtual void DealDamage(IHealth target)
    {
    
        if (hasInvulnerabilityPeriod && Time.time - lastDamageTime < invulnerabilityDuration)
        {
            return;
        }

     
        if (target.TakeDamage(damageAmount))
        {
            lastDamageTime = Time.time;
            OnDamageDealt();

          
            PlayHitEffect();

           
            if (destroyAfterHit)
            {
                Destroy(gameObject);
            }
        }
    }

    protected virtual void OnDamageDealt()
    {
        Debug.Log($"[{GetType().Name}] Dealt {damageAmount} damage to player");
    }

    protected virtual void PlayHitEffect()
    {
      
        if (hitEffect != null)
        {
            hitEffect.gameObject.SetActive(true);
         hitEffect.GetComponent<ParticleSystem>().Play();
        }

     
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }
    }
}