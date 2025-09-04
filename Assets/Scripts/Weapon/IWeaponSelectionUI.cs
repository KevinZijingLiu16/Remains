public interface IWeaponSelectionUI
{
    event System.Action<string> OnWeaponSelected;
    void ShowWeaponPanel(IWeapon[] availableWeapons);
    void HideWeaponPanel();
    bool IsVisible { get; }
}