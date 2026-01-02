using UnityEngine;
using TMPro;
using DG.Tweening;
using System;

namespace PixelVanguard.UI.Animations
{
    /// <summary>
    /// Creates floating text feedback (e.g., "+100 Gold", "-50 Cost").
    /// </summary>
    public class FloatingTextAnimator : MonoBehaviour
    {
        private static GameObject floatingTextPrefab;
        private static Transform canvasTransform;

        [Header("Prefab Settings")]
        [SerializeField] private GameObject floatingTextPrefabReference;

        [Header("Animation Settings")]
        [SerializeField] private float floatDistance = 100f;
        [SerializeField] private float duration = 1f;
        [SerializeField] private Ease easeType = Ease.OutQuad;

        private void Awake()
        {
            // Cache prefab reference
            if (floatingTextPrefabReference != null)
            {
                floatingTextPrefab = floatingTextPrefabReference;
            }

            // Cache canvas transform
            if (canvasTransform == null)
            {
                Canvas canvas = GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    canvasTransform = canvas.transform;
                }
            }
        }

        /// <summary>
        /// Spawn floating text at world position.
        /// </summary>
        public static void Show(string text, Vector3 worldPosition, Color color, float fontSize = 36f)
        {
            // Create text object
            GameObject textObj = new GameObject("FloatingText");
            
            // Set parent to canvas
            if (canvasTransform != null)
            {
                textObj.transform.SetParent(canvasTransform, false);
            }

            // Add TextMeshPro component
            TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = text;
            tmpText.fontSize = fontSize;
            tmpText.color = color;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.fontStyle = FontStyles.Bold;

            // Enable auto-sizing
            tmpText.enableAutoSizing = true;
            tmpText.fontSizeMin = 18;
            tmpText.fontSizeMax = fontSize;

            // Position
            RectTransform rectTransform = textObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 100);

            // Convert world position to screen position
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                Vector2 screenPos = mainCamera.WorldToScreenPoint(worldPosition);
                rectTransform.position = screenPos;
            }
            else
            {
                rectTransform.position = worldPosition;
            }

            // Animate
            Sequence seq = DOTween.Sequence();

            // Float upward
            seq.Append(rectTransform.DOMoveY(rectTransform.position.y + 100f, 1f)
                .SetEase(Ease.OutQuad));

            // Fade out
            seq.Join(tmpText.DOFade(0f, 1f)
                .SetEase(Ease.InQuad));

            // Destroy on complete
            seq.OnComplete(() => Destroy(textObj));
        }

        /// <summary>
        /// Spawn floating text at UI position.
        /// </summary>
        public static void ShowAtUI(string text, Transform uiTransform, Color color, float fontSize = 36f)
        {
            if (uiTransform == null) return;

            Show(text, uiTransform.position, color, fontSize);
        }

        /// <summary>
        /// Show positive value (e.g., "+100").
        /// </summary>
        public static void ShowPositive(string text, Vector3 position)
        {
            Show("+" + text, position, Color.green);
        }

        /// <summary>
        /// Show negative value (e.g., "-50").
        /// </summary>
        public static void ShowNegative(string text, Vector3 position)
        {
            Show("-" + text, position, Color.red);
        }

        /// <summary>
        /// Show gold earned.
        /// </summary>
        public static void ShowGoldEarned(int amount, Vector3 position)
        {
            Color goldColor = new Color(1f, 0.84f, 0f); // Gold color
            Show($"+{amount}", position, goldColor, 42f);
        }

        /// <summary>
        /// Show gold spent.
        /// </summary>
        public static void ShowGoldSpent(int amount, Transform uiPosition)
        {
            Color redColor = new Color(1f, 0.3f, 0.3f);
            ShowAtUI($"-{amount}", uiPosition, redColor, 38f);
        }
    }
}
