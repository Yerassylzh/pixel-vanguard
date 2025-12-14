using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PixelVanguard.Gameplay;

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
        [SerializeField] private UpgradeManager upgradeManager;

        // Currently displayed upgrades
        private Data.UpgradeData option1Upgrade;
        private Data.UpgradeData option2Upgrade;
        private Data.UpgradeData option3Upgrade;

        private void Awake()
        {
            // Hide panel initially
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            // Auto-find UpgradeManager if not assigned
            if (upgradeManager == null)
            {
                upgradeManager = FindObjectOfType<UpgradeManager>();
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

            // Get 3 random upgrades from manager
            if (upgradeManager != null)
            {
                var upgrades = upgradeManager.GetRandomUpgrades(3);

                // Assign upgrades
                option1Upgrade = upgrades.Length > 0 ? upgrades[0] : null;
                option2Upgrade = upgrades.Length > 1 ? upgrades[1] : null;
                option3Upgrade = upgrades.Length > 2 ? upgrades[2] : null;

                // Set button labels
                if (option1Text != null && option1Upgrade != null)
                {
                    option1Text.text = $"{option1Upgrade.upgradeName}\n<size=14>{option1Upgrade.description}</size>";
                }

                if (option2Text != null && option2Upgrade != null)
                {
                    option2Text.text = $"{option2Upgrade.upgradeName}\n<size=14>{option2Upgrade.description}</size>";
                }

                if (option3Text != null && option3Upgrade != null)
                {
                    option3Text.text = $"{option3Upgrade.upgradeName}\n<size=14>{option3Upgrade.description}</size>";
                }
            }
            else
            {
                Debug.LogWarning("[LevelUpPanel] UpgradeManager not found!");
                // Fallback labels
                if (option1Text != null) option1Text.text = "No upgrades available";
                if (option2Text != null) option2Text.text = "No upgrades available";
                if (option3Text != null) option3Text.text = "No upgrades available";
            }

            // Show panel
            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }
        }

        private void OnOption1Selected() => SelectOption(option1Upgrade);
        private void OnOption2Selected() => SelectOption(option2Upgrade);
        private void OnOption3Selected() => SelectOption(option3Upgrade);

        private void SelectOption(Data.UpgradeData upgrade)
        {
            if (upgrade != null && upgradeManager != null)
            {
                upgradeManager.ApplyUpgrade(upgrade);
            }
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
