using UnityEngine;

/// <summary>
/// 负责移动对象沿spline轨道，可以被player和movable object共享使用
/// </summary>
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

    /// <summary>
    /// 移动刚体沿spline轨道到指定t值位置
    /// </summary>
    public SplineMoveResult MoveToSplinePosition(Rigidbody rb, float targetT, float previousT, float movementDirection)
    {
        if (!_splineTracker.IsValid())
        {
            return new SplineMoveResult { success = false, finalT = previousT };
        }

        Vector3 currentPos = rb.position;
        Vector3 splineWorldPos = _splineTracker.GetWorldPositionAtT(targetT);

        // 根据设置决定是否解耦Y轴
        Vector3 targetPos = decoupleYFromSpline
            ? new Vector3(splineWorldPos.x, currentPos.y, splineWorldPos.z)
            : splineWorldPos;

        // 插值到目标位置
        Vector3 lerpedPos = Vector3.Lerp(currentPos, targetPos, Mathf.Clamp01(stickiness));

        if (float.IsNaN(lerpedPos.x))
        {
            return new SplineMoveResult { success = false, finalT = previousT };
        }

        // 碰撞检测和处理
        return HandleCollisionAndMove(rb, currentPos, lerpedPos, targetT, previousT, movementDirection);
    }

    /// <summary>
    /// 处理碰撞并移动对象
    /// </summary>
    private SplineMoveResult HandleCollisionAndMove(Rigidbody rb, Vector3 from, Vector3 to, float targetT, float previousT, float movementDirection)
    {
        if (_collisionSweeper.SweepPath(rb, from, to, out Vector3 safePos, out RaycastHit hit, obstacleLayers | movableLayers))
        {
            // 检查是否撞到可移动对象
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
                // 撞到障碍物，尝试滑动
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
                    finalT = previousT, // 撞墙了，保持原来的t值
                    hitObstacle = true
                };
            }
        }
        else
        {
            // 没有碰撞，正常移动
            rb.MovePosition(to);
            return new SplineMoveResult { success = true, finalT = targetT };
        }
    }

    /// <summary>
    /// 处理推动可移动对象
    /// </summary>
    private PushResult HandleMovablePush(Rigidbody pusher, RaycastHit hit, Vector3 remainingMovement, float direction)
    {
        Rigidbody targetRB = hit.rigidbody;
        if (targetRB == null || targetRB.isKinematic)
        {
            return new PushResult { pushedSuccessfully = false };
        }

        // 计算推力
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

        // 推动对象
        return PushMovableAlongSpline(targetRB, pushDistance);
    }

    /// <summary>
    /// 推动可移动对象沿spline移动
    /// </summary>
    public PushResult PushMovableAlongSpline(Rigidbody targetRB, float pushDistance)
    {
        if (!_splineTracker.IsValid() || Mathf.Abs(pushDistance) < 1e-6f)
        {
            return new PushResult { pushedSuccessfully = false };
        }

        // 找到对象在spline上的位置
        var projection = _splineTracker.ProjectWorldPointToSpline(targetRB.position);
        if (!projection.isValid)
        {
            return new PushResult { pushedSuccessfully = false };
        }

        // 计算推动后的新t值
        float newT = _splineTracker.GetTAfterDistance(projection.t, pushDistance);
        Vector3 newSplinePos = _splineTracker.GetWorldPositionAtT(newT);
        Vector3 targetPos = new Vector3(newSplinePos.x, targetRB.position.y, newSplinePos.z);

        // 检查推动路径是否有障碍
        if (_collisionSweeper.SweepPath(targetRB, targetRB.position, targetPos, out Vector3 safePos, out _, obstacleLayers | movableLayers))
        {
            targetRB.MovePosition(safePos);
            return new PushResult { pushedSuccessfully = true, finalPosition = safePos };
        }
        else
        {
            targetRB.MovePosition(targetPos);
            return new PushResult { pushedSuccessfully = true, finalPosition = targetPos };
        }
    }

    /// <summary>
    /// 将对象快照到spline上
    /// </summary>
    public void SnapToSpline(Rigidbody rb)
    {
        if (!_splineTracker.IsValid()) return;

        Vector3 snappedPos = _splineTracker.SnapToNearestPoint(rb.position, decoupleYFromSpline);
        if (!float.IsNaN(snappedPos.x))
        {
            rb.position = snappedPos;
        }
    }

    /// <summary>
    /// 获取当前设置
    /// </summary>
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

    /// <summary>
    /// 应用设置
    /// </summary>
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

/// <summary>
/// Spline移动结果
/// </summary>
public struct SplineMoveResult
{
    public bool success;
    public float finalT;
    public bool hitObstacle;
    public bool hitMovable;
    public Rigidbody pushedObject;
}

/// <summary>
/// 推动结果
/// </summary>
public struct PushResult
{
    public bool pushedSuccessfully;
    public Vector3 finalPosition;
}

/// <summary>
/// Spline移动器设置
/// </summary>
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