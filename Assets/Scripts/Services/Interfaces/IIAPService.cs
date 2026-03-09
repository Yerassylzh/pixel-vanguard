using System;
using System.Threading.Tasks;

namespace PixelVanguard.Services
{
    /// <summary>
    /// Interface for in-app purchase services.
    /// Implement this for Google Play, Yandex, App Store, etc.
    /// </summary>
    public interface IIAPService
    {
        /// <summary>
        /// Initialize IAP service synchronously (called by GameBootstrap).
        /// </summary>
        bool Initialize();

        /// <summary>
        /// Async initialization for platforms that require it (e.g., Google Play).
        /// Handles store connection, product fetching, and purchase restoration.
        /// Called by GameBootstrap after Initialize().
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Is the IAP service ready?
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Purchase a product by ID.
        /// </summary>
        /// <param name="productId">Product ID (e.g., "gold_pack")</param>
        /// <returns>True if purchase succeeded</returns>
        Task<bool> PurchaseProduct(string productId);

        /// <summary>
        /// Restore previous purchases (required for Google Play / App Store).
        /// </summary>
        Task RestorePurchases();

        /// <summary>
        /// Get localized price string for a product.
        /// </summary>
        string GetLocalizedPrice(string productId);

        /// <summary>
        /// Check if a product exists and is available for purchase.
        /// </summary>
        bool IsProductAvailable(string productId);
    }

    /// <summary>
    /// Product IDs for IAP offerings.
    /// Platform-specific products are marked accordingly.
    /// </summary>
    public static class ProductIDs
    {
        // Shared: available on all platforms
        public const string GOLD_PACK_LARGE = "gold_pack";  // 29900 gold
        
        // Android/iOS only (NOT for Yandex/WebGL)
        public const string REMOVE_ADS = "remove_ads";      // Remove all ads permanently
    }
}
