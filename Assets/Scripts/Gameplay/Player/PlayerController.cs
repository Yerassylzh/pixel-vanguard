using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Refactored PlayerController - Now only maintains singleton reference.
    /// Movement, input, and health are separate components.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        // Component references (cached)
        private PlayerMovement movement;
        private PlayerInput input;
        private PlayerHealth health;
        private Rigidbody2D playerRigidbody; // Cached for MoveDirection

        // Public accessors for backwards compatibility
        public PlayerMovement Movement => movement;
        public PlayerHealth Health => health;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // Cache components
            movement = GetComponent<PlayerMovement>();
            input = GetComponent<PlayerInput>();
            health = GetComponent<PlayerHealth>();
            playerRigidbody = GetComponent<Rigidbody2D>();

            // Validate
            if (movement == null) Debug.LogError("[PlayerController] PlayerMovement component missing!");
            if (input == null) Debug.LogError("[PlayerController] PlayerInput component missing!");
            if (health == null) Debug.LogError("[PlayerController] PlayerHealth component missing!");
            if (playerRigidbody == null) Debug.LogError("[PlayerController] Rigidbody2D component missing!");
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Public API: Set move speed (for upgrades).
        /// </summary>
        public void SetMoveSpeed(float speed)
        {
            movement?.SetMoveSpeed(speed);
        }

        /// <summary>
        /// Public API: Get current move speed.
        /// </summary>
        public float GetMoveSpeed()
        {
            return movement != null ? movement.GetMoveSpeed() : 5f;
        }

        /// <summary>
        /// Public API: Get current movement direction (for animations).
        /// </summary>
        public Vector2 MoveDirection
        {
            get
            {
                if (playerRigidbody != null)
                {
                    return playerRigidbody.linearVelocity.normalized;
                }
                return Vector2.zero;
            }
        }
    }
}
