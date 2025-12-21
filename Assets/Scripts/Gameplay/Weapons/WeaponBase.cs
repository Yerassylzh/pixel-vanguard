using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Abstract base class for all weapons.
    /// Handles common functionality: cooldown, damage calculation, auto-fire.
    /// Each weapon type implements its own Fire() logic.
    /// </summary>
    public abstract class WeaponBase : MonoBehaviour
    {
        [Header("Weapon Data")]
        [SerializeField] protected Data.WeaponData weaponData;

        // Stats loaded from WeaponData
        protected float damage;
        protected float cooldown;
        protected float knockback;
        protected float duration;
        protected float tickRate;

        // Auto-fire timing
        protected float fireCooldownTimer = 0f;

        // Player reference
        protected Transform player;

        protected virtual void Awake()
        {
            // Get player from singleton
            if (PlayerController.Instance != null)
            {
                player = PlayerController.Instance.transform;
            }
            else
            {
                Debug.LogError($"[{GetType().Name}] PlayerController.Instance not found!");
                enabled = false;
            }
        }

        protected virtual void Start()
        {
            LoadWeaponStats(1);
        }

        /// <summary>
        /// Load base stats from WeaponData ScriptableObject.
        /// </summary>
        protected void LoadWeaponStats(int startLevel = 1)
        {
            if (weaponData == null)
            {
                Debug.LogError($"[{GetType().Name}] WeaponData not assigned!");
                return;
            }

            // Load base stats from ScriptableObject
            damage = weaponData.baseDamage;
            cooldown = weaponData.cooldown;
            knockback = weaponData.knockback;
            duration = weaponData.baseDuration;
            tickRate = weaponData.baseTickRate;
            fireCooldownTimer = cooldown;
        }

        /// <summary>
        /// Called by derived classes every frame to handle auto-fire timing.
        /// </summary>
        protected virtual void Update()
        {
            // Don't fire if game is not playing
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            // Auto-fire cooldown logic
            if (fireCooldownTimer > 0f)
            {
                fireCooldownTimer -= Time.deltaTime;
            }
            else
            {
                Fire();
                fireCooldownTimer = cooldown;
            }
        }

        /// <summary>
        /// Abstract method - each weapon implements its own attack logic.
        /// Called automatically when cooldown expires.
        /// </summary>
        protected abstract void Fire();

        /// <summary>
        /// Find the nearest enemy within range using Physics2D overlap.
        /// Optimized to avoid GameObject.FindGameObjectsWithTag.
        /// </summary>
        protected Transform FindNearestEnemy(float maxRange = 15f)
        {
            // Use OverlapCircle to find enemies in range (filtered by Layer)
            // Assuming enemies are on an "Enemy" layer. If not, we can fall back or user needs to set it.
            // For now, let's use a broad check or specific layer if we know it.
            // Using a non-alloc version would be even better for GC, but OverlapCircle is already O(N) faster than FindWithTag.
            
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, maxRange);
            Transform nearest = null;
            float minDistanceSqr = maxRange * maxRange;

            foreach (var hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                {
                    float distSqr = (hit.transform.position - transform.position).sqrMagnitude;
                    if (distSqr < minDistanceSqr)
                    {
                        if (hit.TryGetComponent<EnemyHealth>(out var health) && health.IsAlive)
                        {
                            minDistanceSqr = distSqr;
                            nearest = hit.transform;
                        }
                    }
                }
            }

            return nearest;
        }

        /// <summary>
        /// Public API: Increase weapon damage.
        /// Called by UpgradeManager for weapon damage upgrades.
        /// </summary>
        public virtual void IncreaseDamage(float multiplier)
        {
            damage *= multiplier;
        }

        /// <summary>
        /// Public API: Decrease weapon cooldown (increase fire rate).
        /// Called by UpgradeManager for weapon speed upgrades.
        /// </summary>
        public virtual void IncreaseAttackSpeed(float multiplier)
        {
            cooldown *= multiplier; // e.g., 0.9 = 10% faster (shorter cooldown)
        }

        /// <summary>
        /// Public API: Increase knockback force.
        /// Called by UpgradeManager for knockback upgrades.
        /// </summary>
        public virtual void IncreaseKnockback(float multiplier)
        {
            knockback *= multiplier;
        }

        /// <summary>
        /// Get weapon data reference.
        /// </summary>
        public Data.WeaponData GetWeaponData() => weaponData;

        /// <summary>
        /// Get final damage with character multiplier applied.
        /// Use this method when dealing damage to enemies.
        /// </summary>
        protected float GetFinalDamage()
        {
            var selectedCharacter = Core.CharacterManager.SelectedCharacter;
            float characterMultiplier = selectedCharacter != null ? selectedCharacter.baseDamageMultiplier : 1f;
            return damage * characterMultiplier;
        }
    }
}
