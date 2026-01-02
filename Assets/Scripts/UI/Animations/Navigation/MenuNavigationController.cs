using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

namespace PixelVanguard.UI.Animations
{
    /// <summary>
    /// Manages menu navigation with a stack-based system.
    /// Provides directional slide animations (forward = right, back = left).
    /// </summary>
    public class MenuNavigationController : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float transitionDuration = 0.3f;
        [SerializeField] private bool allowBackDuringTransition = false;

        private Stack<GameObject> navigationStack = new Stack<GameObject>();
        private GameObject currentPanel;
        private bool isTransitioning = false;

        /// <summary>
        /// Initialize with the root panel (MainMenu).
        /// </summary>
        public void Initialize(GameObject rootPanel)
        {
            if (rootPanel == null)
            {
                Debug.LogError("[MenuNavigationController] Root panel is null!");
                return;
            }

            // Clear stack
            navigationStack.Clear();

            // Root panel is the base
            currentPanel = rootPanel;
            currentPanel.SetActive(true);

            Debug.Log($"[MenuNavigationController] Initialized with root: {rootPanel.name}");
        }

        /// <summary>
        /// Navigate forward to a new panel (slides in from right).
        /// </summary>
        public void NavigateToPanel(GameObject targetPanel, System.Action onComplete = null)
        {
            if (targetPanel == null)
            {
                Debug.LogError("[MenuNavigationController] Target panel is null!");
                return;
            }

            if (isTransitioning && !allowBackDuringTransition)
            {
                Debug.LogWarning("[MenuNavigationController] Transition in progress, ignoring navigation.");
                return;
            }

            if (currentPanel == targetPanel)
            {
                Debug.LogWarning($"[MenuNavigationController] Already on panel: {targetPanel.name}");
                return;
            }

            isTransitioning = true;

            // Push current panel to stack
            if (currentPanel != null)
            {
                navigationStack.Push(currentPanel);
            }

            GameObject previousPanel = currentPanel;
            currentPanel = targetPanel;

            // Animate transition
            PerformTransition(
                previousPanel,
                targetPanel,
                isForwardNavigation: true,
                () =>
                {
                    isTransitioning = false;
                    onComplete?.Invoke();
                }
            );

            Debug.Log($"[MenuNavigationController] Navigated to: {targetPanel.name} (Stack depth: {navigationStack.Count})");
        }

        /// <summary>
        /// Navigate back to previous panel (slides out to right).
        /// </summary>
        public void NavigateBack(System.Action onComplete = null)
        {
            if (isTransitioning && !allowBackDuringTransition)
            {
                Debug.LogWarning("[MenuNavigationController] Transition in progress, ignoring back navigation.");
                return;
            }

            if (navigationStack.Count == 0)
            {
                Debug.LogWarning("[MenuNavigationController] Already at root, cannot go back.");
                return;
            }

            isTransitioning = true;

            GameObject previousPanel = currentPanel;
            currentPanel = navigationStack.Pop();

            // Animate transition
            PerformTransition(
                previousPanel,
                currentPanel,
                isForwardNavigation: false,
                () =>
                {
                    isTransitioning = false;
                    onComplete?.Invoke();
                }
            );

            Debug.Log($"[MenuNavigationController] Navigated back to: {currentPanel.name} (Stack depth: {navigationStack.Count})");
        }

        /// <summary>
        /// Navigate back to root panel (clear entire stack).
        /// </summary>
        public void NavigateBackToRoot(System.Action onComplete = null)
        {
            if (navigationStack.Count == 0)
            {
                Debug.LogWarning("[MenuNavigationController] Already at root.");
                return;
            }

            // Get root panel (bottom of stack)
            GameObject rootPanel = null;
            while (navigationStack.Count > 0)
            {
                rootPanel = navigationStack.Pop();
            }

            if (rootPanel == null)
            {
                Debug.LogError("[MenuNavigationController] Root panel is null!");
                return;
            }

            isTransitioning = true;

            GameObject previousPanel = currentPanel;
            currentPanel = rootPanel;

            // Animate transition
            PerformTransition(
                previousPanel,
                currentPanel,
                isForwardNavigation: false,
                () =>
                {
                    isTransitioning = false;
                    onComplete?.Invoke();
                }
            );

            Debug.Log($"[MenuNavigationController] Navigated back to root: {currentPanel.name}");
        }

        /// <summary>
        /// Perform the slide transition between panels.
        /// PROFESSIONAL FIX: Old panel disappears INSTANTLY, new panel slides in smoothly.
        /// </summary>
        private void PerformTransition(GameObject fromPanel, GameObject toPanel, bool isForwardNavigation, System.Action onComplete)
        {
            if (fromPanel == null && toPanel == null)
            {
                onComplete?.Invoke();
                return;
            }

            // Determine slide direction for new panel
            SlideDirection showDirection = isForwardNavigation ? SlideDirection.Right : SlideDirection.Left;

            // INSTANT hide old panel (no overlap!)
            if (fromPanel != null)
            {
                fromPanel.SetActive(false);
            }

            // Smoothly show new panel
            if (toPanel != null)
            {
                Tween showTween = UIAnimator.ShowPanel(toPanel, showDirection, transitionDuration);
                if (showTween != null)
                {
                    showTween.OnComplete(() => onComplete?.Invoke());
                }
                else
                {
                    // Fallback: instant show
                    toPanel.SetActive(true);
                    onComplete?.Invoke();
                }
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        /// <summary>
        /// Get current panel.
        /// </summary>
        public GameObject GetCurrentPanel()
        {
            return currentPanel;
        }

        /// <summary>
        /// Get stack depth.
        /// </summary>
        public int GetStackDepth()
        {
            return navigationStack.Count;
        }

        /// <summary>
        /// Check if at root.
        /// </summary>
        public bool IsAtRoot()
        {
            return navigationStack.Count == 0;
        }

        /// <summary>
        /// Check if transitioning.
        /// </summary>
        public bool IsTransitioning()
        {
            return isTransitioning;
        }

        private void OnDestroy()
        {
            // Kill all tweens
            DOTween.Kill(this);
        }
    }
}
