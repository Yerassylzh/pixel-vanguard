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
            // Load max health from selected character + shop upgrades
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

        private void OnEnable()
        {
            Core.GameEvents.OnPlayerRevived += OnPlayerRevived;
        }

        private void OnDisable()
        {
            Core.GameEvents.OnPlayerRevived -= OnPlayerRevived;
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

            // Fire audio event
            Core.GameEvents.TriggerPlayerDamaged(damage);

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
            Core.GameEvents.TriggerPlayerDeath();

            // Disable player control
            if (playerController != null)
            {
                playerController.enabled = false;
            }

            // NOTE: Actual game over logic handled by GameManager
            // ReviveManager will check if revive is available
        }

        /// <summary>
        /// Revive player with full health.
        /// Called when player watches revive ad.
        /// </summary>
        private void OnPlayerRevived()
        {
            // Restore to full health
            currentHealth = maxHealth;

            // Re-enable player control
            if (playerController != null)
            {
                playerController.enabled = true;
            }

            // Trigger health changed event for UI update
            Core.GameEvents.TriggerPlayerHealthChanged(currentHealth, maxHealth);
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
        /// Load stats from selected character and apply shop upgrades (Vitality, Might).
        /// Called in Start() to initialize character-specific values + shop bonuses.
        /// </summary>
        private void LoadCharacterStats()
        {
            var selectedCharacter = Core.CharacterManager.SelectedCharacter;
            if (selectedCharacter == null)
            {
                Debug.LogWarning("[PlayerHealth] No character selected!");
                maxHealth = 100f; // Fallback
                characterDamageMultiplier = 1f;
                return;
            }

            // Get base stats from character
            float baseHealth = selectedCharacter.maxHealth;
            float baseDamageMultiplier = selectedCharacter.baseDamageMultiplier;

            // Load save data to get shop upgrades
            var saveService = Core.ServiceLocator.Get<Services.ISaveService>();
            if (saveService != null)
            {
                var cachedSave = Core.ServiceLocator.Get<Services.CachedSaveDataService>();

                // Apply Vitality upgrade (+10 HP per level)
                int vitalityLevel = cachedSave.Data.GetStatLevel("vitality");
                int healthBonus = vitalityLevel * 10;
                maxHealth = baseHealth + healthBonus;

                // Apply Might upgrade (+10% damage per level)
                int mightLevel = cachedSave.Data.GetStatLevel("might");
                float damageBonus = mightLevel * 0.10f;
                characterDamageMultiplier = baseDamageMultiplier * (1f + damageBonus);
            }
            else
            {
                // Fallback: use base stats
                maxHealth = baseHealth;
                characterDamageMultiplier = baseDamageMultiplier;
                Debug.LogWarning("[PlayerHealth] SaveService not found, using base stats");
            }
        }
    }
}
