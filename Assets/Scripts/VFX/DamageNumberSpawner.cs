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
        
        private void Awake()
        {
            // Singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            // Create pool parent
            poolParent = new GameObject("DamageNumber Pool").transform;
            poolParent.SetParent(canvas.transform);
            
            // Pre-instantiate pool
            for (int i = 0; i < poolSize; i++)
            {
                CreateNewPoolObject();
            }
        }
        
        /// <summary>
        /// Spawn a damage number at world position.
        /// </summary>
        public void SpawnDamageNumber(Vector3 worldPosition, float damage, DamageNumberType type)
        {
            if (canvas == null)
            {
                Debug.LogError("[DamageNumberSpawner] Canvas not assigned!");
                return;
            }
            
            // Get from pool
            GameObject numberObj = GetFromPool();
            if (numberObj == null) return;
            
            // Convert world position to canvas space
            Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPos,
                canvas.worldCamera,
                out Vector2 localPos
            );
            
            // Position the number
            RectTransform rectTransform = numberObj.GetComponent<RectTransform>();
            rectTransform.localPosition = localPos;
            
            // Initialize with color based on type
            Color color = GetColorForType(type);
            DamageNumber damageNumber = numberObj.GetComponent<DamageNumber>();
            if (damageNumber != null)
            {
                damageNumber.Initialize(damage, color);
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
