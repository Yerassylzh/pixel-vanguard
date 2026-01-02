using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PixelVanguard.Core;
using PixelVanguard.Services;
using System.Threading.Tasks;
using System.Collections;
using System;

namespace PixelVanguard.UI
{
    /// <summary>
    /// Main shop controller managing upgrades and gold packs.
    /// Updated to match user's design with simplified cards.
    /// </summary>
    public class ShopController : MonoBehaviour
    {
        [SerializeField] private GameObject shopPanel;
        [Header("Top Bar")]
        [SerializeField] private TextMeshProUGUI goldText; // Total gold display
        [SerializeField] private Button backButton;

        [Header("Details Panel")]
        [SerializeField] private DetailsPanel detailsPanel; // Shows selected card info

        [Header("Upgrade Cards")]
        [SerializeField] private UpgradeCard mightCard;
        [SerializeField] private UpgradeCard vitalityCard;
        [SerializeField] private UpgradeCard greavesCard;
        [SerializeField] private UpgradeCard magnetCard;

        [Header("Upgrade Icons")]
        [SerializeField] private Sprite mightIcon; // Sword
        [SerializeField] private Sprite vitalityIcon; // Heart
        [SerializeField] private Sprite greavesIcon; // Boot
        [SerializeField] private Sprite magnetIcon; // Magnet

        [Header("Gold Pack Cards")]
        [SerializeField] private AdPackCard adPack1Card;  // 5 ads → 1990 gold
        [SerializeField] private AdPackCard adPack2Card;  // 10 ads → 4990 gold
        [SerializeField] private Sprite adPack1Icon; // Small gold pile (1990)
        [SerializeField] private Sprite adPack2Icon; // Medium gold pile (4990)

        [Header("IAP Card")]
        [SerializeField] private TextMeshProUGUI iapRewardText; // "29900 Coins"
        [SerializeField] private Button iapBuyButton;
        [SerializeField] private TextMeshProUGUI iapButtonText; // "0.99 USD"
        [SerializeField] private Image iapIconImage;
        [SerializeField] private Sprite iapIcon; // Large gold pile
        [SerializeField] private Button iapCardButton; // Button for whole IAP card (to show details)

        private SaveData saveData;
        private ISaveService saveService;
        private IAdService adService;
        private IIAPService iapService;
        private Coroutine cooldownCoroutine;

        private void Awake()
        {            
            // Setup upgrade cards FIRST
            SetupUpgradeCards();

            // Setup gold pack cards
            SetupGoldPackCards();
        }

        private void Start()
        {            
            // Get services
            saveService = ServiceLocator.Get<ISaveService>();
            adService = ServiceLocator.Get<IAdService>();
            iapService = ServiceLocator.Get<IIAPService>();

            // Load save data
            LoadSaveData();

            // Setup buttons
            backButton.onClick.AddListener(OnBackClicked);
            iapBuyButton.onClick.AddListener(OnIAPBuyClicked);

            // Start cooldown timer coroutine
            cooldownCoroutine = StartCoroutine(UpdateAdCooldownTimer());
        }

        private void OnEnable()
        {
            // Reload data when panel is re-enabled (if services are already initialized)
            if (saveService != null)
            {
                LoadSaveData();
            }
        }

        private void LoadSaveData()
        {
            saveData = saveService.LoadData();
            RefreshUI();
        }

        private void SetupUpgradeCards()
        {
            mightCard.Initialize("might", 
                LocalizationManager.Get("ui.shop.might.name"), 
                mightIcon, 100, 
                LocalizationManager.Get("ui.shop.might.desc"));
            vitalityCard.Initialize("vitality", 
                LocalizationManager.Get("ui.shop.vitality.name"), 
                vitalityIcon, 80, 
                LocalizationManager.Get("ui.shop.vitality.desc"));
            
            greavesCard.Initialize("greaves", 
                LocalizationManager.Get("ui.shop.greaves.name"), 
                greavesIcon, 120, 
                LocalizationManager.Get("ui.shop.greaves.desc"));
            
            magnetCard.Initialize("magnet", 
                LocalizationManager.Get("ui.shop.magnet.name"), 
                magnetIcon, 60, 
                LocalizationManager.Get("ui.shop.magnet.desc"));

            // Subscribe to purchase events
            mightCard.OnPurchaseClicked += () => OnUpgradePurchased("might");
            vitalityCard.OnPurchaseClicked += () => OnUpgradePurchased("vitality");
            greavesCard.OnPurchaseClicked += () => OnUpgradePurchased("greaves");
            magnetCard.OnPurchaseClicked += () => OnUpgradePurchased("magnet");

            // Subscribe to card click events (to show details)
            mightCard.OnCardClicked += () => OnUpgradeCardClicked(mightCard);
            vitalityCard.OnCardClicked += () => OnUpgradeCardClicked(vitalityCard);
            greavesCard.OnCardClicked += () => OnUpgradeCardClicked(greavesCard);
            magnetCard.OnCardClicked += () => OnUpgradeCardClicked(magnetCard);
        }

