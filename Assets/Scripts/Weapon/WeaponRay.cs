using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WeaponRay : MonoBehaviour
{
    public Transform origin;                // 发射点（通常是枪口）
    public float extendSpeed = 20f;         // 射线延长速度（m/s）
    public float maxDistance = 50f;         // 最长射程
    public LayerMask hitLayers = ~0;        // 检测层
    public GameObject hitEffectPrefab;      // 命中特效预制体（例如发光小球或粒子）
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
        // 延长射线（直到最大距离或命中）
        if (currentHitCollider == null)
        {
            currentLength += extendSpeed * Time.deltaTime;
            if (currentLength > maxDistance) currentLength = maxDistance;

            Vector3 dir = origin.forward;
            Vector3 start = origin.position;

            // 用 currentLength 做射线长度，看看在这长度范围内是否有命中
            if (Physics.Raycast(start, dir, out RaycastHit hit, currentLength, hitLayers, QueryTriggerInteraction.Ignore))
            {
                // 命中：把线终点设为命中点，创建/移动命中特效并 parent（可选）
                SetLinePositions(start, hit.point);

                if (currentEffect == null)
                {
                    if(hitEffectPrefab!=null)
                    { currentEffect = Instantiate(hitEffectPrefab, hit.point, Quaternion.identity); }
                   
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
                // 未命中：线延伸到当前长度
                Vector3 end = origin.position + origin.forward * currentLength;
                SetLinePositions(origin.position, end);

                // 如果之前有特效，销毁它（因为现在没有命中）
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
            // 已经命中过且 effect 被 parent 到 target：保持线段连接 origin->命中点（动态更新命中点）
            // 我们用当前 effect 的位置作为终点（这样能跟随移动）
            Vector3 start = origin.position;
            Vector3 end = currentEffect != null ? currentEffect.transform.position : (start + origin.forward * currentLength);
            SetLinePositions(start, end);

            // 如果目标被销毁或禁用，重置射线
            if (!currentHitCollider || !currentHitCollider.enabled)
            {
                ResetRay();
            }
        }

        // 可选：当达到 maxDistance 仍未命中，可以自动重置或停在末端
        if (currentLength >= maxDistance && currentHitCollider == null)
        {
            // 如果不想自动消失，把下面注释掉
            // ResetRay();
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

        // 把线收回到 origin
        SetLinePositions(origin.position, origin.position);
    }
}
