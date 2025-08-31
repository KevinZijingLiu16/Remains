using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPower : MonoBehaviour
{
    [SerializeField] private int maxPower = 100;
    [SerializeField] private int initialPower = 50;
    [SerializeField] private float updateInterval = 1f;
   
    [SerializeField] private bool applyInstantOnZoneChange = true;

    private int currentPower;
    private float timer;

    
    private readonly HashSet<IPowerZoneEffect> _activeZones = new();

  
    private IPowerZoneEffect _currentChosenZone;

    public event Action<int, int> OnPowerChanged; // (current, max)
    public int Current => currentPower;
    public int Max => maxPower;

    void Awake()
    {
        currentPower = Mathf.Clamp(initialPower, 0, maxPower);
        OnPowerChanged?.Invoke(currentPower, maxPower);
        ChooseCurrentZone(); 
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            if (_currentChosenZone != null)
                ModifyPower(_currentChosenZone.GetPowerDelta());
        }
    }

    
    public void EnterZone(IPowerZoneEffect zone)
    {
        if (zone == null) return;
        if (_activeZones.Add(zone))
        {
            bool changed = ChooseCurrentZone();
            if (changed && applyInstantOnZoneChange)
            {
                
                ModifyPower(_currentChosenZone != null ? _currentChosenZone.GetPowerDelta() : 0);
                timer = 0f; 
            }
        }
    }

    public void ExitZone(IPowerZoneEffect zone)
    {
        if (zone == null) return;
        if (_activeZones.Remove(zone))
        {
            bool changed = ChooseCurrentZone();
            if (changed && applyInstantOnZoneChange && _currentChosenZone != null)
            {
              
                ModifyPower(_currentChosenZone.GetPowerDelta());
                timer = 0f;
            }
        }
    }

    
    private bool ChooseCurrentZone()
    {
        IPowerZoneEffect best = null;
        int bestDeltaAbs = int.MinValue;
        int bestDelta = int.MinValue;

        foreach (var z in _activeZones)
        {
            int d = z.GetPowerDelta();
            int a = Mathf.Abs(d);

            if (a > bestDeltaAbs)
            {
                best = z; bestDeltaAbs = a; bestDelta = d;
            }
            else if (a == bestDeltaAbs)
            {
              
                if (d > bestDelta)
                {
                    best = z; bestDelta = d;
                }
            }
        }

        if (!ReferenceEquals(best, _currentChosenZone))
        {
            _currentChosenZone = best;
            return true; 
        }
        return false; 
    }

    public void ModifyPower(int amount)
    {
        if (amount == 0) return;
        int before = currentPower;
        currentPower = Mathf.Clamp(currentPower + amount, 0, maxPower);
        if (currentPower != before)
            OnPowerChanged?.Invoke(currentPower, maxPower);
    }
}
