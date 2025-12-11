using UnityEngine;
using UnityEngine.InputSystem;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Controls player movement with platform-aware input.
    /// Desktop: Input Actions (WASD/Gamepad)
    /// Mobile: VirtualJoystick (touch)
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("Input Actions (Desktop)")]
        [SerializeField] private InputActionAsset inputActions;

        [Header("Virtual Joystick (Mobile)")]
        [SerializeField] private UI.VirtualJoystick virtualJoystick;

        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private Vector2 moveDirection;
        private InputAction moveAction;
        private bool useMobileControls = false;

        private void Awake()
        {
            // Auto-get required components
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            // Determine control scheme
            if (Core.PlatformDetector.Instance != null)
            {
                useMobileControls = Core.PlatformDetector.Instance.IsMobile();
                Debug.Log($"[PlayerController] Platform: {(useMobileControls ? "Mobile" : "Desktop")}");
            }

            // Setup desktop input if not mobile
            if (!useMobileControls)
            {
                SetupDesktopInput();
            }
            else
            {
                // Auto-find joystick if not assigned
                if (virtualJoystick == null)
                {
                    virtualJoystick = FindObjectOfType<UI.VirtualJoystick>();
                    if (virtualJoystick == null)
                    {
                        Debug.LogWarning("[PlayerController] VirtualJoystick not found! Create one in Canvas for mobile controls.");
                    }
                    else
                    {
                        Debug.Log("[PlayerController] VirtualJoystick found!");
                    }
                }
            }
        }

        private void SetupDesktopInput()
        {
            // Setup Input Actions
            if (inputActions == null)
            {
                // Try to load from project
                inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
                if (inputActions == null)
                {
                    Debug.LogError("[PlayerController] InputActions asset not found! Assign 'InputSystem_Actions' in Inspector or place in Resources folder.");
                    return;
                }
            }

            // Get the Move action from the Player action map
            var playerActionMap = inputActions.FindActionMap("Player");
            if (playerActionMap != null)
            {
                moveAction = playerActionMap.FindAction("Move");
            }
            else
            {
                Debug.LogError("[PlayerController] 'Player' action map not found in InputActions!");
            }
        }

        private void OnEnable()
        {
            if (moveAction != null && !useMobileControls)
            {
                moveAction.Enable();
            }
        }

        private void OnDisable()
        {
            if (moveAction != null && !useMobileControls)
            {
                moveAction.Disable();
            }
        }

        private void Update()
        {
            GetInput();
        }

        private void FixedUpdate()
        {
            Move();
        }

        private void GetInput()
        {
            if (useMobileControls)
            {
                // Mobile: Read from virtual joystick
                if (virtualJoystick != null)
                {
                    moveDirection = virtualJoystick.Direction;
                }
                else
                {
                    moveDirection = Vector2.zero;
                }
            }
            else
            {
                // Desktop: Read from Input System
                if (moveAction != null)
                {
                    moveDirection = moveAction.ReadValue<Vector2>();
                }
                else
                {
                    moveDirection = Vector2.zero;
                }
            }
        }

        private void Move()
        {
            // Normalize input to prevent diagonal speed boost
            // WASD diagonal = (1,1) magnitude 1.414, joystick already normalized
            Vector2 normalizedDirection = moveDirection.magnitude > 1f 
                ? moveDirection.normalized 
                : moveDirection;

            // Apply movement with consistent speed in all directions
            rb.linearVelocity = normalizedDirection * moveSpeed;

            // Flip sprite based on movement direction
            if (spriteRenderer != null && normalizedDirection.x != 0)
            {
                spriteRenderer.flipX = normalizedDirection.x < 0;
            }
        }

        /// <summary>
        /// Set move speed (for stat upgrades).
        /// </summary>
        public void SetMoveSpeed(float speed)
        {
            moveSpeed = speed;
        }
    }
}
