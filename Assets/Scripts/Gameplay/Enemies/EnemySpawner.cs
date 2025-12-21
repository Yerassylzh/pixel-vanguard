using UnityEngine;
using System.Collections.Generic;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Spawns enemies at screen edges with difficulty scaling over time.
    /// Uses GameManager.GameTime for centralized difficulty control.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn Configuration")]
        [SerializeField] private List<Data.EnemyData> enemyTypes = new List<Data.EnemyData>();
        [SerializeField] private float spawnInterval = 2f; // Seconds between spawns
        [SerializeField] private int maxEnemies = 100; // Max enemies on screen

        [Header("Difficulty Scaling")]
        [SerializeField] private float difficultyIncreaseRate = 0.1f; // Spawn rate increase per minute
        [SerializeField] private float maxSpawnRate = 10f; // Max enemies per interval

        [Header("Spawn Area")]
        [SerializeField] private float spawnDistanceFromCamera = 12f; // Units from camera edge

        private Camera mainCamera;
        private float nextSpawnTime = 0f;
        private int currentEnemyCount = 0;

        private void Start()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("[EnemySpawner] Main Camera not found!");
                enabled = false;
                return;
            }

            // Subscribe to enemy death to track count
            Core.GameEvents.OnEnemyKilled += OnEnemyKilled;

            // Pre-generate placeholder sprite
            InitializePlaceholderSprite();
        }

        private void OnDestroy()
        {
            Core.GameEvents.OnEnemyKilled -= OnEnemyKilled;
        }

        private void Update()
        {
            // Only spawn if game is playing
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            // Check if it's time to spawn
            if (Time.time >= nextSpawnTime && currentEnemyCount < maxEnemies)
            {
                SpawnEnemy();
                ScheduleNextSpawn();
            }
        }

        private void SpawnEnemy()
        {
            // Get enemy data based on spawn eligibility
            var enemyData = GetEnemyDataForCurrentTime();
            if (enemyData == null)
            {
                Debug.LogWarning("[EnemySpawner] No eligible enemy types found!");
                return;
            }

            // Calculate spawn position at screen edge
            Vector3 spawnPosition = GetRandomSpawnPosition();

            GameObject enemyObj;

            // Instantiate from prefab if available
            if (enemyData.prefab != null)
            {
                enemyObj = Instantiate(enemyData.prefab, spawnPosition, Quaternion.identity);
            }
            else
            {
                // Fallback: Create enemy programmatically if no prefab assigned
                Debug.LogWarning($"[EnemySpawner] {enemyData.enemyID} has no prefab! Creating placeholder.");
                enemyObj = CreatePlaceholderEnemy(enemyData, spawnPosition);
            }

            // Initialize enemy with data
            var health = enemyObj.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.Initialize(enemyData);
            }

            var ai = enemyObj.GetComponent<EnemyAI>();
            if (ai != null)
            {
                ai.SetMoveSpeed(enemyData.moveSpeed);
            }

            currentEnemyCount++;
        }

        private GameObject CreatePlaceholderEnemy(Data.EnemyData enemyData, Vector3 position)
        {
            // Create enemy from scratch (temporary solution until prefabs ready)
            GameObject enemyObj = new GameObject($"Enemy_{enemyData.enemyID}");
            enemyObj.transform.position = position;
            enemyObj.tag = "Enemy";

            enemyObj.layer = LayerMask.NameToLayer("Enemy");

            // Add visual (placeholder sprite)
            var spriteRenderer = enemyObj.AddComponent<SpriteRenderer>();
            
            // Ensure sprite exists
            InitializePlaceholderSprite();
            spriteRenderer.sprite = placeholderSprite;
            
            spriteRenderer.color = Color.red;
            spriteRenderer.sortingLayerName = "Enemies";

            // Add Rigidbody2D (required by EnemyHealth and EnemyAI)
            var rb = enemyObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Add Collider
            var collider = enemyObj.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;

            // Add EnemyHealth
            enemyObj.AddComponent<EnemyHealth>();

            // Add EnemyAI
            enemyObj.AddComponent<EnemyAI>();

            return enemyObj;
        }

        private void ScheduleNextSpawn()
        {
            // Get difficulty multiplier based on game time
            float gameTime = GameManager.Instance.GameTime;
            float difficultyMultiplier = 1f + (gameTime / 60f) * difficultyIncreaseRate;
            difficultyMultiplier = Mathf.Min(difficultyMultiplier, maxSpawnRate);

            // Calculate next spawn time (shorter interval = more enemies)
            float adjustedInterval = spawnInterval / difficultyMultiplier;
            nextSpawnTime = Time.time + adjustedInterval;
        }

        private Data.EnemyData GetEnemyDataForCurrentTime()
        {
            if (enemyTypes.Count == 0) return null;

            float gameTime = GameManager.Instance.GameTime;

            // Filter enemies that can spawn at current game time
            List<Data.EnemyData> eligibleEnemies = new List<Data.EnemyData>();
            foreach (var enemy in enemyTypes)
            {
                if (gameTime >= enemy.minGameTimeSeconds)
                {
                    eligibleEnemies.Add(enemy);
                }
            }

            if (eligibleEnemies.Count == 0) return enemyTypes[0]; // Fallback to first enemy

            // Weighted random selection
            float totalWeight = 0f;
            foreach (var enemy in eligibleEnemies)
            {
                totalWeight += enemy.spawnWeight;
            }

            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            foreach (var enemy in eligibleEnemies)
            {
                currentWeight += enemy.spawnWeight;
                if (randomValue <= currentWeight)
                {
                    return enemy;
                }
            }

            return eligibleEnemies[0]; // Fallback
        }

        private Vector3 GetRandomSpawnPosition()
        {
            const int maxAttempts = 10; // Try up to 10 times to find valid position
            Vector3 candidatePosition;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                candidatePosition = CalculateSpawnPosition();

                // Check if this position is valid (not blocked by colliders)
                if (IsValidSpawnPosition(candidatePosition))
                {
                    return candidatePosition;
                }
            }

            // If all attempts failed, return last calculated position anyway
            // (Better to spawn in blocked area than not spawn at all)
            return CalculateSpawnPosition();
        }

        private Vector3 CalculateSpawnPosition()
        {
            // Get camera bounds
            float cameraHeight = mainCamera.orthographicSize * 2f;
            float cameraWidth = cameraHeight * mainCamera.aspect;

            Vector3 cameraPos = mainCamera.transform.position;

            // Pick random edge (0=top, 1=right, 2=bottom, 3=left)
            int edge = Random.Range(0, 4);

            float x = 0f;
            float y = 0f;

            switch (edge)
            {
                case 0: // Top
                    x = Random.Range(cameraPos.x - cameraWidth / 2f, cameraPos.x + cameraWidth / 2f);
                    y = cameraPos.y + cameraHeight / 2f + spawnDistanceFromCamera;
                    break;
                case 1: // Right
                    x = cameraPos.x + cameraWidth / 2f + spawnDistanceFromCamera;
                    y = Random.Range(cameraPos.y - cameraHeight / 2f, cameraPos.y + cameraHeight / 2f);
                    break;
                case 2: // Bottom
                    x = Random.Range(cameraPos.x - cameraWidth / 2f, cameraPos.x + cameraWidth / 2f);
                    y = cameraPos.y - cameraHeight / 2f - spawnDistanceFromCamera;
                    break;
                case 3: // Left
                    x = cameraPos.x - cameraWidth / 2f - spawnDistanceFromCamera;
                    y = Random.Range(cameraPos.y - cameraHeight / 2f, cameraPos.y + cameraHeight / 2f);
                    break;
            }

            return new Vector3(x, y, 0f);
        }

        private bool IsValidSpawnPosition(Vector3 position)
        {
            // Check for colliders at spawn position (e.g., walls blocking isolated areas)
            // Use a small circle to detect if position is blocked
            float checkRadius = 0.5f;

            // Check on Enemy layer and Default layer (where walls/obstacles usually are)
            int layerMask = LayerMask.GetMask("Default", "Ground");

            Collider2D hit = Physics2D.OverlapCircle(position, checkRadius, layerMask);

            // Position is valid if NO collider was found
            return hit == null;
        }

        private Sprite placeholderSprite;

        private void InitializePlaceholderSprite()
        {
            if (placeholderSprite != null) return;
            
            // Create a simple square sprite (placeholder until art ready)
            Texture2D tex = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();

            placeholderSprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
        }

        private void OnEnemyKilled(int totalKills)
        {
            currentEnemyCount--;
            if (currentEnemyCount < 0) currentEnemyCount = 0;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw spawn area in editor
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return;

            float cameraHeight = mainCamera.orthographicSize * 2f;
            float cameraWidth = cameraHeight * mainCamera.aspect;
            Vector3 cameraPos = mainCamera.transform.position;

            Gizmos.color = Color.yellow;

            // Draw spawn boundaries
            float outerWidth = cameraWidth + spawnDistanceFromCamera * 2f;
            float outerHeight = cameraHeight + spawnDistanceFromCamera * 2f;

            Gizmos.DrawWireCube(cameraPos, new Vector3(outerWidth, outerHeight, 0f));
        }
    }
}
