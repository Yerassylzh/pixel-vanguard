using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// XP gem that spawns when enemies die.
    /// Flies toward player when in magnet range, grants XP on pickup.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class XPGem : MonoBehaviour
    {
        [Header("XP Value")]
        [SerializeField] private int xpAmount = 5;

        [Header("Magnet Behavior")]
        [HideInInspector] public float magnetRange = 3f; // NOW PUBLIC for upgrade system
        [SerializeField] private float moveSpeed = 10f;

        private Transform player;
        private bool isBeingPulled = false;
        private CircleCollider2D gemCollider;

        private void Awake()
        {
            gemCollider = GetComponent<CircleCollider2D>();
            gemCollider.isTrigger = true; // Must be trigger for pickup
        }

        private void Start()
        {
            // Find player
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("[XPGem] Player not found!");
            }
            
            // Apply Magnet upgrade
            ApplyMagnetUpgrade();
        }
        
        /// <summary>
        /// Load Magnet upgrade and increase collection range.
        /// </summary>
        private void ApplyMagnetUpgrade()
        {
            float baseRange = 3f; // Default
            var saveService = Core.ServiceLocator.Get<Services.ISaveService>();
            if (saveService != null)
            {
                var saveData = saveService.LoadData();
                int magnetLevel = saveData.GetStatLevel("magnet");
                float radiusBonus = magnetLevel * 0.10f;
                magnetRange = baseRange * (1f + radiusBonus);
                
                Debug.Log($"[XPGem] Base Range: {baseRange}, Magnet: Lv{magnetLevel} (+{radiusBonus * 100}%) → Final: {magnetRange:F2}");
            }
            else
            {
                magnetRange = baseRange;
            }
        }

        private void Update()
        {
            if (player == null) return;

            // Check if within magnet range
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer <= magnetRange)
            {
                isBeingPulled = true;
            }

            // Move toward player if being pulled
            if (isBeingPulled)
            {
                MoveTowardPlayer();
            }
        }

        private void MoveTowardPlayer()
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Only player can collect XP
            if (collision.CompareTag("Player"))
            {
                CollectXP();
            }
        }

        private void CollectXP()
        {
            // Fire XP gained event (AudioManager listens to this)
            Core.GameEvents.TriggerXPGained(xpAmount);

            // Destroy gem
            Destroy(gameObject);
        }

        /// <summary>
        /// Set XP amount (called by EnemyHealth when spawning).
        /// </summary>
        public void SetXPAmount(int amount)
        {
            xpAmount = amount;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw magnet range in editor
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, magnetRange);
        }
    }
}
