using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Handles the application of upgrades to player, weapons, and passive effects.
    /// Contains all upgrade effect implementations separated by category.
    /// </summary>
    public class UpgradeApplicator
    {
        private readonly UpgradeTracker tracker;
        private PlayerController playerController;
        private PlayerHealth playerHealth;
        private WeaponManager weaponManager;

        public UpgradeApplicator(UpgradeTracker tracker)
        {
            this.tracker = tracker;
        }

        public void Initialize(PlayerController player, PlayerHealth health, WeaponManager weapons)
        {
            this.playerController = player;
            this.playerHealth = health;
            this.weaponManager = weapons;
        }

        /// <summary>
        /// Apply an upgrade's effects to the game.
        /// </summary>
        public void ApplyUpgrade(Data.UpgradeData upgrade)
        {
            if (upgrade == null)
            {
                Debug.LogWarning("[UpgradeApplicator] Attempted to apply null upgrade");
                return;
            }

            // Route to appropriate handler based on upgrade type
            switch (upgrade.type)
            {
                // UNIVERSAL STATS
                case Data.UpgradeType.PlayerMoveSpeed:
                    ApplySpeedUpgrade(upgrade.value / 100f);
                    break;
                    
                case Data.UpgradeType.PlayerMaxHP:
                    ApplyMaxHPUpgrade((int)upgrade.value);
                    break;
                    
                case Data.UpgradeType.WeaponDamage:
                    ApplyGlobalDamageUpgrade(upgrade.value / 100f);
                    break;
                    
                case Data.UpgradeType.WeaponAttackSpeed:
                    ApplyGlobalCooldownUpgrade(upgrade.value / 100f);
                    break;
                    
                case Data.UpgradeType.NewWeapon:
                    ApplyNewWeaponUpgrade(upgrade.weaponToEquip);
                    // Track weapon ID in tracker
                    if (upgrade.weaponToEquip != null)
                    {
                        tracker.TrackWeapon(upgrade.weaponToEquip.weaponID);
                    }
                    break;

                // GREATSWORD
                case Data.UpgradeType.GreatswordMirrorSlash:
                    ApplyMirrorSlash();
                    break;
                    
                case Data.UpgradeType.GreatswordDamageBoost:
                    ApplyWeaponSpecificDamage(Data.WeaponType.Greatsword, 1.5f);
                    break;
                    
                case Data.UpgradeType.GreatswordCooldownBoost:
                    ApplyWeaponSpecificCooldown(Data.WeaponType.Greatsword, 0.7f);
                    break;

                // CROSSBOW
                case Data.UpgradeType.CrossbowDualShot:
                    ApplyCrossbowMultiShot(2);
                    break;
                    
                case Data.UpgradeType.CrossbowTripleShot:
                    ApplyCrossbowMultiShot(3);
                    break;
                    
                case Data.UpgradeType.CrossbowPierce:
                    ApplyCrossbowPierce();
                    break;

                // HOLY WATER
                case Data.UpgradeType.HolyWaterRadius:
                    ApplyHolyWaterRadius(1.4f);
                    break;
                    
                case Data.UpgradeType.HolyWaterScaling:
                    ApplyHolyWaterScaling(0.06f);
                    break;
                    
                case Data.UpgradeType.HolyWaterDuration:
                    ApplyHolyWaterDuration(2.0f);
                    break;
                
                // MAGIC ORBITALS
                case Data.UpgradeType.OrbitalsExpandedOrbit:
                    ApplyOrbitalsRadius(1.4f);
                    break;
                    
                case Data.UpgradeType.OrbitalsOverchargedSpheres:
                    ApplyOrbitalsDamage(1.3f);
                    break;
                
                // PASSIVES
                case Data.UpgradeType.PassiveLifesteal:
                    if (tracker.GetPassiveSkillCount() < 3)
                    {
                        tracker.AddLifesteal(0.03f); // 3% lifesteal
                        tracker.IncrementPassiveCount();
                        Debug.Log($"[UpgradeApplicator] ❤️ Lifesteal: {tracker.GetLifestealPercent() * 100}%");
                    }
                    break;
                    
                case Data.UpgradeType.PassiveMagnet:
                    if (tracker.GetPassiveSkillCount() < 3)
                    {
                        ApplyMagnetRadius(1.5f); // +50% pickup radius
                        tracker.IncrementPassiveCount();
                    }
                    break;
                    
                case Data.UpgradeType.PassiveLuckyCoin:
                    if (tracker.GetPassiveSkillCount() < 3)
                    {
                        tracker.AddGoldBonus(0.4f); // +40% gold
                        tracker.IncrementPassiveCount();
                        Debug.Log($"[UpgradeApplicator] 💰 Gold Bonus: {tracker.GetGoldBonusPercent() * 100}%");
                    }
                    break;

                default:
                    Debug.LogWarning($"[UpgradeApplicator] Unknown upgrade type: {upgrade.type}");
                    break;
            }
            
            Debug.Log($"[UpgradeApplicator] Applied upgrade: {upgrade.upgradeName}");
        }

        #region Universal Upgrades

        private void ApplySpeedUpgrade(float multiplier)
        {
            if (playerController != null)
            {
                float currentSpeed = playerController.GetMoveSpeed();
                float newSpeed = currentSpeed * (1f + multiplier);
                playerController.SetMoveSpeed(newSpeed);
                Debug.Log($"[UpgradeApplicator] 🏃 Speed: {currentSpeed:F1} → {newSpeed:F1}");
            }
        }

        private void ApplyMaxHPUpgrade(int amount)
        {
            if (playerHealth != null)
            {
                float currentMax = playerHealth.MaxHealth;
                float newMax = currentMax + amount;
                playerHealth.IncreaseMaxHealth(amount);
                Debug.Log($"[UpgradeApplicator] ❤️ Max HP: {currentMax} → {newMax}");
            }
        }

        private void ApplyGlobalDamageUpgrade(float multiplier)
        {
            if (weaponManager == null) return;
            
            foreach (var weapon in weaponManager.GetEquippedWeapons())
            {
                weapon?.weaponScript?.IncreaseDamage(1f + multiplier);
            }
            Debug.Log($"[UpgradeApplicator] ⚔️ All weapons damage increased by{multiplier * 100}%");
        }

        private void ApplyGlobalCooldownUpgrade(float multiplier)
        {
            if (weaponManager == null) return;
            
            foreach (var weapon in weaponManager.GetEquippedWeapons())
            {
                weapon?.weaponScript?.IncreaseAttackSpeed(1f - multiplier);
            }
            Debug.Log($"[UpgradeApplicator] ⚡ All weapons cooldown reduced by {multiplier * 100}%");
        }

        private void ApplyNewWeaponUpgrade(Data.WeaponData weaponData)
        {
            if (weaponManager == null)
            {
                Debug.LogWarning("[UpgradeApplicator] WeaponManager not found, cannot equip weapon");
                return;
            }
            
            if (weaponData == null)
            {
                Debug.LogWarning("[UpgradeApplicator] Weapon data is null");
                return;
            }
            
            weaponManager.EquipWeapon(weaponData);
            Debug.Log($"[UpgradeApplicator] 🗡️ Equipped: {weaponData.displayName}");
        }

        #endregion

        #region Weapon-Specific Helpers

        private void ApplyWeaponSpecificDamage(Data.WeaponType weaponType, float multiplier)
        {
            if (weaponManager == null) return;
            
            foreach (var weaponInstance in weaponManager.GetEquippedWeapons())
            {
                if (weaponInstance?.weaponData?.type == weaponType)
                {
                    weaponInstance.weaponScript?.IncreaseDamage(multiplier);
                }
            }
        }
        
        private void ApplyWeaponSpecificCooldown(Data.WeaponType weaponType, float multiplier)
        {
            if (weaponManager == null) return;
            
            foreach (var weaponInstance in weaponManager.GetEquippedWeapons())
            {
                if (weaponInstance?.weaponData?.type == weaponType)
                {
                    weaponInstance.weaponScript?.IncreaseAttackSpeed(multiplier);
                }
            }
        }

        #endregion

        #region Greatsword Upgrades

        private void ApplyMirrorSlash()
        {
            weaponManager?.SpawnMirrorGreatsword();
        }

        #endregion

        #region Crossbow Upgrades

        private void ApplyCrossbowMultiShot(int arrowCount)
        {
            ExecuteOnCrossbow(crossbow => crossbow.SetMultiShot(arrowCount));
        }

        private void ApplyCrossbowPierce()
        {
            ExecuteOnCrossbow(crossbow => crossbow.IncrementPierce());
        }

        private void ExecuteOnCrossbow(System.Action<AutoCrossbowWeapon> action)
        {
            if (weaponManager == null) return;
            
            foreach (var weapon in weaponManager.GetEquippedWeapons())
            {
                if (weapon?.weaponData?.type == Data.WeaponType.Crossbow)
                {
                    var crossbow = weapon.weaponScript as AutoCrossbowWeapon;
                    if (crossbow != null)
                    {
                        action(crossbow);
                    }
                }
            }
        }

        #endregion

        #region Holy Water Upgrades

        private void ApplyHolyWaterRadius(float multiplier)
        {
            ExecuteOnHolyWater(holyWater => holyWater.MultiplyPuddleRadius(multiplier));
        }

        private void ApplyHolyWaterScaling(float percent)
        {
            ExecuteOnHolyWater(holyWater => holyWater.SetHPScaling(percent));
        }

        private void ApplyHolyWaterDuration(float additionalSeconds)
        {
            // Convert additionalSeconds (2.0) to percentage (1.0 = 100% increase)
            float percentage = (additionalSeconds - 1f);
            ExecuteOnHolyWater(holyWater => holyWater.UpgradeDuration(percentage));
        }

        private void ExecuteOnHolyWater(System.Action<HolyWaterWeapon> action)
        {
            if (weaponManager == null) return;
            
            foreach (var weapon in weaponManager.GetEquippedWeapons())
            {
                if (weapon?.weaponData?.type == Data.WeaponType.HolyWater)
                {
                    var holyWater = weapon.weaponScript as HolyWaterWeapon;
                    if (holyWater != null)
                    {
                        action(holyWater);
                    }
                }
            }
        }

        #endregion

        #region Magic Orbitals Upgrades

        private void ApplyOrbitalsRadius(float multiplier)
        {
            ExecuteOnOrbitals(orbitals => orbitals.MultiplyOrbitRadius(multiplier));
        }

        private void ApplyOrbitalsDamage(float multiplier)
        {
            // Just use standard damage increase, no special method needed
            if (weaponManager == null) return;
            
            foreach (var weapon in weaponManager.GetEquippedWeapons())
            {
                if (weapon?.weaponData?.type == Data.WeaponType.MagicOrbitals)
                {
                    weapon.weaponScript?.IncreaseDamage(multiplier);
                }
            }
        }

        private void ExecuteOnOrbitals(System.Action<MagicOrbitalsWeapon> action)
        {
            if (weaponManager == null) return;
            
            foreach (var weapon in weaponManager.GetEquippedWeapons())
            {
                if (weapon?.weaponData?.type == Data.WeaponType.MagicOrbitals)
                {
                    var orbitals = weapon.weaponScript as MagicOrbitalsWeapon;
                    if (orbitals != null)
                    {
                        action(orbitals);
                    }
                }
            }
        }

        #endregion

        #region Passive Upgrades

        private void ApplyMagnetRadius(float multiplier)
        {
            var xpGems = Object.FindObjectsByType<XPGem>(FindObjectsSortMode.None);
            foreach (var gem in xpGems)
            {
                gem.magnetRange *= multiplier;
            }
            
            var goldCoins = Object.FindObjectsByType<GoldCoin>(FindObjectsSortMode.None);
            foreach (var coin in goldCoins)
            {
                coin.magnetRange *= multiplier;
            }
            
            Debug.Log($"[UpgradeApplicator] 🧲 Magnet range increased by {(multiplier - 1f) * 100}%");
        }

        #endregion

        #region Public Accessors for Passive Effects

        /// <summary>
        /// Get current lifesteal percentage (for combat systems).
        /// </summary>
        public float GetLifesteal()
        {
            return tracker.GetLifestealPercent();
        }

        /// <summary>
        /// Get current gold bonus percentage (for loot systems).
        /// </summary>
        public float GetGoldBonus()
        {
            return tracker.GetGoldBonusPercent();
        }

        #endregion
    }
}
