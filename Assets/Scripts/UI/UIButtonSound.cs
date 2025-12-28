using UnityEngine;
using UnityEngine.UI;

namespace PixelVanguard.UI
{
    /// <summary>
    /// Plays button click sound when attached button is clicked.
    /// Attach this to any UI Button that needs click feedback.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UIButtonSound : MonoBehaviour
    {
        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            
            if (button != null)
            {
                button.onClick.AddListener(PlayClickSound);
            }
        }

        private void PlayClickSound()
        {
            if (Core.AudioManager.Instance != null)
            {
                Core.AudioManager.Instance.PlayButtonClick();
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(PlayClickSound);
            }
        }
    }
}
