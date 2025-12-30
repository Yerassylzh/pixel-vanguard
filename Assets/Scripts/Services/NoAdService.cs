// using UnityEngine;

// namespace PixelVanguard.Services
// {
//     /// <summary>
//     /// No-operation ad service for platforms without ads (desktop, editor).
//     /// Always returns false/fails gracefully.
//     /// </summary>
//     public class NoAdService : IAdService
//     {
//         public bool IsRewardedAdReady()
//         {
//             return false;
//         }

//         public void ShowRewardedAd(System.Action<bool> onComplete)
//         {
//             Debug.LogWarning("[NoAdService] Ads not available on this platform.");
//             onComplete?.Invoke(false);
//         }

//         public bool IsInterstitialAdReady()
//         {
//             return false;
//         }

//         public void ShowInterstitialAd(System.Action onComplete)
//         {
//             Debug.LogWarning("[NoAdService] Ads not available on this platform.");
//             onComplete?.Invoke();
//         }

//         public System.Threading.Tasks.Task Initialize()
//         {
//             Debug.Log("[NoAdService] Initialized (No ads on this platform)");
//             return System.Threading.Tasks.Task.CompletedTask;
//         }
//     }
// }
