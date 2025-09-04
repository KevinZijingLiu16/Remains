public interface IWeaponDataProvider
{
    IWeapon[] GetAvailableWeapons();
    IWeapon GetWeapon(string weaponId);
}