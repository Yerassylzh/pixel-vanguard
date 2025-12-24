using UnityEngine;
using UnityEngine.InputSystem;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Handles player input processing using Unity's New Input System.
    /// Supports keyboard, gamepad, and mobile virtual joystick.
    /// </summary>
    public class PlayerInput : MonoBehaviour
    {
        private PlayerMovement playerMovement;
        private Core.PlatformDetector platformDetector;
        private UI.VirtualJoystick virtualJoystick;
        private PlayerInputActions inputActions;
        private Vector2 currentInput;

        private void Awake()
        {
            playerMovement = GetComponent<PlayerMovement>();
            
            // Initialize New Input System
            inputActions = new PlayerInputActions();
            inputActions.Player.Move.performed += OnMovePerformed;
            inputActions.Player.Move.canceled += OnMoveCanceled;
        }

        private void Start()
        {
            platformDetector = Core.PlatformDetector.Instance;
            
            // Find virtual joystick for mobile
            if (platformDetector != null && platformDetector.IsMobile())
            {
                virtualJoystick = FindFirstObjectByType<UI.VirtualJoystick>();
            }
        }

        private void OnEnable()
        {
            inputActions?.Player.Enable();
        }

        private void OnDisable()
        {
            inputActions?.Player.Disable();
        }

        private void Update()
        {
            // Check game state
            if (GameManager.Instance != null && 
                GameManager.Instance.CurrentState != GameState.Playing)
            {
                playerMovement?.SetMoveInput(Vector2.zero);
                return;
            }

            // Get input based on platform
            Vector2 input = GetPlatformInput();
            playerMovement?.SetMoveInput(input);
        }

        private Vector2 GetPlatformInput()
        {
            // Mobile: Use virtual joystick
            if (platformDetector != null && platformDetector.IsMobile() && virtualJoystick != null)
            {
                return virtualJoystick.Direction;
            }
            
            // Desktop/Gamepad: Use New Input System
            return currentInput;
        }

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            currentInput = context.ReadValue<Vector2>();
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            currentInput = Vector2.zero;
        }

        private void OnDestroy()
        {
            if (inputActions != null)
            {
                inputActions.Player.Move.performed -= OnMovePerformed;
                inputActions.Player.Move.canceled -= OnMoveCanceled;
                inputActions.Dispose();
            }
        }
    }
}
