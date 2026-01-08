using UnityEngine;
using PixelVanguard.Core;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Handles lifesteal mechanics by listening to damage events.
    /// Replaces direct lifesteal logic in weapons.
    /// </summary>
    public class LifestealSystem : MonoBehaviour
    {
        private PlayerHealth playerHealth;
        private UpgradeManager upgradeManager;

        private void Start()
        {
            // Find dependencies
            if (PlayerController.Instance != null)
            {
                playerHealth = PlayerController.Instance.GetComponent<PlayerHealth>();
            }
            
            upgradeManager = FindFirstObjectByType<UpgradeManager>();

            // Subscribe to damage events
            // NOTE: We need GameEvents to support "OnDamageDealt" for this to work perfectly.
            // Currently GameEvents has OnPlayerDamaged (incoming), but maybe not outgoing.
            // For now, let's assume weapons trigger something or we hook into EnemyHealth.
            // But wait, the previous code was inside WeaponBase calling "DealDamageWithLifesteal".
            // So we need to expose an event "OnDamageDealtByPlayer" in GameEvents.
            GameEvents.OnDamageDealtByPlayer += HandleDamageDealt;
        }

        private void OnDestroy()
        {
            GameEvents.OnDamageDealtByPlayer -= HandleDamageDealt;
        }

        private void HandleDamageDealt(float damageAmount)
        {
            if (upgradeManager == null || playerHealth == null) return;
            if (!playerHealth.IsAlive) return;

            float lifestealPercent = upgradeManager.GetLifestealPercent();
            if (lifestealPercent > 0f)
            {
                float healAmount = damageAmount * (lifestealPercent / 100f);
                if (healAmount > 0)
                {
                    playerHealth.Heal(healAmount);
                }
            }
        }
    }
}
