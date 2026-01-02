using UnityEngine;
using DG.Tweening;
using System;

namespace PixelVanguard.UI.Animations
{
    /// <summary>
    /// Vampire Survivors-style main menu intro animation.
    /// Background scales from bottom, UI elements fade in sequentially.
    /// Play intro music when animation starts.
    /// </summary>
    public class MainMenuIntroAnimator : MonoBehaviour
    {
        [Header("Background Animation")]
        [Tooltip("Can be UI Image or SpriteRenderer GameObject")]
        [SerializeField] private Transform backgroundObject; 
        [SerializeField] private float backgroundAnimationDuration = 5f; // Increased for intro feel
        [SerializeField] private Ease backgroundEase = Ease.OutCubic;

        [Header("Background Scale Settings")]
        [Tooltip("Start scale (Zoomed In). Standard 'Scale Out' effect: Start > 1, End = 1")]
        [SerializeField] private Vector3 startScale = new Vector3(1.5f, 1.5f, 1f); // Uniform scale
        [Tooltip("If true, animates to the object's current scale in scene. If false, uses Target Scale.")]
        [SerializeField] private bool useCurrentScaleAsTarget = true;
        [SerializeField] private Vector3 targetScale = Vector3.one;

        [Header("Background Position Settings (Optional)")]
        [Tooltip("Enable if you need to slide the object while scaling (e.g. to simulate pivot)")]
        [SerializeField] private bool animatePosition = false;
        [SerializeField] private Vector3 startPosition;
        [Tooltip("If true, animates to the object's current position in scene.")]
        [SerializeField] private bool useCurrentPositionAsTarget = true;
        [SerializeField] private Vector3 targetPosition;

        [Header("UI Elements (in order of appearance)")]
        [SerializeField] private CanvasGroup[] uiElements; // Title, buttons, etc.
        [SerializeField] private float elementFadeDelay = 0.15f; // Delay between each element
        [SerializeField] private float elementFadeDuration = 0.5f;

        [Header("Audio")]
        [SerializeField] private AudioClip introMusicClip; // Optional intro music
        [SerializeField] private bool playIntroMusic = true;

        [Header("Settings")]
        [SerializeField] private bool playOnStart = true;
        [SerializeField] private bool disableInteractionDuringIntro = true;
        [Tooltip("If true, intro only plays once per app session. Returns to normal menu immediately on subsequent visits.")]
        [SerializeField] private bool playOncePerSession = true;

        // Static flag to track if intro has been shown in this session
        private static bool hasShownIntro = false;
        private bool isPlaying = false; // Local flag for this instance

        private Vector3 initialOriginalScale;
        private Vector3 initialOriginalPos;

        private void Awake()
        {
            if (backgroundObject != null)
            {
                initialOriginalScale = backgroundObject.localScale;
                initialOriginalPos = backgroundObject.localPosition;
            }
        }

        private void Start()
        {
            if (playOnStart)
            {
                // Check if we should skip
                if (playOncePerSession && hasShownIntro)
                {
                    SkipIntro(); // Instant setup
                }
                else
                {
                    PlayIntro();
                    hasShownIntro = true;
                }
            }
        }

        /// <summary>
        /// Play the full intro animation sequence.
        /// </summary>
        public void PlayIntro(Action onComplete = null)
        {
            if (isPlaying) return;
            isPlaying = true;

            // Determine targets
            Vector3 finalScale = useCurrentScaleAsTarget ? initialOriginalScale : targetScale;
            Vector3 finalPos = useCurrentPositionAsTarget ? initialOriginalPos : targetPosition;

            // Play one-shot intro music
            if (playIntroMusic && introMusicClip != null && Core.AudioManager.Instance != null)
            {
                Core.AudioManager.Instance.PlayOneShotMusic(introMusicClip);
            }

            // Setup initial states
            SetupInitialStates(finalScale, finalPos);

            // Create animation sequence
            Sequence introSeq = DOTween.Sequence();

            // 1. Animate Background
            if (backgroundObject != null)
            {
                // Scale
                introSeq.Append(backgroundObject.DOScale(finalScale, backgroundAnimationDuration).SetEase(backgroundEase));
                
                // Position (concurrently)
                if (animatePosition)
                {
                    introSeq.Join(backgroundObject.DOLocalMove(finalPos, backgroundAnimationDuration).SetEase(backgroundEase));
                }
            }

            // 2. Fade in UI elements sequentially
            if (uiElements != null && uiElements.Length > 0)
            {
                float currentDelay = backgroundAnimationDuration * 0.5f; // Start halfway through background

                foreach (var element in uiElements)
                {
                    if (element != null)
                    {
                        introSeq.Insert(currentDelay, AnimateUIElement(element));
                        currentDelay += elementFadeDelay;
                    }
                }
            }

            // 3. On complete
            introSeq.OnComplete(() =>
            {
                EnableInteraction();
                onComplete?.Invoke();
            });
        }

        private void SetupInitialStates(Vector3 finalScale, Vector3 finalPos)
        {
            // Hide all UI elements
            if (uiElements != null)
            {
                foreach (var element in uiElements)
                {
                    if (element != null)
                    {
                        element.alpha = 0f;
                        if (disableInteractionDuringIntro)
                        {
                            element.interactable = false;
                        }
                    }
                }
            }

            // Setup background
            if (backgroundObject != null)
            {
                // Set start scale
                backgroundObject.localScale = startScale;

                // Set start position if enabled
                if (animatePosition)
                {
                    backgroundObject.localPosition = startPosition;
                }
                else
                {
                    // Ensure it starts at final position if not animating position
                    // (Unless it was moved in editor, but we assume scene is setup in 'final' state usually)
                    backgroundObject.localPosition = finalPos; 
                }
            }
        }

        private Tween AnimateUIElement(CanvasGroup element)
        {
            return element.DOFade(1f, elementFadeDuration)
                .SetEase(Ease.OutQuad);
        }

        private void EnableInteraction()
        {
            if (uiElements != null)
            {
                foreach (var element in uiElements)
                {
                    if (element != null)
                    {
                        element.interactable = true;
                    }
                }
            }
        }

        private void PlayIntroMusic()
        {
            if (Core.AudioManager.Instance != null && introMusicClip != null)
            {
                // Play as one-shot music
                Core.AudioManager.Instance.PlayMusic(introMusicClip);
            }
        }

        /// <summary>
        /// Skip intro immediately (for returning players).
        /// </summary>
        public void SkipIntro()
        {
            // Kill all tweens
            DOTween.Kill(this);

            isPlaying = false;

            // Determine targets
            Vector3 finalScale = useCurrentScaleAsTarget ? initialOriginalScale : targetScale;
            Vector3 finalPos = useCurrentPositionAsTarget ? initialOriginalPos : targetPosition;

            // Set final states (Instant)
            if (backgroundObject != null)
            {
                backgroundObject.localScale = finalScale;
                if (animatePosition)
                {
                    backgroundObject.localPosition = finalPos;
                }
            }

            if (uiElements != null)
            {
                foreach (var element in uiElements)
                {
                    if (element != null)
                    {
                        element.alpha = 1f;
                        element.interactable = true;
                    }
                }
            }

            // If we have a separate 'Main Menu Loop' music, we might want to ensure it plays here.
            // But since this script focuses on Intro, skipping it might mean just letting 
            // whatever persistent music (or default menu music) take over.
        }

        private void OnDestroy()
        {
            DOTween.Kill(this);
        }
    }
}
