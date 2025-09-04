using UnityEngine;

[System.Serializable]
public class WeaponDataWithAttack
{
    [Header("Basic Info")]
    public string weaponId;
    public string weaponName;
    public GameObject weaponPrefab;
    public Sprite weaponIcon;

    [Header("Equipment Settings")]
    public Vector3 equipPosition = Vector3.zero;
    public Vector3 equipRotation = Vector3.zero;
    public Vector3 equipScale = Vector3.one;

    [Header("Attack Settings")]
    public WeaponAttackData attackData;
}