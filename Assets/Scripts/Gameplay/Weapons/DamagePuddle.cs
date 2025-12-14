using System.Collections.Generic;
using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Damage puddle created by AreaDenialWeapon.
    /// Continuously damages enemies standing within the area.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class DamagePuddle : MonoBehaviour
    {
        private float duration;
        private float radius;
        private float damagePerTick;
        private float tickRate;

        private CircleCollider2D puddleCollider;
        private float lifetimeTimer = 0f;
        private float damageTimer = 0f;

        // Track enemies currently in puddle
        private HashSet<EnemyHealth> enemiesInPuddle = new HashSet<EnemyHealth>();

        private void Awake()
        {
            puddleCollider = GetComponent<CircleCollider2D>();
            puddleCollider.isTrigger = true;
        }

        /// <summary>
        /// Initialize puddle with stats from weapon.
        /// </summary>
        public void Initialize(float dur, float rad, float dmg, float tick)
        {
            duration = dur;
            radius = rad;
            damagePerTick = dmg;
            tickRate = tick;

            // Set collider radius
            puddleCollider.radius = radius;

            // Scale visual to match radius (assuming puddle sprite is 1 unit = 1m)
            transform.localScale = Vector3.one * radius;
        }

        private void Update()
        {
            // Lifetime check
            lifetimeTimer += Time.deltaTime;
            if (lifetimeTimer >= duration)
            {
                Destroy(gameObject);
                return;
            }

            // Damage tick
            damageTimer += Time.deltaTime;
            if (damageTimer >= tickRate)
            {
                DamageEnemiesInPuddle();
                damageTimer = 0f;
            }
        }

        private void DamageEnemiesInPuddle()
        {
            // Create a copy to iterate over to avoid collection modification errors
            // (enemies can die and trigger OnTriggerExit2D during iteration)
            var enemiesCopy = new HashSet<EnemyHealth>(enemiesInPuddle);
            
            // Damage all enemies currently in the puddle
            foreach (var enemy in enemiesCopy)
            {
                if (enemy != null && enemy.IsAlive)
                {
                    // No knockback for puddle damage (enemies stay in puddle)
                    enemy.TakeDamage(damagePerTick, Vector2.zero, 0f);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Enemy"))
            {
                var enemyHealth = collision.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemiesInPuddle.Add(enemyHealth);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Enemy"))
            {
                var enemyHealth = collision.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemiesInPuddle.Remove(enemyHealth);
                }
            }
        }

        private void OnDestroy()
        {
            // Clean up references
            enemiesInPuddle.Clear();
        }
    }
}
