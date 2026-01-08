using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PixelVanguard.Core;
using PixelVanguard.Services;
using PixelVanguard.UI.Animations;
using System.Threading.Tasks;
using System.Collections;
using System;
#if UNITY_WEBGL
using YG;
#endif

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

        [Header("Animation References")]
        [SerializeField] private Transform goldIconTransform; // Gold icon in top bar for animations
        [SerializeField] private CoinRewardAnimator coinRewardAnimator; // Handles coin animations

        private CachedSaveDataService cachedSave;
        private IAdService adService;
        private IIAPService iapService;
        private Coroutine cooldownCoroutine;

        // Android Special Offer Config
        private const int SPECIAL_OFFER_AD_Target = 20;

        private void Awake()
        {            
            // Setup upgrade cards FIRST
            SetupUpgradeCards();

            // Setup gold pack cards
            SetupGoldPackCards();

            // Note: Don't call UpdatePlatformLayout here because it relies on cachedSave.Data 
            // which is not loaded until Start(). Use Start() instead.
        }

        private void Start()
        {            
            // Get services
            cachedSave = ServiceLocator.Get<CachedSaveDataService>();
            adService = ServiceLocator.Get<IAdService>();
            iapService = ServiceLocator.Get<IIAPService>();

            // Setup buttons
            backButton.onClick.AddListener(OnBackClicked);
            iapBuyButton.onClick.AddListener(OnIAPBuyClicked);

            // Subscribe to Yandex purchase success event for consumed purchases (WebGL only)
#if UNITY_WEBGL
            YG2.onPurchaseSuccess += OnPurchaseSuccess_GoldPack;
#endif

            // Start cooldown timer coroutine
            cooldownCoroutine = StartCoroutine(UpdateAdCooldownTimer());

            // Initialize UI
            UpdateAllCards();
            UpdateGoldDisplay();
            UpdatePlatformLayout();
            UpdateIAPButton();

            // Subscribe to language change for dynamic UI refresh
            LocalizationManager.OnLanguageChanged += OnLanguageChanged;
        }

        private void OnEnable()
        {
            // Refresh UI when panel opens
            UpdateAllCards();
            UpdateGoldDisplay();
            
            // Only update platform-specific UI if cachedSave is loaded
            if (cachedSave != null)
            {
                UpdatePlatformLayout();
                UpdateIAPButton();
            }
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
            // Null check - OnEnable can run before Start, so cachedSave might not be initialized yet
            if (cachedSave == null)
            {
                return; // Skip refresh, will be called again from Start()
            }

            // Update gold display (top bar)
            goldText.text = cachedSave.Data.totalGold.ToString();

            // Update upgrade cards
            RefreshUpgradeCard(mightCard, "might");
            RefreshUpgradeCard(vitalityCard, "vitality");
            RefreshUpgradeCard(greavesCard, "greaves");
            RefreshUpgradeCard(magnetCard, "magnet");

            // Update gold pack cards (button shows total gold, like in image)
            adPack1Card.UpdateProgress(cachedSave.Data.adsWatchedForPack1, cachedSave.Data.totalGold);
            adPack2Card.UpdateProgress(cachedSave.Data.adsWatchedForPack2, cachedSave.Data.totalGold);

            // Update IAP button
            UpdateIAPButton();
        }
        
        private void UpdateGoldDisplay()
        {
            if (goldText != null && cachedSave != null)
            {
                goldText.text = cachedSave.TotalGold.ToString();
            }
        }
        
        private void UpdateAllCards()
        {
            RefreshUI();
        }

        private void RefreshUpgradeCard(UpgradeCard card, string statKey)
        {
            int currentLevel = cachedSave.Data.GetStatLevel(statKey);
            int cost = CalculateCost(card.BaseCost, currentLevel);
            bool canAfford = cachedSave.Data.totalGold >= cost;
            
            card.UpdateCard(currentLevel, cost, canAfford);
        }

        private int CalculateCost(int baseCost, int currentLevel)
        {
            // Cost formula: baseCost × (1.5 ^ currentLevel)
            return Mathf.RoundToInt(baseCost * Mathf.Pow(1.5f, currentLevel));
        }

        private void OnUpgradePurchased(string statKey)
        {
            int currentLevel = cachedSave.Data.GetStatLevel(statKey);
            
            if (currentLevel >= 10)
            {
                Debug.LogWarning($"[Shop] {statKey} already at max level!");
                return;
            }

            UpgradeCard card = GetCardForStat(statKey);
            int cost = CalculateCost(card.BaseCost, currentLevel);

            if (cachedSave.Data.totalGold < cost)
            {
                Debug.LogWarning("[Shop] Insufficient gold!");
                return;
            }

            // Deduct gold and increment level
            cachedSave.Data.totalGold -= cost;
            cachedSave.Data.SetStatLevel(statKey, currentLevel + 1);

            // Save to disk
            cachedSave.Save();

            // Refresh UI
            RefreshUI();
        }

        private async void OnWatchAdClicked(int packNumber)
        {
            // Ensure adService is available
            if (adService == null)
            {
                adService = ServiceLocator.Get<IAdService>();
                if (adService == null)
                {
                    Debug.LogError("[Shop] AdService missing! Cannot watch ad.");
                    return;
                }
            }
            
            // Check cooldown
            if (!adService.CanWatchAd(cachedSave.Data.lastAdWatchedTime))
            {
                int remaining = adService.GetCooldownRemainingSeconds(cachedSave.Data.lastAdWatchedTime);
                Debug.LogWarning($"[Shop] Ad cooldown active: {remaining}s remaining");
                return;
            }
            
            // Show ad
            bool success = await adService.ShowRewardedAd();
            
            if (success)
            {
                // Update ad watch count based on pack
                int goldEarned = 0;
                int previousGold = cachedSave.Data.totalGold;
                Transform sourceTransform = null;

                if (packNumber == 1)
                {
                    cachedSave.Data.adsWatchedForPack1++;

                    // Check if pack is complete
                    if (cachedSave.Data.adsWatchedForPack1 >= 5)
                    {
                        goldEarned = 1990;
                        cachedSave.Data.totalGold += goldEarned;
                        cachedSave.Data.adsWatchedForPack1 = 0; // Reset progress
                        sourceTransform = adPack1Card.transform;
                    }
                }
                else if (packNumber == 2)
                {
                    cachedSave.Data.adsWatchedForPack2++;

                    // Check if pack is complete
                    if (cachedSave.Data.adsWatchedForPack2 >= 10)
                    {
                        goldEarned = 4990;
                        cachedSave.Data.totalGold += goldEarned;
                        cachedSave.Data.adsWatchedForPack2 = 0; // Reset progress
                        sourceTransform = adPack2Card.transform;
                    }
                }

                // Update timestamp for cooldown
                cachedSave.Data.lastAdWatchedTime = DateTime.Now.ToString("o"); // ISO 8601

                // Save to disk
                cachedSave.Save();

                // Visual feedback if pack completed
                if (goldEarned > 0 && goldIconTransform != null && sourceTransform != null && coinRewardAnimator != null)
                {
                    // Use new coin reward system
                    coinRewardAnimator.PlayCoinReward(
                        sourceTransform.position,
                        goldIconTransform,
                        goldEarned,
                        goldText,  // Text will count up as coins arrive
                        onComplete: null
                    );
                }
                else
                {
                    // Just refresh UI (ad watched but pack not complete yet)
                    RefreshUI();
                }
            }
            else
            {
                Debug.LogWarning("[Shop] Ad failed to show or was cancelled");
            }
        }

        private void OnIAPBuyClicked()
        {
#if UNITY_ANDROID
            // Android: Watch Ads Logic
            OnWatchSpecialAdClicked();
#else
            // WebGL / Editor (Non-Android target): IAP Logic
            if (iapService == null || !iapService.IsInitialized)
            {
                Debug.LogError("[Shop] IAP service not initialized!");
                return;
            }

            // ... (Existing IAP Logic) ...
            PurchaseIAP_GoldPack();
#endif
        }

        private async void PurchaseIAP_GoldPack()
        {
             bool success = await iapService.PurchaseProduct(ProductIDs.GOLD_PACK_LARGE);

             if (success)
             {
                 GrantGoldPackReward();
             }
             else
             {
                 Debug.LogWarning("[Shop] IAP purchase failed or was cancelled");
             }
        }

        private async void OnWatchSpecialAdClicked()
        {
            if (adService == null)
            {
                 adService = ServiceLocator.Get<IAdService>();
                 if (adService == null) return;
            }
            
            // Check cooldown (same as regular ad packs)
            if (!adService.CanWatchAd(cachedSave.Data.lastAdWatchedTime))
            {
                int remaining = adService.GetCooldownRemainingSeconds(cachedSave.Data.lastAdWatchedTime);
                Debug.LogWarning($"[Shop] Ad cooldown active: {remaining}s remaining");
                return;
            }
            
            // Show rewarded ad
            bool success = await adService.ShowRewardedAd();
            
            if (success)
            {
                cachedSave.Data.adsWatchedForSpecialPack++;
                
                // Update cooldown timestamp
                cachedSave.Data.lastAdWatchedTime = System.DateTime.Now.ToString("o");
                
                if (cachedSave.Data.adsWatchedForSpecialPack >= SPECIAL_OFFER_AD_Target)
                {
                    // Complete!
                    cachedSave.Data.adsWatchedForSpecialPack = 0;
                    GrantGoldPackReward();
                }
                
                cachedSave.Save();
                UpdateIAPButton(); // Refresh progress text
            }
        }

        private void GrantGoldPackReward()
        {
             int goldEarned = 29900;
             cachedSave.Data.totalGold += goldEarned;
             cachedSave.Save();
             
             // Visual feedback: coin animation
             if (goldIconTransform != null && iapBuyButton != null && coinRewardAnimator != null)
             {
                 coinRewardAnimator.PlayCoinReward(
                     iapBuyButton.transform.position,
                     goldIconTransform,
                     goldEarned,
                     goldText,
                     onComplete: null
                 );
             }
             else
             {
                 RefreshUI();
             }
        }

        private void UpdateIAPButton()
        {
            // Guard against null cachedSave.Data (can happen in OnEnable before Start)
            if (cachedSave.Data == null) return;
            
#if UNITY_ANDROID
            // Android: Show Ad Progress OR Cooldown Timer
            if (iapButtonText != null)
            {
                // Check if there's a cooldown active
                if (adService != null)
                {
                    int remaining = adService.GetCooldownRemainingSeconds(cachedSave.Data.lastAdWatchedTime);
                    if (remaining > 0)
                    {
                        // Show cooldown timer
                        iapButtonText.text = $"{remaining}s";
                        if (iapBuyButton != null) iapBuyButton.interactable = false;
                        return;
                    }
                }
                
                // No cooldown - show progress
                iapButtonText.text = $"{cachedSave.Data.adsWatchedForSpecialPack}/{SPECIAL_OFFER_AD_Target}";
                if (iapBuyButton != null) iapBuyButton.interactable = true;
            }
#else
            // Standard IAP (WebGL / Non-Android Editor)
            if (iapService == null)
            {
                if (iapButtonText) iapButtonText.text = "---";
                return;
            }
            
            if (!iapService.IsInitialized)
            {
                if (iapButtonText) iapButtonText.text = "---";
                return;
            }
            
            string price = iapService.GetLocalizedPrice(ProductIDs.GOLD_PACK_LARGE);
            if (iapButtonText) iapButtonText.text = price;
#endif
        }

        private void UpdatePlatformLayout()
        {
#if UNITY_ANDROID
             // Android Special Setup
             // Change IAP card visual to look like "Free Ad Pack"
             
             if (iapBuyButton != null) 
             {
                 iapBuyButton.interactable = true;
             }
             
             // Ensure UpdateIAPButton is called next frame or immediately to set text
             UpdateIAPButton();
#endif
        }

        private void OnLanguageChanged()
        {
            // Re-initialize cards to update localized static text (Name, Desc)
            SetupUpgradeCards();
            SetupGoldPackCards();
            
            // Refresh dynamic UI (Labels, Buttons)
            RefreshUI();
        }

