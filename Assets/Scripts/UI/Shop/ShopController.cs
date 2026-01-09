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

        // Helper classes for separation of concerns
        private Shop.UpgradeShopHandler upgradeHandler;
        private Shop.GoldPackHandler goldPackHandler;
        private Shop.IAPHandler iapHandler;
        private Shop.ShopUIHandler uiHandler;

        // Android Special Offer Config
        private const int SPECIAL_OFFER_AD_Target = 20;

        private void Start()
        {            
            // Get services
            cachedSave = ServiceLocator.Get<CachedSaveDataService>();
            adService = ServiceLocator.Get<IAdService>();
            iapService = ServiceLocator.Get<IIAPService>();

            // Initialize helper classes
            upgradeHandler = new Shop.UpgradeShopHandler(
                cachedSave, detailsPanel,
                mightCard, vitalityCard, greavesCard, magnetCard,
                mightIcon, vitalityIcon, greavesIcon, magnetIcon);

            goldPackHandler = new Shop.GoldPackHandler(
                cachedSave, adService,
                adPack1Card, adPack2Card,
                adPack1Icon, adPack2Icon,
                coinRewardAnimator, goldIconTransform, goldText);

            iapHandler = new Shop.IAPHandler(
                cachedSave, adService, iapService,
                iapBuyButton, iapButtonText,
                coinRewardAnimator, goldIconTransform, goldText);

            uiHandler = new Shop.ShopUIHandler(
                cachedSave, goldText, backButton, detailsPanel);

            // Initialize handlers
            upgradeHandler.Initialize();
            upgradeHandler.SubscribeEvents(OnUpgradePurchased);

            goldPackHandler.Initialize();
            goldPackHandler.SubscribeCardClickEvents(OnGoldPackCardClicked);

            // Setup buttons
            backButton.onClick.AddListener(uiHandler.OnBackButtonClicked);
            iapBuyButton.onClick.AddListener(iapHandler.HandleIAPButtonClick);
            
            // Setup IAP card click to show details
            if (iapCardButton != null)
            {
                iapCardButton.onClick.AddListener(OnIAPCardClicked);
            }

            // Subscribe to Yandex purchase success event (WebGL only)
#if UNITY_WEBGL
            YG2.onPurchaseSuccess += OnPurchaseSuccess_GoldPack;
#endif

            // Start cooldown timer UPDATE coroutine for UI refreshing
            cooldownCoroutine = StartCoroutine(ShopUpdateCoroutine.UpdateCooldownTimer(
                cachedSave, adService, goldPackHandler, iapHandler));

            // Initialize UI
            RefreshUI();

            // Subscribe to language change
            LocalizationManager.OnLanguageChanged += OnLanguageChanged;
        }

        private void OnEnable()
        {
            if (upgradeHandler == null || goldPackHandler == null || iapHandler == null || uiHandler == null)
                return; // Not initialized yet

            RefreshUI();
        }


        // Card click delegates (handlers subscribe via their own Initialize methods)
        // private void OnUpgradeCardClicked(UpgradeCard card) { /* Handled by upgradeHandler */ }
        private void OnGoldPackCardClicked(AdPackCard card) { uiHandler.ShowGoldPackDetails(card); }
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

        private void RefreshUI()
        {
            if (cachedSave == null)
            {
                return; // Skip refresh, will be called again from Start()
            }

            // Delegate to handlers
            uiHandler.UpdateGoldDisplay();
            upgradeHandler.RefreshAllCards();
            goldPackHandler.RefreshCards();
            iapHandler.UpdateButton();
        }


        private void OnUpgradePurchased(string statName)
        {
            // Delegate to handler
            bool success = upgradeHandler.PurchaseUpgrade(statName);
            
            if (success)
            {
                // Update UI after purchase
                uiHandler.UpdateGoldDisplay();
            }
        }


        private void OnLanguageChanged()
        {
            // Delegate to handles to update localized text
            upgradeHandler.UpdateLocalization();
            goldPackHandler.UpdateLocalization();
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
            
            if (iapCardButton != null)
            {
                iapCardButton.onClick.RemoveAllListeners();
            }
        }
    }
}
