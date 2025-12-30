using UnityEngine;
using System.Collections.Generic;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// AutoCrossbow weapon - Fires arrows at enemies.
    /// REFACTORED: Uses TargetingUtility, cleaner multi-shot logic.
    /// </summary>
    public class AutoCrossbowWeapon : WeaponBase
    {
        [Header("Crossbow Settings")]
        [SerializeField] private GameObject arrowPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float projectileSpeed = 15f;
        [SerializeField] private float targetRange = 15f;

        [Header("Upgrades")]
        private int arrowCount = 1; // Dual = 2, Triple = 3
        private int pierceCount = 0; // Piercing Bolts upgrade

        protected override void Awake()
        {
            base.Awake();
            if (firePoint == null) {
                firePoint = transform;
                firePoint.localPosition = Vector3.zero;
            }
        }

        protected override void Fire()
        {
            if (arrowPrefab == null || firePoint == null)
            {
                Debug.LogWarning("[AutoCrossbow] Missing arrow prefab or fire point!");
                return;
            }

            // Find targets using centralized utility
            var targets = TargetingUtility.FindUniqueTargets(firePoint.position, arrowCount, targetRange);

            if (targets.Count == 0)
            {
                // No enemies in range - don't play sound, just return
                return;
            }

            // Fire arrow at each target
            foreach (var target in targets)
            {
                FireArrowAt(target);
            }
            
            // Only play sound if we actually fired
            TriggerWeaponFiredEvent();
        }

        private void FireArrowAt(Transform target)
        {
            // Calculate direction
            Vector2 direction = (target.position - firePoint.position).normalized;

            // Spawn arrow
            GameObject arrowObj = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);
            
            // Rotate arrow to face direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            arrowObj.transform.rotation = Quaternion.Euler(0, 0, angle);

            // Initialize arrow
            var arrowScript = arrowObj.GetComponent<ArrowProjectile>();
            if (arrowScript != null)
            {
                float finalDamage = GetFinalDamage();
                arrowScript.Initialize(direction, projectileSpeed, finalDamage, knockback, pierceCount);
            }
        }

        // === UPGRADE API ===
        
        /// <summary>
        /// Set number of arrows (Dual = 2, Triple = 3).
        /// </summary>
        public void SetMultiShot(int count)
        {
            arrowCount = count;
            Debug.Log($"[AutoCrossbow] Multi-shot set to {count} arrows");
        }

        /// <summary>
        /// Increment pierce count (Piercing Bolts upgrade).
        /// </summary>
        public void IncrementPierce()
        {
            pierceCount++;
            Debug.Log($"[AutoCrossbow] Pierce count: {pierceCount}");
        }
    }
}