        private void SetupGoldPackCards()
        {
            // Initialize ad packs with their respective icons and localized descriptions
            adPack1Card.Initialize(5, 1990, adPack1Icon, () => OnWatchAdClicked(1),
                LocalizationManager.Get("ui.shop.ad_pack.desc_5"));
            adPack2Card.Initialize(10, 4990, adPack2Icon, () => OnWatchAdClicked(2),
                LocalizationManager.Get("ui.shop.ad_pack.desc_10"));

            // Subscribe to card click events
            adPack1Card.OnCardClicked += () => OnGoldPackCardClicked(adPack1Card);
            adPack2Card.OnCardClicked += () => OnGoldPackCardClicked(adPack2Card);

            // Set IAP reward text and icon
            if (iapRewardText != null)
            {
                iapRewardText.text = "29900 Coins";
            }
            if (iapIcon != null && iapIconImage != null)
            {
                iapIconImage.sprite = iapIcon;
            }
            
            // Setup IAP card click (to show details)
            if (iapCardButton != null)
            {
                iapCardButton.onClick.AddListener(OnIAPCardClicked);
            }
            else
            {
                Debug.LogWarning("[ShopController] iapCardButton is null - assign in Inspector!");
            }
        }

        private void RefreshUI()
        {
            if (saveData == null)
            {
                Debug.LogWarning("[ShopController] SaveData not loaded yet");
                return;
            }

            // Update gold display (top bar)
            goldText.text = saveData.totalGold.ToString();

            // Update upgrade cards
            RefreshUpgradeCard(mightCard, "might");
            RefreshUpgradeCard(vitalityCard, "vitality");
            RefreshUpgradeCard(greavesCard, "greaves");
            RefreshUpgradeCard(magnetCard, "magnet");

            // Update gold pack cards (button shows total gold, like in image)
            adPack1Card.UpdateProgress(saveData.adsWatchedForPack1, saveData.totalGold);
            adPack2Card.UpdateProgress(saveData.adsWatchedForPack2, saveData.totalGold);

            // Update IAP button
            UpdateIAPButton();
        }

        private void RefreshUpgradeCard(UpgradeCard card, string statKey)
        {
            int currentLevel = saveData.GetStatLevel(statKey);
            int cost = CalculateCost(card.BaseCost, currentLevel);
            bool canAfford = saveData.totalGold >= cost;
            
            card.UpdateCard(currentLevel, cost, canAfford);
        }

        private int CalculateCost(int baseCost, int currentLevel)
        {
            // Cost formula: baseCost × (1.5 ^ currentLevel)
            return Mathf.RoundToInt(baseCost * Mathf.Pow(1.5f, currentLevel));
        }

        private void OnUpgradePurchased(string statKey)
        {
            int currentLevel = saveData.GetStatLevel(statKey);
            
            if (currentLevel >= 10)
            {
                Debug.LogWarning($"[Shop] {statKey} already at max level!");
                return;
            }

            UpgradeCard card = GetCardForStat(statKey);
            int cost = CalculateCost(card.BaseCost, currentLevel);

            if (saveData.totalGold < cost)
            {
                Debug.LogWarning("[Shop] Insufficient gold!");
                return;
            }

            // Deduct gold and increment level
            saveData.totalGold -= cost;
            saveData.SetStatLevel(statKey, currentLevel + 1);

            // Save to disk
            saveService.SaveData(saveData);

            // Refresh UI
            RefreshUI();
        }

