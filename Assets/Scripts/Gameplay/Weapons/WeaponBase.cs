using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Base class for all weapons.
    /// REFACTORED: Integrated characterDamageMultiplier correctly.
    /// </summary>
    public abstract class WeaponBase : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] protected Data.WeaponData weaponData;

        [Header("Stats (Loaded from WeaponData)")]
        protected float damage;
        protected float cooldown;
        protected float knockback;

        // Cooldown tracking
        protected float cooldownTimer = 0f;

        // Player reference
        protected Transform player;
        
        // Clone tracking - prevents Start() from overwriting copied stats
        private bool isClone = false;
        
        // Weapon Fire Event (for audio system)
        public event System.Action OnWeaponFired;

        protected virtual void Awake()
        {
            // Find player
            if (PlayerController.Instance != null)
            {
                player = PlayerController.Instance.transform;
            }
        }

        protected virtual void Start()
        {
            if (weaponData == null)
            {
                Debug.LogError($"[{GetType().Name}] WeaponData not assigned!");
                return;
            }

            // Skip loading base stats if this is a cloned weapon
            // (stats were already copied from the original)
            if (!isClone)
            {
                LoadStatsFromData();
            }
        }

        protected virtual void Update()
        {
            // Cooldown handling
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
            }

            // Auto-fire when cooldown ready
            if (cooldownTimer <= 0f)
            {
                Fire();
                OnWeaponFired?.Invoke(); // Fire audio event
                cooldownTimer = cooldown;
            }
        }

        /// <summary>
        /// Fire the weapon (implemented by subclasses).
        /// </summary>
        protected abstract void Fire();

        /// <summary>
        /// Load base stats from WeaponData.
        /// </summary>
        private void LoadStatsFromData()
        {
            damage = weaponData.baseDamage;
            cooldown = weaponData.cooldown;
            knockback = weaponData.knockback;
            cooldownTimer = cooldown;
        }

        /// <summary>
        /// Get final damage with all multipliers applied.
        /// FIXED: Now properly integrates character damage multiplier.
        /// </summary>
        protected float GetFinalDamage()
        {
            // Base character multiplier
            var selectedCharacter = Core.CharacterManager.SelectedCharacter;
            float characterMultiplier = selectedCharacter != null ? selectedCharacter.baseDamageMultiplier : 1f;
            
            // Might upgrade multiplier
            var playerHealth = PlayerController.Instance?.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                characterMultiplier *= playerHealth.characterDamageMultiplier;
            }
            
            return damage * characterMultiplier;
        }

        public void ResetCooldown()
        {
            cooldownTimer = 0f;
        }
        
        /// <summary>
        /// Copy current stats from another weapon instance.
        /// Used when spawning clones (e.g., mirror slash) to preserve upgrades.
        /// </summary>
        public void CopyStatsFrom(WeaponBase source)
        {
            if (source == null) return;
            
            // Mark as clone to prevent Start() from overwriting these stats
            isClone = true;
            
            damage = source.damage;
            cooldown = source.cooldown;
            knockback = source.knockback;
            cooldownTimer = source.cooldownTimer;
            
            Debug.Log($"[{GetType().Name}] Copied stats from {source.GetType().Name}: damage={damage:F1}, cooldown={cooldown:F2}s");
        }

        // === PUBLIC UPGRADE API ===
        
        /// <summary>
        /// Public API: Increase damage by multiplier.
        /// Called by UpgradeManager for damage upgrades.
        /// </summary>
        public virtual void IncreaseDamage(float multiplier)
        {
            float oldDamage = damage;
            damage *= multiplier;
            Debug.Log($"⚔️ [{weaponData.displayName}] DAMAGE: {oldDamage:F1} → {damage:F1} (+{(damage - oldDamage):F1}, +{((multiplier - 1) * 100):F0}%)");
        }

        /// <summary>
        /// Public API: Increase attack speed by reducing cooldown.
        /// Called by UpgradeManager for attack speed upgrades.
        /// </summary>
        public virtual void IncreaseAttackSpeed(float multiplier)
        {
            float oldCooldown = cooldown;
            cooldown *= multiplier;
            Debug.Log($"⚡ [{weaponData.displayName}] ATTACK SPEED: {oldCooldown:F2}s → {cooldown:F2}s (-{((1 - multiplier) * 100):F0}% cooldown)");
        }

        /// <summary>
        /// Public API: Increase knockback force.
        /// Called by UpgradeManager for knockback upgrades.
        /// </summary>
        public virtual void IncreaseKnockback(float multiplier)
        {
            float oldKnockback = knockback;
            knockback *= multiplier;
            Debug.Log($"💥 [{weaponData.displayName}] KNOCKBACK: {oldKnockback:F1} → {knockback:F1}");
        }

        /// <summary>
        /// Public API: Upgrade weapon to next level.
        /// Applies percentage-based bonuses to damage and cooldown.
        /// </summary>
        public void UpgradeToLevel(int level, float damageBonus, float cooldownReduction)
        {
            float oldDamage = damage;
            float oldCooldown = cooldown;

            damage += damageBonus;
            cooldown -= cooldownReduction;

            Debug.Log($"🔼 [{weaponData.displayName}] LEVEL {level}: Damage {oldDamage:F1}→{damage:F1}, Cooldown {oldCooldown:F2}s→{cooldown:F2}s");
        }
    }
}
