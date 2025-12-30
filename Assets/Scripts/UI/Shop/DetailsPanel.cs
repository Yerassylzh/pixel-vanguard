using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PixelVanguard.UI
{
    /// <summary>
    /// Displays details for selected upgrade or gold pack.
    /// Shows icon and description when card is clicked.
    /// </summary>
    public class DetailsPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI descriptionText;

        [Header("Default State")]
        [SerializeField] private Sprite defaultIcon; // Question mark icon
        [SerializeField] private string defaultText = "Select upgrade to see details";

        private void Start()
        {
            // Show default state on start
            ShowDefault();
        }

        /// <summary>
        /// Show default "Select upgrade" state.
        /// </summary>
        public void ShowDefault()
        {
            if (iconImage != null)
            {
                iconImage.sprite = defaultIcon;
                iconImage.gameObject.SetActive(defaultIcon != null);
            }

            if (descriptionText != null)
            {
                descriptionText.text = defaultText;
            }
        }

        /// <summary>
        /// Show upgrade card details.
        /// </summary>
        public void ShowUpgradeDetails(Sprite icon, string upgradeName, string description)
        {
            if (iconImage != null)
            {
                iconImage.sprite = icon;
                iconImage.gameObject.SetActive(icon != null);
            }

            if (descriptionText != null)
            {
                descriptionText.text = $"<b>{upgradeName}</b>\n{description}";
            }
        }

        /// <summary>
        /// Show gold pack details.
        /// </summary>
        public void ShowGoldPackDetails(Sprite icon, string packName, string description)
        {
            if (iconImage != null)
            {
                iconImage.sprite = icon;
                iconImage.gameObject.SetActive(icon != null);
            }

            if (descriptionText != null)
            {
                descriptionText.text = $"<b>{packName}</b>\n{description}";
            }
        }
    }
}
