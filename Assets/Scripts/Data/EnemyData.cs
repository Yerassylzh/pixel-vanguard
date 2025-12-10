using UnityEngine;

namespace PixelVanguard.Data
{
    /// <summary>
    /// ScriptableObject defining an enemy type.
    /// Assets stored in: Assets/ScriptableObjects/Enemies/
    /// </summary>
    [CreateAssetMenu(fileName = "Enemy", menuName = "PixelVanguard/Enemy Data", order = 3)]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique identifier for this enemy (e.g., 'skeleton')")]
        public string enemyID;
        
        [Tooltip("Display name (e.g., 'Skeleton Grunt')")]
        public string displayName;

        [Header("Stats")]
        [Tooltip("Maximum health points")]
        public float maxHealth = 20f;
        
        [Tooltip("Movement speed in units per second")]
        public float moveSpeed = 3f;
        
        [Tooltip("Damage dealt on contact with player")]
        public float contactDamage = 5f;
        
        [Tooltip("Resistance to knockback (0-1, higher = harder to knock back)")]
        [Range(0f, 1f)]
        public float weightResistance = 0f;

        [Header("Loot")]
        [Tooltip("XP dropped on death")]
        public int xpDrop = 5;
        
        [Tooltip("Gold dropped on death")]
        public int goldDrop = 1;
        
        [Tooltip("Chance to drop health potion (0-1)")]
        [Range(0f, 1f)]
        public float healthPotionDropChance = 0.05f;

        [Header("Spawn Configuration")]
        [Tooltip("Spawn weight (higher = more common)")]
        public int spawnWeight = 10;
        
        [Tooltip("Minimum game time in seconds before this enemy can spawn")]
        public int minGameTimeSeconds = 0;

        [Header("Visuals")]
        [Tooltip("Enemy prefab to spawn")]
        public GameObject prefab;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(enemyID))
            {
                Debug.LogError($"[EnemyData] {name} is missing enemyID!", this);
            }

            if (maxHealth <= 0)
            {
                Debug.LogWarning($"[EnemyData] {enemyID} has invalid maxHealth!", this);
            }

            if (spawnWeight < 0)
            {
                Debug.LogWarning($"[EnemyData] {enemyID} has negative spawnWeight!", this);
            }
        }
    }
}
