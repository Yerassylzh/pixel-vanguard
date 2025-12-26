using UnityEngine;
using TMPro;

namespace PixelVanguard.VFX
{
    /// <summary>
    /// Individual floating damage number behavior.
    /// Floats upward, fades out, then returns to pool.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
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
        
        // Components
        private TextMeshProUGUI textMesh;
        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;
        
        // State
        private float timer;
        private Vector3 velocity;
        
        private void Awake()
        {
            textMesh = GetComponent<TextMeshProUGUI>();
            canvasGroup = GetComponent<CanvasGroup>();
            rectTransform = GetComponent<RectTransform>();
        }
        
        /// <summary>
        /// Initialize and start the floating animation.
        /// </summary>
        public void Initialize(float damage, Color color)
        {
            // Set text
            textMesh.text = Mathf.RoundToInt(damage).ToString();
            textMesh.color = color;
            
            // Set random horizontal offset
            float offsetX = Random.Range(-randomOffsetX, randomOffsetX);
            velocity = new Vector3(offsetX, floatSpeed, 0f);
            
            // Reset state
            timer = 0f;
            canvasGroup.alpha = 1f;
            
            // Scale based on damage (larger numbers for bigger damage)
            float scale = 1f + (damage / 100f); // +1% per 1 damage
            scale = Mathf.Clamp(scale, 0.8f, 2f); // Clamp between 0.8x and 2x
            rectTransform.localScale = Vector3.one * scale;
        }
        
        private void Update()
        {
            timer += Time.deltaTime;
            
            // Float upward
            rectTransform.localPosition += velocity * Time.deltaTime;
            
            // Fade out in last 30% of lifetime
            float fadeStartTime = lifetime * 0.7f;
            if (timer >= fadeStartTime)
            {
                float fadeProgress = (timer - fadeStartTime) / (lifetime - fadeStartTime);
                canvasGroup.alpha = 1f - fadeProgress;
            }
            
            // Return to pool when lifetime expires
            if (timer >= lifetime)
            {
                ReturnToPool();
            }
        }
        
        private void ReturnToPool()
        {
            // Disable and notify spawner to reclaim
            gameObject.SetActive(false);
        }
    }
}
