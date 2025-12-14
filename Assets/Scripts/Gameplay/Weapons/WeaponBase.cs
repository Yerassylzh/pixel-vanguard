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

            Debug.Log($"[{GetType().Name}] Loaded: {damage} damage, {cooldown}s cooldown, {knockback} knockback");
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
        /// Find the nearest enemy within range.
        /// </summary>
        protected Transform FindNearestEnemy(float maxRange = 15f)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            Transform nearest = null;
            float minDistance = maxRange;

            foreach (var enemy in enemies)
            {
                float distance = Vector3.Distance(player.position, enemy.transform.position);
                if (distance < minDistance)
                {
                    // Check if enemy is alive
                    var health = enemy.GetComponent<EnemyHealth>();
                    if (health != null && health.IsAlive)
                    {
                        minDistance = distance;
                        nearest = enemy.transform;
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
            float oldDamage = damage;
            damage *= multiplier;
            float increase = damage - oldDamage;
            Debug.Log($"💥 [{weaponData.displayName}] DAMAGE: {oldDamage:F1} → {damage:F1} (+{increase:F1}, +{((multiplier - 1) * 100):F0}%)");
        }

        /// <summary>
        /// Public API: Decrease weapon cooldown (increase fire rate).
        /// Called by UpgradeManager for weapon speed upgrades.
        /// </summary>
        public virtual void IncreaseAttackSpeed(float multiplier)
        {
            float oldCooldown = cooldown;
            cooldown *= multiplier; // e.g., 0.9 = 10% faster (shorter cooldown)
            float reduction = oldCooldown - cooldown;
            Debug.Log($"⚔️ [{weaponData.displayName}] ATTACK SPEED: Cooldown {oldCooldown:F2}s → {cooldown:F2}s (-{reduction:F2}s, {((1 - multiplier) * 100):F0}% faster)");
        }

        /// <summary>
        /// Public API: Increase knockback force.
        /// Called by UpgradeManager for knockback upgrades.
        /// </summary>
        public virtual void IncreaseKnockback(float multiplier)
        {
            float oldKnockback = knockback;
            knockback *= multiplier;
            float increase = knockback - oldKnockback;
            Debug.Log($"💨 [{weaponData.displayName}] KNOCKBACK: {oldKnockback:F1} → {knockback:F1} (+{increase:F1}, +{((multiplier - 1) * 100):F0}%)");
        }

        /// <summary>
        /// Get weapon data reference.
        /// </summary>
        public Data.WeaponData GetWeaponData() => weaponData;
    }
}
