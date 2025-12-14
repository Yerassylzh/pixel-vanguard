using System;
using UnityEngine;

namespace PixelVanguard.Core
{
    /// <summary>
    /// Simple event system for decoupled communication between systems.
    /// </summary>
    public static class GameEvents
    {
        // Game Flow Events
        public static event Action OnGameStart;
        public static event Action OnGamePause;
        public static event Action OnGameResume;
        public static event Action<GameOverReason> OnGameOver;

        // Player Events
        public static event Action<float, float> OnPlayerHealthChanged; // currentHP, maxHP
        public static event Action OnPlayerDeath;
        public static event Action OnPlayerLevelUp;

        // Combat Events
        public static event Action<int> OnEnemyKilled; // killCount
        public static event Action<float> OnXPGained; // xp amount
        public static event Action<int> OnGoldCollected; // gold amount

        // Weapon Events
        public static event Action<string> OnWeaponEquipped; // weaponID
        public static event Action<string, int> OnWeaponUpgraded; // weaponID, newLevel

        // Platform Events
        public static event Action<PlatformType> OnPlatformChanged;

        // Invoke methods
        public static void TriggerGameStart() => OnGameStart?.Invoke();
        public static void TriggerGamePause() => OnGamePause?.Invoke();
        public static void TriggerGameResume() => OnGameResume?.Invoke();
        public static void TriggerGameOver(GameOverReason reason) => OnGameOver?.Invoke(reason);
        
        public static void TriggerPlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);
        public static void TriggerPlayerDeath() => OnPlayerDeath?.Invoke();
        public static void TriggerPlayerLevelUp() => OnPlayerLevelUp?.Invoke();
        
        public static void TriggerEnemyKilled(int totalKills) => OnEnemyKilled?.Invoke(totalKills);
        public static void TriggerXPGained(float amount) => OnXPGained?.Invoke(amount);
        public static void TriggerGoldCollected(int amount) => OnGoldCollected?.Invoke(amount);
        
        public static void TriggerWeaponEquipped(string weaponID) => OnWeaponEquipped?.Invoke(weaponID);
        public static void TriggerWeaponUpgraded(string weaponID, int newLevel) => OnWeaponUpgraded?.Invoke(weaponID, newLevel);

        public static void TriggerPlatformChanged(PlatformType platform) => OnPlatformChanged?.Invoke(platform);

        /// <summary>
        /// Clear all event subscribers. Call when changing scenes.
        /// </summary>
        public static void ClearAll()
        {
            OnGameStart = null;
            OnGamePause = null;
            OnGameResume = null;
            OnGameOver = null;
            OnPlayerHealthChanged = null;
            OnPlayerDeath = null;
            OnPlayerLevelUp = null;
            OnEnemyKilled = null;
            OnXPGained = null;
            OnGoldCollected = null;
            OnWeaponEquipped = null;
            OnWeaponUpgraded = null;
            OnPlatformChanged = null;
        }
    }

    public enum GameOverReason
    {
        Victory,      // Survived to time limit
        PlayerDied    // HP reached 0 and declined revive
    }
}
