using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ZoneTrigger : MonoBehaviour
{
    private IPowerZoneEffect effect;

    void Awake()
    {
        effect = GetComponent<IPowerZoneEffect>();
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        var power = other.GetComponentInParent<PlayerPower>();
        if (power != null) power.EnterZone(effect);
    }

    void OnTriggerExit(Collider other)
    {
        var power = other.GetComponentInParent<PlayerPower>();
        if (power != null) power.ExitZone(effect);
    }
}
