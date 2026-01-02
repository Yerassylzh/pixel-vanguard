using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Validates whether upgrades can be selected based on current game state.
    /// Handles all filtering logic for upgrade availability.
    /// </summary>
    public class UpgradeValidator
    {
        private readonly UpgradeTracker tracker;
        private readonly WeaponManager weaponManager;

        public UpgradeValidator(UpgradeTracker tracker, WeaponManager weaponManager)
        {
            this.tracker = tracker;
            this.weaponManager = weaponManager;
        }

        /// <summary>
        /// Check if an upgrade is valid for selection.
        /// </summary>
        public bool IsUpgradeValid(Data.UpgradeData upgrade)
        {
            if (upgrade == null)
            {
                Debug.LogWarning("[UpgradeValidator] Null upgrade passed to validation");
                return false;
            }

            // Core stat upgrades are REPEATABLE - never filter them
            if (IsRepeatableStat(upgrade.type))
            {
                return true; // Always valid
            }
            
            // Check if upgrade already applied (non-repeatable)
            if (tracker.HasUpgrade(upgrade.type))
            {
                return false;
            }
            
            // Type-specific validation
            switch (upgrade.type)
            {
                case Data.UpgradeType.NewWeapon:
                    return ValidateNewWeapon(upgrade);
                
                // Greatsword upgrades
                case Data.UpgradeType.GreatswordMirrorSlash:
                case Data.UpgradeType.GreatswordDamageBoost:
                case Data.UpgradeType.GreatswordCooldownBoost:
                    return ValidateWeaponEquipped(Data.WeaponType.Greatsword, upgrade.upgradeName);
                
                // Crossbow upgrades
                case Data.UpgradeType.CrossbowDualShot:
                case Data.UpgradeType.CrossbowPierce:
                    return ValidateWeaponEquipped(Data.WeaponType.Crossbow, upgrade.upgradeName);
                
                case Data.UpgradeType.CrossbowTripleShot:
                    return ValidateCrossbowTripleShot(upgrade);
                
                // Holy Water upgrades
                case Data.UpgradeType.HolyWaterRadius:
                case Data.UpgradeType.HolyWaterScaling:
                case Data.UpgradeType.HolyWaterDuration:
                    return ValidateWeaponEquipped(Data.WeaponType.HolyWater, upgrade.upgradeName);
                
                // Magic Orbitals upgrades
                case Data.UpgradeType.OrbitalsExpandedOrbit:
                case Data.UpgradeType.OrbitalsOverchargedSpheres:
                    return ValidateWeaponEquipped(Data.WeaponType.MagicOrbitals, upgrade.upgradeName);
                
                // Passives
                case Data.UpgradeType.PassiveLifesteal:
                case Data.UpgradeType.PassiveMagnet:
                case Data.UpgradeType.PassiveLuckyCoin:
                    return ValidatePassive(upgrade.upgradeName);
            }
            
            return true; // Default: upgrade is valid
        }

        #region Helper Methods

        private bool IsRepeatableStat(Data.UpgradeType type)
        {
            return type == Data.UpgradeType.PlayerMoveSpeed ||
                   type == Data.UpgradeType.PlayerMaxHP ||
                   type == Data.UpgradeType.WeaponDamage ||
                   type == Data.UpgradeType.WeaponAttackSpeed;
        }

        private bool ValidateNewWeapon(Data.UpgradeData upgrade)
        {
            if (upgrade.weaponToEquip == null)
            {
                return false;
            }
            
            if (tracker.HasWeapon(upgrade.weaponToEquip.weaponID))
            {
                return false;
            }
            
            return true;
        }

        private bool ValidateWeaponEquipped(Data.WeaponType weaponType, string upgradeName)
        {
            if (!IsWeaponEquipped(weaponType))
            {
                return false;
            }
            return true;
        }

        private bool ValidateCrossbowTripleShot(Data.UpgradeData upgrade)
        {
            if (!IsWeaponEquipped(Data.WeaponType.Crossbow))
            {
                return false;
            }
            
            // PREREQUISITE: Must have Dual Crossbows first
            if (!tracker.HasUpgrade(Data.UpgradeType.CrossbowDualShot))
            {
                return false;
            }
            
            return true;
        }

        private bool ValidatePassive(string upgradeName)
        {
            if (tracker.GetPassiveSkillCount() >= 3)
            {
                return false;
            }
            return true;
        }

        private bool IsWeaponEquipped(Data.WeaponType weaponType)
        {
            if (weaponManager == null) return false;
            
            var equippedWeapons = weaponManager.GetEquippedWeapons();
            if (equippedWeapons == null) return false;
            
            foreach (var weaponInstance in equippedWeapons)
            {
                if (weaponInstance?.weaponData?.type == weaponType)
                {
                    return true;
                }
            }
            
            return false;
        }

        #endregion
    }
}
