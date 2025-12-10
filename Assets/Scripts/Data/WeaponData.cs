using System;
using UnityEngine;

namespace PixelVanguard.Data
{
    /// <summary>
    /// ScriptableObject defining a weapon system.
    /// Assets stored in: Assets/ScriptableObjects/Weapons/
    /// </summary>
    [CreateAssetMenu(fileName = "Weapon", menuName = "PixelVanguard/Weapon Data", order = 2)]
    public class WeaponData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique identifier for this weapon (e.g., 'greatsword')")]
        public string weaponID;
        
        [Tooltip("Display name shown in UI (e.g., 'Orbiting Greatsword')")]
        public string displayName;
        
        [Tooltip("Description for level-up cards")]
        [TextArea(2, 3)]
        public string description;
        
        public Sprite icon;

        [Header("Weapon Type")]
        public WeaponType type;

        [Header("Base Stats (Level 1)")]
        [Tooltip("Base damage per hit")]
        public float baseDamage = 10f;
        
        [Tooltip("Seconds between attacks")]
        public float cooldown = 1f;
        
        [Tooltip("Knockback force applied to enemies")]
        public float knockback = 5f;

        [Header("Upgrade Scaling")]
        [Tooltip("Stat changes per level (Level 2, 3, 4...)")]
        public UpgradeScaling[] upgrades = new UpgradeScaling[3];

        [Header("Prefab")]
        [Tooltip("Visual prefab for projectile/effect")]
        public GameObject prefab;

        /// <summary>
        /// Get stats for a specific level.
        /// </summary>
        public WeaponStats GetStatsForLevel(int level)
        {
            if (level == 1)
            {
                return new WeaponStats
                {
                    damage = baseDamage,
                    cooldown = cooldown,
                    knockback = knockback
                };
            }

            // Find the upgrade for this level
            var stats = new WeaponStats
            {
                damage = baseDamage,
                cooldown = cooldown,
                knockback = knockback
            };

            for (int i = 0; i < upgrades.Length && i < level - 1; i++)
            {
                var upgrade = upgrades[i];
                stats.damage *= upgrade.damageMultiplier;
                stats.cooldown *= upgrade.cooldownReduction;
                stats.knockback *= upgrade.knockbackMultiplier;
            }

            return stats;
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(weaponID))
            {
                Debug.LogError($"[WeaponData] {name} is missing weaponID!", this);
            }

            if (baseDamage <= 0)
            {
                Debug.LogWarning($"[WeaponData] {weaponID} has 0 or negative damage!", this);
            }

            if (cooldown <= 0)
            {
                Debug.LogWarning($"[WeaponData] {weaponID} has 0 or negative cooldown!", this);
            }
        }
    }

    public enum WeaponType
    {
        OrbitingMelee,  // Greatsword, Magic Orbitals
        Projectile,     // Crossbow
        AreaDenial      // Molotov
    }

    [Serializable]
    public class UpgradeScaling
    {
        [Tooltip("Level this upgrade applies to (2, 3, 4...)")]
        public int level = 2;
        
        [Tooltip("Damage multiplier (1.2 = +20% damage)")]
        public float damageMultiplier = 1.2f;
        
        [Tooltip("Cooldown reduction (0.9 = -10% cooldown)")]
        public float cooldownReduction = 0.9f;
        
        [Tooltip("Knockback multiplier (1.1 = +10% knockback)")]
        public float knockbackMultiplier = 1.0f;
        
        [Tooltip("Special upgrade description (e.g., 'Adds 1 more orbital')")]
        public string specialUpgrade;
    }

    /// <summary>
    /// Runtime weapon stats calculated from base + upgrades.
    /// </summary>
    public struct WeaponStats
    {
        public float damage;
        public float cooldown;
        public float knockback;
    }
}
