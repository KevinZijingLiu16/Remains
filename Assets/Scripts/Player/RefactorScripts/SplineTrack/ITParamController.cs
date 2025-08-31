public interface ITParamController
{
   
    /// change the speed unit m/s to t (0..1) and returen new t
    float AdvanceT(float currentT, float moveAxis, float dtSeconds,
                   float moveSpeedMetersPerSec, float trackLengthMeters, bool loop);
}
