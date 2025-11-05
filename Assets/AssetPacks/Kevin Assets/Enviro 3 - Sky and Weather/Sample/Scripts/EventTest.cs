using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enviro
{
    public class EventTest : MonoBehaviour
    {
        int lastDay;
        // Start is called before the first frame update
        void Start()
        {
            EnviroManager.instance.OnHourPassed += () =>
            {
                Debug.Log("Hour Passed!");
            };

            EnviroManager.instance.OnDayPassed += () =>
            {
                Debug.Log("New Day!"); 
                if(EnviroManager.instance.Time.days != lastDay)
                StartCoroutine(dayminus(EnviroManager.instance.Time.days));
            };

            EnviroManager.instance.OnYearPassed += () =>
            {
                Debug.Log("New Year!");
            };
                   
            EnviroManager.instance.OnDayTime += () =>
            {
                Debug.Log("Day!");
            };

            EnviroManager.instance.OnNightTime += () =>
            {
                Debug.Log("Night!");
            };

            EnviroManager.instance.OnSeasonChanged += (EnviroEnvironment.Seasons s) =>
            {
                Debug.Log("Season changed to: " + s.ToString());
            };

            EnviroManager.instance.OnWeatherChanged += (EnviroWeatherType w) =>
            {
                Debug.Log("Weather changed to: " + w.name);
            };

            EnviroManager.instance.OnZoneWeatherChanged += (EnviroWeatherType w, EnviroZone z) =>
            {
                Debug.Log("Weather changed to: " + w.name.ToString() + " in zone:" + z.name);
            };

            
        } 

            IEnumerator dayminus (int day)
            {
                yield return null;
                lastDay = day;
                EnviroManager.instance.Time.days = day;
            }   
    }
}
