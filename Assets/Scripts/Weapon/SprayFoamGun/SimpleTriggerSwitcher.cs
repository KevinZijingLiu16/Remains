using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SimpleTriggerSwitcher : MonoBehaviour
{
    private Collider _collider;

    void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
   
        if (other.CompareTag("head"))
        {
            _collider.isTrigger = true;
            Debug.Log($"{name} hit 'head' ¡ú set isTrigger = true");
        }
 
        else if (other.CompareTag("wheel"))
        {
            _collider.isTrigger = false;
            Debug.Log($"{name} hit 'wheel' ¡ú set isTrigger = false");
        }
    }
}
