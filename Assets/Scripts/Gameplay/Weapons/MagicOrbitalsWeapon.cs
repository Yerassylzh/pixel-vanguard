using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Weapon that orbits around the player and damages enemies on contact.
    /// Extends WeaponBase for shared functionality.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class MagicOrbitalsWeapon : WeaponBase
    {
        [Header("Orbit Settings")]
        [SerializeField] private float orbitRadius = 2f;
        [SerializeField] private float orbitSpeed = 180f; // Degrees per second

        private CircleCollider2D weaponCollider;
        private float currentAngle = 0f;
        private float damageTimer = 0f;

        protected override void Awake()
        {
            base.Awake(); // Get player from singleton

            weaponCollider = GetComponent<CircleCollider2D>();
            weaponCollider.isTrigger = true;

            // Random starting angle for visual variety
            currentAngle = Random.Range(0f, 360f);
        }

        protected override void Update()
        {
            base.Update(); // Handles auto-fire (though we override Fire to do nothing)

            // Don't orbit if game is paused
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            OrbitAroundPlayer();
            UpdateDamageTimer();
        }

        /// <summary>
        /// Fire method - not used for orbiting weapons (collision-based damage instead).
        /// </summary>
        protected override void Fire()
        {
            // Orbiting weapons don't "fire" - they damage on collision
            // This method intentionally left empty
        }

        private void OrbitAroundPlayer()
        {
            if (player == null) return;

            // Increment angle based on orbit speed
            currentAngle += orbitSpeed * Time.deltaTime;
            if (currentAngle >= 360f) currentAngle -= 360f;

            // Calculate position using sin/cos
            float rad = currentAngle * Mathf.Deg2Rad;
            float x = player.position.x + Mathf.Cos(rad) * orbitRadius;
            float y = player.position.y + Mathf.Sin(rad) * orbitRadius;

            transform.position = new Vector3(x, y, player.position.z);
        }

        private void UpdateDamageTimer()
        {
            if (damageTimer > 0f)
            {
                damageTimer -= Time.deltaTime;
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            // Only damage if cooldown is ready
            if (damageTimer > 0f) return;

            // Check if hit an enemy
            if (collision.CompareTag("Enemy"))
            {
                var enemyHealth = collision.GetComponent<EnemyHealth>();
                if (enemyHealth != null && enemyHealth.IsAlive)
                {
                    // Calculate knockback direction (away from weapon)
                    Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;

                    // Deal damage
                    enemyHealth.TakeDamage(damage, knockbackDir, knockback);

                    // Start cooldown
                    damageTimer = cooldown;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw orbit radius in editor
            if (player != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(player.position, orbitRadius);
            }
        }
    }
}
