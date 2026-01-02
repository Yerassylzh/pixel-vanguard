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
        [SerializeField] private TextMeshProUGUI option1TitleText;
        [SerializeField] private TextMeshProUGUI option1DescText;
        [SerializeField] private Image option1Icon;
        [SerializeField] private TextMeshProUGUI option2TitleText;
        [SerializeField] private TextMeshProUGUI option2DescText;
        [SerializeField] private Image option2Icon;
        [SerializeField] private TextMeshProUGUI option3TitleText;
        [SerializeField] private TextMeshProUGUI option3DescText;
        [SerializeField] private Image option3Icon;

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
                upgradeManager = FindAnyObjectByType<UpgradeManager>();
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

                // Set button labels and icons with localized text
                if (option1TitleText != null && option1DescText != null && option1Upgrade != null)
                {
                    // Use localization key if available, otherwise fallback to English
                    if (!string.IsNullOrEmpty(option1Upgrade.localizationKey))
                    {
                        option1TitleText.text = Core.LocalizationManager.Get($"{option1Upgrade.localizationKey}.name");
                        option1DescText.text = Core.LocalizationManager.Get($"{option1Upgrade.localizationKey}.desc");
                    }
                    else
                    {
                        option1TitleText.text = option1Upgrade.upgradeName;
                        option1DescText.text = option1Upgrade.description;
                    }
                    if (option1Icon != null)
                    {
                        option1Icon.sprite = option1Upgrade.icon;
                        option1Icon.enabled = option1Upgrade.icon != null;
                    }
                }

                if (option2TitleText != null && option2DescText != null && option2Upgrade != null)
                {
                    // Use localization key if available, otherwise fallback to English
                    if (!string.IsNullOrEmpty(option2Upgrade.localizationKey))
                    {
                        option2TitleText.text = Core.LocalizationManager.Get($"{option2Upgrade.localizationKey}.name");
                        option2DescText.text = Core.LocalizationManager.Get($"{option2Upgrade.localizationKey}.desc");
                    }
                    else
                    {
                        option2TitleText.text = option2Upgrade.upgradeName;
                        option2DescText.text = option2Upgrade.description;
                    }
                    if (option2Icon != null)
                    {
                        option2Icon.sprite = option2Upgrade.icon;
                        option2Icon.enabled = option2Upgrade.icon != null;
                    }
                }

                if (option3TitleText != null && option3DescText != null && option3Upgrade != null)
                {
                    // Use localization key if available, otherwise fallback to English
                    if (!string.IsNullOrEmpty(option3Upgrade.localizationKey))
                    {
                        option3TitleText.text = Core.LocalizationManager.Get($"{option3Upgrade.localizationKey}.name");
                        option3DescText.text = Core.LocalizationManager.Get($"{option3Upgrade.localizationKey}.desc");
                    }
                    else
                    {
                        option3TitleText.text = option3Upgrade.upgradeName;
                        option3DescText.text = option3Upgrade.description;
                    }
                    if (option3Icon != null)
                    {
                        option3Icon.sprite = option3Upgrade.icon;
                        option3Icon.enabled = option3Upgrade.icon != null;
                    }
                }
            }
            else
            {
                Debug.LogWarning("[LevelUpPanel] UpgradeManager not found!");
                // Fallback labels
                if (option1TitleText != null) option1TitleText.text = "No upgrades";
                if (option2TitleText != null) option2TitleText.text = "No upgrades";
                if (option3TitleText != null) option3TitleText.text = "No upgrades";
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
                
                // Trigger upgrade selected SFX
                Core.GameEvents.TriggerUpgradeSelected();
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
