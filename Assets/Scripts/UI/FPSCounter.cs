using UnityEngine;
using TMPro;
using System.Collections;

namespace PixelVanguard.UI
{
    /// <summary>
    /// Displays FPS (Frames Per Second) counter.
    /// Toggleable via Settings. Persists via PlayerPrefs.
    /// </summary>
    public class FPSCounter : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI fpsText;
        
        [Header("Update Settings")]
        [Tooltip("How often to refresh the FPS display (in seconds)")]
        [SerializeField] private float updateInterval = 0.5f;

        private float _accumulatedTime;
        private int _frameCount;
        private float _currentFPS;
        private bool _isVisible;

        private const string PREF_KEY = "ShowFPS";

        private void Start()
        {
            // Load visibility preference
            _isVisible = PlayerPrefs.GetInt(PREF_KEY, 0) == 1;
            UpdateVisibility();

            // Start FPS calculation coroutine
            StartCoroutine(CalculateFPS());
        }

        private void Update()
        {
            if (!_isVisible) return;

            // Accumulate time and frames
            _accumulatedTime += Time.unscaledDeltaTime;
            _frameCount++;
        }

        private IEnumerator CalculateFPS()
        {
            while (true)
            {
                yield return new WaitForSeconds(updateInterval);

                if (_isVisible && _accumulatedTime > 0f)
                {
                    _currentFPS = _frameCount / _accumulatedTime;
                    
                    // Update text
                    if (fpsText != null)
                    {
                        fpsText.text = $"FPS: {_currentFPS:F1}";
                        
                        // Color-code based on performance
                        if (_currentFPS >= 55f)
                            fpsText.color = Color.green;
                        else if (_currentFPS >= 30f)
                            fpsText.color = Color.yellow;
                        else
                            fpsText.color = Color.red;
                    }

                    // Reset counters
                    _accumulatedTime = 0f;
                    _frameCount = 0;
                }
            }
        }

        /// <summary>
        /// Toggle FPS counter visibility (called from Settings).
        /// </summary>
        public void SetVisible(bool visible)
        {
            _isVisible = visible;
            PlayerPrefs.SetInt(PREF_KEY, visible ? 1 : 0);
            PlayerPrefs.Save();
            UpdateVisibility();
        }

        /// <summary>
        /// Get current visibility state.
        /// </summary>
        public bool IsVisible()
        {
            return _isVisible;
        }

        private void UpdateVisibility()
        {
            if (fpsText != null)
            {
                fpsText.gameObject.SetActive(_isVisible);
            }
        }

        /// <summary>
        /// Static helper to get current setting value (for Settings UI).
        /// </summary>
        public static bool GetSavedVisibility()
        {
            return PlayerPrefs.GetInt(PREF_KEY, 0) == 1;
        }
    }
}
