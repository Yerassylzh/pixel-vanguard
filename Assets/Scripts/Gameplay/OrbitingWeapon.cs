using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Weapon that orbits around the player and damages enemies on contact.
    /// Classic horde survivor weapon (like Vampire Survivors' Greatsword).
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class OrbitingWeapon : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private Data.WeaponData weaponData;

        [Header("Orbit Settings")]
        [SerializeField] private Transform player;
        [SerializeField] private float orbitRadius = 2f;
        [SerializeField] private float orbitSpeed = 180f; // Degrees per second

        private CircleCollider2D weaponCollider;
        private SpriteRenderer spriteRenderer;
        private float currentAngle = 0f;
        private float damageTimer = 0f;

        // Weapon stats (from WeaponData)
        private float damage;
        private float cooldown;
        private float knockback;

        private void Awake()
        {
            // Auto-get required components
            weaponCollider = GetComponent<CircleCollider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            // Set collider as trigger (no physics collision, just damage detection)
            weaponCollider.isTrigger = true;
        }

        private void Start()
        {
            // Find player if not assigned
            if (player == null)
            {
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    player = playerObj.transform;
                }
                else
                {
                    Debug.LogError("[OrbitingWeapon] Player not found! Assign Player or add 'Player' tag.");
                    enabled = false;
                    return;
                }
            }

            // Load stats from WeaponData
            if (weaponData != null)
            {
                var stats = weaponData.GetStatsForLevel(1); // Start at level 1
                damage = stats.damage;
                cooldown = stats.cooldown;
                knockback = stats.knockback;
            }
            else
            {
                // Fallback values
                damage = 10f;
                cooldown = 0.5f;
                knockback = 5f;
                Debug.LogWarning("[OrbitingWeapon] No WeaponData assigned! Using default values.");
            }

            // Random starting angle for visual variety
            currentAngle = Random.Range(0f, 360f);
        }

        private void Update()
        {
            // Don't orbit if game is paused
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            OrbitAroundPlayer();
            UpdateDamageTimer();
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

        /// <summary>
        /// Upgrade weapon to new level (called by LevelUpManager in the future).
        /// </summary>
        public void UpgradeToLevel(int level)
        {
            if (weaponData == null) return;

            var stats = weaponData.GetStatsForLevel(level);
            damage = stats.damage;
            cooldown = stats.cooldown;
            knockback = stats.knockback;

            Debug.Log($"[OrbitingWeapon] Upgraded to level {level}: Damage={damage}, Cooldown={cooldown}");
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
