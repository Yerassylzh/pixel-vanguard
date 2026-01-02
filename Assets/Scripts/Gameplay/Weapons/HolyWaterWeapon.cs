using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Holy Water weapon - Area denial with damage puddles.
    /// </summary>
    public class HolyWaterWeapon : WeaponBase
    {
        [Header("Puddle Setup")]
        [SerializeField] private GameObject puddlePrefab;
        [SerializeField] private float spawnRadius = 3.5f;
        
        [Header("Holy Water Stats")]
        [SerializeField] private float puddleDuration = 5f;
        [SerializeField] private float damageTickRate = 0.5f;

        // Upgrade tracking
        private float puddleRadiusMultiplier = 1.0f;
        private float hpScalingPercent = 0f;

        protected override void Fire()
        {
            if (puddlePrefab == null)
            {
                Debug.LogWarning("[HolyWaterWeapon] Puddle Prefab is missing!");
                return;
            }

            SpawnPuddle();
            
            // Fire audio event (holy water thrown)
            // Note: Uses GameEvents instead of TriggerWeaponFiredEvent because AudioManager listens to this
            Core.GameEvents.TriggerWeaponSpawned();
        }

        private void SpawnPuddle()
        {
            // Random position around player
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = player.position + (Vector3)randomOffset;

            GameObject puddleObj = Instantiate(puddlePrefab, spawnPos, Quaternion.identity);
            
            // Fix: Apply radius multiplier to scale
            puddleObj.transform.localScale = Vector3.one * puddleRadiusMultiplier;
            
            // Initialize puddle
            DamagePuddle puddleScript = puddleObj.GetComponent<DamagePuddle>();
            if (puddleScript != null)
            {
                float finalDamage = GetFinalDamage();
                
                // Apply HP scaling if enabled
                if (hpScalingPercent > 0f)
                {
                    var playerHealth = PlayerController.Instance?.Health;
                    if (playerHealth != null)
                    {
                        float hpBonus = playerHealth.MaxHealth * hpScalingPercent;
                        finalDamage += hpBonus;
                    }
                }
                
                puddleScript.Initialize(puddleDuration, finalDamage, damageTickRate);
            }
        }

        // === UPGRADE API ===
        
        public void UpgradeDuration(float percent)
        {
            puddleDuration *= (1f + percent);
        }

        public void MultiplyPuddleRadius(float multiplier)
        {
            puddleRadiusMultiplier *= multiplier;
        }

        public void SetHPScaling(float percentPercentage)
        {
            hpScalingPercent = percentPercentage;
        }
    }
}
