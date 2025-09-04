using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponInputHandler : MonoBehaviour, IWeaponInputHandler
{
    [Header("Input Settings")]
    public InputActionProperty weaponSelectionAction;

    public event System.Action OnWeaponSelectionRequested;

    void OnEnable()
    {
        Enable();
    }

    void OnDisable()
    {
        Disable();
    }

    public void Enable()
    {
        weaponSelectionAction.action?.Enable();
        if (weaponSelectionAction.action != null)
        {
            weaponSelectionAction.action.performed += OnWeaponSelectionInput;
        }
    }

    public void Disable()
    {
        if (weaponSelectionAction.action != null)
        {
            weaponSelectionAction.action.performed -= OnWeaponSelectionInput;
        }
        weaponSelectionAction.action?.Disable();
    }

    private void OnWeaponSelectionInput(InputAction.CallbackContext context)
    {
        OnWeaponSelectionRequested?.Invoke();
    }
}