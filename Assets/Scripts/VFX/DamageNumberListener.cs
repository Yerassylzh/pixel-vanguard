using UnityEngine;
using PixelVanguard.Interfaces;
using PixelVanguard.VFX;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Subscribes to IDamageable events and spawns damage numbers.
    /// Attach to any GameObject with IDamageable component.
    /// </summary>
    public class DamageNumberListener : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Type of damage number to spawn")]
        [SerializeField] private DamageNumberType damageType = DamageNumberType.Normal;
        
        [Tooltip("Type of healing number to spawn")]
        [SerializeField] private DamageNumberType healType = DamageNumberType.Healing;
        
        private IDamageable damageable;
        
        private void Awake()
        {
            damageable = GetComponent<IDamageable>();
            if (damageable == null)
            {
                Debug.LogError($"[DamageNumberListener] No IDamageable component found on {gameObject.name}!");
            }
            else
            {
                Debug.Log($"[DamageNumberListener] Found IDamageable on {gameObject.name}");
            }
        }
        
        private void OnEnable()
        {
            if (damageable != null)
            {
                damageable.OnDamaged += HandleDamage;
                damageable.OnHealed += HandleHeal;
                Debug.Log($"[DamageNumberListener] Subscribed to events on {gameObject.name}");
            }
        }
        
        private void OnDisable()
        {
            if (damageable != null)
            {
                damageable.OnDamaged -= HandleDamage;
                damageable.OnHealed -= HandleHeal;
            }
        }
        
        private void HandleDamage(float damage, Vector3 position)
        {
            Debug.Log($"[DamageNumberListener] HandleDamage called: {damage} at {position}, Spawner exists: {DamageNumberSpawner.Instance != null}");
            if (DamageNumberSpawner.Instance != null)
            {
                DamageNumberSpawner.Instance.SpawnDamageNumber(position, damage, damageType);
            }
            else
            {
                Debug.LogError("[DamageNumberListener] DamageNumberSpawner.Instance is null!");
            }
        }
        
        private void HandleHeal(float amount, Vector3 position)
        {
            if (DamageNumberSpawner.Instance != null)
            {
                DamageNumberSpawner.Instance.SpawnDamageNumber(position, amount, healType);
            }
        }
    }
}
