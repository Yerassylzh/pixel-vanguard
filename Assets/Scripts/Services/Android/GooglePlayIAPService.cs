#if UNITY_ANDROID
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;

namespace PixelVanguard.Services
{
    /// <summary>
    /// Google Play IAP implementation using Unity IAP v5.
    /// Handles initialization, purchases, confirmation, and non-consumable restore.
    /// </summary>
    public class GooglePlayIAPService : IIAPService
    {
        private StoreController m_StoreController;
        private bool _initialized = false;
        private bool _productsFetched = false;

        // Cached products for price lookups and entitlement checks
        private readonly Dictionary<string, Product> _cachedProducts = new();

        // Task completion sources for async flows
        private TaskCompletionSource<bool> _initTcs;
        private TaskCompletionSource<bool> _purchaseTcs;
        private string _pendingPurchaseProductId;

        // De-duplication: track processed purchases to prevent double-granting
        private readonly HashSet<string> _processedPurchaseKeys = new();

        private const int GOLD_PACK_REWARD = 29900;

        public bool IsInitialized => _initialized;

        // ============================================================
        // INITIALIZATION
        // ============================================================

        public bool Initialize()
        {
            // Synchronous part: get StoreController reference
            m_StoreController = UnityIAPServices.StoreController();
            Debug.Log("[GooglePlayIAP] StoreController obtained.");
            return true;
        }

