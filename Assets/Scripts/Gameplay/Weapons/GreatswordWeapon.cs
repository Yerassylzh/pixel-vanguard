using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Greatsword weapon with periodic swing attack.
    /// Rests at player's side, then performs fast 360° swing every few seconds.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class GreatswordWeapon : WeaponBase
    {
        [Header("Swing Settings")]
        [SerializeField] private float swingRadius = 2f;
        [SerializeField] private float swingDuration = 0.3f;

        private CircleCollider2D weaponCollider;
        private float currentAngle = 0f;
        private float damageTimer = 0f;
        
        // Swing state
        private bool isSwinging = false;
        private float swingTimer = 0f;

        protected override void Awake()
        {
            base.Awake(); // Get player from singleton

            weaponCollider = GetComponent<CircleCollider2D>();
            weaponCollider.isTrigger = true;
            weaponCollider.radius = swingRadius;
            
            // CRITICAL FIX: Unparent from player to use world space positioning
            // Weapons follow player via code (PerformSwing/RestPosition), not hierarchy
            if (transform.parent != null)
            {
                Debug.Log($"[GreatswordWeapon] Unparenting from {transform.parent.name} to use world space");
                transform.SetParent(null);
            }
        }

        protected override void Update()
        {
            // CRITICAL: Call base to handle auto-fire cooldown timer
            base.Update();

            // Don't update if game is paused
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            if (isSwinging)
            {
                PerformSwing();
            }
            else
            {
                // Rest at player's side
                RestPosition();
            }

            UpdateDamageTimer();
        }

        /// <summary>
        /// Fire method - triggers swing attack.
        /// </summary>
        protected override void Fire()
        {
            if (!isSwinging)
            {
                isSwinging = true;
                swingTimer = 0f;
                currentAngle = 0f;
            }
        }

        private void PerformSwing()
        {
            if (player == null) return;

            swingTimer += Time.deltaTime;

            // Calculate swing progress (0 to 1)
            float progress = swingTimer / swingDuration;

            if (progress >= 1f)
            {
                // Swing complete
                isSwinging = false;
                return;
            }

            // Full 360° rotation during swing
            currentAngle = progress * 360f;

            // Calculate position
            float rad = currentAngle * Mathf.Deg2Rad;
            float x = player.position.x + Mathf.Cos(rad) * swingRadius;
            float y = player.position.y + Mathf.Sin(rad) * swingRadius;

            transform.position = new Vector3(x, y, player.position.z);
            
            // Rotate sprite to face direction of movement
            transform.rotation = Quaternion.Euler(0, 0, currentAngle - 90f);
        }

        private void RestPosition()
        {
            if (player == null) return;

            // Position at player's right side
            transform.position = new Vector3(
                player.position.x + 0.8f,
                player.position.y,
                player.position.z
            );
            
            // Face right
            transform.rotation = Quaternion.identity;
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
            // Only damage during swing
            if (!isSwinging) return;
            
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
            // Draw swing radius in editor
            if (player != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(player.position, swingRadius);
            }
        }

        // Note: IncreaseDamage() and IncreaseAttackSpeed() inherited from WeaponBase
    }
}
