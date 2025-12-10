using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PixelVanguard.UI
{
    /// <summary>
    /// Level-up panel that pauses game and shows upgrade options.
    /// Currently implements basic greatsword speed upgrade.
    /// </summary>
    public class LevelUpPanel : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject panelRoot;

        [Header("Buttons")]
        [SerializeField] private Button option1Button;
        [SerializeField] private Button option2Button;
        [SerializeField] private Button option3Button;

        [Header("Button Labels")]
        [SerializeField] private TextMeshProUGUI option1Text;
        [SerializeField] private TextMeshProUGUI option2Text;
        [SerializeField] private TextMeshProUGUI option3Text;

        [Header("Upgrade Settings")]
        [SerializeField] private float orbitSpeedIncrease = 30f; // Degrees per second

        private void Awake()
        {
            // Hide panel initially
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            // Setup button listeners
            if (option1Button != null) option1Button.onClick.AddListener(OnOption1Selected);
            if (option2Button != null) option2Button.onClick.AddListener(OnOption2Selected);
            if (option3Button != null) option3Button.onClick.AddListener(OnOption3Selected);
        }

        private void OnEnable()
        {
            // Listen to level up event
            Core.GameEvents.OnPlayerLevelUp += ShowLevelUpOptions;
        }

        private void OnDisable()
        {
            Core.GameEvents.OnPlayerLevelUp -= ShowLevelUpOptions;
        }

        private void ShowLevelUpOptions()
        {
            // Use GameManager's pause system (sets state to LevelUp)
            if (Gameplay.GameManager.Instance != null)
            {
                Gameplay.GameManager.Instance.PauseGame();
            }
            else
            {
                // Fallback if no GameManager
                Time.timeScale = 0f;
            }

            // Set button labels
            if (option1Text != null) option1Text.text = "Increase Greatsword Speed";
            if (option2Text != null) option2Text.text = "Placeholder Option";
            if (option3Text != null) option3Text.text = "Placeholder Option";

            // Show panel
            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }
        }

        private void OnOption1Selected()
        {
            // Find the orbiting weapon and increase its speed
            var weapon = FindObjectOfType<Gameplay.OrbitingWeapon>();
            if (weapon != null)
            {
                // Access the weapon via reflection to modify orbitSpeed
                var type = typeof(Gameplay.OrbitingWeapon);
                var field = type.GetField("orbitSpeed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (field != null)
                {
                    float currentSpeed = (float)field.GetValue(weapon);
                    field.SetValue(weapon, currentSpeed + orbitSpeedIncrease);
                    Debug.Log($"[LevelUpPanel] Greatsword speed increased to {currentSpeed + orbitSpeedIncrease}");
                }
            }

            ClosePanel();
        }

        private void OnOption2Selected()
        {
            // Placeholder - does nothing yet
            Debug.Log("[LevelUpPanel] Option 2 selected (not implemented)");
            ClosePanel();
        }

        private void OnOption3Selected()
        {
            // Placeholder - does nothing yet
            Debug.Log("[LevelUpPanel] Option 3 selected (not implemented)");
            ClosePanel();
        }

        private void ClosePanel()
        {
            // Hide panel
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            // Resume game via GameManager (sets state back to Playing)
            if (Gameplay.GameManager.Instance != null)
            {
                Gameplay.GameManager.Instance.ResumeGame();
            }
            else
            {
                // Fallback if no GameManager
                Time.timeScale = 1f;
            }
        }
    }
}
