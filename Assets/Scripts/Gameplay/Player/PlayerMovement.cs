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
        /// Load movement speed from selected character and apply Greaves upgrade.
        /// </summary>
        private void LoadCharacterStats()
        {
            var selectedCharacter = Core.CharacterManager.SelectedCharacter;
            if (selectedCharacter == null)
            {
                moveSpeed = 5f; // Default
                Debug.LogWarning("[PlayerMovement] No character selected, using default speed: 5");
                return;
            }

            float baseSpeed = selectedCharacter.moveSpeed;

            // Load save data to get Greaves upgrade
            var saveService = Core.ServiceLocator.Get<Services.ISaveService>();
            if (saveService != null)
            {
                var cachedSave = Core.ServiceLocator.Get<Services.CachedSaveDataService>();

                // Apply Greaves upgrade (+5% speed per level)
                int greavesLevel = cachedSave.Data.GetStatLevel("greaves");
                float speedBonus = greavesLevel * 0.05f;
                moveSpeed = baseSpeed * (1f + speedBonus);
            }
            else
            {
                moveSpeed = baseSpeed;
                Debug.LogWarning("[PlayerMovement] SaveService not found, using base speed");
            }
        }
    }
}
