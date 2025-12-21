using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Utility class for dealing damage to enemies.
    /// Centralizes the common pattern of enemy tag check, health component access, and damage application.
    /// </summary>
    public static class EnemyDamageUtility
    {
        /// <summary>
        /// Attempts to damage an enemy if the collision is valid.
        /// </summary>
        /// <param name="collision">The collider that was hit</param>
        /// <param name="damage">Amount of damage to deal</param>
        /// <param name="knockbackDir">Direction of knockback force</param>
        /// <param name="knockback">Knockback force magnitude</param>
        /// <returns>True if damage was successfully applied, false otherwise</returns>
        public static bool TryDamageEnemy(Collider2D collision, float damage, Vector2 knockbackDir, float knockback)
        {
            if (!collision.CompareTag("Enemy")) return false;
            
            if (collision.TryGetComponent<EnemyHealth>(out var enemyHealth) && enemyHealth.IsAlive)
            {
                enemyHealth.TakeDamage(damage, knockbackDir, knockback);
                return true;
            }
            
            return false;
        }
    }
}
