using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Area denial weapon that throws flasks creating damage puddles.
    /// Puddles deal damage over time to enemies standing in them.
    /// </summary>
    public class HolyWaterWeapon : WeaponBase
    {
        [Header("Flask Settings")]
        [SerializeField] private GameObject puddlePrefab;
        [SerializeField] private float throwDistance = 5f;

        // Upgrade-dependent values
        private float puddleDuration = 3f;
        private float puddleRadius = 1.5f;
        private float damageTickRate = 0.5f; // Seconds between damage ticks

        protected override void Fire()
        {
            if (puddlePrefab == null)
            {
                Debug.LogWarning("[AreaDenialWeapon] Puddle prefab not assigned!");
                return;
            }

            // Throw flask in semi-random direction
            Vector2 throwDirection = GetThrowDirection();
            Vector3 landingPosition = player.position + (Vector3)(throwDirection * throwDistance);

            // Instantiate puddle at landing position
            GameObject puddle = Instantiate(puddlePrefab, landingPosition, Quaternion.identity);

            // Initialize puddle with stats
            var puddleScript = puddle.GetComponent<DamagePuddle>();
            if (puddleScript != null)
            {
                float damagePerTick = damage; // Full damage per tick
                puddleScript.Initialize(puddleDuration, puddleRadius, damagePerTick, damageTickRate);
            }
        }

        private Vector2 GetThrowDirection()
        {
            // Find nearest enemy for targeting bias
            Transform nearestEnemy = FindNearestEnemy(15f);

            if (nearestEnemy != null)
            {
                // Throw towards enemy with some randomness
                Vector2 toEnemy = (nearestEnemy.position - player.position).normalized;
                float randomAngle = Random.Range(-30f, 30f);
                return Quaternion.Euler(0, 0, randomAngle) * toEnemy;
            }
            else
            {
                // Random direction if no enemies nearby
                float angle = Random.Range(0f, 360f);
                return new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            }
        }

        /// <summary>
        /// Override to enforce minimum cooldown = puddle duration.
        /// Prevents overlapping puddles from attack speed upgrades.
        /// </summary>
        public override void IncreaseAttackSpeed(float multiplier)
        {
            float oldCooldown = cooldown;
            base.IncreaseAttackSpeed(multiplier);
            
            // Enforce minimum: cooldown cannot be less than puddle duration
            if (cooldown < puddleDuration)
            {
                cooldown = puddleDuration;
                Debug.Log($"⚔️ [Holy Water] ATTACK SPEED: Capped at puddle duration ({puddleDuration}s) to prevent overlap");
            }
            else
            {
                // Normal upgrade log
                float reduction = oldCooldown - cooldown;
                Debug.Log($"⚔️ [{weaponData.displayName}] ATTACK SPEED: Cooldown {oldCooldown:F2}s → {cooldown:F2}s (-{reduction:F2}s, {((1 - multiplier) * 100):F0}% faster)");
            }
        }
    }
}
