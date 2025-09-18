using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ZoneTrigger : MonoBehaviour
{
    private IPowerZoneEffect effect;
    private HashSet<PlayerPower> playersInZone = new HashSet<PlayerPower>();

    void Awake()
    {
        effect = GetComponent<IPowerZoneEffect>();
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        var power = other.GetComponentInParent<PlayerPower>();
        if (power != null)
        {
            power.EnterZone(effect);
            playersInZone.Add(power);
            Debug.Log($"Player entered {gameObject.name}");
        }
    }

    void OnTriggerExit(Collider other)
    {
        var power = other.GetComponentInParent<PlayerPower>();
        if (power != null)
        {
            power.ExitZone(effect);
            playersInZone.Remove(power);
            Debug.Log($"Player exited {gameObject.name}");
        }
    }

    void OnTriggerStay(Collider other)
    {
        var power = other.GetComponentInParent<PlayerPower>();
        if (power != null && !playersInZone.Contains(power))
        {
            power.EnterZone(effect);
            playersInZone.Add(power);
            Debug.Log($"Player re-entered {gameObject.name} (OnTriggerStay)");
        }
    }
}