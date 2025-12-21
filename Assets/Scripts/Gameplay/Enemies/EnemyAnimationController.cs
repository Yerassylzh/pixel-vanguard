using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Handles enemy sprite animations based on movement direction.
    /// Enemies are always moving (chasing player), so only 2 states needed: WalkLeft, WalkRight.
    /// Uses only FacingRight (Bool) parameter.
    /// Animation is based on DIRECTION TO PLAYER, not velocity (so knockback doesn't flip animation).
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyAnimationController : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float directionUpdateThreshold = 0.1f; // Minimum distance to update direction

        private Animator animator;
        private EnemyAI enemyAI;
        private EnemyHealth enemyHealth;
        private bool isFacingRight = true; // Start facing right

        // Animator parameter hash (for performance)
        private static readonly int FacingRightHash = Animator.StringToHash("FacingRight");

        private void Awake()
        {
            animator = GetComponent<Animator>();
            enemyAI = GetComponent<EnemyAI>();
            enemyHealth = GetComponent<EnemyHealth>();

            if (animator == null)
            {
                Debug.LogError("[EnemyAnimationController] No Animator component found!");
            }

            if (enemyAI == null)
            {
                Debug.LogError("[EnemyAnimationController] No EnemyAI component found!");
            }
        }

        private void Update()
        {
            UpdateAnimation();
        }

        /// <summary>
        /// Updates animator based on direction TO player (not velocity).
        /// This ensures enemy always faces toward player even when being knocked back.
        /// </summary>
        private void UpdateAnimation()
        {
            if (animator == null || enemyAI == null) return;

            // Stop animation if dead
            if (enemyHealth != null && !enemyHealth.IsAlive)
            {
                animator.enabled = false;
                return;
            }

            // Get direction to player from EnemyAI
            Vector2 directionToPlayer = enemyAI.GetDirectionToPlayer();

            // Update facing direction based on target direction (not velocity!)
            if (Mathf.Abs(directionToPlayer.x) > directionUpdateThreshold)
            {
                isFacingRight = directionToPlayer.x > 0;
                animator.SetBool(FacingRightHash, isFacingRight);
            }
            // If direction is near zero, keep last direction
        }
    }
}

