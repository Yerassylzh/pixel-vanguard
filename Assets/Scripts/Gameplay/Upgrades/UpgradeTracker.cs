using System.Collections.Generic;
using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Tracks the state of applied upgrades and equipped weapons.
    /// Separated from UpgradeManager for clean state management.
    /// </summary>
    public class UpgradeTracker
    {
        private HashSet<Data.UpgradeType> appliedUpgrades = new HashSet<Data.UpgradeType>();
        private HashSet<string> equippedWeaponIDs = new HashSet<string>();
        private int passiveSkillCount = 0;
        
        // Passive effect values (integrated into gameplay)
        private float lifestealPercent = 0f;
        private float goldBonusPercent = 0f;

        #region Public API - State Queries
        
        /// <summary>
        /// Check if a specific upgrade type has been applied.
        /// </summary>
        public bool HasUpgrade(Data.UpgradeType type)
        {
            return appliedUpgrades.Contains(type);
        }

        /// <summary>
        /// Check if a specific weapon (by ID) is equipped.
        /// </summary>
        public bool HasWeapon(string weaponID)
        {
            return equippedWeaponIDs.Contains(weaponID);
        }

        /// <summary>
        /// Get current passive skill count (max 3).
        /// </summary>
        public int GetPassiveSkillCount()
        {
            return passiveSkillCount;
        }

        /// <summary>
        /// Get current lifesteal percentage for damage calculation.
        /// </summary>
        public float GetLifestealPercent()
        {
            return lifestealPercent;
        }

        /// <summary>
        /// Get current gold bonus percentage for loot drops.
        /// </summary>
        public float GetGoldBonusPercent()
        {
            return goldBonusPercent;
        }

        #endregion

        #region Public API - State Modifications

        /// <summary>
        /// Track that an upgrade has been applied (one-time upgrades only).
        /// </summary>
        public void TrackUpgrade(Data.UpgradeType type)
        {
            appliedUpgrades.Add(type);
        }

        /// <summary>
        /// Track that a weapon has been equipped.
        /// </summary>
        public void TrackWeapon(string weaponID)
        {
            equippedWeaponIDs.Add(weaponID);
        }

        /// <summary>
        /// Increment passive skill count (used for Lifesteal, Magnet, Lucky Coins).
        /// </summary>
        public void IncrementPassiveCount()
        {
            passiveSkillCount++;
        }

        /// <summary>
        /// Add to lifesteal percentage.
        /// </summary>
        public void AddLifesteal(float amount)
        {
            lifestealPercent += amount;
        }

        /// <summary>
        /// Add to gold bonus percentage.
        /// </summary>
        public void AddGoldBonus(float amount)
        {
            goldBonusPercent += amount;
        }

        #endregion

        #region Debug Info

        /// <summary>
        /// Get debug string showing all tracked state.
        /// </summary>
        public string GetDebugInfo()
        {
            return $"Upgrades: {appliedUpgrades.Count}, Weapons: {equippedWeaponIDs.Count}, " +
                   $"Passives: {passiveSkillCount}/3, Lifesteal: {lifestealPercent * 100:F1}%, " +
                   $"Gold Bonus: {goldBonusPercent * 100:F1}%";
        }

        #endregion
    }
}
