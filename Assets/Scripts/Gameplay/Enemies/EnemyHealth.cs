using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Manages enemy health, damage, and death behavior.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyHealth : MonoBehaviour
    {
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
               // Vector3 spawnPos = dropPosition + offsets[itemCount % offsets.Length];
               // SpawnHealthPotion(spawnPos);
               // TODO: Implement health potion prefab when health system is expanded
            }
        }

        private void SpawnXPGem(Vector3 position, int xpAmount)
        {
            // Create XP gem GameObject
            GameObject gemObj = new GameObject("XPGem");
            gemObj.transform.position = position;
            gemObj.tag = "Pickup"; // For organization

            // Add visual (placeholder)
            var spriteRenderer = gemObj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateGemSprite();
            spriteRenderer.color = Color.cyan;
            spriteRenderer.sortingLayerName = "Collectibles";
            spriteRenderer.sortingOrder = 1;

            // Add circle collider
            var collider = gemObj.AddComponent<CircleCollider2D>();
            collider.radius = 0.3f;
            collider.isTrigger = true;

            // Add XPGem script
            var gem = gemObj.AddComponent<XPGem>();
            gem.SetXPAmount(xpAmount);
        }

        private void SpawnGoldCoin(Vector3 position, int goldAmount)
        {
            // Create gold coin GameObject
            GameObject coinObj = new GameObject("GoldCoin");
            coinObj.transform.position = position;
            coinObj.tag = "Pickup"; // For organization

            // Add visual (placeholder)
            var spriteRenderer = coinObj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateCoinSprite();
            spriteRenderer.color = Color.yellow;
            spriteRenderer.sortingLayerName = "Collectibles";
            spriteRenderer.sortingOrder = 1;

            // Add circle collider
            var collider = coinObj.AddComponent<CircleCollider2D>();
            collider.radius = 0.3f;
            collider.isTrigger = true;

            // Add GoldCoin script
            var coin = coinObj.AddComponent<GoldCoin>();
            coin.SetGoldValue(goldAmount);
        }

        private Sprite CreateGemSprite()
        {
            // Simple diamond shape (placeholder)
            Texture2D tex = new Texture2D(16, 16);
            Color[] pixels = new Color[16 * 16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;
            
            // Draw diamond
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    int dx = Mathf.Abs(x - 8);
                    int dy = Mathf.Abs(y - 8);
                    if (dx + dy < 6) pixels[y * 16 + x] = Color.white;
                }
            }
            
            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
        }

        private Sprite CreateCoinSprite()
        {
            // Simple circle shape (placeholder for gold coin)
            Texture2D tex = new Texture2D(16, 16);
            Color[] pixels = new Color[16 * 16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;
            
            // Draw circle
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    float dx = x - 8;
                    float dy = y - 8;
                    if (dx * dx + dy * dy < 25) pixels[y * 16 + x] = Color.white;
                }
            }
            
            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
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
