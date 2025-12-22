using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Manages enemy health, damage, and death behavior.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyHealth : MonoBehaviour
    {
        [Header("Loot Prefabs")]
        [SerializeField] private GameObject xpGemPrefab;
        [SerializeField] private GameObject goldCoinPrefab;
        [SerializeField] private GameObject healthPotionPrefab;

        [Header("Configuration")]
        [SerializeField] private Data.EnemyData enemyData;

        [Header("Runtime Stats")]
        [SerializeField] private float currentHealth;
        [SerializeField] private bool isAlive = true;

        private Rigidbody2D rb;
        private static int totalKillCount = 0; // Track total kills across all enemies

        public bool IsAlive => isAlive;
        public float ContactDamage => enemyData != null ? enemyData.contactDamage : 5f;
        public Data.EnemyData EnemyData => enemyData; // Expose for EnemyAI to access move speed

        private void Awake()
        {
            // Auto-get required component
            rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            if (enemyData != null)
            {
                currentHealth = enemyData.maxHealth;
            }
        }

        /// <summary>
        /// Apply damage to this enemy.
        /// </summary>
        public void TakeDamage(float damage, Vector2 knockbackDirection, float knockbackForce)
        {
            if (!isAlive) return;

            currentHealth -= damage;

            // Apply knockback
            if (rb != null && enemyData != null)
            {
                float actualKnockback = knockbackForce * (1f - enemyData.weightResistance);
                rb.AddForce(knockbackDirection * actualKnockback, ForceMode2D.Impulse);
            }

            // Check for death
            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        private void Die()
        {
            if (!isAlive) return;

            isAlive = false;

            // Update kill count
            totalKillCount++;
            Core.GameEvents.TriggerEnemyKilled(totalKillCount);

            // Drop loot
            DropLoot();

            // Destroy this enemy
            Destroy(gameObject);
        }

        private void DropLoot()
        {
            if (enemyData == null) return;

            Vector3 dropPosition = transform.position;

            // Track how many items we're dropping to offset them properly
            int itemCount = 0;
            Vector3[] offsets = new Vector3[]
            {
                new Vector3(-0.3f, 0.2f, 0f),   // Left-up
                new Vector3(0.3f, 0.2f, 0f),    // Right-up
                new Vector3(0f, -0.3f, 0f),     // Down
                new Vector3(-0.3f, -0.2f, 0f),  // Left-down
                new Vector3(0.3f, -0.2f, 0f)    // Right-down
            };

            // Drop XP Gem (always if xpDrop > 0)
            if (enemyData.xpDrop > 0)
            {
                Vector3 spawnPos = dropPosition + offsets[itemCount % offsets.Length];
                SpawnXPGem(spawnPos, enemyData.xpDrop);
                itemCount++;
            }

            // Drop Gold Coin (chance-based)
            if (enemyData.goldDrop > 0 && Random.value < enemyData.goldDropChance)
            {
                Vector3 spawnPos = dropPosition + offsets[itemCount % offsets.Length];
                SpawnGoldCoin(spawnPos, enemyData.goldDrop);
                itemCount++;
            }

            // Drop health potion (chance-based)
            if (Random.value < enemyData.healthPotionDropChance)
            {
                Vector3 spawnPos = dropPosition + offsets[itemCount % offsets.Length];
                SpawnHealthPotion(spawnPos);
            }
        }

        private void SpawnXPGem(Vector3 position, int xpAmount)
        {
            if (xpGemPrefab == null) 
            {
                Debug.LogWarning("[EnemyHealth] XPGem Prefab not assigned!");
                return;
            }

            GameObject gemObj = Instantiate(xpGemPrefab, position, Quaternion.identity);
            
            // Configure XP amount
            var gem = gemObj.GetComponent<XPGem>();
            if (gem != null)
            {
                gem.SetXPAmount(xpAmount);
            }
        }

        private void SpawnGoldCoin(Vector3 position, int goldAmount)
        {
            if (goldCoinPrefab == null)
            {
                Debug.LogWarning("[EnemyHealth] GoldCoin Prefab not assigned!");
                return;
            }

            GameObject coinObj = Instantiate(goldCoinPrefab, position, Quaternion.identity);

            // Configure Gold amount
            var coin = coinObj.GetComponent<GoldCoin>();
            if (coin != null)
            {
                coin.SetGoldValue(goldAmount);
            }
        }

        private void SpawnHealthPotion(Vector3 position)
        {
            if (healthPotionPrefab == null)
            {
                // Silent return or warning depending on preference
                return;
            }

            Instantiate(healthPotionPrefab, position, Quaternion.identity);
        }


        /// <summary>
        /// Set enemy data (called by spawner).
        /// </summary>
        public void Initialize(Data.EnemyData data)
        {
            enemyData = data;
            currentHealth = data.maxHealth;
        }

        /// <summary>
        /// Reset kill count (call when starting new game).
        /// </summary>
        public static void ResetKillCount()
        {
            totalKillCount = 0;
        }
    }
}