        private async void OnWatchAdClicked(int packNumber)
        {
            
            // Check cooldown
            if (!adService.CanWatchAd(saveData.lastAdWatchedTime))
            {
                int remaining = adService.GetCooldownRemainingSeconds(saveData.lastAdWatchedTime);
                Debug.LogWarning($"[Shop] Ad cooldown active: {remaining}s remaining");
                return;
            }
            
            // Show ad
            bool success = await adService.ShowRewardedAd();
            
            if (success)
            {
                // Update ad watch count based on pack
                if (packNumber == 1)
                {
                    saveData.adsWatchedForPack1++;

                    // Check if pack is complete
                    if (saveData.adsWatchedForPack1 >= 5)
                    {
                        saveData.totalGold += 1990;
                        saveData.adsWatchedForPack1 = 0; // Reset progress
                    }
                }
                else if (packNumber == 2)
                {
                    saveData.adsWatchedForPack2++;

                    // Check if pack is complete
                    if (saveData.adsWatchedForPack2 >= 10)
                    {
                        saveData.totalGold += 4990;
                        saveData.adsWatchedForPack2 = 0; // Reset progress
                    }
                }

                // Update timestamp for cooldown
                saveData.lastAdWatchedTime = DateTime.Now.ToString("o"); // ISO 8601

                // Save to disk
                saveService.SaveData(saveData);

                // Refresh UI
                RefreshUI();
            }
            else
            {
                Debug.LogWarning("[Shop] Ad failed to show or was cancelled");
            }
        }

        private async void OnIAPBuyClicked()
        {
            if (iapService == null || !iapService.IsInitialized)
            {
                Debug.LogError("[Shop] IAP service not initialized!");
                return;
            }

            bool success = await iapService.PurchaseProduct(ProductIDs.GOLD_PACK_LARGE);

            if (success)
            {
                // Award gold
                saveData.totalGold += 29900;
                saveService.SaveData(saveData);
                
                // Refresh UI
                RefreshUI();
            }
            else
            {
                Debug.LogWarning("[Shop] IAP purchase failed or was cancelled");
            }
        }

        private void UpdateIAPButton()
        {
            if (iapService == null)
            {
                Debug.LogWarning("[ShopController] IAP service is null!");
                iapButtonText.text = "---";
                return;
            }
            
            if (!iapService.IsInitialized)
            {
                Debug.LogWarning("[ShopController] IAP service not initialized!");
                iapButtonText.text = "---";
                return;
            }
            
            string price = iapService.GetLocalizedPrice(ProductIDs.GOLD_PACK_LARGE);
            iapButtonText.text = price;
        }

        private IEnumerator UpdateAdCooldownTimer()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);

                if (saveData != null && adService != null)
                {
                    int remaining = adService.GetCooldownRemainingSeconds(saveData.lastAdWatchedTime);

                    // Update both ad pack cards with same cooldown
                    adPack1Card.UpdateCooldown(remaining);
                    adPack2Card.UpdateCooldown(remaining);

                    // When cooldown ends, refresh to show gold again
                    if (remaining == 0)
                    {
                        adPack1Card.UpdateProgress(saveData.adsWatchedForPack1, saveData.totalGold);
                        adPack2Card.UpdateProgress(saveData.adsWatchedForPack2, saveData.totalGold);
                    }
                }
            }
        }

        private UpgradeCard GetCardForStat(string statKey)
        {
            switch (statKey)
            {
                case "might": return mightCard;
                case "vitality": return vitalityCard;
                case "greaves": return greavesCard;
                case "magnet": return magnetCard;
                default:
                    Debug.LogError($"[Shop] Unknown stat key: {statKey}");
                    return null;
            }
        }

        private void OnBackClicked()
        {
            shopPanel.SetActive(false);
            FindFirstObjectByType<MainMenuManager>().ReturnToMainMenu();
        }

        // ============================================
        // DETAILS PANEL HANDLERS
        // ============================================

        private void OnUpgradeCardClicked(UpgradeCard card)
        {
            if (detailsPanel != null)
            {
                detailsPanel.ShowUpgradeDetails(
                    card.GetIcon(),
                    card.GetDisplayName(),
                    card.GetDescription()
                );
            }
        }

        private void OnGoldPackCardClicked(AdPackCard card)
        {
            if (detailsPanel != null)
            {
                detailsPanel.ShowGoldPackDetails(
                    card.GetIcon(),
                    card.GetTitle(),
                    card.GetDescription()
                );
            }
        }

        private void OnIAPCardClicked()
        {
            if (detailsPanel != null && iapIcon != null)
            {
                detailsPanel.ShowGoldPackDetails(
                    iapIcon,
                    LocalizationManager.Get("ui.shop.gold_pack.title"),
                    LocalizationManager.Get("ui.shop.gold_pack.desc")
                );
            }
        }

        private void OnDestroy()
        {
            // Stop coroutine
            if (cooldownCoroutine != null)
            {
                StopCoroutine(cooldownCoroutine);
            }

            // Remove listeners
            backButton.onClick.RemoveAllListeners();
            iapBuyButton.onClick.RemoveAllListeners();
        }
    }
}
