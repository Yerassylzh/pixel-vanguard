using System;
using UnityEngine;

namespace PixelVanguard.Data
{
    /// <summary>
    /// Defines a weapon's base stats and identity.
    /// ScriptableObject pattern allows easy editing in Unity Inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "Weapon", menuName = "PixelVanguard/Weapon Data", order = 2)]
    public class WeaponData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique identifier for save system and code references")]
        public string weaponID;
        
        [Tooltip("Name displayed in UI")]
        public string displayName;
        
        [Tooltip("Shown on weapon card and upgrade panel")]
        [TextArea(2, 3)]
        public string description;
        
        public Sprite icon;

        [Header("Weapon Type")]
        [Tooltip("Determines which script/prefab WeaponManager instantiates")]
        public WeaponType type;

        [Header("Base Stats")]
        [Tooltip("Initial damage value")]
        public float baseDamage = 10f;
        
        [Tooltip("Seconds between auto-fire attacks")]
        public float cooldown = 1f;
        
        [Tooltip("Knockback force applied to enemies")]
        public float knockback = 3f;

        [Header("Optional Override")]
        [Tooltip("Leave empty to use type-based prefab from WeaponManager")]
        public GameObject prefab;

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
        Greatsword,      // Periodic swing attack
        MagicOrbitals,   // Continuous orbit shields
        Crossbow,        // Fires arrows
        HolyWater        // Throws puddle flask
    }
}
