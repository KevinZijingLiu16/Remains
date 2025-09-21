using UnityEngine;


[RequireComponent(typeof(SplineTracker), typeof(CollisionSweeper))]
public class SplineObjectMover : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField, Range(0.01f, 1f)] private float stickiness = 0.9f;
    [SerializeField] private bool decoupleYFromSpline = true;

    [Header("Push Settings")]
    [SerializeField] private float pushTransferRatio = 1.0f;
    [SerializeField] private float pushMaxPerStep = 1.0f;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask obstacleLayers = ~0;
    [SerializeField] private LayerMask movableLayers = 0;

    private SplineTracker _splineTracker;
    private CollisionSweeper _collisionSweeper;

    public SplineTracker SplineTracker => _splineTracker;
    public CollisionSweeper CollisionSweeper => _collisionSweeper;

    void Awake()
    {
        _splineTracker = GetComponent<SplineTracker>();
        _collisionSweeper = GetComponent<CollisionSweeper>();
    }

    public SplineMoveResult MoveToSplinePosition(Rigidbody rb, float targetT, float previousT, float movementDirection)
    {
        if (!_splineTracker.IsValid())
        {
            return new SplineMoveResult { success = false, finalT = previousT };
        }

        Vector3 currentPos = rb.position;
        Vector3 splineWorldPos = _splineTracker.GetWorldPositionAtT(targetT);

    
        Vector3 targetPos = decoupleYFromSpline
            ? new Vector3(splineWorldPos.x, currentPos.y, splineWorldPos.z)
            : splineWorldPos;

     
        Vector3 lerpedPos = Vector3.Lerp(currentPos, targetPos, Mathf.Clamp01(stickiness));

        if (float.IsNaN(lerpedPos.x))
        {
            return new SplineMoveResult { success = false, finalT = previousT };
        }

        return HandleCollisionAndMove(rb, currentPos, lerpedPos, targetT, previousT, movementDirection);
    }


    private SplineMoveResult HandleCollisionAndMove(Rigidbody rb, Vector3 from, Vector3 to, float targetT, float previousT, float movementDirection)
    {
        if (_collisionSweeper.SweepPath(rb, from, to, out Vector3 safePos, out RaycastHit hit, obstacleLayers | movableLayers))
        {
       
            if (CollisionSweeper.IsInLayerMask(hit.collider.gameObject.layer, movableLayers))
            {
                var pushResult = HandleMovablePush(rb, hit, to - safePos, movementDirection);
                rb.MovePosition(safePos);

                return new SplineMoveResult
                {
                    success = true,
                    finalT = pushResult.pushedSuccessfully ? targetT : previousT,
                    hitMovable = true,
                    pushedObject = hit.rigidbody
                };
            }
            else
            {
              
                Vector3 remaining = to - safePos;
                Vector3 slide = _collisionSweeper.SlideAlongNormal(remaining, hit.normal);

                if (slide.sqrMagnitude > 1e-6f)
                {
                    Vector3 slideTarget = safePos + slide;
                    if (_collisionSweeper.SweepPath(rb, safePos, slideTarget, out Vector3 safeSlide, out _, obstacleLayers | movableLayers))
                    {
                        rb.MovePosition(safeSlide);
                    }
                    else
                    {
                        rb.MovePosition(slideTarget);
                    }
                }
                else
                {
                    rb.MovePosition(safePos);
                }

                return new SplineMoveResult
                {
                    success = true,
                    finalT = previousT, 
                    hitObstacle = true
                };
            }
        }
        else
        {
         
            rb.MovePosition(to);
            return new SplineMoveResult { success = true, finalT = targetT };
        }
    }

    private PushResult HandleMovablePush(Rigidbody pusher, RaycastHit hit, Vector3 remainingMovement, float direction)
    {
        Rigidbody targetRB = hit.rigidbody;
        if (targetRB == null || targetRB.isKinematic)
        {
            return new PushResult { pushedSuccessfully = false };
        }

        Vector3 splineTangent = _splineTracker.GetWorldTangentAtT(_splineTracker.ProjectWorldPointToSpline(pusher.position).t);
        Vector3 horizSplineFwd = new Vector3(splineTangent.x, 0f, splineTangent.z);

        if (horizSplineFwd.sqrMagnitude > 1e-6f)
            horizSplineFwd.Normalize();
        else
            horizSplineFwd = pusher.transform.forward;

        Vector3 effectivePush = Vector3.Project(remainingMovement, horizSplineFwd);
        float pushDistance = Mathf.Clamp(
            effectivePush.magnitude * Mathf.Sign(Vector3.Dot(effectivePush, horizSplineFwd)) * pushTransferRatio,
            -pushMaxPerStep, pushMaxPerStep);

      
        return PushMovableAlongSpline(targetRB, pushDistance);
    }

 
    public PushResult PushMovableAlongSpline(Rigidbody targetRB, float pushDistance)
    {
        if (!_splineTracker.IsValid() || Mathf.Abs(pushDistance) < 1e-6f)
        {
            return new PushResult { pushedSuccessfully = false };
        }

       
        var projection = _splineTracker.ProjectWorldPointToSpline(targetRB.position);
        if (!projection.isValid)
        {
            return new PushResult { pushedSuccessfully = false };
        }

        float newT = _splineTracker.GetTAfterDistance(projection.t, pushDistance);
        Vector3 newSplinePos = _splineTracker.GetWorldPositionAtT(newT);
        Vector3 targetPos = new Vector3(newSplinePos.x, targetRB.position.y, newSplinePos.z);

        Vector3 finalPos;
        if (_collisionSweeper.SweepPath(targetRB, targetRB.position, targetPos, out Vector3 safePos, out _, obstacleLayers | movableLayers))
        {
          
            var reprojection = _splineTracker.ProjectWorldPointToSpline(safePos);
            if (reprojection.isValid)
            {
                finalPos = new Vector3(reprojection.worldPosition.x, safePos.y, reprojection.worldPosition.z);
            }
            else
            {
                finalPos = safePos;
            }
        }
        else
        {
            finalPos = targetPos;
        }

        targetRB.MovePosition(finalPos);
        return new PushResult { pushedSuccessfully = true, finalPosition = finalPos };
    }

  
    public void SnapToSpline(Rigidbody rb)
    {
        if (!_splineTracker.IsValid()) return;

        Vector3 snappedPos = _splineTracker.SnapToNearestPoint(rb.position, decoupleYFromSpline);
        if (!float.IsNaN(snappedPos.x))
        {
            rb.position = snappedPos;
        }
    }


    public SplineMoverSettings GetSettings()
    {
        return new SplineMoverSettings
        {
            stickiness = stickiness,
            decoupleYFromSpline = decoupleYFromSpline,
            pushTransferRatio = pushTransferRatio,
            pushMaxPerStep = pushMaxPerStep,
            obstacleLayers = obstacleLayers,
            movableLayers = movableLayers
        };
    }

  
    public void ApplySettings(SplineMoverSettings settings)
    {
        stickiness = settings.stickiness;
        decoupleYFromSpline = settings.decoupleYFromSpline;
        pushTransferRatio = settings.pushTransferRatio;
        pushMaxPerStep = settings.pushMaxPerStep;
        obstacleLayers = settings.obstacleLayers;
        movableLayers = settings.movableLayers;
    }
}


public struct SplineMoveResult
{
    public bool success;
    public float finalT;
    public bool hitObstacle;
    public bool hitMovable;
    public Rigidbody pushedObject;
}


public struct PushResult
{
    public bool pushedSuccessfully;
    public Vector3 finalPosition;
}


[System.Serializable]
public struct SplineMoverSettings
{
    public float stickiness;
    public bool decoupleYFromSpline;
    public float pushTransferRatio;
    public float pushMaxPerStep;
    public LayerMask obstacleLayers;
    public LayerMask movableLayers;
}