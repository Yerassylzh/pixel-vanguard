using UnityEngine;

namespace PixelVanguard.UI
{
    /// <summary>
    /// Adaptive panel that maintains fixed height but adjusts width based on orientation.
    /// - Portrait (height > width): Panel stretches to full width
    /// - Landscape (width >= height): Panel becomes square (width = height)
    /// Useful for Level Up panels that need to adapt to different screen sizes.
    /// </summary>
    [ExecuteAlways] // Runs in Editor for live preview
    public class FixedHeightPanel : MonoBehaviour
    {
        private RectTransform rectTransform;
        private int lastScreenWidth;
        private int lastScreenHeight;

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            
            if (rectTransform == null)
            {
                Debug.LogError("[FixedHeightPanel] RectTransform component not found!");
                return;
            }

            // Initial update
            UpdatePanelSize();
        }

        private void Update()
        {
            if (rectTransform == null) return;

            // Only update when screen size changes (optimization)
            if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
            {
                UpdatePanelSize();
                lastScreenWidth = Screen.width;
                lastScreenHeight = Screen.height;
            }
        }

        private void UpdatePanelSize()
        {
            // Get the current fixed height (set in Inspector via RectTransform)
            float currentFixedHeight = rectTransform.rect.height;

            if (Screen.height > Screen.width)
            {
                // --- PORTRAIT MODE: Full Width ---
                
                // Stretch X anchors to left (0) and right (1) edges
                rectTransform.anchorMin = new Vector2(0f, rectTransform.anchorMin.y);
                rectTransform.anchorMax = new Vector2(1f, rectTransform.anchorMax.y);

                // Reset X offsets so it snaps to the edges
                rectTransform.offsetMin = new Vector2(0f, rectTransform.offsetMin.y);
                rectTransform.offsetMax = new Vector2(0f, rectTransform.offsetMax.y);
            }
            else
            {
                // --- LANDSCAPE MODE: Square (Width = Height) ---

                // Center X anchors at 0.5
                rectTransform.anchorMin = new Vector2(0.5f, rectTransform.anchorMin.y);
                rectTransform.anchorMax = new Vector2(0.5f, rectTransform.anchorMax.y);

                // Set Width to match the Height (make it square)
                Vector2 newSize = rectTransform.sizeDelta;
                newSize.x = currentFixedHeight;
                rectTransform.sizeDelta = newSize;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Auto-update when values change in editor
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }
        }
#endif
    }
}
