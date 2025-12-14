using System.Collections.Generic;
using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Manages player's equipped weapons.
    /// Handles weapon equipping, upgrading, and tracking.
    /// Max 4 weapons simultaneously.
    /// </summary>
    public class WeaponManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private int maxWeapons = 4;

        [Header("Available Weapons Pool")]
        [Tooltip("All weapons that can be unlocked")]
        [SerializeField] private Data.WeaponData[] availableWeapons;

        [Header("Weapon Prefabs")]
        [Tooltip("Prefabs for each weapon type implementation")]
        [SerializeField] private GameObject greatswordPrefab;         // GreatswordWeapon.cs
        [SerializeField] private GameObject magicOrbitalsPrefab;      // MagicOrbitalsWeapon.cs
        [SerializeField] private GameObject autoCrossbowPrefab;       // AutoCrossbowWeapon.cs
        [SerializeField] private GameObject holyWaterPrefab;          // HolyWaterWeapon.cs

        // Currently equipped weapons
        private List<WeaponInstance> equippedWeapons = new List<WeaponInstance>();

        private void Start()
        {
            // Equip starter weapon from selected character
            var selectedCharacter = Core.CharacterManager.SelectedCharacter;
            if (selectedCharacter != null && selectedCharacter.starterWeapon != null)
            {
                EquipWeapon(selectedCharacter.starterWeapon);
                Debug.Log($"[WeaponManager] Equipped starter weapon: {selectedCharacter.starterWeapon.displayName}");
            }
            else if (availableWeapons != null && availableWeapons.Length > 0)
            {
                // Fallback: equip first weapon in array (Greatsword)
                EquipWeapon(availableWeapons[0]);
                Debug.LogWarning("[WeaponManager] No character starter weapon, using fallback");
            }
            else
            {
                Debug.LogError("[WeaponManager] No weapons available to equip!");
            }
        }

        /// <summary>
        /// Equip a new weapon.
        /// </summary>
        public bool EquipWeapon(Data.WeaponData weaponData)
        {
            if (weaponData == null)
            {
                Debug.LogWarning("[WeaponManager] Attempted to equip null weapon!");
                return false;
            }

            // Check if already equipped
            if (IsWeaponEquipped(weaponData.weaponID))
            {
                Debug.LogWarning($"[WeaponManager] {weaponData.displayName} already equipped!");
                return false;
            }

            // Check max weapons limit
            if (equippedWeapons.Count >= maxWeapons)
            {
                Debug.LogWarning($"[WeaponManager] Max weapons ({maxWeapons}) already equipped!");
                return false;
            }

            // Instantiate weapon based on type
            GameObject weaponObj = InstantiateWeaponByType(weaponData);
            if (weaponObj == null)
            {
                Debug.LogError($"[WeaponManager] Failed to instantiate weapon: {weaponData.displayName}");
                return false;
            }

            // Get weapon component
            WeaponBase weaponScript = weaponObj.GetComponent<WeaponBase>();
            if (weaponScript == null)
            {
                Debug.LogError($"[WeaponManager] Weapon prefab missing WeaponBase component!");
                Destroy(weaponObj);
                return false;
            }

            // Add to equipped list
            var instance = new WeaponInstance
            {
                weaponData = weaponData,
                weaponObject = weaponObj,
                weaponScript = weaponScript
            };
            equippedWeapons.Add(instance);

            Debug.Log($"[WeaponManager] Equipped: {weaponData.displayName}");
            return true;
        }

        /// <summary>
        /// Check if weapon is already equipped.
        /// </summary>
        public bool IsWeaponEquipped(string weaponID)
        {
            return GetEquippedWeapon(weaponID) != null;
        }

        /// <summary>
        /// Get equipped weapon count.
        /// </summary>
        public int GetEquippedWeaponCount() => equippedWeapons.Count;

        /// <summary>
        /// Get list of all equipped weapons.
        /// Used by UpgradeManager to apply universal upgrades.
        /// </summary>
        public List<WeaponInstance> GetEquippedWeapons()
        {
            return new List<WeaponInstance>(equippedWeapons);
        }

        /// <summary>
        /// Get list of weapons not yet equipped.
        /// Used by LevelUpPanel to show weapon unlock cards.
        /// </summary>
        public List<Data.WeaponData> GetAvailableUnequippedWeapons()
        {
            var unequipped = new List<Data.WeaponData>();

            foreach (var weaponData in availableWeapons)
            {
                bool alreadyEquipped = equippedWeapons.Exists(w => w.weaponData.weaponID == weaponData.weaponID);
                if (!alreadyEquipped)
                {
                    unequipped.Add(weaponData);
                }
            }

            return unequipped;
        }

        private WeaponInstance GetEquippedWeapon(string weaponID)
        {
            return equippedWeapons.Find(w => w.weaponData.weaponID == weaponID);
        }

        private GameObject InstantiateWeaponByType(Data.WeaponData weaponData)
        {
            GameObject prefabToUse = null;

            switch (weaponData.type)
            {
                case Data.WeaponType.Greatsword:
                    prefabToUse = greatswordPrefab;
                    break;
                case Data.WeaponType.MagicOrbitals:
                    prefabToUse = magicOrbitalsPrefab;
                    break;
                case Data.WeaponType.Crossbow:
                    prefabToUse = autoCrossbowPrefab;
                    break;
                case Data.WeaponType.HolyWater:
                    prefabToUse = holyWaterPrefab;
                    break;
            }

            if (prefabToUse == null)
            {
                Debug.LogError($"[WeaponManager] No prefab assigned for weapon type: {weaponData.type}");
                return null;
            }

            // Instantiate as child of this GameObject (player)
            GameObject weaponObj = Instantiate(prefabToUse, transform);
            weaponObj.name = weaponData.displayName;

            return weaponObj;
        }
    }

    /// <summary>
    /// Represents a weapon instance equipped by the player.
    /// </summary>
    [System.Serializable]
    public class WeaponInstance
    {
        public Data.WeaponData weaponData;
        public GameObject weaponObject;
        public WeaponBase weaponScript;
    }
}
