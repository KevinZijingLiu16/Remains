using UnityEngine;

public interface IStickProjector
{
   
    Vector3 Project(Vector3 current, Vector3 targetOnTrack, float stick01, bool decoupleY, float? overrideY = null);
}
