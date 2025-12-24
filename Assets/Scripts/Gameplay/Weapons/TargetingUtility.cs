using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Centralized targeting utility for all weapons.
    /// Eliminates code duplication across weapon scripts.
    /// </summary>
    public static class TargetingUtility
    {
        /// <summary>
        /// Find the nearest enemy within range.
        /// </summary>
        public static Transform FindNearestEnemy(Vector3 fromPosition, float maxRange)
        {
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            Transform nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var enemy in enemies)
            {
                if (enemy == null) continue;

                var enemyHealth = enemy.GetComponent<EnemyHealth>();
                if (enemyHealth == null || !enemyHealth.IsAlive) continue;

                float distance = Vector3.Distance(fromPosition, enemy.transform.position);
                
                if (distance <= maxRange && distance < nearestDistance)
                {
                    nearest = enemy.transform;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Find multiple unique enemies for multi-targeting (e.g., AutoCrossbow).
        /// </summary>
        public static List<Transform> FindUniqueTargets(Vector3 fromPosition, int count, float maxRange)
        {
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            var validEnemies = new List<(Transform transform, float distance)>();

            foreach (var enemy in enemies)
            {
                if (enemy == null) continue;

                var enemyHealth = enemy.GetComponent<EnemyHealth>();
                if (enemyHealth == null || !enemyHealth.IsAlive) continue;

                float distance = Vector3.Distance(fromPosition, enemy.transform.position);
                
                if (distance <= maxRange)
                {
                    validEnemies.Add((enemy.transform, distance));
                }
            }

            var sorted = validEnemies.OrderBy(e => e.distance).ToList();
            var targets = new List<Transform>();
            
            for (int i = 0; i < Mathf.Min(count, sorted.Count); i++)
            {
                targets.Add(sorted[i].transform);
            }

            return targets;
        }

        /// <summary>
        /// Get all enemies within range.
        /// </summary>
        public static List<Transform> GetEnemiesInRange(Vector3 fromPosition, float maxRange)
        {
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            var inRange = new List<Transform>();

            foreach (var enemy in enemies)
            {
                if (enemy == null) continue;

                var enemyHealth = enemy.GetComponent<EnemyHealth>();
                if (enemyHealth == null || !enemyHealth.IsAlive) continue;

                float distance = Vector3.Distance(fromPosition, enemy.transform.position);
                
                if (distance <= maxRange)
                {
                    inRange.Add(enemy.transform);
                }
            }

            return inRange;
        }
    }
}
