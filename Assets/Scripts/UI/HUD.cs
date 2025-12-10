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
        [Header("Health Bar")]
        [SerializeField] private Slider hpSlider;
        [SerializeField] private TextMeshProUGUI hpText;

        [Header("XP Bar")]
        [SerializeField] private Slider xpSlider;
        [SerializeField] private TextMeshProUGUI levelText;

        [Header("Stats Display")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI killCountText;

        [Header("XP Progression")]
        [SerializeField] private int xpPerLevel = 100;
        [SerializeField] private float xpScalingPerLevel = 1.2f;

        // Runtime tracking
        private float currentHP;
        private float maxHP;
        private int currentXP = 0;
        private int currentLevel = 1;
        private int xpNeededForNextLevel;

        private void Awake()
        {
            // Calculate initial XP requirement
            xpNeededForNextLevel = xpPerLevel;
        }

        private void OnEnable()
        {
            // Subscribe to events
            Core.GameEvents.OnPlayerHealthChanged += UpdateHealth;
            Core.GameEvents.OnXPGained += AddXP;
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            Core.GameEvents.OnPlayerHealthChanged -= UpdateHealth;
            Core.GameEvents.OnXPGained -= AddXP;
        }

        private void Update()
        {
            UpdateTimer();
            UpdateKillCount();
        }

        private void UpdateHealth(float current, float max)
        {
            currentHP = current;
            maxHP = max;

            // Update HP slider
            if (hpSlider != null)
            {
                hpSlider.maxValue = maxHP;
                hpSlider.value = currentHP;
            }

            // Update HP text
            if (hpText != null)
            {
                hpText.text = $"{Mathf.CeilToInt(currentHP)} / {Mathf.CeilToInt(maxHP)}";
            }
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
                levelText.text = $"LV {currentLevel}";
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
                killCountText.text = $"Kills: {kills}";
            }
        }
    }
}
