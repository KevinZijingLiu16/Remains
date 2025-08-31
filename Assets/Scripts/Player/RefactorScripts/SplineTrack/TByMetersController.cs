using UnityEngine;

public class TByMetersController : ITParamController
{
    public float AdvanceT(float currentT, float moveAxis, float dt,
                          float speedMps, float lengthMeters, bool loop)
    {
     
        float control = Mathf.Clamp01(1f); 
        float dir = Mathf.Sign(moveAxis);
        float speed = Mathf.Abs(moveAxis) * Mathf.Max(0f, speedMps) * control;

        float dtAsT = (lengthMeters > 0.001f) ? (speed * dt / lengthMeters) : 0f;
        float t = currentT + dir * dtAsT;

        if (!float.IsFinite(t)) t = 0f;
        return loop ? Mathf.Repeat(t, 1f) : Mathf.Clamp01(t);
    }
}
