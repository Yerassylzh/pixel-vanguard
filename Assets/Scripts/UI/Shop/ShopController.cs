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
            Debug.Log("[ShopController] Awake() - Setting up cards BEFORE their Start() methods...");
            
            // Setup upgrade cards FIRST
            SetupUpgradeCards();

            // Setup gold pack cards
            SetupGoldPackCards();
            
            Debug.Log("[ShopController] ✅ Awake() complete - cards initialized");
        }

        private void Start()
        {
            Debug.Log("[ShopController] Start() called - initializing shop...");
            
            // Get services
            saveService = ServiceLocator.Get<ISaveService>();
            adService = ServiceLocator.Get<IAdService>();
            iapService = ServiceLocator.Get<IIAPService>();
            
            Debug.Log($"[ShopController] Services retrieved - SaveService: {saveService != null}, AdService: {adService != null}, IAPService: {iapService != null}");

            // Load save data
            LoadSaveData();

            // Setup buttons
            backButton.onClick.AddListener(OnBackClicked);
            iapBuyButton.onClick.AddListener(OnIAPBuyClicked);

            // Start cooldown timer coroutine
            cooldownCoroutine = StartCoroutine(UpdateAdCooldownTimer());
            
            Debug.Log("[ShopController] ✅ Start() complete");
        }

        private async void LoadSaveData()
        {
            Debug.Log("[ShopController] LoadSaveData() - Starting async load...");
            saveData = await saveService.LoadData();
            Debug.Log($"[ShopController] LoadSaveData() - Complete! Gold={saveData.totalGold}");
            RefreshUI();
        }

        private void SetupUpgradeCards()
        {
            Debug.Log($"[ShopController] SetupUpgradeCards() - Checking references...");
            Debug.Log($"[ShopController] mightCard={mightCard != null}, vitalityCard={vitalityCard != null}, greavesCard={greavesCard != null}, magnetCard={magnetCard != null}");
            
            // Initialize each card with static data and descriptions
            Debug.Log("[ShopController] Initializing MIGHT card with baseCost=100");
            mightCard.Initialize("might", "MIGHT", mightIcon, 100, 
                "Increases base damage by 10% per level. Stacks multiplicatively with weapon upgrades.");
            Debug.Log($"[ShopController] MIGHT card initialized - BaseCost property = {mightCard.BaseCost}");
            
            Debug.Log("[ShopController] Initializing VITALITY card with baseCost=80");
            vitalityCard.Initialize("vitality", "VITALITY", vitalityIcon, 80, 
                "Increases maximum health by 10 HP per level. Take more hits before dying.");
            Debug.Log($"[ShopController] VITALITY card initialized - BaseCost property = {vitalityCard.BaseCost}");
            
            Debug.Log("[ShopController] Initializing GREAVES card with baseCost=120");
            greavesCard.Initialize("greaves", "GREAVES", greavesIcon, 120, 
                "Increases movement speed by 5% per level. Dodge enemies and reposition faster.");
            Debug.Log($"[ShopController] GREAVES card initialized - BaseCost property = {greavesCard.BaseCost}");
            
            Debug.Log("[ShopController] Initializing MAGNET card with baseCost=60");
            magnetCard.Initialize("magnet", "MAGNET", magnetIcon, 60, 
                "Increases XP/item collection radius by 10% per level. Collect from farther away.");
            Debug.Log($"[ShopController] MAGNET card initialized - BaseCost property = {magnetCard.BaseCost}");

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
            
            Debug.Log("[ShopController] ✅ All upgrade cards setup complete");
        }

        private void SetupGoldPackCards()
        {
            // Initialize ad packs with their respective icons and descriptions
            adPack1Card.Initialize(5, 1990, adPack1Icon, () => OnWatchAdClicked(1),
                "Watch 5 ads to earn 1,990 coins. Progress persists across sessions.");
            adPack2Card.Initialize(10, 4990, adPack2Icon, () => OnWatchAdClicked(2),
                "Watch 10 ads to earn 4,990 coins. Best value for your time!");

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
                Debug.Log("[ShopController] IAP card button wired to OnIAPCardClicked");
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
            
            Debug.Log($"[ShopController] RefreshUpgradeCard({statKey}) - Level={currentLevel}, BaseCost={card.BaseCost}, CalculatedCost={cost}, Gold={saveData.totalGold}, CanAfford={canAfford}");

            card.UpdateCard(currentLevel, cost, canAfford);
        }

        private int CalculateCost(int baseCost, int currentLevel)
        {
            // Cost formula: baseCost × (1.5 ^ currentLevel)
            return Mathf.RoundToInt(baseCost * Mathf.Pow(1.5f, currentLevel));
        }

        private async void OnUpgradePurchased(string statKey)
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
            await saveService.SaveData(saveData);

            // Refresh UI
            RefreshUI();

            Debug.Log($"[Shop] Purchased {statKey} level {currentLevel + 1} for {cost}G");
        }

        private async void OnWatchAdClicked(int packNumber)
        {
            Debug.Log($"[Shop] OnWatchAdClicked(Pack {packNumber}) - Starting...");
            
            // Check cooldown
            if (!adService.CanWatchAd(saveData.lastAdWatchedTime))
            {
                int remaining = adService.GetCooldownRemainingSeconds(saveData.lastAdWatchedTime);
                Debug.LogWarning($"[Shop] Ad cooldown active: {remaining}s remaining");
                return;
            }

            Debug.Log($"[Shop] Showing ad for Pack {packNumber}...");
            
            // Show ad
            bool success = await adService.ShowRewardedAd();
            
            Debug.Log($"[Shop] Ad result: {(success ? "SUCCESS" : "FAILED")}");

            if (success)
            {
                // Update ad watch count based on pack
                if (packNumber == 1)
                {
                    saveData.adsWatchedForPack1++;
                    Debug.Log($"[Shop] Pack 1 progress: {saveData.adsWatchedForPack1}/5 ads watched");

                    // Check if pack is complete
                    if (saveData.adsWatchedForPack1 >= 5)
                    {
                        saveData.totalGold += 1990;
                        saveData.adsWatchedForPack1 = 0; // Reset progress
                        Debug.Log($"[Shop] 🎉 Ad Pack 1 COMPLETE! Awarded 1,990 gold. New total: {saveData.totalGold}");
                    }
                }
                else if (packNumber == 2)
                {
                    saveData.adsWatchedForPack2++;
                    Debug.Log($"[Shop] Pack 2 progress: {saveData.adsWatchedForPack2}/10 ads watched");

                    // Check if pack is complete
                    if (saveData.adsWatchedForPack2 >= 10)
                    {
                        saveData.totalGold += 4990;
                        saveData.adsWatchedForPack2 = 0; // Reset progress
                        Debug.Log($"[Shop] 🎉 Ad Pack 2 COMPLETE! Awarded 4,990 gold. New total: {saveData.totalGold}");
                    }
                }

                // Update timestamp for cooldown
                saveData.lastAdWatchedTime = DateTime.Now.ToString("o"); // ISO 8601

                // Save to disk
                await saveService.SaveData(saveData);
                Debug.Log("[Shop] SaveData written to disk");

                // Refresh UI
                RefreshUI();
                Debug.Log("[Shop] UI refreshed");

                Debug.Log($"[Shop] ✅ Ad watched successfully (Pack {packNumber})");
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
                await saveService.SaveData(saveData);
                
                // Refresh UI
                RefreshUI();
                
                Debug.Log("[Shop] IAP purchase successful! +29,900 gold");
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
            Debug.Log($"[ShopController] IAP price retrieved: {price}");
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
                    "SPECIAL OFFER",
                    "Premium gold pack with the best value. Purchase directly to support development!"
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
