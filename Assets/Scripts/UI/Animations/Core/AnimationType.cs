namespace PixelVanguard.UI.Animations
{
    /// <summary>
    /// Defines all available UI animation types.
    /// </summary>
    public enum AnimationType
    {
        None,
        Fade,
        SlideFromLeft,
        SlideFromRight,
        SlideFromTop,
        SlideFromBottom,
        ScaleBounce,
        ScaleElastic,
        ScalePop
    }

    /// <summary>
    /// Direction for slide animations.
    /// </summary>
    public enum SlideDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    /// <summary>
    /// Configuration for UI animations.
    /// </summary>
    [System.Serializable]
    public class UIAnimationConfig
    {
        public AnimationType animationType = AnimationType.Fade;
        public float duration = 0.3f;
        public DG.Tweening.Ease easeType = DG.Tweening.Ease.OutQuad;
        public float delay = 0f;

        // Slide specific
        public float slideDistance = 1920f; // Default screen width offset

        // Scale specific
        public float scaleMultiplier = 1.1f;

        public UIAnimationConfig() { }

        public UIAnimationConfig(AnimationType type, float duration = 0.3f, DG.Tweening.Ease ease = DG.Tweening.Ease.OutQuad)
        {
            this.animationType = type;
            this.duration = duration;
            this.easeType = ease;
        }
    }
}
