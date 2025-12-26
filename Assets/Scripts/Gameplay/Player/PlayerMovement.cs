using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Handles player movement logic.
    /// Separated from PlayerController for single responsibility.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        private float moveSpeed = 5f;
        private Rigidbody2D rb;
        private Vector2 moveInput;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
        }

        private void Start()
        {
            LoadCharacterStats();
        }

        /// <summary>
        /// Set movement input (called by PlayerInput).
        /// </summary>
        public void SetMoveInput(Vector2 input)
        {
            moveInput = input.normalized;
        }

        /// <summary>
        /// Set move speed (called by UpgradeManager).
        /// </summary>
        public void SetMoveSpeed(float speed)
        {
            moveSpeed = speed;
            Debug.Log($"[PlayerMovement] Speed set to {moveSpeed}");
        }

        public float GetMoveSpeed() => moveSpeed;
        
        /// <summary>
        /// Get current move input (for animations - not affected by knockback).
        /// </summary>
        public Vector2 MoveInput => moveInput;

        private void FixedUpdate()
        {
            // Apply movement
            rb.linearVelocity = moveInput * moveSpeed;
        }

        /// <summary>
        /// Load movement speed from selected character.
        /// </summary>
        private void LoadCharacterStats()
        {
            var selectedCharacter = Core.CharacterManager.SelectedCharacter;
            if (selectedCharacter != null)
            {
                moveSpeed = selectedCharacter.moveSpeed;
                Debug.Log($"[PlayerMovement] Loaded character speed: {moveSpeed}");
            }
            else
            {
                moveSpeed = 5f; // Default
                Debug.LogWarning("[PlayerMovement] No character selected, using default speed: 5");
            }
        }
    }
}
