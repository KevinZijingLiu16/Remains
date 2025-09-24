using UnityEngine;

public class DelayedTrap : BaseTrap
{
    [Header("Delay Settings")]
    [SerializeField] private float activationDelay = 1f;
    [SerializeField] private GameObject warningEffect;

    private bool isArmed = true;
    private Coroutine activationCoroutine;

    protected override void OnTriggerEnter(Collider other)
    {
        if (!isArmed) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null && activationCoroutine == null)
        {
            activationCoroutine = StartCoroutine(DelayedActivation(playerHealth.Health));
        }
    }

    private System.Collections.IEnumerator DelayedActivation(IHealth target)
    {
    
        if (warningEffect != null)
        {
            warningEffect.SetActive(true);
        }

    
        yield return new WaitForSeconds(activationDelay);

     
        DealDamage(target);

   
        if (warningEffect != null)
        {
            warningEffect.SetActive(false);
        }

        isArmed = false;
        activationCoroutine = null;
    }
}