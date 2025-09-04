using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class InputSystemMove : MonoBehaviour, IMoveInput
{
    [Header("Input (New Input System)")]
    [SerializeField] private InputActionProperty moveAction;
    [SerializeField] private InputActionProperty jumpAction;

    private float _cachedMove;
    private bool _jumpPressed;

    void OnEnable()
    {
        moveAction.action?.Enable();
        jumpAction.action?.Enable();
    }

    void OnDisable()
    {
        moveAction.action?.Disable();
        jumpAction.action?.Disable();
    }

    void Update()
    {
        _cachedMove = moveAction.action != null ? moveAction.action.ReadValue<float>() : 0f;
        if (jumpAction.action != null && jumpAction.action.WasPressedThisFrame())
            _jumpPressed = true;
    }

    public float MoveAxis => _cachedMove;

    public bool ConsumeJumpPressed()
    {
        if (_jumpPressed)
        {
            _jumpPressed = false;
            return true;
        }
        return false;
    }
}
