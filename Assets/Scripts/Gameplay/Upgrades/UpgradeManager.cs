using UnityEngine;
using System.Collections.Generic;
using PixelVanguard.Data;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Orchestrates the upgrade system by coordinating validation, selection, and application.
    /// REFACTORED: Split into focused components (Tracker, Validator, Applicator).
    /// </summary>
    public class UpgradeManager : MonoBehaviour
    {
        [Header("Available Upgrades")]
        [SerializeField] private UpgradeData[] allUpgrades;

        [Header("References (Auto-Find)")]
        private PlayerController playerController;
        private PlayerHealth playerHealth;
        private WeaponManager weaponManager;

        // Core Components (Responsibility Separation)
        private UpgradeTracker tracker;
        private UpgradeValidator validator;
        private UpgradeApplicator applicator;

        private void Start()
        {
            // Find game objects
            playerController = FindAnyObjectByType<PlayerController>();
            playerHealth = FindAnyObjectByType<PlayerHealth>();
            weaponManager = FindAnyObjectByType<WeaponManager>();
            
            // Validate critical references
            if (weaponManager == null)
            {
                Debug.LogError("[UpgradeManager] WeaponManager not found! Upgrade system will not function.");
            }
            
            if (allUpgrades == null || allUpgrades.Length == 0)
            {
                Debug.LogError("[UpgradeManager] allUpgrades array is empty! Assign upgrade assets in Inspector.");
            }
            
            // Initialize components
            tracker = new UpgradeTracker();
            validator = new UpgradeValidator(tracker, weaponManager);
            applicator = new UpgradeApplicator(tracker);
            applicator.Initialize(playerController, playerHealth, weaponManager);
            
            // Track starter weapons
            InitializeStarterWeapons();
        }

        private void InitializeStarterWeapons()
        {
            if (weaponManager == null) return;
            
            var equippedWeapons = weaponManager.GetEquippedWeapons();
            if (equippedWeapons == null) return;
            
            foreach (var weaponInstance in equippedWeapons)
            {
                if (weaponInstance?.weaponData != null)
                {
                    tracker.TrackWeapon(weaponInstance.weaponData.weaponID);
                }
            }
        }

        /// <summary>
        /// Get random upgrades for level-up panel with proper filtering and rarity weights.
        /// </summary>
        public UpgradeData[] GetRandomUpgrades(int count = 3)
        {
            
            if (allUpgrades == null || allUpgrades.Length == 0)
            {
                Debug.LogError("[UpgradeManager] allUpgrades array is empty or null!");
                return new UpgradeData[0];
            }

            // Build valid upgrades list using validator
            var validUpgrades = new List<UpgradeData>();
            int filteredCount = 0;
            
            foreach (var upgrade in allUpgrades)
            {
                if (upgrade == null)
                {
                    Debug.LogWarning("[UpgradeManager] Null upgrade in array, skipping");
                    continue;
                }
                
                if (validator.IsUpgradeValid(upgrade))
                {
                    validUpgrades.Add(upgrade);
                }
                else
                {
                    filteredCount++;
                }
            }
            
            if (validUpgrades.Count == 0)
            {
                Debug.LogError("[UpgradeManager] ❌ NO VALID UPGRADES!");
                return new UpgradeData[0];
            }

            // Select random upgrades with weighted probability
            count = Mathf.Min(count, validUpgrades.Count);
            var selectedUpgrades = new List<UpgradeData>();
            
            for (int i = 0; i < count; i++)
            {
                var selected = SelectWeightedRandom(validUpgrades);
                if (selected != null)
                {
                    selectedUpgrades.Add(selected);
                    validUpgrades.Remove(selected); // Prevent duplicates in same selection
                }
            }
            
            return selectedUpgrades.ToArray();
        }

        /// <summary>
        /// Apply selected upgrade to player/weapons.
        /// </summary>
        public void ApplyUpgrade(UpgradeData upgrade)
        {
            if (upgrade == null)
            {
                Debug.LogWarning("[UpgradeManager] Attempted to apply null upgrade");
                return;
            }

            // Apply the upgrade through applicator
            applicator.ApplyUpgrade(upgrade);

            // Track the upgrade if it's not repeatable
            bool isRepeatableStat = upgrade.type == UpgradeType.PlayerMoveSpeed ||
                                   upgrade.type == UpgradeType.PlayerMaxHP ||
                                   upgrade.type == UpgradeType.WeaponDamage ||
                                   upgrade.type == UpgradeType.WeaponAttackSpeed;
            
            bool isNewWeapon = upgrade.type == UpgradeType.NewWeapon;
            
            if (!isRepeatableStat && !isNewWeapon)
            {
                tracker.TrackUpgrade(upgrade.type);
            }
        }

        /// <summary>
        /// Select weighted random upgrade from list based on rarity.
        /// </summary>
        private UpgradeData SelectWeightedRandom(List<UpgradeData> upgrades)
        {
            if (upgrades.Count == 0) return null;
            
            // Calculate total weight
            int totalWeight = 0;
            foreach (var upgrade in upgrades)
            {
                totalWeight += upgrade.rarityWeight;
            }
            
            // Pick random value
            int randomValue = Random.Range(0, totalWeight);
            int currentWeight = 0;
            
            // Find upgrade matching random value
            foreach (var upgrade in upgrades)
            {
                currentWeight += upgrade.rarityWeight;
                if (randomValue < currentWeight)
                {
                    return upgrade;
                }
            }
            
            return upgrades[0]; // Fallback
        }

        #region Public Accessors (For Other Systems)

        /// <summary>
        /// Get current lifesteal percentage (for combat damage calculation).
        /// </summary>
        public float GetLifestealPercent()
        {
            return tracker.GetLifestealPercent();
        }

        /// <summary>
        /// Get current gold bonus percentage (for loot drops).
        /// </summary>
        public float GetGoldBonusPercent()
        {
            return tracker.GetGoldBonusPercent();
        }

        #endregion
    }
}
