using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ColliderWaterTriggerSwitcher : MonoBehaviour
{

    [SerializeField] private string waterLayerName = "Water";

    private Collider _collider;
    private int _waterLayer;
    private bool _insideWater = false;

    void Awake()
    {
        _collider = GetComponent<Collider>();
        _waterLayer = LayerMask.NameToLayer(waterLayerName);

    }

 
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == _waterLayer)
            SetTriggerState(true);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == _waterLayer)
            SetTriggerState(true);
    }

   
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == _waterLayer)
            SetTriggerState(true);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == _waterLayer)
            SetTriggerState(true);
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == _waterLayer)
            SetTriggerState(false);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == _waterLayer)
            SetTriggerState(false);
    }

    private void SetTriggerState(bool inWater)
    {
        if (_insideWater == inWater) return;

        _insideWater = inWater;
        _collider.isTrigger = inWater;

      
    }
}
