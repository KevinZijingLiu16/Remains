using UnityEngine;

public class ShadowZone : MonoBehaviour, IPowerZoneEffect
{
    [SerializeField] private int powerPerTick = -1;

    public int GetPowerDelta() => powerPerTick;
}
