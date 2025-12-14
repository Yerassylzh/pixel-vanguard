using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using PixelVanguard.Data;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Manages upgrade selection and application.
    /// Generates 3 random upgrade cards on level up.
    /// Applies upgrades to ALL equipped weapons via WeaponManager.
    /// </summary>
    public class UpgradeManager : MonoBehaviour
    {
        [Header("Available Upgrades")]
        [SerializeField] private UpgradeData[] allUpgrades;

        [Header("References (Auto-Find)")]
        private PlayerController playerController;
        private PlayerHealth playerHealth;
        private WeaponManager weaponManager;

        private void Start()
        {
            playerController = FindObjectOfType<PlayerController>();
            playerHealth = FindObjectOfType<PlayerHealth>();
            weaponManager = FindObjectOfType<WeaponManager>();
        }

        /// <summary>
        /// Get 3 random unique upgrades for level-up panel.
        /// Filters out weapons that are already equipped.
        /// </summary>
        public UpgradeData[] GetRandomUpgrades(int count = 3)
        {
            if (allUpgrades == null || allUpgrades.Length == 0)
            {
                Debug.LogWarning("[UpgradeManager] No upgrades available!");
                return new UpgradeData[0];
            }

            // Filter out invalid upgrades
            var validUpgrades = new System.Collections.Generic.List<UpgradeData>();
            foreach (var upgrade in allUpgrades)
            {
                if (upgrade == null) continue;

                // Filter weapon upgrades
                if (upgrade.type == UpgradeType.NewWeapon)
                {
                    // Skip if weapon not assigned
                    if (upgrade.weaponToEquip == null) continue;

                    // Skip if weapon already equipped
                    if (weaponManager != null && weaponManager.IsWeaponEquipped(upgrade.weaponToEquip.weaponID))
                    {
                        continue;
                    }

                    // Skip if already have max weapons (4)
                    if (weaponManager != null && weaponManager.GetEquippedWeapons().Count >= 4)
                    {
                        continue;
                    }
                }

                validUpgrades.Add(upgrade);
            }

            // If no valid upgrades, return empty
            if (validUpgrades.Count == 0)
            {
                Debug.LogWarning("[UpgradeManager] No valid upgrades available after filtering!");
                return new UpgradeData[0];
            }

            // Ensure we don't request more upgrades than available
            count = Mathf.Min(count, validUpgrades.Count);

            // Shuffle valid upgrades
            for (int i = 0; i < validUpgrades.Count; i++)
            {
                var temp = validUpgrades[i];
                int randomIndex = Random.Range(i, validUpgrades.Count);
                validUpgrades[i] = validUpgrades[randomIndex];
                validUpgrades[randomIndex] = temp;
            }

            // Return first 'count' upgrades
            var result = new UpgradeData[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = validUpgrades[i];
            }

            return result;
        }

        /// <summary>
        /// Apply selected upgrade to player.
        /// </summary>
        public void ApplyUpgrade(UpgradeData upgrade)
        {
            if (upgrade == null)
            {
                Debug.LogWarning("[UpgradeManager] Null upgrade selected!");
                return;
            }

            switch (upgrade.type)
            {
                case UpgradeType.PlayerMoveSpeed:
                    // Value is percentage (e.g., 15 = +15% speed)
                    float speedMultiplier = 1.0f + (upgrade.value / 100f);
                    ApplySpeedUpgrade(speedMultiplier);
                    break;

                case UpgradeType.PlayerMaxHP:
                    ApplyHealthUpgrade(upgrade.value);
                    break;

                case UpgradeType.WeaponAttackSpeed:
                    // Value is percentage (e.g., 10 = +10% faster)
                    // For attack speed: DECREASE cooldown (inverse multiplier)
                    float attackSpeedMultiplier = 1.0f - (upgrade.value / 100f);
                    ApplyWeaponSpeedUpgrade(attackSpeedMultiplier);
                    break;

                case UpgradeType.WeaponDamage:
                    // Value is percentage (e.g., 20 = +20% damage)
                    float damageMultiplier = 1.0f + (upgrade.value / 100f);
                    ApplyWeaponDamageUpgrade(damageMultiplier);
                    break;

                case UpgradeType.NewWeapon:
                    ApplyNewWeaponUpgrade(upgrade.weaponToEquip);
                    break;

                default:
                    Debug.LogWarning($"[UpgradeManager] Unknown upgrade type: {upgrade.type}");
                    break;
            }

            Debug.Log($"[UpgradeManager] Applied upgrade: {upgrade.upgradeName}");
        }

        private void ApplySpeedUpgrade(float multiplier)
        {
            if (playerController != null)
            {
                // Get current speed via reflection (PlayerController doesn't expose moveSpeed)
                var field = typeof(PlayerController).GetField("moveSpeed", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (field != null)
                {
                    float currentSpeed = (float)field.GetValue(playerController);
                    float newSpeed = currentSpeed * multiplier;
                    playerController.SetMoveSpeed(newSpeed);
                    Debug.Log($"⚡ [PLAYER] SPEED: {currentSpeed:F1} → {newSpeed:F1} (+{(newSpeed - currentSpeed):F1}, +{((multiplier - 1) * 100):F0}%)");
                }
            }
        }

        private void ApplyHealthUpgrade(float additionalHealth)
        {
            if (playerHealth != null)
            {
                int amount = (int)additionalHealth;
                playerHealth.IncreaseMaxHealth(amount);
                Debug.Log($"❤️ [PLAYER] MAX HP increased by +{amount}");
            }
        }

        private void ApplyWeaponSpeedUpgrade(float multiplier)
        {
            ApplyToAllWeapons(w => w.IncreaseAttackSpeed(multiplier));
        }

        private void ApplyWeaponDamageUpgrade(float multiplier)
        {
            ApplyToAllWeapons(w => w.IncreaseDamage(multiplier));
        }

        private void ApplyNewWeaponUpgrade(Data.WeaponData weaponData)
        {
            if (weaponManager == null)
            {
                Debug.LogWarning("[UpgradeManager] WeaponManager not found!");
                return;
            }

            if (weaponData == null)
            {
                Debug.LogWarning("[UpgradeManager] No weapon assigned to NewWeapon upgrade!");
                return;
            }

            // Check if already equipped
            if (weaponManager.IsWeaponEquipped(weaponData.weaponID))
            {
                Debug.LogWarning($"🗡️ [UPGRADE] {weaponData.displayName} already equipped!");
                return;
            }

            // Check max weapons (4)
            var currentWeapons = weaponManager.GetEquippedWeapons();
            if (currentWeapons.Count >= 4)
            {
                Debug.LogWarning($"🗡️ [UPGRADE] Cannot equip {weaponData.displayName} - Max weapons (4) already equipped!");
                return;
            }

            // Equip the weapon
            bool success = weaponManager.EquipWeapon(weaponData);
            if (success)
            {
                Debug.Log($"🗡️ [UPGRADE] NEW WEAPON ACQUIRED: {weaponData.displayName} ({currentWeapons.Count + 1}/4 weapons)");
            }
            else
            {
                Debug.LogWarning($"🗡️ [UPGRADE] Failed to equip {weaponData.displayName}");
            }
        }

        /// <summary>
        /// Helper method to apply an action to all equipped weapons.
        /// Eliminates code duplication in upgrade methods.
        /// </summary>
        private void ApplyToAllWeapons(System.Action<WeaponBase> action)
        {
            if (weaponManager == null)
            {
                Debug.LogWarning("[UpgradeManager] WeaponManager not found!");
                return;
            }

            var equippedWeapons = weaponManager.GetEquippedWeapons();
            foreach (var weapon in equippedWeapons)
            {
                if (weapon.weaponScript != null)
                {
                    action(weapon.weaponScript);
                }
            }
        }
    }
}
