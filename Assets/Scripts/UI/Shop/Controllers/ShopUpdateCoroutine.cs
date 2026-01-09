using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PixelVanguard.Core;
using PixelVanguard.Services;
using PixelVanguard.UI.Animations;
using PixelVanguard.UI.Shop;
using System.Threading.Tasks;
using System.Collections;
using System;
#if UNITY_WEBGL
using YG;
#endif

namespace PixelVanguard.UI
{
    /// <summary>
    /// Coroutine helper for updating shop UI every second.
    /// Updates cooldown timers and progress displays.
    /// </summary>
    public class ShopUpdateCoroutine
    {
        public static IEnumerator UpdateCooldownTimer(
            CachedSaveDataService cachedSave,
            IAdService adService,
            GoldPackHandler goldPackHandler,
            IAPHandler iapHandler)
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);

                if (cachedSave != null && adService != null && cachedSave.Data != null)
                {
                    int remaining = adService.GetCooldownRemainingSeconds(cachedSave.Data.lastAdWatchedTime);

                    // Update ad pack cards with cooldown
                    goldPackHandler.UpdateCooldown(remaining);
                    
                    // Update IAP button
                    iapHandler.UpdateButton();
                }
            }
        }
    }
}
