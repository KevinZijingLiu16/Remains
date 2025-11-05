using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WeaponRay : MonoBehaviour
{
    public Transform origin;
    public float extendSpeed = 20f;
    public float maxDistance = 50f;
    public LayerMask hitLayers = ~0;

    [Header("Hit Effect (visual only)")]
    public GameObject hitEffectPrefab;   
    public bool parentEffectToTarget = false; 

    LineRenderer lr;
    float currentLength = 0f;
    GameObject currentEffect;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        if (origin == null) origin = transform;
    }

    void OnEnable() => ResetRay();

    void Update()
    {
     
        currentLength = Mathf.Min(currentLength + extendSpeed * Time.deltaTime, maxDistance);

        Vector3 start = origin.position;
        Vector3 dir = origin.forward;
        Vector3 end = start + dir * currentLength; 

     
        // Physics.Raycast(start, dir, out RaycastHit hit, currentLength, hitLayers, QueryTriggerInteraction.Ignore);

     
        SetLinePositions(start, end);

    
        if (hitEffectPrefab != null)
        {
            if (currentEffect == null)
                currentEffect = Instantiate(hitEffectPrefab);

            currentEffect.transform.SetParent(null, true); 
            currentEffect.transform.position = end;
           
            currentEffect.transform.rotation = Quaternion.LookRotation(-dir);
        }
    }

    void SetLinePositions(Vector3 a, Vector3 b)
    {
        lr.SetPosition(0, a);
        lr.SetPosition(1, b);
    }

    public void ResetRay()
    {
        currentLength = 0f;
        if (currentEffect != null)
        {
            Destroy(currentEffect);
            currentEffect = null;
        }
        SetLinePositions(origin.position, origin.position);
    }
}
