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
            // Update all enemy cooldowns
            var enemyIds = new List<int>(enemyCooldowns.Keys);
            foreach (var enemyId in enemyIds)
            {
                enemyCooldowns[enemyId] -= Time.deltaTime;
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

            // Calculate knockback direction (away from ball)
            Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;

            // Attempt damage and start per-enemy cooldown if successful
            if (EnemyDamageUtility.TryDamageEnemy(collision, damage, knockbackDir, knockback))
            {
                enemyCooldowns[enemyId] = damageInterval;
            }
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
