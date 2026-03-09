#if UNITY_EDITOR
using UnityEngine;
using PixelVanguard.Data;
using PixelVanguard.Gameplay;
using System;
using System.Collections;

namespace PixelVanguard.Testing
{
    /// <summary>
    /// Editor-only testing tool for quickly applying weapons and upgrades for trailer filming.
    /// Place this component in the GameScene. It will automatically apply configured weapons
    /// and upgrades when the scene starts in the Unity Editor.
    /// </summary>
    public class TrailerTestingTool : MonoBehaviour
    {
        [Header("Testing Configuration")]
        [SerializeField] private bool enableTestingMode = false;
        
        [Header("Weapons to Equip")]
        [Tooltip("Weapons to unlock (starter weapon is auto-equipped)")]
        [SerializeField] private WeaponData[] weaponsToEquip;
        
        [Header("Upgrades to Apply")]
        [Tooltip("Upgrades and how many times to apply each")]
        [SerializeField] private UpgradeConfig[] upgradesToApply;

        private void Start()
        {
            if (!enableTestingMode) return;

            StartCoroutine(ApplyTestingConfiguration());
        }

        private IEnumerator ApplyTestingConfiguration()
        {
            // Wait one frame to ensure all managers are initialized
            yield return null;

            // Find required managers
            UpgradeManager upgradeManager = FindAnyObjectByType<UpgradeManager>();
            
            if (upgradeManager == null)
            {
                Debug.LogError("[TrailerTestingTool] UpgradeManager not found!");
                yield break;
            }

            // Apply weapons
            if (weaponsToEquip != null && weaponsToEquip.Length > 0)
            {
                foreach (var weaponData in weaponsToEquip)
                {
                    if (weaponData != null)
                    {
                        UpgradeData weaponUnlock = CreateWeaponUnlockUpgrade(weaponData);
                        upgradeManager.ApplyUpgrade(weaponUnlock);
                    }
                }
            }

            // Apply upgrades
            if (upgradesToApply != null && upgradesToApply.Length > 0)
            {
                foreach (var config in upgradesToApply)
                {
                    if (config?.upgrade != null)
                    {
                        for (int i = 0; i < config.timesToApply; i++)
                        {
                            upgradeManager.ApplyUpgrade(config.upgrade);
                        }
                    }
                }
            }
        }

        private UpgradeData CreateWeaponUnlockUpgrade(WeaponData weaponData)
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeData>();
            upgrade.type = UpgradeType.NewWeapon;
            upgrade.weaponToEquip = weaponData;
            return upgrade;
        }
    }

    [Serializable]
    public class UpgradeConfig
    {
        public UpgradeData upgrade;
        [Min(1)]
        public int timesToApply = 1;
    }
}
#endif
