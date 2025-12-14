using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Projectile weapon that fires arrows at the nearest enemy.
    /// Supports multi-shot (level 2+) and pierce (level 3+).
    /// </summary>
    public class AutoCrossbowWeapon : WeaponBase
    {
        [Header("Projectile Settings")]
        [SerializeField] private GameObject arrowPrefab;
        [SerializeField] private float projectileSpeed = 10f;
        [SerializeField] private float maxRange = 15f;

        // Upgrade-dependent values
        private int arrowCount = 1; // Number of arrows per shot
        private int pierceCount = 0; // How many enemies arrow can pierce

        protected override void Fire()
        {
            // Find nearest enemy
            Transform target = FindNearestEnemy(maxRange);
            if (target == null) return;

            // Fire multiple arrows based on level
            FireArrows(target);
        }

        private void FireArrows(Transform target)
        {
            if (arrowPrefab == null)
            {
                Debug.LogWarning("[ProjectileWeapon] Arrow prefab not assigned!");
                return;
            }

            Vector2 baseDirection = (target.position - player.position).normalized;

            // Fire arrows in a spread pattern if multi-shot
            for (int i = 0; i < arrowCount; i++)
            {
                // Calculate spread angle
                float spreadAngle = 0f;
                if (arrowCount > 1)
                {
                    float spreadRange = 15f; // Degrees
                    spreadAngle = Mathf.Lerp(-spreadRange, spreadRange, i / (float)(arrowCount - 1));
                }

                // Rotate direction by spread angle
                Vector2 direction = Quaternion.Euler(0, 0, spreadAngle) * baseDirection;

                // Instantiate arrow
                GameObject arrow = Instantiate(arrowPrefab, player.position, Quaternion.identity);

                // Set arrow rotation to face direction
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                arrow.transform.rotation = Quaternion.Euler(0, 0, angle);

                // Initialize arrow with stats
                var arrowScript = arrow.GetComponent<ArrowProjectile>();
                if (arrowScript != null)
                {
                    arrowScript.Initialize(direction, projectileSpeed, damage, knockback, pierceCount);
                }
            }
        }

    }
}
