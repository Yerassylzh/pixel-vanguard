using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Simple AI that makes enemies chase the player.
    /// Uses basic vector math - no pathfinding needed for top-down horde game.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyAI : MonoBehaviour
    {
        private Transform player;

        private Rigidbody2D rb;
        private EnemyHealth enemyHealth;
        private Data.EnemyData enemyData;
        private float moveSpeed;

        private void Awake()
        {
            // Auto-get required components
            rb = GetComponent<Rigidbody2D>();
            enemyHealth = GetComponent<EnemyHealth>();
        }

        private void Start()
        {
            // Find player in scene
            if (player == null)
            {
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    player = playerObj.transform;
                }
                else
                {
                    Debug.LogError("[EnemyAI] Player not found! Make sure Player GameObject has 'Player' tag.");
                }
            }

            // Get move speed from EnemyData (if enemy was initialized)
            // Note: EnemySpawner will call enemyHealth.Initialize(enemyData) which sets the data
            // For manually placed enemies, you can assign EnemyData in Inspector
            UpdateMoveSpeed();
        }

        private void FixedUpdate()
        {
            // Only move if alive and player exists
            if (!enemyHealth.IsAlive || player == null)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            // Don't move if game is paused
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            ChasePlayer();
        }

        private void ChasePlayer()
        {
            // Calculate direction to player
            Vector2 direction = (player.position - transform.position).normalized;

            // Move toward player
            rb.linearVelocity = direction * moveSpeed;
        }

        /// <summary>
        /// Update move speed from EnemyData. Called when enemy is initialized.
        /// </summary>
        public void UpdateMoveSpeed()
        {
            if (enemyHealth != null && enemyHealth.EnemyData != null)
            {
                moveSpeed = enemyHealth.EnemyData.moveSpeed;
            }
            else
            {
                // Fallback speed if no data found
                moveSpeed = 3f;
            }
        }

        /// <summary>
        /// Set move speed directly (for enemy spawner to call after Initialize).
        /// </summary>
        public void SetMoveSpeed(float speed)
        {
            moveSpeed = speed;
        }

        /// <summary>
        /// Get the direction vector pointing toward the player.
        /// Used by EnemyAnimationController to determine facing direction.
        /// </summary>
        public Vector2 GetDirectionToPlayer()
        {
            if (player == null) return Vector2.right; // Default to right if no player

            return (player.position - transform.position).normalized;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw a line to player in editor for debugging
            if (player != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, player.position);
            }
        }
    }
}
