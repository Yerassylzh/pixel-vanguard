using System.Diagnostics;
using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Manages player health and damage reception.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;

        [Header("Damage Cooldown")]
        [SerializeField] private float damageCooldown = 1f; // Seconds between taking damage
        private float lastDamageTime = -999f;

        [Header("Invincibility")]
        [SerializeField] private bool isInvincible = false;
        [SerializeField] private float invincibilityDuration = 0f;

        private float invincibilityTimer = 0f;
        private PlayerController playerController;

        private void Awake()
        {
            // Auto-get required component
            playerController = GetComponent<PlayerController>();
        }

        private void Start()
        {
            currentHealth = maxHealth;
            Core.GameEvents.TriggerPlayerHealthChanged(currentHealth, maxHealth);
        }

        private void Update()
        {
            // Handle invincibility timer
            if (isInvincible && invincibilityDuration > 0f)
            {
                invincibilityTimer -= Time.deltaTime;
                if (invincibilityTimer <= 0f)
                {
                    isInvincible = false;
                }
            }
        }

        /// <summary>
        /// Apply damage to the player.
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (isInvincible) return;

            // Check damage cooldown (prevent taking damage too frequently)
            if (Time.time - lastDamageTime < damageCooldown)
            {
                return;
            }

            lastDamageTime = Time.time;
            currentHealth -= damage;
            currentHealth = Mathf.Max(0f, currentHealth);

            Core.GameEvents.TriggerPlayerHealthChanged(currentHealth, maxHealth);

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        /// <summary>
        /// Heal the player.
        /// </summary>
        public void Heal(float amount)
        {
            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);

            Core.GameEvents.TriggerPlayerHealthChanged(currentHealth, maxHealth);
        }

        /// <summary>
        /// Set max health (for stat upgrades).
        /// </summary>
        public void SetMaxHealth(float newMaxHealth)
        {
            maxHealth = newMaxHealth;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            Core.GameEvents.TriggerPlayerHealthChanged(currentHealth, maxHealth);
        }

        /// <summary>
        /// Grant temporary invincibility (e.g., after revive).
        /// </summary>
        public void GrantInvincibility(float duration)
        {
            isInvincible = true;
            invincibilityDuration = duration;
            invincibilityTimer = duration;
        }

        private void Die()
        {
            UnityEngine.Debug.Log("Player Died");

            Core.GameEvents.TriggerPlayerDeath();

            // Disable player control
            if (playerController != null)
            {
                playerController.enabled = false;
            }

            // NOTE: Actual game over logic handled by GameManager
            // ReviveManager will check if revive is available
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            // Take damage from enemies on contact
            // Note: TakeDamage has cooldown, so won't spam damage every frame
            if (collision.gameObject.CompareTag("Enemy"))
            {
                var enemyHealth = collision.gameObject.GetComponent<EnemyHealth>();
                if (enemyHealth != null && enemyHealth.IsAlive)
                {
                    TakeDamage(enemyHealth.ContactDamage);
                }
            }
        }
    }
}
