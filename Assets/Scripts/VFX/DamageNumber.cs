using UnityEngine;
using TMPro;

namespace PixelVanguard.VFX
{
    /// <summary>
    /// Individual floating damage number behavior.
    /// Floats upward, fades in then out, then returns to pool.
    /// Supports multiple TextMeshPro components for shadow/outline effects.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class DamageNumber : MonoBehaviour
    {
        [Header("Animation Settings")]
        [Tooltip("How fast the number floats upward")]
        [SerializeField] private float floatSpeed = 2f;
        
        [Tooltip("Duration before fading out")]
        [SerializeField] private float lifetime = 1.2f;
        
        [Tooltip("Random horizontal offset range")]
        [SerializeField] private float randomOffsetX = 0.3f;
        
        [Tooltip("Vertical spawn offset (higher = spawns above target)")]
        [SerializeField] private float spawnOffsetY = 0.5f;
        
        [Tooltip("Fade-in duration at start")]
        [SerializeField] private float fadeInDuration = 0.15f;
        
        [Header("Text References")]
        [Tooltip("Primary text to color (drag the main text here, not shadow)")]
        [SerializeField] private TextMeshProUGUI primaryText;
        
        // Components - supports multiple text components for shadow/outline
        private TextMeshProUGUI[] textMeshes;
        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;
        
        // World tracking
        private Vector3 worldPosition;
        private Canvas canvas;
        
        // State
        private float timer;
        private Vector3 velocity;
        
        private void Awake()
        {
            // Get ALL TextMeshPro components (for shadow/outline support)
            textMeshes = GetComponentsInChildren<TextMeshProUGUI>();
            canvasGroup = GetComponent<CanvasGroup>();
            rectTransform = GetComponent<RectTransform>();
            
            if (textMeshes.Length == 0)
            {
                Debug.LogError("[DamageNumber] No TextMeshProUGUI components found!");
            }
            
            // Find canvas (will be set when spawned)
            canvas = GetComponentInParent<Canvas>();
        }
        
        /// <summary>
        /// Initialize and start the floating animation.
        /// </summary>
        public void Initialize(float damage, Color color, Vector3 startWorldPosition)
        {
            // Set text on ALL text components
            string damageText = Mathf.RoundToInt(damage).ToString();
            foreach (var textMesh in textMeshes)
            {
                if (textMesh != null)
                {
                    textMesh.text = damageText;
                    
                    // Color the primary text, keep others (shadows) black
                    if (textMesh == primaryText)
                    {
                        textMesh.color = color;
                    }
                    else
                    {
                        textMesh.color = Color.black; // Shadow is always black
                    }
                }
            }
            
            // Store world position with upward offset
            worldPosition = startWorldPosition + Vector3.up * spawnOffsetY;
            
            // Set random horizontal offset
            float offsetX = Random.Range(-randomOffsetX, randomOffsetX);
            velocity = new Vector3(offsetX, floatSpeed, 0f);
            
            // Reset state
            timer = 0f;
            canvasGroup.alpha = 0f; // Start invisible for fade-in
            
            // Scale based on damage (larger numbers for bigger damage)
            float scale = 1f + (damage / 100f); // +1% per 1 damage
            scale = Mathf.Clamp(scale, 0.8f, 2f); // Clamp between 0.8x and 2x
            rectTransform.localScale = Vector3.one * scale;
        }
        
        private void Update()
        {
            timer += Time.deltaTime;
            
            // Update world position to float upward
            worldPosition += velocity * Time.deltaTime;
            
            // Convert world position to canvas position every frame (tracks camera movement)
            UpdateCanvasPosition();
            
            // Fade-in at start
            if (timer < fadeInDuration)
            {
                canvasGroup.alpha = timer / fadeInDuration;
            }
            // Fade out in last 30% of lifetime
            else
            {
                float fadeStartTime = lifetime * 0.7f;
                if (timer >= fadeStartTime)
                {
                    float fadeProgress = (timer - fadeStartTime) / (lifetime - fadeStartTime);
                    canvasGroup.alpha = 1f - fadeProgress;
                }
                else
                {
                    canvasGroup.alpha = 1f; // Fully visible during middle phase
                }
            }
            
            // Return to pool when lifetime expires
            if (timer >= lifetime)
            {
                ReturnToPool();
            }
        }
        
        private void UpdateCanvasPosition()
        {
            if (canvas == null || Camera.main == null) return;
            
            // Get render camera based on canvas mode
            Camera renderCamera = null;
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                renderCamera = canvas.worldCamera ?? Camera.main;
            }
            
            // Convert world to screen
            Vector2 screenPos = renderCamera != null 
                ? renderCamera.WorldToScreenPoint(worldPosition)
                : Camera.main.WorldToScreenPoint(worldPosition);
            
            // Convert screen to local space of the PARENT (poolParent)
            // Using canvas directly causes issues if poolParent has different scaling/offsets
            RectTransform parentRect = transform.parent as RectTransform;
            if (parentRect == null) parentRect = canvas.transform as RectTransform; // Fallback

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                screenPos,
                renderCamera,
                out Vector2 localPos))
            {
                // Use anchoredPosition3D to enforce Z=0 (fixes visibility issues in ScreenSpaceCamera)
                rectTransform.anchoredPosition3D = new Vector3(localPos.x, localPos.y, 0f);
            }
        }
        
        private void ReturnToPool()
        {
            // Return to spawner's pool
            if (DamageNumberSpawner.Instance != null)
            {
                DamageNumberSpawner.Instance.ReturnToPool(gameObject);
            }
            else
            {
                // Fallback: just disable
                gameObject.SetActive(false);
            }
        }
    }
}
