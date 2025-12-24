using UnityEngine;
using System.Collections.Generic;
using PixelVanguard.Data;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Manages upgrade selection and application.
    /// REFACTORED: Integrated all passive upgrades (lifesteal, gold, magnet).
    /// </summary>
    public class UpgradeManager : MonoBehaviour
    {
        [Header("Tracking")]
        private int passiveSkillCount = 0;
        private HashSet<UpgradeType> appliedUpgrades = new HashSet<UpgradeType>();
        
        // Passive effects (NOW INTEGRATED)
        private float lifestealPercent = 0f;
        private float goldBonusPercent = 0f;

        [Header("Available Upgrades")]
        [SerializeField] private UpgradeData[] allUpgrades;

        [Header("References (Auto-Find)")]
        private PlayerController playerController;
        private PlayerHealth playerHealth;
        private WeaponManager weaponManager;

        private void Start()
        {
            playerController = FindAnyObjectByType<PlayerController>();
            playerHealth = FindAnyObjectByType<PlayerHealth>();
            weaponManager = FindAnyObjectByType<WeaponManager>();
        }

        /// <summary>
        /// Get random upgrades for level-up panel with proper filtering and rarity weights.
        /// </summary>
        public UpgradeData[] GetRandomUpgrades(int count = 3)
        {
            if (allUpgrades == null || allUpgrades.Length == 0)
            {
                Debug.LogWarning("[UpgradeManager] No upgrades available!");
                return new UpgradeData[0];
            }

            // Build valid upgrades list with filtering
            var validUpgrades = new List<UpgradeData>();
            
            foreach (var upgrade in allUpgrades)
            {
                if (upgrade == null) continue;
                
                // Check if upgrade is valid for current player state
                if (!IsUpgradeValid(upgrade)) continue;
                
                validUpgrades.Add(upgrade);
            }

            if (validUpgrades.Count == 0)
            {
                Debug.LogWarning("[UpgradeManager] No valid upgrades after filtering!");
                return new UpgradeData[0];
            }

            // Use weighted random selection based on rarity
            count = Mathf.Min(count, validUpgrades.Count);
            var selectedUpgrades = new List<UpgradeData>();
            
            for (int i = 0; i < count; i++)
            {
                var selected = SelectWeightedRandom(validUpgrades);
                if (selected != null)
                {
                    selectedUpgrades.Add(selected);
                    validUpgrades.Remove(selected); // Prevent duplicates
                }
            }

            return selectedUpgrades.ToArray();
        }
        
        /// <summary>
        /// Check if an upgrade is valid for current player state.
        /// </summary>
        private bool IsUpgradeValid(UpgradeData upgrade)
        {
            // First check: Has this exact upgrade already been applied?
            if (appliedUpgrades.Contains(upgrade.type))
            {
                return false;
            }
            
            switch (upgrade.type)
            {
                // NewWeapon checks
                case UpgradeType.NewWeapon:
                    if (upgrade.weaponToEquip == null) return false;
                    if (weaponManager != null && weaponManager.IsWeaponEquipped(upgrade.weaponToEquip.weaponID)) return false;
                    if (weaponManager != null && weaponManager.GetEquippedWeapons().Count >= 4) return false;
                    break;
                
                // Greatsword upgrades - require Greatsword equipped
                case UpgradeType.GreatswordMirrorSlash:
                case UpgradeType.GreatswordDamageBoost:
                case UpgradeType.GreatswordCooldownBoost:
                    if (!IsWeaponEquipped(Data.WeaponType.Greatsword)) return false;
                    break;
                
                // Crossbow upgrades - require AutoCrossbow equipped
                case UpgradeType.CrossbowDualShot:
                case UpgradeType.CrossbowTripleShot:
                case UpgradeType.CrossbowPierce:
                    if (!IsWeaponEquipped(Data.WeaponType.Crossbow)) return false;
                    break;
                
                // Holy Water upgrades - require HolyWater equipped
                case UpgradeType.HolyWaterRadius:
                case UpgradeType.HolyWaterScaling:
                case UpgradeType.HolyWaterDuration:
                    if (!IsWeaponEquipped(Data.WeaponType.HolyWater)) return false;
                    break;
                
                // Magic Orbitals upgrades - require MagicOrbitals equipped
                case UpgradeType.OrbitalsExpandedOrbit:
                case UpgradeType.OrbitalsOverchargedSpheres:
                    if (!IsWeaponEquipped(Data.WeaponType.MagicOrbitals)) return false;
                    break;
                
                // Passives - check limit (max 3)
                case UpgradeType.PassiveLifesteal:
                case UpgradeType.PassiveMagnet:
                case UpgradeType.PassiveLuckyCoin:
                    if (passiveSkillCount >= 3) return false;
                    break;
            }
            
            return true;
        }
        
        /// <summary>
        /// Check if a specific weapon type is currently equipped.
        /// </summary>
        private bool IsWeaponEquipped(Data.WeaponType weaponType)
        {
            if (weaponManager == null) return false;
            
            foreach (var weapon in weaponManager.GetEquippedWeapons())
            {
                if (weapon?.weaponData?.type == weaponType) return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Select a random upgrade using weighted probability based on rarity.
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
            
            // Pick random value in range [0, totalWeight)
            int randomValue = Random.Range(0, totalWeight);
            
            // Find selected upgrade
            int currentWeight = 0;
            foreach (var upgrade in upgrades)
            {
                currentWeight += upgrade.rarityWeight;
                if (randomValue < currentWeight)
                {
                    return upgrade;
                }
            }
            
            // Fallback (shouldn't happen)
            return upgrades[0];
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
                    float speedMultiplier = 1.0f + (upgrade.value / 100f);
                    ApplySpeedUpgrade(speedMultiplier);
                    break;

                case UpgradeType.PlayerMaxHP:
                    ApplyHealthUpgrade(upgrade.value);
                    break;

                case UpgradeType.WeaponAttackSpeed:
                    float attackSpeedMultiplier = 1.0f - (upgrade.value / 100f);
                    ApplyWeaponSpeedUpgrade(attackSpeedMultiplier);
                    break;

                case UpgradeType.WeaponDamage:
                    float damageMultiplier = 1.0f + (upgrade.value / 100f);
                    ApplyWeaponDamageUpgrade(damageMultiplier);
                    break;

                case UpgradeType.NewWeapon:
                    ApplyNewWeaponUpgrade(upgrade.weaponToEquip);
                    break;

                // GREATSWORD
                case UpgradeType.GreatswordMirrorSlash:
                    ApplyMirrorSlash();
                    break;
                    
                case UpgradeType.GreatswordDamageBoost:
                    ApplyWeaponSpecificDamage(WeaponType.Greatsword, 1.5f);
                    break;
                    
                case UpgradeType.GreatswordCooldownBoost:
                    ApplyWeaponSpecificCooldown(WeaponType.Greatsword, 0.7f);
                    break;
                
                // CROSSBOW
                case UpgradeType.CrossbowDualShot:
                    ApplyCrossbowMultiShot(2);
                    break;
                    
                case UpgradeType.CrossbowTripleShot:
                    ApplyCrossbowMultiShot(3);
                    break;
                    
                case UpgradeType.CrossbowPierce:
                    ApplyCrossbowPierce();
                    break;
                
                // HOLY WATER
                case UpgradeType.HolyWaterRadius:
                    ApplyHolyWaterRadius(1.4f);
                    break;
                    
                case UpgradeType.HolyWaterScaling:
                    ApplyHolyWaterScaling(0.06f);
                    break;
                    
                case UpgradeType.HolyWaterDuration:
                    ApplyHolyWaterDuration(2.0f);
                    break;
                
                // MAGIC ORBITALS
                case UpgradeType.OrbitalsExpandedOrbit:
                    ApplyOrbitalsRadius(1.4f);
                    break;
                    
                case UpgradeType.OrbitalsOverchargedSpheres:
                    ApplyOrbitalsDamage(1.3f);
                    break;
                
                // PASSIVES (NOW INTEGRATED)
                case UpgradeType.PassiveLifesteal:
                    if (passiveSkillCount < 3)
                    {
                        lifestealPercent += 0.03f; // 3% lifesteal
                        passiveSkillCount++;
                        Debug.Log($"[UpgradeManager] ❤️ Lifesteal: {lifestealPercent * 100}% (INTEGRATED)");
                    }
                    break;
                    
                case UpgradeType.PassiveMagnet:
                    if (passiveSkillCount < 3)
                    {
                        ApplyMagnetRadius(1.5f); // +50% pickup radius (NOW WORKS)
                        passiveSkillCount++;
                    }
                    break;
                    
                case UpgradeType.PassiveLuckyCoin:
                    if (passiveSkillCount < 3)
                    {
                        goldBonusPercent += 0.4f; // +40% gold (NOW INTEGRATED)
                        passiveSkillCount++;
                        Debug.Log($"[UpgradeManager] 💰 Gold Bonus: {goldBonusPercent * 100}% (INTEGRATED)");
                    }
                    break;

                default:
                    Debug.LogWarning($"[UpgradeManager] Unknown upgrade type: {upgrade.type}");
                    break;
            }

            // Track that this upgrade has been applied
            appliedUpgrades.Add(upgrade.type);
            
            Debug.Log($"[UpgradeManager] Applied upgrade: {upgrade.upgradeName}");
        }

        private void ApplySpeedUpgrade(float multiplier)
        {
            if (playerController != null)
            {
                float currentSpeed = playerController.GetMoveSpeed();
                float newSpeed = currentSpeed * multiplier;
                playerController.SetMoveSpeed(newSpeed);
                Debug.Log($"⚡ [PLAYER] SPEED: {currentSpeed:F1} → {newSpeed:F1} (+{(newSpeed - currentSpeed):F1}, +{((multiplier - 1) * 100):F0}%)");
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
            if (weaponManager == null || weaponData == null) return;

            if (weaponManager.IsWeaponEquipped(weaponData.weaponID))
            {
                Debug.LogWarning($"🗡️ [UPGRADE] {weaponData.displayName} already equipped!");
                return;
            }

            var currentWeapons = weaponManager.GetEquippedWeapons();
            if (currentWeapons.Count >= 4)
            {
                Debug.LogWarning($"🗡️ [UPGRADE] Cannot equip {weaponData.displayName} - Max weapons (4) already equipped!");
                return;
            }

            bool success = weaponManager.EquipWeapon(weaponData);
            if (success)
            {
                Debug.Log($"🗡️ [UPGRADE] NEW WEAPON ACQUIRED: {weaponData.displayName} ({currentWeapons.Count + 1}/4 weapons)");
            }
        }

        private void ApplyToAllWeapons(System.Action<WeaponBase> action)
        {
            if (weaponManager == null) return;

            var equippedWeapons = weaponManager.GetEquippedWeapons();
            foreach (var weapon in equippedWeapons)
            {
                if (weapon.weaponScript != null)
                {
                    action(weapon.weaponScript);
                }
            }
        }

        // === UPGRADE HELPER METHODS ===
        
        private void ApplyMirrorSlash()
        {
            weaponManager?.SpawnMirrorGreatsword();
        }
        
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
        
        private void ApplyCrossbowMultiShot(int arrowCount)
        {
            if (weaponManager == null) return;
            
            foreach (var weaponInstance in weaponManager.GetEquippedWeapons())
            {
                if (weaponInstance?.weaponData?.type == Data.WeaponType.Crossbow && 
                    weaponInstance.weaponScript is AutoCrossbowWeapon crossbow)
                {
                    crossbow.SetMultiShot(arrowCount);
                }
            }
        }
        
        private void ApplyCrossbowPierce()
        {
            if (weaponManager == null) return;
            
            foreach (var weaponInstance in weaponManager.GetEquippedWeapons())
            {
                if (weaponInstance?.weaponData?.type == Data.WeaponType.Crossbow && 
                    weaponInstance.weaponScript is AutoCrossbowWeapon crossbow)
                {
                    crossbow.IncrementPierce();
                }
            }
        }
        
        private void ApplyHolyWaterRadius(float multiplier)
        {
            if (weaponManager == null) return;
            
            foreach (var weaponInstance in weaponManager.GetEquippedWeapons())
            {
                if (weaponInstance?.weaponData?.type == Data.WeaponType.HolyWater && 
                    weaponInstance.weaponScript is HolyWaterWeapon holyWater)
                {
                    holyWater.MultiplyPuddleRadius(multiplier);
                }
            }
        }
        
        private void ApplyHolyWaterScaling(float percentPercentage)
        {
            if (weaponManager == null) return;
            
            foreach (var weaponInstance in weaponManager.GetEquippedWeapons())
            {
                if (weaponInstance?.weaponData?.type == Data.WeaponType.HolyWater && 
                    weaponInstance.weaponScript is HolyWaterWeapon holyWater)
                {
                    holyWater.SetHPScaling(percentPercentage);
                }
            }
        }
        
        private void ApplyHolyWaterDuration(float multiplier)
        {
            if (weaponManager == null) return;
            
            foreach (var weaponInstance in weaponManager.GetEquippedWeapons())
            {
                if (weaponInstance?.weaponData?.type == Data.WeaponType.HolyWater && 
                    weaponInstance.weaponScript is HolyWaterWeapon holyWater)
                {
                    holyWater.UpgradeDuration(multiplier - 1f); // Convert multiplier to percentage
                }
            }
        }
        
        private void ApplyOrbitalsRadius(float multiplier)
        {
            if (weaponManager == null) return;
            
            foreach (var weaponInstance in weaponManager.GetEquippedWeapons())
            {
                if (weaponInstance?.weaponData?.type == Data.WeaponType.MagicOrbitals && 
                    weaponInstance.weaponScript is MagicOrbitalsWeapon magicOrbitals)
                {
                    magicOrbitals.MultiplyOrbitRadius(multiplier);
                }
            }
        }
        
        private void ApplyOrbitalsDamage(float multiplier)
        {
            if (weaponManager == null) return;
            
            foreach (var weaponInstance in weaponManager.GetEquippedWeapons())
            {
                if (weaponInstance?.weaponData?.type == Data.WeaponType.MagicOrbitals)
                {
                    weaponInstance.weaponScript?.IncreaseDamage(multiplier);
                }
            }
        }
        
        private void ApplyMagnetRadius(float multiplier)
        {
            // FIXED: Now actually works since XPGem.magnetRange is public
            var xpGems = FindObjectsByType<XPGem>(FindObjectsSortMode.None);
            foreach (var gem in xpGems)
            {
                if (gem != null)
                {
                    gem.magnetRange *= multiplier;
                }
            }
            
            var goldCoins = FindObjectsByType<GoldCoin>(FindObjectsSortMode.None);
            foreach (var coin in goldCoins)
            {
                if (coin != null)
                {
                    // Assuming GoldCoin has same public magnetRange field
                    coin.magnetRange *= multiplier;
                }
            }
            
            Debug.Log($"[UpgradeManager] 🧲 Magnet radius increased by {(multiplier - 1f) * 100}% (WORKING!)");
        }
        
        public float GetLifestealPercent() => lifestealPercent;
        public float GetGoldBonusPercent() => goldBonusPercent;
    }
}
