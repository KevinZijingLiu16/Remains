using UnityEngine;

public interface IAirFlowMovement
{
    void HandleAirFlowMovement(bool isBlowing, bool isReversed, float intensity);
}
