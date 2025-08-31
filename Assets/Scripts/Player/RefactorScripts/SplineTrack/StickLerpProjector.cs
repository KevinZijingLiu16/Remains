using UnityEngine;

public class StickLerpProjector : IStickProjector
{
    public Vector3 Project(Vector3 current, Vector3 targetOnTrack, float stick01, bool decoupleY, float? overrideY = null)
    {
        Vector3 target = targetOnTrack;

        if (decoupleY)
        {
            float y = overrideY.HasValue ? overrideY.Value : current.y;
            target = new Vector3(targetOnTrack.x, y, targetOnTrack.z);
        }

        float s = Mathf.Clamp01(stick01);
 
        return Vector3.Lerp(current, target, 1f - s);
    }
}
