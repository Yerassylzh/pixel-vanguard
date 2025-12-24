using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Arrow projectile behavior.
    /// REFACTORED: Directly calls EnemyHealth.TakeDamage (no wrapper).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class ArrowProjectile : MonoBehaviour
    {
        private Vector2 direction;
        private float speed;
        private float damage;
        private float knockback;
        private int pierceCount;
        private int enemiesHit = 0;

        private Rigidbody2D rb;
        private float lifetime = 5f; // Despawn after 5 seconds
        private float lifetimeTimer = 0f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            
            // Configure rigidbody for projectile physics
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            
            // Configure collider as trigger
            GetComponent<Collider2D>().isTrigger = true;
        }

        /// <summary>
        /// Initialize arrow with stats from weapon.
        /// </summary>
        public void Initialize(Vector2 dir, float spd, float dmg, float kb, int pierce)
        {
            direction = dir.normalized;
            speed = spd;
            damage = dmg;
            knockback = kb;
            pierceCount = pierce;

            // Set velocity
            rb.linearVelocity = direction * speed;
        }

        private void Update()
        {
            // Lifetime check
            lifetimeTimer += Time.deltaTime;
            if (lifetimeTimer >= lifetime)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Ignore non-enemy collisions
            if (!collision.CompareTag("Enemy")) return;

            // Try to damage enemy (REFACTORED: Direct call, no wrapper)
            var enemyHealth = collision.GetComponent<EnemyHealth>();
            if (enemyHealth != null && enemyHealth.IsAlive)
            {
                // Knockback in arrow direction
                Vector2 knockbackDir = direction;

                // Deal damage (using the damage passed in Initialize)
                enemyHealth.TakeDamage(damage, knockbackDir, knockback);

                // Increment hit count
                enemiesHit++;

                // Check if arrow should be destroyed
                if (enemiesHit > pierceCount)
                {
                    Destroy(gameObject);
                }
            }
        }

        private void OnBecameInvisible()
        {
            // Destroy arrow if it goes off-screen
            Destroy(gameObject);
        }
    }
}
