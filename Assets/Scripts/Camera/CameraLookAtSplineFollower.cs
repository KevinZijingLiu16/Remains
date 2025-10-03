using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

[DisallowMultipleComponent]
public class CameraLookAtSplineFollower : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SplineRunnerRB playerRunner;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private SplineTracker splineTracker; 
    public SplineContainer splineContainer; 

    [Header("Input (New Input System)")]
    public InputActionProperty moveAction;

    [Header("Motion Settings")]
    [SerializeField] private bool loop = true;
    [SerializeField] private bool decoupleYFromSpline = true;
    [SerializeField] private float offsetAlongSplineMeters = 2.0f;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.6f, 0f);
    [Range(0.01f, 1f)]
    [SerializeField] private float stick = 0.9f;

    [Header("Facing")]
    [SerializeField] private bool faceHorizontalOnly = true;
    [SerializeField] private bool rotateAlongTangent = false;

    // runtime
    private float _t;
    private float _cachedMove;

    void Awake()
    {
        ResolveRefs();

        if (!ValidateSplineTracker())
        {
            Debug.LogWarning("[CameraLookAtSplineFollower] No valid spline tracker.");
            //enabled = false;
            //return;
        }

   
        float tPlayer = GetNearestTOnSpline(playerTransform ? playerTransform.position : transform.position);
        float dtOffset = Mathf.Clamp(offsetAlongSplineMeters / math.max(0.001f, splineTracker.ApproximateLength), -1f, 1f);
        _t = loop ? Mathf.Repeat(tPlayer + dtOffset, 1f) : Mathf.Clamp01(tPlayer + dtOffset);

        SnapToT(_t, immediate: true);
    }

    void OnEnable()
    {
        moveAction.action?.Enable();
    }

    void OnDisable()
    {
        moveAction.action?.Disable();
    }

    void Update()
    {
        _cachedMove = moveAction.action != null ? moveAction.action.ReadValue<float>() : 0f;
    }

    void FixedUpdate()
    {
        if (!ValidateSplineTracker()) return;
        if (playerRunner == null) return;

      
        float playerT = playerRunner.GetCurrentT();

       
        float dtOffset = offsetAlongSplineMeters / math.max(0.001f, splineTracker.ApproximateLength);
        _t = loop ? Mathf.Repeat(playerT + dtOffset, 1f) : Mathf.Clamp01(playerT + dtOffset);

        SnapToT(_t, immediate: false);
    }

    void SnapToT(float t, bool immediate)
    {
        if (!ValidateSplineTracker()) return;

     
        Vector3 splineWorldPos = splineTracker.GetWorldPositionAtT(t);
        Vector3 splineWorldTangent = splineTracker.GetWorldTangentAtT(t);

        Vector3 pos = splineWorldPos;

        if (decoupleYFromSpline)
        {
         
            float baseY = (playerTransform ? playerTransform.position.y : pos.y) + worldOffset.y;
            pos.y = baseY;

        
            pos.x += worldOffset.x;
            pos.z += worldOffset.z;
        }
        else
        {
        
            pos += worldOffset;
        }

    
        if (immediate || stick >= 0.999f)
            transform.position = pos;
        else
            transform.position = Vector3.Lerp(transform.position, pos, Mathf.Clamp01(stick));

   
        if (rotateAlongTangent)
        {
            Vector3 fwd = splineWorldTangent;
            if (faceHorizontalOnly)
            {
                fwd.y = 0f;
                if (fwd.sqrMagnitude > 1e-8f)
                    fwd.Normalize();
                else
                    fwd = transform.forward;
            }
            transform.rotation = Quaternion.LookRotation(fwd, Vector3.up);
        }
    }

    void ResolveRefs()
    { 
        if (playerRunner == null)
            playerRunner = FindFirstObjectByType<SplineRunnerRB>();

        if (playerRunner != null && playerTransform == null)
            playerTransform = playerRunner.transform;

     
        if (splineTracker == null && playerRunner != null)
            splineTracker = playerRunner.GetComponent<SplineTracker>();

    
        if (splineContainer == null && splineTracker != null)
            splineContainer = splineTracker.splineContainer;

        if (moveAction.action == null && playerRunner != null && playerRunner.moveAction.action != null)
        {
            moveAction = playerRunner.moveAction;
        }
    }

    bool ValidateSplineTracker()
    {
        if (splineTracker == null) return false;
        return splineTracker.IsValid();
    }

    float GetNearestTOnSpline(Vector3 worldPos)
    {
        if (!ValidateSplineTracker()) return 0f;

        var projection = splineTracker.ProjectWorldPointToSpline(worldPos);
        return projection.isValid ? projection.t : 0f;
    }


    public void SetOffset(float offsetMeters)
    {
        offsetAlongSplineMeters = offsetMeters;
    }

    public void SetWorldOffset(Vector3 offset)
    {
        worldOffset = offset;
    }

    public float GetCurrentT() => _t;
    public Vector3 GetCurrentWorldOffset() => worldOffset;
    public float GetCurrentSplineOffset() => offsetAlongSplineMeters;


    [ContextMenu("Sync to Player")]
    public void SyncToPlayer()
    {
        if (playerRunner != null && ValidateSplineTracker())
        {
            float playerT = playerRunner.GetCurrentT();
            float dtOffset = offsetAlongSplineMeters / math.max(0.001f, splineTracker.ApproximateLength);
            _t = loop ? Mathf.Repeat(playerT + dtOffset, 1f) : Mathf.Clamp01(playerT + dtOffset);
            SnapToT(_t, immediate: true);
        }
    }
}