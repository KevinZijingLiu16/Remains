using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [Header("Vertical Follow (keep your original behavior)")]
    [Range(0f, 1f)]
  
    public float verticalFollow = 0f;

   
    public float ySmoothTime = 0.0f;

    [Header("Anti Jitter")]
  
    public float xzSmoothTime = 0.05f;

  
    public float rotationLerp = 20f;

 
    public bool sampleParentInFixedUpdate = true;

    Transform parentT;


    Vector3 localOffset;
    Quaternion localRot;


    Vector3 sampledParentPos;
    Quaternion sampledParentRot;
    bool hasSample;


    float xVel, zVel, yVel;

    void OnEnable()
    {
        parentT = transform.parent;
        if (parentT == null) return;

        localOffset = transform.localPosition;
        localRot = transform.localRotation;

        sampledParentPos = parentT.position;
        sampledParentRot = parentT.rotation;
        hasSample = true;
    }

    void FixedUpdate()
    {
        if (!sampleParentInFixedUpdate || parentT == null) return;
        sampledParentPos = parentT.position;
        sampledParentRot = parentT.rotation;
        hasSample = true;
    }

    void LateUpdate()
    {
        if (parentT == null) return;

      
        Vector3 pPos = (sampleParentInFixedUpdate && hasSample) ? sampledParentPos : parentT.position;
        Quaternion pRot = (sampleParentInFixedUpdate && hasSample) ? sampledParentRot : parentT.rotation;

        Vector3 targetPos = pPos + pRot * localOffset;
        Quaternion targetRot = pRot * localRot;

   
        float desiredY = Mathf.Lerp(transform.position.y, targetPos.y, verticalFollow);
        float newY = (ySmoothTime <= 0f)
            ? desiredY
            : Mathf.SmoothDamp(transform.position.y, desiredY, ref yVel, ySmoothTime);

      
        float newX = (xzSmoothTime <= 0f)
            ? targetPos.x
            : Mathf.SmoothDamp(transform.position.x, targetPos.x, ref xVel, xzSmoothTime);

        float newZ = (xzSmoothTime <= 0f)
            ? targetPos.z
            : Mathf.SmoothDamp(transform.position.z, targetPos.z, ref zVel, xzSmoothTime);

        transform.position = new Vector3(newX, newY, newZ);

   
        float t = 1f - Mathf.Exp(-rotationLerp * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, t);
    }

  
    public void ResetBaseline()
    {
        if (parentT == null) return;
        localOffset = transform.localPosition;
        localRot = transform.localRotation;
        sampledParentPos = parentT.position;
        sampledParentRot = parentT.rotation;
        hasSample = true;
        xVel = zVel = yVel = 0f;
    }
}
