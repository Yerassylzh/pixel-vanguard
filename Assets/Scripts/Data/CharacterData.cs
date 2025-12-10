using UnityEngine;

namespace PixelVanguard.Data
{
    /// <summary>
    /// ScriptableObject defining a playable character.
    /// Assets stored in: Assets/ScriptableObjects/Characters/
    /// </summary>
    [CreateAssetMenu(fileName = "Character", menuName = "PixelVanguard/Character Data", order = 1)]
    public class CharacterData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique identifier for this character (e.g., 'knight')")]
        public string characterID;
        
        [Tooltip("Display name shown in UI (e.g., 'The Knight')")]
        public string displayName;
        
        [Tooltip("Short description of character playstyle")]
        [TextArea(2, 4)]
        public string description;

        [Header("Visuals")]
        [Tooltip("Portrait for character selection screen")]
        public Sprite portrait;
        
        [Tooltip("Idle sprite for main menu background")]
        public Sprite idleSprite;
        
        [Tooltip("Character prefab to spawn in GameScene")]
        public GameObject characterPrefab;

        [Header("Base Stats")]
        [Tooltip("Maximum health points")]
        public float maxHealth = 100f;
        
        [Tooltip("Movement speed in units per second")]
        public float moveSpeed = 5f;
        
        [Tooltip("Base damage multiplier (1.0 = 100%)")]
        public float baseDamageMultiplier = 1f;

        [Header("Starting Loadout")]
        [Tooltip("Weapon this character starts with")]
        public WeaponData starterWeapon;

        [Header("Unlock Requirement")]
        public UnlockType unlockType = UnlockType.FreeStarter;
        
        [Tooltip("Gold cost if unlockType is Gold")]
        public int goldCost = 10000;
        
        [Tooltip("Number of ads to watch if unlockType is Ads")]
        public int adWatchCount = 5;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(characterID))
            {
                Debug.LogError($"[CharacterData] {name} is missing characterID!", this);
            }

            if (maxHealth <= 0)
            {
                Debug.LogWarning($"[CharacterData] {characterID} has invalid maxHealth!", this);
            }

            if (moveSpeed <= 0)
            {
                Debug.LogWarning($"[CharacterData] {characterID} has invalid moveSpeed!", this);
            }
        }
    }

    public enum UnlockType
    {
        FreeStarter,  // Always unlocked (e.g., Knight)
        Gold,         // Unlock with gold
        Ads           // Unlock by watching ads
    }
}
