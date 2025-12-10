using UnityEngine;
using UnityEngine.InputSystem;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Controls player movement and basic input handling.
    /// Uses Unity's new Input System for cross-platform support (keyboard, gamepad, touch).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("Input Actions")]
        [SerializeField] private InputActionAsset inputActions;

        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private Vector2 moveDirection;
        private InputAction moveAction;

        private void Awake()
        {
            // Auto-get required components
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

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
            if (moveAction != null)
            {
                moveAction.Enable();
            }
        }

        private void OnDisable()
        {
            if (moveAction != null)
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
            if (moveAction != null)
            {
                // Read input from new Input System (works for keyboard, gamepad, touch)
                moveDirection = moveAction.ReadValue<Vector2>();
            }
            else
            {
                // No input if action not configured
                moveDirection = Vector2.zero;
                Debug.LogWarning("[PlayerController] Move action is null! Check InputActions configuration.");
            }
        }

        private void Move()
        {
            // Apply movement
            rb.linearVelocity = moveDirection * moveSpeed;

            // Flip sprite based on movement direction (optional visual polish)
            if (spriteRenderer != null && moveDirection.x != 0)
            {
                spriteRenderer.flipX = moveDirection.x < 0;
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
