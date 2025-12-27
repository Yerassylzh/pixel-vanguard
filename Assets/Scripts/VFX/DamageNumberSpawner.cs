using UnityEngine;
using System.Collections.Generic;

namespace PixelVanguard.VFX
{
    /// <summary>
    /// Singleton spawner for damage numbers with object pooling.
    /// Call SpawnDamageNumber() to create floating text at world position.
    /// </summary>
    public class DamageNumberSpawner : MonoBehaviour
    {
        public static DamageNumberSpawner Instance { get; private set; }
        
        [Header("References")]
        [Tooltip("Prefab with DamageNumber component")]
        [SerializeField] private GameObject damageNumberPrefab;
        
        [Tooltip("UI Canvas to parent damage numbers to")]
        [SerializeField] private Canvas canvas;
        
        [Header("Pool Settings")]
        [Tooltip("Initial pool size")]
        [SerializeField] private int poolSize = 300; // Pre-instantiate 300, never create more
        
        [Header("Color Settings")]
        [SerializeField] private Color normalDamageColor = Color.white;
        [SerializeField] private Color criticalDamageColor = Color.yellow;
        [SerializeField] private Color playerDamageColor = new Color(1f, 0.3f, 0.3f); // Light red
        [SerializeField] private Color healingColor = Color.green;
        
        // Pool
        private Queue<GameObject> pool = new Queue<GameObject>();
        private Transform poolParent;
        
        // Public accessor for pool parent (so DamageNumber can return correctly)
        public Transform PoolParent => poolParent;
        
        private void Awake()
        {
            // Singleton
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[DamageNumberSpawner] Duplicate spawner destroyed");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Debug.Log($"[DamageNumberSpawner] Singleton initialized, Canvas: {canvas != null}, Prefab: {damageNumberPrefab != null}");
            
            // Validate
            if (canvas == null)
            {
                Debug.LogError("[DamageNumberSpawner] Canvas not assigned in Inspector!");
                return;
            }
            
            if (damageNumberPrefab == null)
            {
                Debug.LogError("[DamageNumberSpawner] Damage number prefab not assigned in Inspector!");
                return;
            }
            
            // Create pool parent as UI element (RectTransform)
            GameObject poolGO = new GameObject("DamageNumber Pool");
            poolParent = poolGO.AddComponent<RectTransform>();
            poolParent.SetParent(canvas.transform, false);
            
            // Set poolParent to fill canvas (so local coords match canvas coords)
            RectTransform poolRect = poolParent as RectTransform;
            poolRect.anchorMin = Vector2.zero;
            poolRect.anchorMax = Vector2.one;
            poolRect.offsetMin = Vector2.zero;
            poolRect.offsetMax = Vector2.zero;
            
            // Pre-instantiate ALL pool objects (never create more at runtime)
            Debug.Log($"[DamageNumberSpawner] Pre-instantiating {poolSize} damage numbers...");
            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(damageNumberPrefab, poolParent);
                obj.name = $"DamageNumber_{i:000}";
                obj.SetActive(false);
                pool.Enqueue(obj);
            }
            
            Debug.Log($"[DamageNumberSpawner] Pool created with {poolSize} objects. NO MORE WILL BE CREATED.");
        }
        
        /// <summary>
        /// Spawn a damage number at world position.
        /// Uses pooled objects - NO instantiation at runtime!
        /// </summary>
        public void SpawnDamageNumber(Vector3 worldPosition, float damage, DamageNumberType type)
        {
            if (canvas == null || damageNumberPrefab == null) return;
            
            // Get from pool (already parented to poolParent, don't reparent!)
            GameObject numberObj = GetFromPool();
            if (numberObj == null)
            {
                Debug.LogError("[DamageNumberSpawner] Pool exhausted and recycling failed!");
                return;
            }
            
            // Get camera for coordinate conversion
            Camera renderCamera = null;
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                renderCamera = canvas.worldCamera ?? Camera.main;
            }
            
            // Convert world → screen → poolParent local (NOT canvas!)
            Vector2 screenPos = renderCamera != null 
                ? renderCamera.WorldToScreenPoint(worldPosition) 
                : Camera.main.WorldToScreenPoint(worldPosition);
            
            // CRITICAL: Convert to poolParent space, since objects are parented there
            RectTransform poolRect = poolParent as RectTransform;
            if (poolRect == null)
            {
                Debug.LogError("[DamageNumberSpawner] poolParent is not a RectTransform!");
                ReturnToPool(numberObj);
                return;
            }
            
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                poolRect, screenPos, renderCamera, out Vector2 localPos))
            {
                Debug.LogWarning("[DamageNumberSpawner] Failed to convert position, skipping");
                ReturnToPool(numberObj);
                return;
            }
            
            // Position the number (NO REPARENTING - stays in poolParent)
            RectTransform rectTransform = numberObj.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = localPos;
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;
            
            // Initialize
            Color color = GetColorForType(type);
            DamageNumber damageNumber = numberObj.GetComponent<DamageNumber>();
            if (damageNumber != null)
            {
                damageNumber.Initialize(damage, color, worldPosition);
            }
            
            // Activate (object is already parented to poolParent, never moves)
            numberObj.SetActive(true);
        }
        
        private GameObject GetFromPool()
        {
            // Try to get inactive object from pool
            if (pool.Count > 0)
            {
                GameObject obj = pool.Dequeue();
                if (obj != null)
                {
                    return obj;
                }
            }
            
            // Pool exhausted - forcibly reuse oldest active number
            Debug.LogWarning("[DamageNumberSpawner] Pool exhausted! Force-recycling active damage number.");
            
            // Find any active damage number in canvas and recycle it
            DamageNumber[] activeNumbers = canvas.GetComponentsInChildren<DamageNumber>();
            if (activeNumbers.Length > 0)
            {
                GameObject recycled = activeNumbers[0].gameObject;
                recycled.SetActive(false);
                ReturnToPool(recycled);
                
                // Now get it from pool
                if (pool.Count > 0)
                {
                    return pool.Dequeue();
                }
            }
            
            Debug.LogError("[DamageNumberSpawner] CRITICAL: No objects in pool to recycle!");
            return null;
        }
        
        /// <summary>
        /// Return object to pool. Objects never leave poolParent, just get disabled.
        /// </summary>
        public void ReturnToPool(GameObject obj)
        {
            if (obj == null) return;
            
            // Just disable - object is already in poolParent
            obj.SetActive(false);
            
            // Add back to pool queue
            if (!pool.Contains(obj))
            {
                pool.Enqueue(obj);
            }
        }
        
        private Color GetColorForType(DamageNumberType type)
        {
            switch (type)
            {
                case DamageNumberType.Normal:
                    return normalDamageColor;
                case DamageNumberType.Critical:
                    return criticalDamageColor;
                case DamageNumberType.PlayerDamage:
                    return playerDamageColor;
                case DamageNumberType.Healing:
                    return healingColor;
                default:
                    return Color.white;
            }
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
    
    /// <summary>
    /// Type of damage number to display.
    /// </summary>
    public enum DamageNumberType
    {
        Normal,        // White - standard enemy damage
        Critical,      // Yellow - critical hits (future)
        PlayerDamage,  // Red - damage to player
        Healing        // Green - healing
    }
}
