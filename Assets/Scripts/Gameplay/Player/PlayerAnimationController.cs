using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Handles player sprite animations based on movement input.
    /// Uses IsMoving (Bool) + FacingRight (Bool) parameters.
    /// Supports 4 states: IdleLeft, IdleRight, WalkLeft, WalkRight.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationController : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float inputThreshold = 0.1f; // Deadzone for stopping animation

        private Animator animator;
        private PlayerController playerController;
        private bool isFacingRight = true; // Start facing right

        // Animator parameter hashes (for performance)
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int FacingRightHash = Animator.StringToHash("FacingRight");

        /// <summary>
        /// Get the last direction the player was facing.
        /// 1 = right, -1 = left
        /// </summary>
        public float LastFacingDirection => isFacingRight ? 1f : -1f;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            playerController = GetComponent<PlayerController>();

            if (animator == null)
            {
                Debug.LogError("[PlayerAnimationController] No Animator component found!");
            }

            if (playerController == null)
            {
                Debug.LogError("[PlayerAnimationController] No PlayerController component found!");
            }
        }

        private void Update()
        {
            UpdateAnimation();
        }

        /// <summary>
        /// Updates animator based on player's movement input.
        /// Uses IsMoving and FacingRight parameters for clean state transitions.
        /// </summary>
        private void UpdateAnimation()
        {
            if (animator == null || playerController == null) return;

            // Get horizontal input from player controller
            float horizontalInput = playerController.MoveDirection.x;
            bool isMoving = Mathf.Abs(horizontalInput) > inputThreshold;

            // Update facing direction when moving
            if (isMoving)
            {
                isFacingRight = horizontalInput > 0;
            }

            // Set animator parameters
            animator.SetBool(IsMovingHash, isMoving);
            animator.SetBool(FacingRightHash, isFacingRight);
        }
    }
}

