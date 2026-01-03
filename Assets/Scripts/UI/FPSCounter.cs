using UnityEngine;
using TMPro;
using System.Collections;
using PixelVanguard.Core;

namespace PixelVanguard.UI
{
    /// <summary>
    /// Displays FPS (Frames Per Second) counter.
    /// Visibility is controlled by GameSettings service.
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
        
        // Cache GameSettings reference
        private GameSettings _gameSettings;

        private void Start()
        {
            // Get settings service
            _gameSettings = ServiceLocator.Get<GameSettings>();
            
            // Initial sync
            UpdateVisibility();

            // Start FPS calculation coroutine
            StartCoroutine(CalculateFPS());
        }

        private void Update()
        {
            // Sync visibility with settings every frame (or could assume SettingsController updates it)
            // Ideally we'd use an event, but polling settings is cheap enough here
            if (_gameSettings != null)
            {
                bool shouldBeVisible = _gameSettings.ShowFPS;
                if (_isVisible != shouldBeVisible)
                {
                    SetVisible(shouldBeVisible);
                }
            }

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
                        fpsText.text = $"{Core.LocalizationManager.Get("ui.hud.fps")}: {_currentFPS:F1}";

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
        /// Update local visibility state.
        /// Actual persistence is handled by GameSettings.
        /// </summary>
        public void SetVisible(bool visible)
        {
            _isVisible = visible;
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            if (fpsText != null)
            {
                fpsText.gameObject.SetActive(_isVisible);
            }
        }
    }
}
