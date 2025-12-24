using UnityEngine;

namespace PixelVanguard.Gameplay
{
    /// <summary>
    /// Gold coin that spawns when enemies die.
    /// REFACTORED: Made magnetRange public for upgrade system.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class GoldCoin : MonoBehaviour
    {
        [Header("Gold Value")]
        [SerializeField] private int goldAmount = 1;

        [Header("Magnet Behavior")]
        [HideInInspector] public float magnetRange = 3f; // NOW PUBLIC for upgrade system
        [SerializeField] private float moveSpeed = 10f;

        private Transform player;
        private bool isBeingPulled = false;
        private CircleCollider2D coinCollider;

        private void Awake()
        {
            coinCollider = GetComponent<CircleCollider2D>();
            coinCollider.isTrigger = true;
        }

        private void Start()
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("[GoldCoin] Player not found!");
            }
        }

        private void Update()
        {
            if (player == null) return;

            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer <= magnetRange)
            {
                isBeingPulled = true;
            }

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
            if (collision.CompareTag("Player"))
            {
                CollectGold();
            }
        }

        private void CollectGold()
        {
            Core.GameEvents.TriggerGoldCollected(goldAmount);
            Destroy(gameObject);
        }

        public void SetGoldAmount(int amount)
        {
            goldAmount = amount;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, magnetRange);
        }
    }
}
