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
        [SerializeField] private int maxEnemies = 123; // Max enemies on screen (Vampire Survivors style)

        [Header("Difficulty Scaling")]
        [SerializeField] private float difficultyIncreaseRate = 0.1f; // Spawn rate increase per minute
        [SerializeField] private float maxSpawnRate = 3f; // Max spawn rate multiplier (capped for balance)

        [Header("Spawn Area")]
        [SerializeField] private float spawnDistanceFromCamera = 12f; // Units from camera edge
        
        [Header("Spawn Bounds (Optional)")]
        [Tooltip("Top-left corner of allowed spawn rectangle (leave empty for no bounds)")]
        [SerializeField] private Transform spawnBoundsTopLeft;
        [Tooltip("Bottom-right corner of allowed spawn rectangle (leave empty for no bounds)")]
        [SerializeField] private Transform spawnBoundsBottomRight;
        [Tooltip("Show spawn bounds in Scene view")]
        [SerializeField] private bool showBoundsGizmo = true;
        
        [Header("Collision Detection")]
        [Tooltip("Layers to check for blocking spawn (e.g., Ground, Water, Obstacles)")]
        [SerializeField] private LayerMask blockedLayers = -1; // Default: check all layers

        private Camera mainCamera;
        private float nextSpawnTime = 0f;
        private int currentEnemyCount = 0;
        
        // FIFO queue for enemy removal (Vampire Survivors style)
        private Queue<GameObject> spawnedEnemies = new Queue<GameObject>();

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
                SpawnEnemyWave();
                ScheduleNextSpawn();
            }
        }

        /// <summary>
        /// Spawn multiple enemies distributed across edges (Vampire Survivors style).
        /// Spawns 2-4 enemies per wave, one per edge to prevent clumping.
        /// </summary>
        private void SpawnEnemyWave()
        {
            float gameTime = GameManager.Instance != null ? GameManager.Instance.GameTime : Time.time;
            float minutesElapsed = gameTime / 60f;
            
            // Start with 2 enemies, add 1 every 2 minutes, cap at 4
            int enemiesToSpawn = 2 + Mathf.FloorToInt(minutesElapsed / 2f);
            enemiesToSpawn = Mathf.Clamp(enemiesToSpawn, 2, 4);
            
            // Spawn enemies distributed across different edges (prevents clumping)
            for (int i = 0; i < enemiesToSpawn; i++)
            {
                int forcedEdge = i % 4; // 0=top, 1=right, 2=bottom, 3=left
                SpawnSingleEnemy(forcedEdge);
            }
        }

        private void SpawnSingleEnemy(int forcedEdge = -1)
        {
            // FIFO: If at cap, destroy oldest enemy before spawning new
            if (currentEnemyCount >= maxEnemies)
            {
                RemoveOldestEnemy();
            }
            
            // Get enemy data based on spawn eligibility
            var enemyData = GetEnemyDataForCurrentTime();
            if (enemyData == null)
            {
                Debug.LogWarning("[EnemySpawner] No eligible enemy types found!");
                return;
            }

            // Calculate spawn position at screen edge (use forced edge if specified)
            Vector3 spawnPosition = forcedEdge >= 0 
                ? GetSpawnPositionAtEdge(forcedEdge) 
                : GetRandomSpawnPosition();

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
            
            // Track in FIFO queue
            spawnedEnemies.Enqueue(enemyObj);
            currentEnemyCount++;
        }
        
        /// <summary>
        /// Remove oldest spawned enemy (FIFO queue)
        /// </summary>
        private void RemoveOldestEnemy()
        {
            while (spawnedEnemies.Count > 0)
            {
                GameObject oldest = spawnedEnemies.Dequeue();
                
                if (oldest != null)
                {
                    Destroy(oldest);
                    currentEnemyCount--;
                    return;
                }
            }
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

        /// <summary>
        /// Get spawn position at specific edge (for distributed spawning)
        /// </summary>
        private Vector3 GetSpawnPositionAtEdge(int edge)
        {
            const int maxAttempts = 10;
            
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Vector3 pos = CalculateSpawnPositionAtEdge(edge);
                
                if (IsValidSpawnPosition(pos))
                {
                    return pos;
                }
            }
            
            // Fallback: return position anyway
            return CalculateSpawnPositionAtEdge(edge);
        }
        
        private Vector3 CalculateSpawnPositionAtEdge(int edge)
        {
            float cameraHeight = mainCamera.orthographicSize * 2f;
            float cameraWidth = cameraHeight * mainCamera.aspect;
            Vector3 cameraPos = mainCamera.transform.position;
            
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
            
            Vector3 position = new Vector3(x, y, 0f);
            
            // CRITICAL FIX: Clamp position to bounds if defined
            position = ClampToBounds(position);
            
            return position;
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

            Vector3 position = new Vector3(x, y, 0f);
            
            // CRITICAL FIX: Clamp position to bounds if defined
            position = ClampToBounds(position);
            
            return position;
        }

        private bool IsValidSpawnPosition(Vector3 position)
        {
            // Position is already clamped to bounds in CalculateSpawnPosition()
            // Now just check for blocking colliders
            
            float checkRadius = 0.5f;

            // Use configurable layer mask (set in Inspector)
            Collider2D hit = Physics2D.OverlapCircle(position, checkRadius, blockedLayers);

            // Position is valid if NO collider was found
            return hit == null;
        }
        
        /// <summary>
        /// Clamp position to spawn bounds rectangle if bounds are defined.
        /// Returns original position if no bounds are set.
        /// </summary>
        private Vector3 ClampToBounds(Vector3 position)
        {
            // If no bounds defined, return original position
            if (spawnBoundsTopLeft == null || spawnBoundsBottomRight == null)
            {
                return position;
            }
            
            // Get bounds corners
            Vector2 topLeft = spawnBoundsTopLeft.position;
            Vector2 bottomRight = spawnBoundsBottomRight.position;
            
            // Ensure corners are in correct order
            float minX = Mathf.Min(topLeft.x, bottomRight.x);
            float maxX = Mathf.Max(topLeft.x, bottomRight.x);
            float minY = Mathf.Min(topLeft.y, bottomRight.y);
            float maxY = Mathf.Max(topLeft.y, bottomRight.y);
            
            // Clamp position to rectangle
            float clampedX = Mathf.Clamp(position.x, minX, maxX);
            float clampedY = Mathf.Clamp(position.y, minY, maxY);
            
            return new Vector3(clampedX, clampedY, position.z);
        }
        
        /// <summary>
        /// Check if position is within the defined rectangular spawn bounds.
        /// Returns true if no bounds are defined (allows spawning anywhere).
        /// </summary>
        private bool IsWithinSpawnBounds(Vector3 position)
        {
            // If no bounds defined, allow all positions
            if (spawnBoundsTopLeft == null || spawnBoundsBottomRight == null)
            {
                return true;
            }
            
            // Get bounds corners
            Vector2 topLeft = spawnBoundsTopLeft.position;
            Vector2 bottomRight = spawnBoundsBottomRight.position;
            
            // Ensure corners are in correct order (swap if needed)
            float minX = Mathf.Min(topLeft.x, bottomRight.x);
            float maxX = Mathf.Max(topLeft.x, bottomRight.x);
            float minY = Mathf.Min(topLeft.y, bottomRight.y);
            float maxY = Mathf.Max(topLeft.y, bottomRight.y);
            
            // Check if position is within rectangle
            bool withinBounds = position.x >= minX && position.x <= maxX && 
                               position.y >= minY && position.y <= maxY;
            
            return withinBounds;
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

            // Draw spawn boundaries (camera-based)
            float outerWidth = cameraWidth + spawnDistanceFromCamera * 2f;
            float outerHeight = cameraHeight + spawnDistanceFromCamera * 2f;

            Gizmos.DrawWireCube(cameraPos, new Vector3(outerWidth, outerHeight, 0f));
            
            // Draw rectangular spawn bounds if defined
            if (showBoundsGizmo && spawnBoundsTopLeft != null && spawnBoundsBottomRight != null)
            {
                Vector2 topLeft = spawnBoundsTopLeft.position;
                Vector2 bottomRight = spawnBoundsBottomRight.position;
                
                // Calculate center and size
                float minX = Mathf.Min(topLeft.x, bottomRight.x);
                float maxX = Mathf.Max(topLeft.x, bottomRight.x);
                float minY = Mathf.Min(topLeft.y, bottomRight.y);
                float maxY = Mathf.Max(topLeft.y, bottomRight.y);
                
                Vector3 center = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, 0f);
                Vector3 size = new Vector3(maxX - minX, maxY - minY, 0f);
                
                // Draw in green to distinguish from camera bounds
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(center, size);
                
                // Draw corners as small spheres
                Gizmos.DrawWireSphere(new Vector3(minX, maxY, 0f), 0.5f); // Top-left
                Gizmos.DrawWireSphere(new Vector3(maxX, minY, 0f), 0.5f); // Bottom-right
            }
        }
    }
}
