using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Gold coin pickup - attracted to player via magnet, gives gold when collected.
    /// Similar to XPGem but for gold currency.
    /// </summary>
    public class GoldCoin : MonoBehaviour
    {
        [SerializeField] private int goldValue = 1;
        [SerializeField] private float magnetRadius = 3f; // Distance at which player attracts coin
        [SerializeField] private float magnetSpeed = 8f;

        private Transform player;
        private bool isBeingAttracted = false;

        private void Start()
        {
            // Find player
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        private void Update()
        {
            if (player == null) return;

            // Check if player is within magnet range
            float distance = Vector2.Distance(transform.position, player.position);

            if (distance <= magnetRadius)
            {
                isBeingAttracted = true;
            }

            // Move towards player if attracted
            if (isBeingAttracted)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    player.position,
                    magnetSpeed * Time.deltaTime
                );
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Only collect when touching player
            if (collision.CompareTag("Player"))
            {
                Collect();
            }
        }

        private void Collect()
        {
            // Fire gold collected event
            Core.GameEvents.TriggerGoldCollected(goldValue);

            // Destroy this coin
            Destroy(gameObject);
        }

        /// <summary>
        /// Set the gold amount this coin gives (called by spawner).
        /// </summary>
        public void SetGoldValue(int value)
        {
            goldValue = value;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw magnet radius in editor
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, magnetRadius);
        }
    }
}
