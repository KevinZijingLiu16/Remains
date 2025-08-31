using UnityEngine;

public class SunZone : MonoBehaviour, IPowerZoneEffect
{
    [SerializeField] private int powerPerTick = 1;

    public int GetPowerDelta() => powerPerTick;
}
