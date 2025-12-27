using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Health Potion pickup - heals player on contact.
    /// Only collected if player is missing health.
    /// </summary>
    public class HealthPotion : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float healAmount = 25f;
        [SerializeField] private float magnetRange = 3f;
        [SerializeField] private float moveSpeed = 8f;

        private Transform player;
        private bool isBeingPulled = false;
        private PlayerHealth playerHealth;

        private void Start()
        {
            // Find player
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerHealth = playerObj.GetComponent<PlayerHealth>();
            }
        }

        private void Update()
        {
            if (player == null || playerHealth == null) return;
            // Check if player needs health (don't magnet if full)
            if (playerHealth != null)
            {
                // Use Approximately for float precision safety logic
                // If health is full (or greater), stop any pulling
                if (playerHealth.CurrentHealth >= playerHealth.MaxHealth - 0.01f)
                {
                    isBeingPulled = false;
                    return;
                }
            }

            // Check distance
            float distance = Vector2.Distance(transform.position, player.position);
            
            // Magnet logic
            if (distance <= magnetRange)
            {
                isBeingPulled = true;
            }

            // Move toward player if being pulled
            if (isBeingPulled)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position, 
                    player.position, 
                    moveSpeed * Time.deltaTime
                );
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                TryCollect(collision.GetComponent<PlayerHealth>());
            }
        }

        private void TryCollect(PlayerHealth playerHealth)
        {
            if (playerHealth == null) return;

            // CRITICAL CHECK: Only pickup if not at max health
            // Use Approximately for float precision safety
            if (!Mathf.Approximately(playerHealth.CurrentHealth, playerHealth.MaxHealth) && playerHealth.CurrentHealth < playerHealth.MaxHealth)
            {
                // Heal player
                playerHealth.Heal(healAmount);
                
                // Fire event (AudioManager listens to this)
                Core.GameEvents.TriggerHealthPotionPickup(healAmount);

                // Destroy object
                Destroy(gameObject);
            }
            // else: Leave the potion on the ground!
        }
        
        /// <summary>
        /// Set heal amount (optional, if spawner wants to override).
        /// </summary>
        public void SetHealAmount(float amount)
        {
            healAmount = amount;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, magnetRange);
        }
    }
}
