public interface IWeaponEquipmentManager
{
    IWeapon CurrentWeapon { get; }
    event System.Action<IWeapon> OnWeaponEquipped;
    event System.Action<IWeapon> OnWeaponUnequipped;
    void EquipWeapon(string weaponId);
    void UnequipCurrentWeapon();
}