        public async Task InitializeAsync()
        {
            if (_initialized)
            {
                Debug.Log("[GooglePlayIAP] Already initialized, skipping.");
                return;
            }

            _initTcs = new TaskCompletionSource<bool>();

            try
            {
                // Step 1: Subscribe to all events BEFORE connecting
                SubscribeEvents();

                // Step 2: Connect to the store
                Debug.Log("[GooglePlayIAP] Connecting to store...");
                await m_StoreController.Connect();
                Debug.Log("[GooglePlayIAP] Connected to store.");

                // Step 3: Fetch products
                var products = new List<ProductDefinition>
                {
                    new(ProductIDs.REMOVE_ADS, ProductType.NonConsumable),
                    new(ProductIDs.GOLD_PACK_LARGE, ProductType.Consumable)
                };
                Debug.Log("[GooglePlayIAP] Fetching products...");
                m_StoreController.FetchProducts(products);

                // Wait for the full init chain: FetchProducts → OnProductsFetched → FetchPurchases → OnPurchasesFetched
                bool success = await _initTcs.Task;

                if (success)
                {
                    _initialized = true;
                    Debug.Log("[GooglePlayIAP] Initialization complete! ✅");
                }
                else
                {
                    Debug.LogError("[GooglePlayIAP] Initialization failed.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GooglePlayIAP] Initialization exception: {ex.Message}");
                _initTcs?.TrySetResult(false);
            }
        }

        private void SubscribeEvents()
        {
            m_StoreController.OnPurchasePending += OnPurchasePending;
            m_StoreController.OnPurchaseConfirmed += OnPurchaseConfirmed;
            m_StoreController.OnPurchaseFailed += OnPurchaseFailed;
            m_StoreController.OnProductsFetched += OnProductsFetched;
            m_StoreController.OnPurchasesFetched += OnPurchasesFetched;
            m_StoreController.OnCheckEntitlement += OnCheckEntitlement;
        }

        // ============================================================
        // EVENT HANDLERS
        // ============================================================

        private void OnProductsFetched(List<Product> products)
        {
            Debug.Log($"[GooglePlayIAP] Products fetched: {products.Count}");

            // Cache products for price lookups and entitlement checks
            _cachedProducts.Clear();
            foreach (var product in products)
            {
                _cachedProducts[product.definition.id] = product;
                Debug.Log($"  - {product.definition.id}: {product.metadata.localizedPriceString} ({product.definition.type})");
            }

            _productsFetched = true;

            // Step 4: Fetch existing purchases for restore
            Debug.Log("[GooglePlayIAP] Fetching existing purchases...");
            m_StoreController.FetchPurchases();
        }

        private void OnPurchasesFetched(Orders orders)
        {
            Debug.Log("[GooglePlayIAP] Purchases fetched.");

            // Restore non-consumable entitlements from confirmed orders
            if (orders.ConfirmedOrders != null)
            {
                foreach (var confirmedOrder in orders.ConfirmedOrders)
                {
                    var product = confirmedOrder.CartOrdered.Items().FirstOrDefault()?.Product;
                    if (product != null && product.definition.type != ProductType.Consumable)
                    {
                        // This is a non-consumable — restore entitlement
                        string productId = product.definition.id;
                        Debug.Log($"[GooglePlayIAP] Restoring entitlement: {productId}");
                        RestoreEntitlement(productId);
                    }
                }
            }

            // Handle any pending orders that weren't confirmed yet (crash recovery)
            if (orders.PendingOrders != null)
            {
                Debug.Log($"[GooglePlayIAP] {orders.PendingOrders.Count()} pending orders found.");
            }

            // Initialization chain complete
            _initTcs?.TrySetResult(true);
        }

        private void OnPurchasePending(PendingOrder order)
        {
            var item = order.CartOrdered.Items().FirstOrDefault();
            if (item == null)
            {
                Debug.LogError("[GooglePlayIAP] OnPurchasePending: No items in cart!");
                m_StoreController.ConfirmPurchase(order);
                return;
            }

            string productId = item.Product.definition.id;
            // Use product ID as a de-duplication key for this session
            string purchaseKey = productId + "_" + DateTime.UtcNow.Ticks;

            Debug.Log($"[GooglePlayIAP] Purchase pending: {productId}");

            // De-duplication: for non-consumables, only grant once per session
            if (item.Product.definition.type == ProductType.NonConsumable
                && _processedPurchaseKeys.Any(k => k.StartsWith(productId + "_")))
            {
                Debug.LogWarning($"[GooglePlayIAP] Non-consumable already granted this session: {productId}");
                m_StoreController.ConfirmPurchase(order);
                return;
            }

            // Fulfill the purchase
            switch (productId)
            {
                case ProductIDs.REMOVE_ADS:
                    GrantRemoveAds();
                    break;

                case ProductIDs.GOLD_PACK_LARGE:
                    GrantGoldPack();
                    break;

                default:
                    Debug.LogWarning($"[GooglePlayIAP] Unknown product in pending order: {productId}");
                    break;
            }

            // Mark as processed for de-duplication
            _processedPurchaseKeys.Add(purchaseKey);

            // CRITICAL: Confirm the purchase so the store doesn't refund it
            m_StoreController.ConfirmPurchase(order);
            Debug.Log($"[GooglePlayIAP] Purchase confirmed: {productId}");

            // Complete the purchase task if one is pending
            if (productId == _pendingPurchaseProductId)
            {
                _purchaseTcs?.TrySetResult(true);
            }
        }

        private void OnPurchaseConfirmed(Order confirmedOrder)
        {
            var product = confirmedOrder.CartOrdered.Items().FirstOrDefault()?.Product;
            string productId = product?.definition.id ?? "unknown";
            Debug.Log($"[GooglePlayIAP] Purchase confirmed by store: {productId}");
        }

        private void OnPurchaseFailed(FailedOrder failedOrder)
        {
            string reason = failedOrder.FailureReason.ToString();
            Debug.LogWarning($"[GooglePlayIAP] Purchase failed: {reason}");

            // Complete the purchase task with failure
            _purchaseTcs?.TrySetResult(false);
        }

        private void OnCheckEntitlement(Entitlement entitlement)
        {
            string productId = entitlement.Product.definition.id;
            Debug.Log($"[GooglePlayIAP] Entitlement check: {productId} = {entitlement.Status}");

            if (productId == ProductIDs.REMOVE_ADS)
            {
                if (entitlement.Status == EntitlementStatus.FullyEntitled ||
                    entitlement.Status == EntitlementStatus.EntitledButNotFinished ||
                    entitlement.Status == EntitlementStatus.EntitledUntilConsumed)
                {
                    RestoreEntitlement(productId);
                }
                else
                {
                    Debug.Log($"[GooglePlayIAP] remove_ads not entitled: {entitlement.Status}");
                }
            }
        }

        // ============================================================
        // PURCHASE API
        // ============================================================

        public async Task<bool> PurchaseProduct(string productId)
        {
            if (!_initialized)
            {
                Debug.LogError("[GooglePlayIAP] Cannot purchase — not initialized!");
                return false;
            }

            if (!_cachedProducts.TryGetValue(productId, out var product))
            {
                Debug.LogError($"[GooglePlayIAP] Product not found: {productId}");
                return false;
            }

            Debug.Log($"[GooglePlayIAP] Initiating purchase: {productId}");

            _pendingPurchaseProductId = productId;
            _purchaseTcs = new TaskCompletionSource<bool>();

            // Initiate the purchase — result comes via OnPurchasePending or OnPurchaseFailed
            m_StoreController.PurchaseProduct(product);

            bool result = await _purchaseTcs.Task;
            _pendingPurchaseProductId = null;

            return result;
        }

        public async Task RestorePurchases()
        {
            if (!_initialized || !_productsFetched)
            {
                Debug.LogWarning("[GooglePlayIAP] Cannot restore — not initialized!");
                return;
            }

            // Use CheckEntitlement for remove_ads specifically
            if (_cachedProducts.TryGetValue(ProductIDs.REMOVE_ADS, out var removeAdsProduct))
            {
                Debug.Log("[GooglePlayIAP] Checking entitlement for remove_ads...");
                m_StoreController.CheckEntitlement(removeAdsProduct);
            }

            await Task.CompletedTask;
        }

        // ============================================================
        // PRODUCT INFO
        // ============================================================

        public string GetLocalizedPrice(string productId)
        {
            if (_cachedProducts.TryGetValue(productId, out var product))
            {
                return $"{product.metadata.localizedPriceString} {product.metadata.isoCurrencyCode}";
            }

            return "---";
        }

        public bool IsProductAvailable(string productId)
        {
            return _initialized && _cachedProducts.ContainsKey(productId);
        }

        // ============================================================
        // FULFILLMENT HELPERS
        // ============================================================

        private void GrantRemoveAds()
        {
            var cachedSave = Core.ServiceLocator.TryGet<CachedSaveDataService>(out var svc)
                ? svc : null;

            if (cachedSave != null)
            {
                cachedSave.Data.adsRemoved = true;
                cachedSave.Save();
                Debug.Log("[GooglePlayIAP] ✅ Ads removed!");
            }
            else
            {
                // Fallback: persist directly to PlayerPrefs in case ServiceLocator isn't ready
                PlayerPrefs.SetInt("iap_ads_removed", 1);
                PlayerPrefs.Save();
                Debug.LogWarning("[GooglePlayIAP] CachedSaveDataService not available, saved to PlayerPrefs as fallback.");
            }
        }

        private void GrantGoldPack()
        {
            var cachedSave = Core.ServiceLocator.TryGet<CachedSaveDataService>(out var svc)
                ? svc : null;

            if (cachedSave != null)
            {
                cachedSave.AddGold(GOLD_PACK_REWARD);
                Debug.Log($"[GooglePlayIAP] ✅ Granted {GOLD_PACK_REWARD} gold!");
            }
            else
            {
                Debug.LogError("[GooglePlayIAP] Cannot grant gold — CachedSaveDataService not available!");
            }
        }

        private void RestoreEntitlement(string productId)
        {
            if (productId == ProductIDs.REMOVE_ADS)
            {
                GrantRemoveAds();
                Debug.Log("[GooglePlayIAP] ✅ remove_ads entitlement restored!");
            }
        }
    }
}
#endif
