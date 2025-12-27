using System.Collections.Generic;
using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Individual orbital ball that orbits around a parent weapon.
    /// Handles collision-based damage independently.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class OrbitalBall : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpriteRenderer orbSprite;
        
        // Set by parent weapon
        private float damage;
        private float knockback;
        private float damageInterval; // Time between damage ticks per enemy
        
        // Track per-enemy damage cooldowns
        private readonly Dictionary<int, float> enemyCooldowns = new();
        
        // Performance optimization: cleanup timer
        private float lastCleanupTime = 0f;
        private const float cleanupInterval = 5f;

        private CircleCollider2D ballCollider;

        private void Awake()
        {
            ballCollider = GetComponent<CircleCollider2D>();
            ballCollider.isTrigger = true;
        }

        /// <summary>
        /// Initialize the orbital ball with damage stats.
        /// Called by MagicOrbitalsWeapon after spawning.
        /// </summary>
        public void Initialize(float dmg, float kb, float interval)
        {
            damage = dmg;
            knockback = kb;
            damageInterval = interval;
        }

        private void Update()
        {
            // Performance: Only clean up cooldown dictionary every 5 seconds
            if (Time.time - lastCleanupTime > cleanupInterval)
            {
                CleanupCooldowns();
                lastCleanupTime = Time.time;
            }
        }
        
        private void CleanupCooldowns()
        {
            // Update all enemy cooldowns and remove expired ones
            var enemyIds = new List<int>(enemyCooldowns.Keys);
            foreach (var enemyId in enemyIds)
            {
                enemyCooldowns[enemyId] -= cleanupInterval;
                if (enemyCooldowns[enemyId] <= 0f)
                {
                    enemyCooldowns.Remove(enemyId);
                }
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (!collision.CompareTag("Enemy")) return;

            int enemyId = collision.gameObject.GetInstanceID();
            
            // Check if this specific enemy is on cooldown
            if (enemyCooldowns.ContainsKey(enemyId)) return;

            // Performance: Use TryGetComponent (faster than GetComponent)
            if (!collision.TryGetComponent<EnemyHealth>(out var enemyHealth)) return;
            if (!enemyHealth.IsAlive) return;

            // Calculate knockback direction (away from ball)
            Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;

            // Deal damage
            enemyHealth.TakeDamage(damage, knockbackDir, knockback);
            
            // Start cooldown for this enemy
            enemyCooldowns[enemyId] = damageInterval;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            // Clean up cooldown tracking when enemy leaves
            if (collision.CompareTag("Enemy"))
            {
                int enemyId = collision.gameObject.GetInstanceID();
                enemyCooldowns.Remove(enemyId);
            }
        }
    }
}
