using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PixelVanguard.UI
{
    /// <summary>
    /// Floating virtual joystick for mobile touch controls.
    /// Appears where you touch, auto-hides when released.
    /// Disabled during pause/level-up/game-over.
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
        private bool isInputBlocked = false;
        private Image touchAreaImage; // Fullscreen image for touch detection

        public Vector2 Direction => isInputBlocked ? Vector2.zero : inputDirection;

        private void Start()
        {
            canvas = GetComponentInParent<Canvas>();
            canvasRect = canvas.GetComponent<RectTransform>();
            touchAreaImage = GetComponent<Image>();

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

        private void OnEnable()
        {
            // Subscribe to game state events
            Core.GameEvents.OnGamePause += BlockInput;
            Core.GameEvents.OnGameResume += UnblockInput;
            Core.GameEvents.OnPlayerLevelUp += BlockInput;
            Core.GameEvents.OnGameOver += OnGameOver;
            Core.GameEvents.OnPlatformChanged += OnPlatformChanged;
        }

        private void OnDisable()
        {
            Core.GameEvents.OnGamePause -= BlockInput;
            Core.GameEvents.OnGameResume -= UnblockInput;
            Core.GameEvents.OnPlayerLevelUp -= UnblockInput;
            Core.GameEvents.OnGameOver -= OnGameOver;
            Core.GameEvents.OnPlatformChanged -= OnPlatformChanged;
        }

        private void BlockInput()
        {
            isInputBlocked = true;
            ReleaseJoystick();
            
            // Disable raycast target so clicks pass through to pause menu
            if (touchAreaImage != null)
            {
                touchAreaImage.raycastTarget = false;
            }
        }

        private void UnblockInput()
        {
            isInputBlocked = false;
            
            // Re-enable raycast target for touch detection
            if (touchAreaImage != null)
            {
                touchAreaImage.raycastTarget = true;
            }
        }

        private void OnGameOver(Core.GameOverReason reason)
        {
            BlockInput();
        }

        private void OnPlatformChanged(Core.PlatformType platform)
        {
            // Show/hide based on new platform
            if (hideOnDesktop)
            {
                bool isMobile = platform == Core.PlatformType.NativeMobile || platform == Core.PlatformType.WebMobile;
                gameObject.SetActive(isMobile);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (isInputBlocked) return;

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
            
            // CRITICAL FIX: Reset handle to center BEFORE OnDrag calculates position
            // Must happen after joystickContainer is positioned
            if (handle != null)
            {
                handle.anchoredPosition = Vector2.zero;
            }
            
            // Reset input direction
            inputDirection = Vector2.zero;

            // Now calculate drag from center
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isInputBlocked || background == null || handle == null) return;

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
            ReleaseJoystick();
        }

        private void ReleaseJoystick()
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
