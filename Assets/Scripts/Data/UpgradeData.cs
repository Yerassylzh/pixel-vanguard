using UnityEngine;

namespace PixelVanguard.Data
{
    /// <summary>
    /// Defines a single upgrade option that can be offered on level up.
    /// Create instances via: Assets → Create → PixelVanguard → Upgrade Data
    /// </summary>
    [CreateAssetMenu(fileName = "Upgrade", menuName = "PixelVanguard/Upgrade Data", order = 4)]
    public class UpgradeData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Display name shown on upgrade card")]
        public string upgradeName;

        [Tooltip("Description explaining what this upgrade does")]
        [TextArea(2, 3)]
        public string description;

        public Sprite icon; // For future card visuals

        [Header("Upgrade Type")]
        public UpgradeType type;

        [Header("Effect")]
        [Tooltip("Amount to increase (e.g., 20 for +20% speed, 10 for +10 HP)")]
        public float value;

        [Header("Weapon (only for NewWeapon type)")]
        [Tooltip("Weapon to equip when this upgrade is selected")]
        public WeaponData weaponToEquip;
    }

    public enum UpgradeType
    {
        PlayerMoveSpeed,    // Increase player movement speed
        PlayerMaxHP,        // Increase player max health
        WeaponAttackSpeed,  // Increase weapon attack speed (lower cooldown) - affects ALL weapons
        WeaponDamage,       // Increase weapon damage - affects ALL weapons
        NewWeapon           // Acquire a new weapon (max 4 total)
    }
}
