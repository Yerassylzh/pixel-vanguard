using UnityEngine;
using System.Threading.Tasks;

namespace PixelVanguard.Services
{
    /// <summary>
    /// Placeholder IAP service for testing.
    /// Replace with real implementation (Google Play, Yandex, App Store).
    /// </summary>
    public class PlaceholderIAPService : IIAPService
    {
        private bool _initialized = false;

        public bool IsInitialized => _initialized;

        public async Task<bool> Initialize()
        {            
            // Simulate initialization delay
            await Task.Delay(500);
            
            _initialized = true;
            return true;
        }

        public async Task<bool> PurchaseProduct(string productId)
        {
            
            // Simulate purchase flow delay
            await Task.Delay(1000);
            
            // Placeholder: always succeeds
            return true;
        }

        public async Task RestorePurchases()
        {
            await Task.CompletedTask;
        }

        public string GetLocalizedPrice(string productId)
        {
            // Placeholder prices
            switch (productId)
            {
                case ProductIDs.GOLD_PACK_LARGE:
                    return "$4.99";
                case ProductIDs.REMOVE_ADS:
                    return "29 YAN";  // Or "$0.99" depending on your monetization
                default:
                    return "---";
            }
        }

        public bool IsProductAvailable(string productId)
        {
            // Placeholder: all products available
            return _initialized;
        }
    }
}
