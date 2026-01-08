using UnityEngine;
using PixelVanguard.Data;
using PixelVanguard.Interfaces;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Handles enemy health, damage, knockback, death, and loot drops.
    /// Implements IDamageable for event-driven damage feedback.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        [Header("Data")]
        [SerializeField] private EnemyData enemyData;

        [Header("Stats")]
        private float currentHealth;
        public float ContactDamage => enemyData != null ? enemyData.contactDamage : 5f;

        // Public accessors
        public EnemyData EnemyData => enemyData;
        public bool IsAlive => isAlive;

        private bool isAlive = true;
        private Rigidbody2D rb;

        // IDamageable implementation
        public event System.Action<float, Vector3> OnDamaged;
        public event System.Action<float, Vector3> OnHealed;
        
        // Static kill count (shared across all enemies)
        private static int totalKillCount = 0;

        private void Awake()
        {
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
        /// Initialize enemy with data (called by EnemySpawner).
        /// </summary>
        public void Initialize(Data.EnemyData data)
        {
            enemyData = data;
            currentHealth = data.maxHealth;
        }

        /// <summary>
        /// Apply damage to the enemy with knockback.
        /// </summary>
        public void TakeDamage(float damage, Vector2 knockbackDirection, float knockbackForce)
        {
            if (!isAlive) return;

            currentHealth -= damage;

            // Fire damage event for feedback systems
            OnDamaged?.Invoke(damage, transform.position);

            float actualKnockback = knockbackForce * (1f - enemyData.weightResistance);
            rb.AddForce(knockbackDirection * actualKnockback, ForceMode2D.Force);

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        private void Die()
        {
            if (!isAlive) return;

            isAlive = false;

            totalKillCount++;
            Core.GameEvents.TriggerEnemyKilled(totalKillCount);

            DropLoot();
            Destroy(gameObject);
        }

        private void DropLoot()
        {
            if (enemyData == null) return;

            Vector3 dropPosition = transform.position;
            int itemCount = 0;
            Vector3[] offsets = new Vector3[]
            {
                new Vector3(-0.3f, 0.2f, 0f),   // Left-up
                new Vector3(0.3f, 0.2f, 0f),    // Right-up
                new Vector3(0f, -0.3f, 0f),     // Down
                new Vector3(-0.3f, -0.2f, 0f),  // Left-down
                new Vector3(0.3f, -0.2f, 0f)    // Right-down
            };

            // Drop XP Gem (always if > 0)
            if (enemyData.xpDrop > 0)
            {
                Vector3 spawnPos = dropPosition + offsets[itemCount % offsets.Length];
                SpawnXPGem(spawnPos, enemyData.xpDrop);
                itemCount++;
            }

            // Drop Gold Coin (chance-based + GOLD BONUS INTEGRATED)
            if (enemyData.goldDrop > 0 && Random.value < enemyData.goldDropChance)
            {
                Vector3 spawnPos = dropPosition + offsets[itemCount % offsets.Length];
                
                // ✅ INTEGRATED: Apply gold bonus from UpgradeManager
                int baseGold = enemyData.goldDrop;
                int finalGold = CalculateGoldWithBonus(baseGold);
                
                SpawnGoldCoin(spawnPos, finalGold);
                itemCount++;
            }

            // Drop Health Potion (chance-based)
            if (enemyData.healthPotionPrefab != null && Random.value < enemyData.healthPotionDropChance)
            {
                Vector3 spawnPos = dropPosition + offsets[itemCount % offsets.Length];
                Instantiate(enemyData.healthPotionPrefab, spawnPos, Quaternion.identity);
                itemCount++;
            }
        }

        /// <summary>
        /// Calculate final gold amount with Lucky Coins upgrade bonus.
        /// </summary>
        private int CalculateGoldWithBonus(int baseGold)
        {
            var upgradeManager = FindFirstObjectByType<UpgradeManager>();
            if (upgradeManager != null)
            {
                float bonus = upgradeManager.GetGoldBonusPercent();
                int finalGold = Mathf.RoundToInt(baseGold * (1f + bonus));
                
                return finalGold;
            }
            
            return baseGold;
        }

        /// <summary>
        /// Spawn an XP gem at the drop location.
        /// </summary>
        private void SpawnXPGem(Vector3 position, int xpAmount)
        {
            if (enemyData.xpGemPrefab == null)
            {
                Debug.LogWarning($"[EnemyHealth] XP gem prefab not assigned for {enemyData.enemyName}!");
                return;
            }

            GameObject gemObj = Instantiate(enemyData.xpGemPrefab, position, Quaternion.identity);
            var xpGem = gemObj.GetComponent<XPGem>();
            if (xpGem != null)
            {
                xpGem.SetXPAmount(xpAmount);
            }
        }

        /// <summary>
        /// Spawn a gold coin at the drop location.
        /// </summary>
        private void SpawnGoldCoin(Vector3 position, int goldAmount)
        {
            if (enemyData.goldCoinPrefab == null)
            {
                Debug.LogWarning($"[EnemyHealth] Gold coin prefab not assigned for {enemyData.enemyName}!");
                return;
            }

            GameObject coinObj = Instantiate(enemyData.goldCoinPrefab, position, Quaternion.identity);
            var goldCoin = coinObj.GetComponent<GoldCoin>();
            if (goldCoin != null)
            {
                goldCoin.SetGoldAmount(goldAmount);
            }
        }
    }
}
