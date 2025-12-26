using UnityEngine;
using PixelVanguard.Interfaces;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Manages player health and damage reception.
    /// Implements IDamageable for event-driven damage feedback.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [Header("Stats")]
        private float maxHealth = 100f;
        private float currentHealth;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;

        [Header("Damage Multiplier")]
        [HideInInspector] public float characterDamageMultiplier = 1f; // For Might upgrade

        [Header("Damage Cooldown")]
        [SerializeField] private float damageCooldown = 1f; // Seconds between taking damage
        private float lastDamageTime = -999f;

        [Header("Invincibility")]
        [SerializeField] private bool isInvincible = false;
        [SerializeField] private float invincibilityDuration = 0f;

        // IDamageable implementation
        public event System.Action<float, Vector3> OnDamaged;
        public event System.Action<float, Vector3> OnHealed;
        public bool IsAlive => currentHealth > 0f;
        
        private float invincibilityTimer = 0f;
        private PlayerController playerController;

        private void Awake()
        {
            // Auto-get required component
            playerController = GetComponent<PlayerController>();
            
            // Initialize safe default to prevent early access issues
            currentHealth = maxHealth; 
        }

        private void Start()
        {
            // Load max health from selected character
            LoadCharacterStats();

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

            // Fire damage event for feedback systems
            OnDamaged?.Invoke(damage, transform.position);

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

            // Fire heal event for feedback systems
            OnHealed?.Invoke(amount, transform.position);

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

        /// <summary>
        /// Increase max health and heal by the same amount.
        /// Used by upgrade system.
        /// </summary>
        public void IncreaseMaxHealth(float amount)
        {
            maxHealth += amount;
            currentHealth += amount; // Also heal by the same amount
            Core.GameEvents.TriggerPlayerHealthChanged(currentHealth, maxHealth);
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

            Debug.Log($"[PlayerHealth] ❤️ Max HP: {MaxHealth}");
            Debug.Log($"[PlayerHealth] ❤️ Current HP: {CurrentHealth}");

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

        /// <summary>
        /// Load stats from selected character.
        /// Called in Start() to initialize character-specific values.
        /// </summary>
        private void LoadCharacterStats()
        {
            var selectedCharacter = Core.CharacterManager.SelectedCharacter;
            if (selectedCharacter != null)
            {
                maxHealth = selectedCharacter.maxHealth;
                characterDamageMultiplier = selectedCharacter.baseDamageMultiplier; // Initialize from character
                Debug.Log($"[PlayerHealth] Loaded character stats: MaxHP={maxHealth}, DamageMulti={characterDamageMultiplier}");
            }
            else
            {
                // Fallback to default if no character selected
                maxHealth = 100f;
                characterDamageMultiplier = 1f;
                Debug.LogWarning("[PlayerHealth] No character selected, using defaults: HP=100, DamageMult=1");
            }
        }
    }
}
