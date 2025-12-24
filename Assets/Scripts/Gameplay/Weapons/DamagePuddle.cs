using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Damage puddle created by AreaDenialWeapon.
    /// Handles visual expansion (Shader/Particles) and continuous damage.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class DamagePuddle : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpriteRenderer runeRenderer;
        [SerializeField] private ParticleSystem fireParticles;

        [Header("Visual Settings")]
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;

        private float duration;
        private float damagePerTick;
        private float tickRate;
        private float hpScalingPercent = 0f; // HP scaling percentage from upgrade

        private CircleCollider2D puddleCollider;
        private float lifetimeTimer = 0f;
        private float damageTimer = 0f;
        private bool isFadingOut = false;

        // Shader Props
        private Material instanceMaterial;
        private int revealPropID;

        // Track enemies currently in puddle
        private readonly HashSet<EnemyHealth> enemiesInPuddle = new();

        private void Awake()
        {
            puddleCollider = GetComponent<CircleCollider2D>();
            puddleCollider.isTrigger = true;
            
            // Setup Shader for "Expansion"
            if (runeRenderer != null)
            {
                (instanceMaterial, revealPropID) = ShaderHelper.CreateRevealMaterial(runeRenderer);
                instanceMaterial.SetFloat(revealPropID, 0f); // Start hidden
            }
        }

        public void Initialize(float dur, float dmg, float tick, float hpScale = 0f)
        {
            duration = dur;
            damagePerTick = dmg;
            tickRate = tick;
            hpScalingPercent = hpScale;
        }

        private void Start()
        {
            StartCoroutine(AnimateExpansion());
        }

        private void Update()
        {
            lifetimeTimer += Time.deltaTime;
            
            // Start fade-out when time remaining equals fadeOutDuration
            float timeRemaining = duration - lifetimeTimer;
            if (!isFadingOut && timeRemaining <= fadeOutDuration)
            {
                isFadingOut = true;
                StartCoroutine(AnimateShrink());
            }

            // Damage tick (continue dealing damage during fade-out)
            damageTimer += Time.deltaTime;
            if (damageTimer >= tickRate)
            {
                DamageEnemiesInPuddle();
                damageTimer = 0f;
            }
        }

        private IEnumerator AnimateExpansion()
        {
            if (instanceMaterial == null) yield break;

            float currentReveal = 0f;
            float expandSpeed = 1f / fadeInDuration; // Convert duration to speed
            
            while (currentReveal < 1f)
            {
                currentReveal += Time.deltaTime * expandSpeed;
                instanceMaterial.SetFloat(revealPropID, Mathf.Clamp01(currentReveal));
                yield return null;
            }
            instanceMaterial.SetFloat(revealPropID, 1f);
            
            // Start fire after expansion
            if (fireParticles != null) fireParticles.Play();
        }

        private IEnumerator AnimateShrink()
        {
            // Stop particles first (they should fade before rune shrinks)
            if (fireParticles != null) fireParticles.Stop();
            
            if (instanceMaterial == null)
            {
                Destroy(gameObject);
                yield break;
            }

            float currentReveal = 1f;
            float shrinkSpeed = 1f / fadeOutDuration; // Shrink over fadeOutDuration seconds
            
            while (currentReveal > 0f)
            {
                currentReveal -= Time.deltaTime * shrinkSpeed;
                instanceMaterial.SetFloat(revealPropID, Mathf.Clamp01(currentReveal));
                yield return null;
            }
            instanceMaterial.SetFloat(revealPropID, 0f);
            
            Destroy(gameObject);
        }

        private void DamageEnemiesInPuddle()
        {
            var enemiesCopy = new HashSet<EnemyHealth>(enemiesInPuddle);
            foreach (var enemy in enemiesCopy)
            {
                if (enemy != null && enemy.IsAlive)
                {
                    float finalDamage = damagePerTick;
                    
                    // Add HP scaling if enabled
                    if (hpScalingPercent > 0 && enemy.EnemyData != null)
                    {
                        float scalingDamage = enemy.EnemyData.maxHealth * hpScalingPercent;
                        finalDamage += scalingDamage;
                    }
                    
                    enemy.TakeDamage(finalDamage, Vector2.zero, 0f);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Enemy"))
            {
                if (collision.TryGetComponent<EnemyHealth>(out var enemyHealth)) enemiesInPuddle.Add(enemyHealth);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Enemy"))
            {
                if (collision.TryGetComponent<EnemyHealth>(out var enemyHealth)) enemiesInPuddle.Remove(enemyHealth);
            }
        }
        
        private void OnDestroy()
        {
            if (instanceMaterial != null) Destroy(instanceMaterial);
            enemiesInPuddle.Clear();
        }


    }
}
