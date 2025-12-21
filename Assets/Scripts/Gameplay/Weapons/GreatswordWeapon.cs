using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Greatsword weapon with periodic swing attack.
    /// Rests at player's side, then performs fast 360° swing every few seconds.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class GreatswordWeapon : WeaponBase
    {
        [Header("Visual Settings")]
        [SerializeField]
        private AnimationCurve opacityCurve = new(
            new Keyframe(0f, 0f),
            new Keyframe(0.2f, 1f),
            new Keyframe(1f, 0f)
        );

        [Header("Animation")]
        [Tooltip("How fast the slash 'Fills' (0 to 1). Percentage of swingDuration.")]
        [Range(0.1f, 1f)]
        [SerializeField] private float revealSpeedFactor = 0.5f;

        [SerializeField] private float fadeDuration = 0.15f;
        [SerializeField] private float swingDuration = 0.3f;
        [SerializeField] private bool revealVertical = false; // Toggle to switch axis

        private SpriteRenderer spriteRenderer;
        private Material instanceMaterial;
        private int revealPropID;
        private int axisPropID;

        // Swing state
        private bool isSwinging = false;
        private float swingTimer = 0f;

        // Hit tracking
        private readonly System.Collections.Generic.HashSet<int> hitEnemies = new();

        // State Machine for the swing: 0 = Revealing, 1 = Fading
        private enum SwingPhase { Revealing, Fading }
        private SwingPhase currentPhase;

        protected override void Awake()
        {
            base.Awake(); 

            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            
            if (spriteRenderer != null)
            {
                (instanceMaterial, revealPropID) = ShaderHelper.CreateRevealMaterial(spriteRenderer);
                axisPropID = Shader.PropertyToID("_VerticalReveal");
                
                // Set Axis
                instanceMaterial.SetFloat(axisPropID, revealVertical ? 1f : 0f);

                UpdateVisuals(false);
            }

            // Unparent for world space rotation
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }
        }

        protected override void Update()
        {
            transform.position = player.position;

            base.Update();

            if (isSwinging)
            {
                PerformSwing();
            }
            else
            {
                RestPosition();
            }
        }

        protected override void Fire()
        {
            if (isSwinging) return;

            isSwinging = true;
            swingTimer = 0f;

            // Clear hit list for new swing
            hitEnemies.Clear();

            currentPhase = SwingPhase.Revealing;
            
            // 1. Aim based on last facing direction (Horizontal Only)
            float rotationAngle = 0f; // Default Right
            if (player != null)
            {
                // Try to get PlayerAnimationController to respect last facing direction
                if (player.TryGetComponent<PlayerAnimationController>(out var animController))
                {
                    // Use last facing direction: -1 = left (180°), 1 = right (0°)
                    rotationAngle = animController.LastFacingDirection < 0 ? 180f : 0f;
                }
                else
                {
                    // Fallback to PlayerController movement direction
                    var pc = player.GetComponent<PlayerController>();
                    if (pc != null && pc.MoveDirection.x < -0.01f)
                    {
                        rotationAngle = 180f;
                    }
                }
            }
            
            transform.rotation = Quaternion.Euler(0, 0, rotationAngle);

            // 2. Reset Visuals
            if (instanceMaterial != null)
            {
                instanceMaterial.SetFloat(revealPropID, 0f);
                Color c = spriteRenderer.color;
                c.a = 1f;
                spriteRenderer.color = c;
            }
            UpdateVisuals(true);
        
        }


        private void PerformSwing()
        {
            // Position locked in Update(), just handle animation here
            swingTimer += Time.deltaTime;

            if (currentPhase == SwingPhase.Revealing)
            {
                // Duration of the "Fill" part
                float revealTime = swingDuration * revealSpeedFactor;
                float progress = Mathf.Clamp01(swingTimer / revealTime);

                // Animate Shader Property
                if (instanceMaterial != null)
                {
                    // easeOutCubic for a satisfying "Swish" feel
                    float easedProgress = 1f - Mathf.Pow(1f - progress, 3);
                    instanceMaterial.SetFloat(revealPropID, easedProgress);
                }

                if (progress >= 1f)
                {
                    // Reveal Complete -> Switch to Fade
                    currentPhase = SwingPhase.Fading;
                    swingTimer = 0f; // Reset timer for fade phase
                }
            }
            else if (currentPhase == SwingPhase.Fading)
            {
                // Fade Phase
                float progress = Mathf.Clamp01(swingTimer / fadeDuration);

                if (spriteRenderer != null)
                {
                    Color c = spriteRenderer.color;
                    c.a = Mathf.Lerp(1f, 0f, progress);
                    spriteRenderer.color = c;
                }

                if (progress >= 1f)
                {
                    // Animation Complete
                    isSwinging = false;
                    UpdateVisuals(false);
                }
            }
        }

        private void RestPosition()
        {
            UpdateVisuals(false);
            transform.rotation = Quaternion.identity;
        }

        private void UpdateVisuals(bool active)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = active;
            }
        }



        private void OnTriggerStay2D(Collider2D collision)
        {
            // Only damage during swing
            if (!isSwinging) return;

            // Check if hit an enemy
            if (collision.CompareTag("Enemy"))
            {
                // Get InstanceID for unique tracking per swing
                int enemyID = collision.gameObject.GetInstanceID();

                // If already hit this swing, ignore
                if (hitEnemies.Contains(enemyID)) return;

                // Calculate knockback direction (away from weapon center)
                Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;

                // Attempt damage and track if successful
                if (EnemyDamageUtility.TryDamageEnemy(collision, damage, knockbackDir, knockback))
                {
                    hitEnemies.Add(enemyID);
                }
            }
        }
    }
    // Note: IncreaseDamage() and IncreaseAttackSpeed() inherited from WeaponBase
}