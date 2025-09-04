using UnityEngine;

public interface IWeaponAttackBehavior
{
    void StartAttack(Transform weaponTransform, PlayerPower playerPower);
    void UpdateAttack(Transform weaponTransform, PlayerPower playerPower);
    void StopAttack(Transform weaponTransform, PlayerPower playerPower);
    bool CanAttack(PlayerPower playerPower);
    int GetPowerCostPerSecond();
}