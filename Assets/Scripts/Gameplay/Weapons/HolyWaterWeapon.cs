using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Area Denial weapon (Holy Water / Sanctified Ground).
    /// Spawns Damage zones at random offsets.
    /// </summary>
    public class HolyWaterWeapon : WeaponBase
    {
        [Header("Puddle Setup")]
        [SerializeField] private GameObject puddlePrefab;
        [SerializeField] private float spawnRadius = 3.5f; // Max distance from player
        
        [Header("Weapon Stats")]
        // Stats are now loaded from WeaponData into base class fields: duration, tickRate

        // Upgrade capability
        private int zoneCount = 1;

        protected override void Fire()
        {
            if (puddlePrefab == null)
            {
                Debug.LogWarning("[HolyWaterWeapon] Puddle Prefab is missing!");
                return;
            }

            for (int i = 0; i < zoneCount; i++)
            {
                SpawnPuddle();
            }
        }

        private void SpawnPuddle()
        {
            // Find random valid position
            // TODO: In future, check for obstacles so we don't spawn inside a wall
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = player.position + (Vector3)randomOffset;

            GameObject puddleObj = Instantiate(puddlePrefab, spawnPos, Quaternion.identity);
            
            // Connect logic
            DamagePuddle puddleScript = puddleObj.GetComponent<DamagePuddle>();
            if (puddleScript != null)
            {
                puddleScript.Initialize(duration, GetFinalDamage(), tickRate);
            }
        }

        // Optional: Custom upgrades for Duration/Area
        public void UpgradeDuration(float percent)
        {
            duration *= (1f + percent);
        }
    }
}
