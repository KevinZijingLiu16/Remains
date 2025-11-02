using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WeaponRay : MonoBehaviour
{
    public Transform origin;                
    public float extendSpeed = 20f;         
    public float maxDistance = 50f;         
    public LayerMask hitLayers = ~0;        
     
    public bool parentEffectToTarget = true;

    LineRenderer lr;
    float currentLength = 0f;
    GameObject currentEffect;
    Collider currentHitCollider;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        if (origin == null) origin = this.transform;
    }

    void OnEnable()
    {
        ResetRay();
    }

    void Update()
    {
        if (currentHitCollider == null)
        {
            currentLength += extendSpeed * Time.deltaTime;
            if (currentLength > maxDistance) currentLength = maxDistance;

            Vector3 dir = origin.forward;
            Vector3 start = origin.position;

            if (Physics.Raycast(start, dir, out RaycastHit hit, currentLength, hitLayers, QueryTriggerInteraction.Ignore))
            {
                SetLinePositions(start, hit.point);

                if (currentEffect == null)
                {
                  
                   
                    if (parentEffectToTarget && hit.collider != null)
                        currentEffect.transform.SetParent(hit.collider.transform, true);
                }
                else
                {
                    currentEffect.transform.position = hit.point;
                    if (parentEffectToTarget && currentEffect.transform.parent != hit.collider.transform)
                        currentEffect.transform.SetParent(hit.collider.transform, true);
                }

                currentHitCollider = hit.collider;
            }
            else
            {
                Vector3 end = origin.position + origin.forward * currentLength;
                SetLinePositions(origin.position, end);

                if (currentEffect != null)
                {
                    Destroy(currentEffect);
                    currentEffect = null;
                    currentHitCollider = null;
                }
            }
        }
        else
        {
   
            Vector3 start = origin.position;
            Vector3 end = currentEffect != null ? currentEffect.transform.position : (start + origin.forward * currentLength);
            SetLinePositions(start, end);

   
            if (!currentHitCollider || !currentHitCollider.enabled)
            {
                ResetRay();
            }
        }

        if (currentLength >= maxDistance && currentHitCollider == null)
        {
         
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
        currentHitCollider = null;
        if (currentEffect != null)
        {
            Destroy(currentEffect);
            currentEffect = null;
        }

   
        SetLinePositions(origin.position, origin.position);
    }
}
