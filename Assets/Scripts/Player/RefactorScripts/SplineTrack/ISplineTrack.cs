using UnityEngine;

public interface ISplineTrack
{
  
    bool Loop { get; set; }

 
    float LengthMeters { get; }

    
    Vector3 EvaluatePosition(float t01);
    Vector3 EvaluateTangent(float t01);

  
    float GetNearestT(Vector3 worldPos);

   
    void RecomputeLength();
}
