using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PixelVanguard.UI
{
    /// <summary>
    /// Floating virtual joystick for mobile touch controls.
    /// Appears where you touch, auto-hides when released.
    /// Provides normalized direction vector for player movement.
    /// </summary>
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Joystick Components")]
        [SerializeField] private RectTransform joystickContainer;
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;

        [Header("Settings")]
        [SerializeField] private float handleRange = 50f;
        [SerializeField] private bool hideOnDesktop = true;

        private Vector2 inputDirection = Vector2.zero;
        private Canvas canvas;
        private RectTransform canvasRect;

        public Vector2 Direction => inputDirection;

        private void Start()
        {
            canvas = GetComponentInParent<Canvas>();
            canvasRect = canvas.GetComponent<RectTransform>();

            // Hide joystick initially
            if (joystickContainer != null)
            {
                joystickContainer.gameObject.SetActive(false);
            }

            // Hide entire UI on desktop if enabled
            if (hideOnDesktop && Core.PlatformDetector.Instance != null)
            {
                gameObject.SetActive(Core.PlatformDetector.Instance.IsMobile());
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // Show joystick at touch position
            if (joystickContainer != null)
            {
                joystickContainer.gameObject.SetActive(true);
                
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    eventData.position,
                    canvas.worldCamera,
                    out localPoint
                );
                
                joystickContainer.anchoredPosition = localPoint;
            }

            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (background == null || handle == null) return;

            Vector2 localPoint;
            
            // Convert touch position to local point relative to background
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background,
                eventData.position,
                canvas.worldCamera,
                out localPoint))
            {
                // Calculate direction
                Vector2 offset = localPoint;
                
                // Limit to circular area
                inputDirection = offset.magnitude > handleRange 
                    ? offset.normalized 
                    : offset / handleRange;

                // Move handle within range
                handle.anchoredPosition = inputDirection * handleRange;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // Reset and hide
            inputDirection = Vector2.zero;
            
            if (handle != null)
            {
                handle.anchoredPosition = Vector2.zero;
            }
            
            if (joystickContainer != null)
            {
                joystickContainer.gameObject.SetActive(false);
            }
        }
    }
}
