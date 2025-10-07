using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "StateTransitionConfig", menuName = "AI/State Transition Config")]
public class StateTransitionConfig : ScriptableObject
{
    [Header("Transition Settings")]
   
    public string targetStateName = "Running";


    public float affectRadius = 10f;

    [Header("Filter Settings")]

    public List<GameObject> specificTargets = new List<GameObject>();


    public List<string> affectedTags = new List<string> { "Animal" };


    public List<AnimalType> affectedAnimalTypes = new List<AnimalType>();

    [Header("Behavior Settings")]

    public bool canInterruptRunning = false;


    public bool triggerOnce = false;


    public float cooldownTime = 0f;


    public bool IsTargetValid(ITriggerable target)
    {
        GameObject targetGO = (target as MonoBehaviour)?.gameObject;
        if (targetGO == null) return false;

     
        if (specificTargets.Count > 0)
        {
            if (!specificTargets.Contains(targetGO))
                return false;
        }

 
        if (affectedTags.Count > 0)
        {
            bool hasValidTag = false;
            foreach (string tag in affectedTags)
            {
                if (targetGO.CompareTag(tag))
                {
                    hasValidTag = true;
                    break;
                }
            }
            if (!hasValidTag) return false;
        }


        if (affectedAnimalTypes.Count > 0)
        {
            AnimalNPCStateMachine animalSM = target as AnimalNPCStateMachine;
            if (animalSM != null)
            {
                if (!affectedAnimalTypes.Contains(animalSM.AnimalType))
                    return false;
            }
        }

        return true;
    }
}