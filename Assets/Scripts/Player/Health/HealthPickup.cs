
using UnityEngine;
using System.Collections;

public class HealthPickup : MonoBehaviour, IHealthRestore
{
    [Header("Restore Settings")]
    [SerializeField] private int restoreAmount = 1;
    [SerializeField] private bool destroyAfterUse = true;
    [SerializeField] private bool oneTimeUse = true;

    [Header("Visual Effects")]
    [SerializeField] private GameObject collectEffect;
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.5f;
    [SerializeField] private float rotateSpeed = 45f;

    [Header("Collection Animation")]
    [SerializeField] private float collectDuration = 0.5f;
    [SerializeField] private AnimationCurve collectCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector3 startPosition;
    private bool hasBeenCollected = false;
    private Renderer[] renderers;
    private Collider itemCollider;

    public int RestoreAmount => restoreAmount;

    void Start()
    {
        startPosition = transform.position;
        renderers = GetComponentsInChildren<Renderer>();
        itemCollider = GetComponent<Collider>();

      
        if (itemCollider == null)
        {
            itemCollider = gameObject.AddComponent<SphereCollider>();
            Debug.LogWarning("[HealthPickup] No collider found, added SphereCollider automatically");
        }
        itemCollider.isTrigger = true;
    }

    void Update()
    {
        if (!hasBeenCollected)
        {
            AnimatePickup();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasBeenCollected) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            RestoreHealth(playerHealth.Health);
        }
    }

    public void RestoreHealth(IHealth target)
    {
      
        if (hasBeenCollected || target.CurrentHealth >= target.MaxHealth)
        {
            if (target.CurrentHealth >= target.MaxHealth)
            {
                Debug.Log("[HealthPickup] Player already at max health, pickup ignored");
            }
            return;
        }

      
        bool restored = target.Heal(restoreAmount);

        if (restored)
        {
            Debug.Log($"[HealthPickup] Restored {restoreAmount} health to player");
            OnHealthRestored();

          
            StartCoroutine(CollectAnimation());
        }
    }

    protected virtual void OnHealthRestored()
    {
      
        hasBeenCollected = oneTimeUse;
    }

    private void AnimatePickup()
    {
       
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

     
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.World);
    }

    private IEnumerator CollectAnimation()
    {
       
        if (itemCollider != null)
        {
            itemCollider.enabled = false;
        }

    
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, transform.rotation);
        }

      
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

       
        Vector3 startScale = transform.localScale;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * 2f;

        float elapsedTime = 0f;

        while (elapsedTime < collectDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / collectDuration;
            float curveValue = collectCurve.Evaluate(normalizedTime);

           
            float scaleMultiplier = 1f - curveValue;
            transform.localScale = startScale * scaleMultiplier;

         
            transform.position = Vector3.Lerp(startPos, endPos, curveValue);

          
            SetAlpha(1f - curveValue);

            yield return null;
        }

       
        if (destroyAfterUse)
        {
            Destroy(gameObject);
        }
        else
        {
           
            gameObject.SetActive(false);
            if (!oneTimeUse)
            {
                StartCoroutine(RespawnAfterDelay(10f)); 
            }
        }
    }

    private void SetAlpha(float alpha)
    {
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null && renderer.material != null)
            {
               
                Material materialInstance = renderer.material;
                Color color = materialInstance.color;
                color.a = alpha;
                materialInstance.color = color;
            }
        }
    }

    private IEnumerator RespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

       
        hasBeenCollected = false;
        transform.localScale = Vector3.one;
        transform.position = startPosition;
        SetAlpha(1f);

        if (itemCollider != null)
        {
            itemCollider.enabled = true;
        }

        gameObject.SetActive(true);
        Debug.Log("[HealthPickup] Health pickup respawned");
    }
}
