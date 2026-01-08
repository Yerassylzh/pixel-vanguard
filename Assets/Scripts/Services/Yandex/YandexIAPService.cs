#if UNITY_WEBGL
using System;
using System.Threading.Tasks;
using UnityEngine;
using YG;

namespace PixelVanguard.Services
{
    /// <summary>
    /// IAP service implementation for Yandex Games (WebGL).
    /// Uses Yandex Payments API via PluginYG.
    /// </summary>
    public class YandexIAPService : IIAPService
    {
        private bool _initialized = false;
        private TaskCompletionSource<bool> purchaseTCS;

        public bool IsInitialized => _initialized;

        public async Task<bool> Initialize()
        {
            // Subscribe to Yandex payment events
            YG2.onPurchaseSuccess += OnPurchaseSuccess;
            YG2.onPurchaseFailed += OnPurchaseFailed;
            YG2.onGetPayments += OnGetPayments;

            _initialized = true;
            
            // Wait a frame for PluginYG to fully initialize
            await Task.Yield();
            
            // YANDEX REQUIREMENT: Consume unconsumed purchases at startup
            ConsumePurchases();
            
            return true;
        }

        /// <summary>
        /// Check for and consume unconsumed purchases.
        /// Called once at startup per Yandex requirements.
        /// Sets onPurchaseSuccess = true to trigger existing reward logic in ShopController/SettingsController.
        /// </summary>
        private void ConsumePurchases()
        {
            Debug.Log("[YandexIAPService] Checking for unconsumed purchases...");
            
            // This will automatically trigger onPurchaseSuccess for each unconsumed purchase
            // onPurchaseSuccess = true means our event handlers will be called
            YG2.ConsumePurchases(onPurchaseSuccess: true);
        }

        public async Task<bool> PurchaseProduct(string productId)
        {
            if (!_initialized)
            {
                Debug.LogError("[YandexIAPService] Cannot purchase - service not initialized");
                return false;
            }

            // Create task completion source for async purchase
            purchaseTCS = new TaskCompletionSource<bool>();

            // Call Yandex purchase method
            YG2.BuyPayments(productId);

            // Wait for purchase result (success or failure)
            bool result = await purchaseTCS.Task;

            return result;
        }

        public async Task RestorePurchases()
        {
            await Task.CompletedTask;
        }

        public string GetLocalizedPrice(string productId)
        {
            // Get price from Yandex purchases catalog
            if (YG2.purchases != null && YG2.purchases.Length > 0)
            {
                var purchase = YG2.PurchaseByID(productId);
                if (purchase != null)
                {
                    return purchase.price; // Format: "<price> <currency code>"
                }
            }

            // Fallback if product not found
            return "---";
        }

        public bool IsProductAvailable(string productId)
        {
            if (!_initialized) return false;

            // Check if product exists in Yandex catalog
            if (YG2.purchases != null && YG2.purchases.Length > 0)
            {
                var purchase = YG2.PurchaseByID(productId);
                return purchase != null;
            }

            return false;
        }

        // === Yandex Event Handlers ===

        private void OnPurchaseSuccess(string productId)
        {            
            // Complete the purchase task with success
            purchaseTCS?.TrySetResult(true);
        }

        private void OnPurchaseFailed(string productId)
        {
            Debug.LogWarning($"[YandexIAPService] Purchase failed: {productId}");
            
            // Complete the purchase task with failure
            purchaseTCS?.TrySetResult(false);
        }

        private void OnGetPayments()
        {
            // Log available products
            if (YG2.purchases != null)
            {
                Debug.Log($"[YandexIAPService] {YG2.purchases.Length} products available");
                foreach (var purchase in YG2.purchases)
                {
                    Debug.Log($"  - {purchase.id}: {purchase.title} ({purchase.price})");
                }
            }
        }
    }
}
#endif
