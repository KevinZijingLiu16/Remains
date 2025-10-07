using UnityEngine;

public interface IZoneEffect 
{
    void OnPlayerEnter(GameObject player);
    void OnPlayerExit(GameObject player);
    void UpdateEffect(GameObject player);
}
