using UnityEngine;
using UnityEngine.UI;

namespace PixelVanguard.UI
{
    /// <summary>
    /// World-space health bar that follows player character.
    /// Simple slider-based approach (no text, black background, red fill).
    /// </summary>
    public class PlayerHealthBarUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Slider hpSlider;

        [Header("Positioning")]
        [SerializeField] private Vector3 offset = new Vector3(0f, -0.8f, 0f);
        [SerializeField] private bool faceCamera = true;

        private Transform playerTransform;

        private void Start()
        {
            FindPlayer();
        }

        private void OnEnable()
        {
            Core.GameEvents.OnPlayerHealthChanged += UpdateHealth;
        }

        private void OnDisable()
        {
            Core.GameEvents.OnPlayerHealthChanged -= UpdateHealth;
        }

        private void Update()
        {
            // Re-find player if lost
            if (playerTransform == null)
            {
                FindPlayer();
            }

            // Follow player
            if (playerTransform != null)
            {
                transform.position = playerTransform.position + offset;

                // Billboard effect
                if (faceCamera && Camera.main != null)
                {
                    transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                                     Camera.main.transform.rotation * Vector3.up);
                }
            }
        }

        private void FindPlayer()
        {
            var playerController = Gameplay.PlayerController.Instance;
            if (playerController != null)
            {
                playerTransform = playerController.transform;
            }
        }

        private void UpdateHealth(float current, float max)
        {
            if (hpSlider != null)
            {
                hpSlider.maxValue = max;
                hpSlider.value = current;
            }
        }
    }
}
