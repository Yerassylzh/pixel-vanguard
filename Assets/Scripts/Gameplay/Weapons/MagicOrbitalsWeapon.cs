using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Weapon that spawns orbiting balls around the player.
    /// Manages lifecycle: Radius expands → Orbits for duration → Radius shrinks → Destroy
    /// </summary>
    public class MagicOrbitalsWeapon : WeaponBase
    {
        [Header("Orbital Setup")]
        [SerializeField] private GameObject orbitalPrefab;
        [SerializeField] private int orbitalCount = 3;
        
        [Header("Orbit Settings")]
        [SerializeField] private float targetRadius = 2.5f;
        [SerializeField] private float orbitSpeed = 180f; // Degrees per second
        
        [Header("Animation Timing")]
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        
        [Header("Visual Settings")]
        [Tooltip("Delay in seconds before balls become visible during expand (prevents overlap at spawn)")]
        [SerializeField] private float ballAppearDelay = 2f;
        
        [Tooltip("Delay in seconds before balls disappear during shrink (hide before reaching radius 0)")]
        [SerializeField] private float ballDisappearDelay = 0.15f;

        // Runtime state
        private List<OrbitalBall> activeBalls = new List<OrbitalBall>();
        private float currentRadius = 0f;
        private float currentAngle = 0f;
        private float lifetimeTimer = 0f;
        private bool isFadingOut = false;
        private bool isActive = false;

        protected override void Awake()
        {
            base.Awake();
            
            // Random starting angle for visual variety
            currentAngle = Random.Range(0f, 360f);
        }

        protected override void Update()
        {
            base.Update(); // Handle auto-fire!
            
            // Don't orbit if game is paused
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            // Update lifetime and check for fade-out (only when active)
            if (isActive)
            {
                lifetimeTimer += Time.deltaTime;
                float timeRemaining = duration - lifetimeTimer;
                
                if (!isFadingOut && timeRemaining <= fadeOutDuration)
                {
                    isFadingOut = true;
                    
                    // Schedule balls to disappear, then animate radius shrink
                    Invoke(nameof(DisableBalls), ballDisappearDelay);
                    StartCoroutine(AnimateRadius(targetRadius, 0f, fadeOutDuration, () =>
                    {
                        DestroyOrbitals();
                        isActive = false;
                        isFadingOut = false;
                    }));
                }
            }

            // Always orbit balls (even during expand/shrink animations!)
            OrbitBalls();
        }

        /// <summary>
        /// Fire method - spawns a new set of orbitals.
        /// Called by base.Update() based on cooldown.
        /// </summary>
        protected override void Fire()
        {
            // Don't spawn if previous orbitals are still active
            if (isActive || isFadingOut)
            {
                return;
            }

            // Reset state for new cycle
            lifetimeTimer = 0f;
            isFadingOut = false;
            currentRadius = 0f;

            SpawnOrbitals();
            StartCoroutine(AnimateRadius(0f, targetRadius, fadeInDuration, () => isActive = true));
        }

        private void SpawnOrbitals()
        {
            if (orbitalPrefab == null)
            {
                Debug.LogError("[MagicOrbitalsWeapon] Orbital prefab is not assigned!");
                return;
            }

            for (int i = 0; i < orbitalCount; i++)
            {
                // Spawn at player position (will be positioned by orbit logic)
                GameObject orbObj = Instantiate(orbitalPrefab, player.position, Quaternion.identity);
                
                // Disable entire ball initially (handles sprites, particles, etc.)
                orbObj.SetActive(false);
                
                OrbitalBall ball = orbObj.GetComponent<OrbitalBall>();
                if (ball != null)
                {
                    ball.Initialize(GetFinalDamage(), knockback, cooldown);
                    activeBalls.Add(ball);
                }
                else
                {
                    Debug.LogWarning("[MagicOrbitalsWeapon] Orbital prefab missing OrbitalBall component!");
                }
            }
            
            // Schedule balls to appear after delay
            Invoke(nameof(EnableBalls), ballAppearDelay);
        }

        private void EnableBalls()
        {
            foreach (var ball in activeBalls)
            {
                if (ball != null)
                {
                    ball.gameObject.SetActive(true);
                }
            }
        }

        private void OrbitBalls()
        {
            if (player == null) return;

            // Increment angle based on orbit speed
            currentAngle += orbitSpeed * Time.deltaTime;
            if (currentAngle >= 360f) currentAngle -= 360f;

            float angleStep = 360f / orbitalCount;

            for (int i = 0; i < activeBalls.Count; i++)
            {
                if (activeBalls[i] == null) continue;

                // Calculate position with offset angle
                float ballAngle = currentAngle + (angleStep * i);
                float rad = ballAngle * Mathf.Deg2Rad;
                
                float x = player.position.x + Mathf.Cos(rad) * currentRadius;
                float y = player.position.y + Mathf.Sin(rad) * currentRadius;

                activeBalls[i].transform.position = new Vector3(x, y, player.position.z);
            }
        }

        /// <summary>
        /// Unified radius animation method for both expand and shrink.
        /// </summary>
        private IEnumerator AnimateRadius(float startRadius, float endRadius, float duration, System.Action onComplete = null)
        {
            currentRadius = startRadius;
            float totalChange = endRadius - startRadius;
            float speed = Mathf.Abs(totalChange) / duration;
            float direction = Mathf.Sign(totalChange);

            while ((direction > 0 && currentRadius < endRadius) || 
                   (direction < 0 && currentRadius > endRadius))
            {
                currentRadius += Time.deltaTime * speed * direction;
                currentRadius = Mathf.Clamp(currentRadius, 
                    Mathf.Min(startRadius, endRadius), 
                    Mathf.Max(startRadius, endRadius));
                yield return null;
            }

            currentRadius = endRadius;
            onComplete?.Invoke();
        }

        private void DisableBalls()
        {
            foreach (var ball in activeBalls)
            {
                if (ball != null)
                {
                    ball.gameObject.SetActive(false);
                }
            }
        }

        private void DestroyOrbitals()
        {
            foreach (var ball in activeBalls)
            {
                if (ball != null)
                {
                    Destroy(ball.gameObject);
                }
            }
            activeBalls.Clear();
        }

        private void OnDestroy()
        {
            // Cleanup in case destroyed early
            DestroyOrbitals();
        }

        private void OnDrawGizmosSelected()
        {
            // Draw target orbit radius in editor
            if (player != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(player.position, targetRadius);
            }
            else if (Application.isPlaying == false)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(transform.position, targetRadius);
            }
        }
    }
}
