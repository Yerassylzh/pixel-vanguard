using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Manages upgrade selection and application.
    /// Provides random upgrades on level up and applies chosen effects.
    /// </summary>
    public class UpgradeManager : MonoBehaviour
    {
        [Header("Available Upgrades")]
        [SerializeField] private List<Data.UpgradeData> allUpgrades = new List<Data.UpgradeData>();

        [Header("References (Auto-Find)")]
        private PlayerController playerController;
        private PlayerHealth playerHealth;
        private OrbitingWeapon orbitingWeapon;

        private void Start()
        {
            // Auto-find references
            playerController = FindObjectOfType<PlayerController>();
            playerHealth = FindObjectOfType<PlayerHealth>();
            orbitingWeapon = FindObjectOfType<OrbitingWeapon>();
        }

        /// <summary>
        /// Get 3 random upgrades for level up choices.
        /// </summary>
        public List<Data.UpgradeData> GetRandomUpgrades(int count = 3)
        {
            if (allUpgrades.Count == 0)
            {
                Debug.LogWarning("[UpgradeManager] No upgrades available!");
                return new List<Data.UpgradeData>();
            }

            // Shuffle and take first 'count' items
            var shuffled = allUpgrades.OrderBy(x => Random.value).Take(count).ToList();
            return shuffled;
        }

        /// <summary>
        /// Apply the chosen upgrade to the game.
        /// </summary>
        public void ApplyUpgrade(Data.UpgradeData upgrade)
        {
            if (upgrade == null)
            {
                Debug.LogError("[UpgradeManager] Tried to apply null upgrade!");
                return;
            }

            switch (upgrade.type)
            {
                case Data.UpgradeType.PlayerMoveSpeed:
                    ApplyMoveSpeedUpgrade(upgrade.value);
                    break;

                case Data.UpgradeType.PlayerMaxHP:
                    ApplyMaxHPUpgrade(upgrade.value);
                    break;

                case Data.UpgradeType.WeaponOrbitSpeed:
                    ApplyWeaponOrbitSpeedUpgrade(upgrade.value);
                    break;

                case Data.UpgradeType.WeaponDamage:
                    ApplyWeaponDamageUpgrade(upgrade.value);
                    break;

                default:
                    Debug.LogWarning($"[UpgradeManager] Unknown upgrade type: {upgrade.type}");
                    break;
            }

            Debug.Log($"[UpgradeManager] Applied upgrade: {upgrade.upgradeName}");
        }

        private void ApplyMoveSpeedUpgrade(float percentIncrease)
        {
            if (playerController == null)
            {
                Debug.LogWarning("[UpgradeManager] PlayerController not found!");
                return;
            }

            // Access moveSpeed using reflection
            var type = typeof(PlayerController);
            var field = type.GetField("moveSpeed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                float currentSpeed = (float)field.GetValue(playerController);
                float newSpeed = currentSpeed * (1f + percentIncrease / 100f);
                field.SetValue(playerController, newSpeed);
                Debug.Log($"[UpgradeManager] Move speed: {currentSpeed:F1} → {newSpeed:F1}");
            }
        }

        private void ApplyMaxHPUpgrade(float hpIncrease)
        {
            if (playerHealth == null)
            {
                Debug.LogWarning("[UpgradeManager] PlayerHealth not found!");
                return;
            }

            // Increase max HP and heal by the same amount
            playerHealth.IncreaseMaxHealth(hpIncrease);
            Debug.Log($"[UpgradeManager] Max HP increased by {hpIncrease}");
        }

        private void ApplyWeaponOrbitSpeedUpgrade(float speedIncrease)
        {
            if (orbitingWeapon == null)
            {
                Debug.LogWarning("[UpgradeManager] OrbitingWeapon not found!");
                return;
            }

            // Access orbitSpeed using reflection
            var type = typeof(OrbitingWeapon);
            var field = type.GetField("orbitSpeed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                float currentSpeed = (float)field.GetValue(orbitingWeapon);
                float newSpeed = currentSpeed + speedIncrease;
                field.SetValue(orbitingWeapon, newSpeed);
                Debug.Log($"[UpgradeManager] Orbit speed: {currentSpeed:F0}°/s → {newSpeed:F0}°/s");
            }
        }

        private void ApplyWeaponDamageUpgrade(float damageIncrease)
        {
            if (orbitingWeapon == null)
            {
                Debug.LogWarning("[UpgradeManager] OrbitingWeapon not found!");
                return;
            }

            // Call public method to increase level (which increases damage)
            // For now, we'll use reflection to directly increase damage
            var type = typeof(OrbitingWeapon);
            var field = type.GetField("damage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                float currentDamage = (float)field.GetValue(orbitingWeapon);
                float newDamage = currentDamage + damageIncrease;
                field.SetValue(orbitingWeapon, newDamage);
                Debug.Log($"[UpgradeManager] Weapon damage: {currentDamage:F0} → {newDamage:F0}");
            }
        }
    }
}
