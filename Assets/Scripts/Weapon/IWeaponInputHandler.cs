public interface IWeaponInputHandler
{
    event System.Action OnWeaponSelectionRequested;
    void Enable();
    void Disable();
}