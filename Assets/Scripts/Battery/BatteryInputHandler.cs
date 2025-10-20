using UnityEngine;
using UnityEngine.InputSystem;

public class BatteryInputHandler : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionProperty pickupAction;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    public event System.Action OnPickupRequested;

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
        pickupAction.action?.Enable();
        if (pickupAction.action != null)
        {
            pickupAction.action.performed += OnPickupInput;
        }

        if (enableDebugLogs)
        {
            Debug.Log("[BatteryInputHandler] Input enabled");
        }
    }

    public void Disable()
    {
        if (pickupAction.action != null)
        {
            pickupAction.action.performed -= OnPickupInput;
        }
        pickupAction.action?.Disable();

        if (enableDebugLogs)
        {
            Debug.Log("[BatteryInputHandler] Input disabled");
        }
    }

    private void OnPickupInput(InputAction.CallbackContext context)
    {
        if (enableDebugLogs)
        {
            Debug.Log("[BatteryInputHandler] Pickup input received (E key)");
        }

        OnPickupRequested?.Invoke();
    }
}