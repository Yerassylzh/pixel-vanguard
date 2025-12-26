using UnityEngine;
using System.Collections;
using PixelVanguard.Interfaces;

namespace PixelVanguard.VFX
{
    /// <summary>
    /// Handles sprite flash effect when entity takes damage.
    /// Attach to any GameObject with SpriteRenderer that should flash on hit.
    /// Automatically subscribes to IDamageable.OnDamaged event.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class DamageFlash : MonoBehaviour
    {
        [Header("Flash Settings")]
        [Tooltip("Duration of flash in seconds")]
        [SerializeField] private float flashDuration = 0.1f;
        
        [Tooltip("Color to flash (usually white or red)")]
        [SerializeField] private Color flashColor = Color.white;
        
        // Components
        private SpriteRenderer spriteRenderer;
        private IDamageable damageable;
        
        // State
        private Color originalColor;
        private Coroutine flashCoroutine;
        
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            originalColor = spriteRenderer.color;
            
            // Find IDamageable on this GameObject
            damageable = GetComponent<IDamageable>();
            if (damageable == null)
            {
                Debug.LogWarning($"[DamageFlash] No IDamageable component found on {gameObject.name}!");
            }
        }
        
        private void OnEnable()
        {
            if (damageable != null)
            {
                damageable.OnDamaged += HandleDamage;
                Debug.Log($"[DamageFlash] Subscribed to OnDamaged on {gameObject.name}");
            }
        }
        
        private void OnDisable()
        {
            if (damageable != null)
            {
                damageable.OnDamaged -= HandleDamage;
            }
        }
        
        private void HandleDamage(float damage, Vector3 position)
        {
            Flash();
        }
        
        /// <summary>
        /// Trigger a flash effect.
        /// </summary>
        public void Flash()
        {
            if (spriteRenderer == null) return;
            
            // Stop existing flash if running
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            
            flashCoroutine = StartCoroutine(FlashCoroutine());
        }
        
        private IEnumerator FlashCoroutine()
        {
            // Set to flash color
            spriteRenderer.color = flashColor;
            
            // Wait for duration
            yield return new WaitForSeconds(flashDuration);
            
            // Fade back to original color
            float elapsed = 0f;
            float fadeDuration = flashDuration * 0.5f; // Fade in half the time
            
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                spriteRenderer.color = Color.Lerp(flashColor, originalColor, t);
                yield return null;
            }
            
            // Ensure we end at original color
            spriteRenderer.color = originalColor;
            flashCoroutine = null;
        }
        
        private void OnDestroy()
        {
            // Clean up coroutine
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
        }
    }
}
