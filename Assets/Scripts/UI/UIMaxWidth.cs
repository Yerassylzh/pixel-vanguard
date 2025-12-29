using UnityEngine;
using UnityEngine.UI;

namespace PixelVanguard.UI
{
    /// <summary>
    /// Constrains the RectTransform to a maximum width while maintaining aspect ratio.
    /// Useful for headers/images that should stretch on mobile but not get too huge on tablets/desktop.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class UIMaxWidth : MonoBehaviour
    {
        [Header("Constraint Settings")]
        [Tooltip("The maximum width this element is allowed to be (in pixels).")]
        [SerializeField] private float maxWidth = 800f;
        
        [Tooltip("If true, ensures the height adjusts to maintain aspect ratio based on the new width.")]
        [SerializeField] private bool maintainAspectRatio = false;

        [Header("Debug")]
        [SerializeField] private bool updateEveryFrame = false; // Only use for testing/animations

        private RectTransform _rectTransform;
        private Image _image;
        private float _originalAspectRatio;
        
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _image = GetComponent<Image>();
            
            if (_image != null && _image.sprite != null)
            {
                _originalAspectRatio = _image.sprite.rect.width / _image.sprite.rect.height;
            }
        }

        private void OnEnable()
        {
            ApplyConstraint();
        }

        private void Update()
        {
            if (updateEveryFrame || !Application.isPlaying)
            {
                ApplyConstraint();
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            ApplyConstraint();
        }

        public void ApplyConstraint()
        {
            if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();

            // Get the parent width (or screen width if no parent)
            float parentWidth = 0f;
            if (_rectTransform.parent != null)
            {
                RectTransform parentRect = _rectTransform.parent as RectTransform;
                parentWidth = parentRect.rect.width;
            }
            else
            {
                parentWidth = Screen.width;
            }

            // Determine target width: Smallest of (Parent Width, Max Width)
            float targetWidth = Mathf.Min(parentWidth, maxWidth);
            
            // Allow some tolerance to prevent jitter
            if (Mathf.Abs(_rectTransform.rect.width - targetWidth) < 1f) return;

            // Apply Width
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);

            // Apply Height (Aspect Ratio)
            if (maintainAspectRatio)
            {
                // Try grabbing ratio from AspectRatioFitter first
                var ratioFitter = GetComponent<AspectRatioFitter>();
                float ratio = (ratioFitter != null) ? ratioFitter.aspectRatio : _originalAspectRatio;

                // If we have a valid ratio, set height
                if (ratio > 0)
                {
                    float targetHeight = targetWidth / ratio;
                    _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
                }
            }
        }

        // Auto-update aspect ratio if sprite changes
        public void RefreshAspectRatio()
        {
            if (_image != null && _image.sprite != null)
            {
                _originalAspectRatio = _image.sprite.rect.width / _image.sprite.rect.height;
                ApplyConstraint();
            }
        }
    }
}
