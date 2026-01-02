using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PixelVanguard.UI
{
    /// <summary>
    /// Heads-Up Display showing player stats during gameplay.
    /// Displays HP, XP, time, and kill count.
    /// </summary>
    public class HUD : MonoBehaviour
    {
        [Header("XP Bar")]
        [SerializeField] private Slider xpSlider;
        [SerializeField] private TextMeshProUGUI levelText;

        [Header("Stats Display")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI killCountText;
        [SerializeField] private TextMeshProUGUI goldText;

        [Header("XP Progression")]
        [SerializeField] private int xpPerLevel = 100;
        [SerializeField] private float xpScalingPerLevel = 1.2f;

        // Runtime tracking
        private int currentXP = 0;
        private int currentLevel = 1;
        private int xpNeededForNextLevel;

        private void Awake()
        {
            // Calculate initial XP requirement
            xpNeededForNextLevel = xpPerLevel;
            
            // Initialize level text with localized label
            UpdateLevelText();
        }

        private void OnEnable()
        {
            // Subscribe to events
            Core.GameEvents.OnXPGained += AddXP;
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            Core.GameEvents.OnXPGained -= AddXP;
        }

        private void Update()
        {
            UpdateTimer();
            UpdateKillCount();
            UpdateGoldCount();
        }

        private void AddXP(float amount)
        {
            currentXP += Mathf.RoundToInt(amount);

            // Check for level up
            while (currentXP >= xpNeededForNextLevel)
            {
                LevelUp();
            }

            UpdateXPBar();
        }

        private void LevelUp()
        {
            currentLevel++;
            currentXP -= xpNeededForNextLevel;

            // Calculate next level requirement (scales with level)
            xpNeededForNextLevel = Mathf.RoundToInt(xpPerLevel * Mathf.Pow(xpScalingPerLevel, currentLevel - 1));

            // Fire level up event
            Core.GameEvents.TriggerPlayerLevelUp();

            // Update level text
            UpdateLevelText();
        }

        private void UpdateXPBar()
        {
            if (xpSlider != null)
            {
                xpSlider.maxValue = xpNeededForNextLevel;
                xpSlider.value = currentXP;
            }
        }

        private void UpdateLevelText()
        {
            if (levelText != null)
            {
                levelText.text = $"{Core.LocalizationManager.Get("ui.hud.level_short")} {currentLevel}";
            }
        }

        private void UpdateTimer()
        {
            if (timerText != null && Gameplay.GameManager.Instance != null)
            {
                float gameTime = Gameplay.GameManager.Instance.GameTime;
                int minutes = Mathf.FloorToInt(gameTime / 60f);
                int seconds = Mathf.FloorToInt(gameTime % 60f);
                timerText.text = $"{minutes:00}:{seconds:00}";
            }
        }

        private void UpdateKillCount()
        {
            if (killCountText != null && Gameplay.GameManager.Instance != null)
            {
                int kills = Gameplay.GameManager.Instance.KillCount;
                killCountText.text = $"{kills}";
            }
        }

        private void UpdateGoldCount()
        {
            if (goldText != null && Gameplay.GameManager.Instance != null)
            {
                int gold = Gameplay.GameManager.Instance.GoldCollected;
                goldText.text = $"{gold}";
            }
        }
    }
}
