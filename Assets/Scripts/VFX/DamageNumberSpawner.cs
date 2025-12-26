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
        [SerializeField] private int poolSize = 50;
        
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
            
            // Create pool parent
            poolParent = new GameObject("DamageNumber Pool").transform;
            poolParent.SetParent(canvas.transform);
            
            // Pre-instantiate pool
            for (int i = 0; i < poolSize; i++)
            {
                CreateNewPoolObject();
            }
            
            Debug.Log($"[DamageNumberSpawner] Pool created with {poolSize} objects");
        }
        
        /// <summary>
        /// Spawn a damage number at world position.
        /// </summary>
        public void SpawnDamageNumber(Vector3 worldPosition, float damage, DamageNumberType type)
        {
            Debug.Log($"[DamageNumberSpawner] SpawnDamageNumber called: {damage} at {worldPosition}, type: {type}");
            
            if (canvas == null)
            {
                Debug.LogError("[DamageNumberSpawner] Canvas not assigned!");
                return;
            }
            
            // Get from pool
            GameObject numberObj = GetFromPool();
            if (numberObj == null)
            {
                Debug.LogError("[DamageNumberSpawner] Failed to get object from pool!");
                return;
            }
            
            Debug.Log($"[DamageNumberSpawner] Got pooled object: {numberObj.name}");
            
            // CRITICAL: Get the correct camera for this canvas mode
            Camera renderCamera = null;
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                renderCamera = canvas.worldCamera;
                if (renderCamera == null)
                {
                    renderCamera = Camera.main;
                    Debug.LogWarning("[DamageNumberSpawner] Canvas worldCamera not assigned, using Camera.main");
                }
            }
            // For ScreenSpaceOverlay, camera should be null
            
            Debug.Log($"[DamageNumberSpawner] Canvas mode: {canvas.renderMode}, Using camera: {renderCamera?.name ?? "null (overlay)"}");
            
            // Convert world position to screen position
            Vector2 screenPos = renderCamera != null 
                ? renderCamera.WorldToScreenPoint(worldPosition) 
                : Camera.main.WorldToScreenPoint(worldPosition);
            
            Debug.Log($"[DamageNumberSpawner] World pos: {worldPosition}, Screen pos: {screenPos}");
            
            // Convert screen position to canvas local position
            RectTransform canvasRect = canvas.transform as RectTransform;
            bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos,
                renderCamera, // Use the same camera as canvas
                out Vector2 localPos
            );
            
            if (!success)
            {
                Debug.LogError("[DamageNumberSpawner] Failed to convert screen point to local point!");
                numberObj.SetActive(false);
                return;
            }
            
            Debug.Log($"[DamageNumberSpawner] Canvas local pos: {localPos}, Success: {success}");
            
            // CRITICAL FIX: Reparent to canvas BEFORE positioning to avoid transform issues
            RectTransform rectTransform = numberObj.GetComponent<RectTransform>();
            rectTransform.SetParent(canvas.transform, false); // worldPositionStays = false
            
            // Reset transform to ensure clean state
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;
            
            // Set position in canvas space
            rectTransform.anchoredPosition = localPos;
            
            Debug.Log($"[DamageNumberSpawner] Final anchored position: {rectTransform.anchoredPosition}");
            
            // Initialize with color based on type
            Color color = GetColorForType(type);
            DamageNumber damageNumber = numberObj.GetComponent<DamageNumber>();
            if (damageNumber != null)
            {
                damageNumber.Initialize(damage, color, worldPosition);
            }
            
            // Activate
            numberObj.SetActive(true);
        }
        
        private GameObject GetFromPool()
        {
            // Try to get inactive object from pool
            while (pool.Count > 0)
            {
                GameObject obj = pool.Dequeue();
                if (obj != null && !obj.activeInHierarchy)
                {
                    return obj;
                }
            }
            
            // Pool exhausted, create new one
            return CreateNewPoolObject();
        }
        
        private GameObject CreateNewPoolObject()
        {
            if (damageNumberPrefab == null)
            {
                Debug.LogError("[DamageNumberSpawner] Damage number prefab not assigned!");
                return null;
            }
            
            GameObject newObj = Instantiate(damageNumberPrefab, poolParent);
            newObj.SetActive(false);
            pool.Enqueue(newObj);
            return newObj;
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