#if UNITY_WEBGL
        /// <summary>
        /// Yandex purchase success event handler for consumed purchases.
        /// Called when YG2.ConsumePurchases() processes unconsumed gold pack purchases.
        /// Also called for normal purchases as a secondary event (already handled inline).
        /// NOTE: Remove Ads is Android-only, not handled here.
        /// </summary>
        private void OnPurchaseSuccess_GoldPack(string productId)
        {
            // Only handle gold pack purchases
            if (productId != ProductIDs.GOLD_PACK_LARGE)
            {
                return;
            }

            Debug.Log($"[ShopController] Purchase success event received for: {productId}");

            // Check if this shop panel is active (if not, this is likely a consumed purchase at startup)
            bool isConsumingAtStartup = !shopPanel.activeSelf;

            if (isConsumingAtStartup)
            {
                Debug.Log("[ShopController] Consuming unconsumed gold pack purchase from startup");
            }

            // Grant gold reward
            int goldEarned = 29900;
            cachedSave.Data.totalGold += goldEarned;
            cachedSave.Save();

            // Only play animation if shop is open (don't animate during startup consuming)
            if (!isConsumingAtStartup && goldIconTransform != null && iapBuyButton != null && coinRewardAnimator != null)
            {
                coinRewardAnimator.PlayCoinReward(
                    iapBuyButton.transform.position,
                    goldIconTransform,
                    goldEarned,
                    goldText,
                    onComplete: null
                );
            }
            else
            {
                // Just refresh UI (startup consuming or missing animation components)
                RefreshUI();
            }
        }
#endif

        private IEnumerator UpdateAdCooldownTimer()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);

                if (cachedSave.Data != null && adService != null)
                {
                    int remaining = adService.GetCooldownRemainingSeconds(cachedSave.Data.lastAdWatchedTime);

                    // Update both ad pack cards with same cooldown
                    adPack1Card.UpdateCooldown(remaining);
                    adPack2Card.UpdateCooldown(remaining);
                    
                    // Update Special Offer (IAP button) cooldown display
                    UpdateIAPButton();

                    // When cooldown ends, refresh to show gold again
                    if (remaining == 0)
                    {
                        adPack1Card.UpdateProgress(cachedSave.Data.adsWatchedForPack1, cachedSave.Data.totalGold);
                        adPack2Card.UpdateProgress(cachedSave.Data.adsWatchedForPack2, cachedSave.Data.totalGold);
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

            // Unsubscribe from Yandex events
#if UNITY_WEBGL
            YG2.onPurchaseSuccess -= OnPurchaseSuccess_GoldPack;
#endif

            // Remove listeners
            LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
            
            backButton.onClick.RemoveAllListeners();
            iapBuyButton.onClick.RemoveAllListeners();
        }
    }
}
