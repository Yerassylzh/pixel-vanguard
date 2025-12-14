using UnityEngine;
using UnityEngine.InputSystem;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Controls player movement with platform-aware input and proper state management.
    /// Desktop: WASD + Arrow Keys
    /// Mobile: VirtualJoystick
    /// Respects game state (blocks input when paused/level-up/game-over).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        [Header("Movement")]
        private float moveSpeed = 5f;

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
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            // Auto-find VirtualJoystick if not assigned (FIX for mobile input)
            if (virtualJoystick == null)
            {
                virtualJoystick = FindAnyObjectByType<UI.VirtualJoystick>();
            }

            // Load move speed from selected character
            LoadCharacterStats();

            DeterminePlatform();
            SetupInput();
        }

        private void OnEnable()
        {
            // Subscribe to platform changes
            Core.GameEvents.OnPlatformChanged += OnPlatformChanged;

            // Enable desktop input if applicable
            if (moveAction != null && !useMobileControls)
            {
                moveAction.Enable();
            }
        }

        private void OnDisable()
        {
            Core.GameEvents.OnPlatformChanged -= OnPlatformChanged;

            if (moveAction != null && !useMobileControls)
            {
                moveAction.Disable();
            }
        }

        private void DeterminePlatform()
        {
            // Otherwise use platform detector
            if (Core.PlatformDetector.Instance != null)
            {
                useMobileControls = Core.PlatformDetector.Instance.IsMobile();
                Debug.Log($"[PlayerController] Platform: {(useMobileControls ? "Mobile" : "Desktop")}");
            }
            else
            {
                Debug.LogWarning("[PlayerController] PlatformDetector not found! Defaulting to desktop.");
                useMobileControls = false;
            }
        }

        private void SetupInput()
        {
            if (!useMobileControls)
            {
                SetupDesktopInput();
            }
            else
            {
                SetupMobileInput();
            }
        }

        private void SetupDesktopInput()
        {
            if (inputActions == null)
            {
                inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
            }

            if (inputActions != null)
            {
                var playerActionMap = inputActions.FindActionMap("Player");
                if (playerActionMap != null)
                {
                    moveAction = playerActionMap.FindAction("Move");
                }
                else
                {
                    Debug.LogError("[PlayerController] 'Player' action map not found!");
                }
            }
            else
            {
                Debug.LogError("[PlayerController] InputActions asset not found!");
            }
        }

        private void SetupMobileInput()
        {
            virtualJoystick = FindAnyObjectByType<UI.VirtualJoystick>();
        }

        private void OnPlatformChanged(Core.PlatformType newPlatform)
        {
            // Re-determine platform and re-setup input
            DeterminePlatform();
            SetupInput();
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

            // Block input if game not playing
            if (!CanAcceptInput())
            {
                moveDirection = Vector2.zero;
                return;
            }

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
                // Desktop: Read from Input System (supports WASD + Arrow keys)
                moveDirection = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
            }
        }

        private bool CanAcceptInput()
        {
            // Check game state - only accept input when playing
            if (GameManager.Instance == null) return true; // Fallback if no manager

            GameState state = GameManager.Instance.CurrentState;
            return state == GameState.Playing;
        }

        private void Move()
        {
            // Normalize input to prevent diagonal speed boost
            Vector2 normalizedDirection = moveDirection.magnitude > 1f
                ? moveDirection.normalized
                : moveDirection;

            // Apply movement
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

        /// <summary>
        /// Load stats from selected character.
        /// Called in Awake() to initialize character-specific values.
        /// </summary>
        private void LoadCharacterStats()
        {
            var selectedCharacter = Core.CharacterManager.SelectedCharacter;
            if (selectedCharacter != null)
            {
                moveSpeed = selectedCharacter.moveSpeed;
                Debug.Log($"[PlayerController] Loaded character stats: Speed={moveSpeed}");
            }
            else
            {
                // Fallback to default if no character selected
                moveSpeed = 5f;
                Debug.LogWarning("[PlayerController] No character selected, using default speed: 5.0");
            }
        }
    }
}
