using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPower : MonoBehaviour
{
    [SerializeField] private int maxPower = 100;
    [SerializeField] private int initialPower = 50;
    [SerializeField] private float updateInterval = 1f;
    [SerializeField] private bool applyInstantOnZoneChange = true;

    [Header("Low Power Audio")]
    [SerializeField] private int lowPowerThreshold = 20;
    [SerializeField] private string lowPowerSoundName = "LowBattery";
    [SerializeField, Range(0f, 1f)] private float lowPowerSoundVolume = 0.6f;

    [Header("Sound Play Mode")]
    [SerializeField] private LowPowerSoundMode soundMode = LowPowerSoundMode.Continuous;
    [SerializeField] private float intervalPlayDelay = 3f;

    private int currentPower;
    private float timer;
    private readonly HashSet<IPowerZoneEffect> _activeZones = new();
    private IPowerZoneEffect _currentChosenZone;


    private bool _isLowPowerActive = false; 
    private bool _isLowPowerSoundPlaying = false;
    private Coroutine _intervalPlayCoroutine;
    private const string LOW_POWER_SOUND_ID = "low_battery_warning";

    public event Action<int, int> OnPowerChanged; // (current, max)

    public int Current => currentPower;
    public int Max => maxPower;

    void Awake()
    {
        currentPower = Mathf.Clamp(initialPower, 0, maxPower);
        OnPowerChanged?.Invoke(currentPower, maxPower);
        ChooseCurrentZone();

   
        CheckLowPowerSound();
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

    void OnDestroy()
    {
     
        StopLowPowerSound();
    }

    private void ForceCheckCurrentZones()
    {
        var colliders = Physics.OverlapSphere(transform.position, 0.1f);
        var foundZones = new HashSet<IPowerZoneEffect>();
        foreach (var col in colliders)
        {
            if (col.isTrigger)
            {
                var zone = col.GetComponent<IPowerZoneEffect>();
                if (zone != null)
                {
                    foundZones.Add(zone);
                }
            }
        }

        _activeZones.Clear();
        foreach (var zone in foundZones)
        {
            _activeZones.Add(zone);
        }
        ChooseCurrentZone();
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
                best = z;
                bestDeltaAbs = a;
                bestDelta = d;
            }
            else if (a == bestDeltaAbs)
            {
                if (d > bestDelta)
                {
                    best = z;
                    bestDelta = d;
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
        {
            OnPowerChanged?.Invoke(currentPower, maxPower);

        
            CheckLowPowerSound();
        }
    }


    private void CheckLowPowerSound()
    {
        bool shouldBeActive = currentPower <= lowPowerThreshold;

        if (shouldBeActive && !_isLowPowerActive)
        {
         
            _isLowPowerActive = true;
            StartLowPowerSound();

            Debug.Log($"[PlayerPower] Entered low power state (Power: {currentPower}/{maxPower})");
        }
        else if (!shouldBeActive && _isLowPowerActive)
        {
          
            _isLowPowerActive = false;
            StopLowPowerSound();

            Debug.Log($"[PlayerPower] Exited low power state (Power: {currentPower}/{maxPower})");
        }
    }


    private void StartLowPowerSound()
    {
        switch (soundMode)
        {
            case LowPowerSoundMode.Continuous:
                StartContinuousSound();
                break;

            case LowPowerSoundMode.Interval:
                StartIntervalSound();
                break;
        }
    }

    private void StopLowPowerSound()
    {
    
        if (_isLowPowerSoundPlaying)
        {
            SoundManager.Instance?.StopNamedLoop(LOW_POWER_SOUND_ID);
            _isLowPowerSoundPlaying = false;
        }

        if (_intervalPlayCoroutine != null)
        {
            StopCoroutine(_intervalPlayCoroutine);
            _intervalPlayCoroutine = null;
        }
    }

 
    private void StartContinuousSound()
    {
        if (!_isLowPowerSoundPlaying)
        {
            SoundManager.Instance?.PlayNamedLoop(
                LOW_POWER_SOUND_ID,
                lowPowerSoundName,
                lowPowerSoundVolume
            );
            _isLowPowerSoundPlaying = true;

            Debug.Log("[PlayerPower] Started continuous low battery warning sound");
        }
    }


    private void StartIntervalSound()
    {
        if (_intervalPlayCoroutine == null)
        {
            _intervalPlayCoroutine = StartCoroutine(IntervalPlayCoroutine());
            Debug.Log($"[PlayerPower] Started interval low battery warning (every {intervalPlayDelay}s)");
        }
    }

   
    private IEnumerator IntervalPlayCoroutine()
    {
        while (_isLowPowerActive)
        {
           
            SoundManager.Instance?.Play(lowPowerSoundName);

          
            yield return new WaitForSeconds(intervalPlayDelay);
        }

        _intervalPlayCoroutine = null;
    }


    public void SetLowPowerThreshold(int threshold)
    {
        lowPowerThreshold = Mathf.Clamp(threshold, 0, maxPower);
        CheckLowPowerSound();
    }


    public void SetSoundMode(LowPowerSoundMode mode)
    {
        if (soundMode != mode)
        {
            soundMode = mode;

            if (_isLowPowerActive)
            {
                StopLowPowerSound();
                StartLowPowerSound();
            }
        }
    }

    public void SetIntervalDelay(float delay)
    {
        intervalPlayDelay = Mathf.Max(0.5f, delay);

    
        if (_isLowPowerActive && soundMode == LowPowerSoundMode.Interval)
        {
            StopLowPowerSound();
            StartLowPowerSound();
        }
    }


    [ContextMenu("Force Check Low Power Sound")]
    public void ForceCheckLowPowerSound()
    {
        CheckLowPowerSound();
    }

 
    [ContextMenu("Test Interval Play")]
    public void TestIntervalPlay()
    {
        if (Application.isPlaying)
        {
            _isLowPowerActive = true;
            soundMode = LowPowerSoundMode.Interval;
            StartIntervalSound();
        }
    }

   
    [ContextMenu("Test Continuous Play")]
    public void TestContinuousPlay()
    {
        if (Application.isPlaying)
        {
            _isLowPowerActive = true;
            soundMode = LowPowerSoundMode.Continuous;
            StartContinuousSound();
        }
    }
}


public enum LowPowerSoundMode
{
   
    Continuous,

  
    Interval
}