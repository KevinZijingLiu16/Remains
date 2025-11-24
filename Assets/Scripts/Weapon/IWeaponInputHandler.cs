public interface IWeaponInputHandler
{
    event System.Action OnWeaponSelectionRequested;
    event System.Action<float> OnNavigationInput;
    event System.Action OnSelectionConfirmed;

    // New events
    event System.Action OnQuickCyclePressed;
    event System.Action<int> OnHotkeyPressed;

    void Enable();
    void Disable();
}