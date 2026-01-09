using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PixelVanguard.Core;
using PixelVanguard.Services;

namespace PixelVanguard.UI.Shop
{
    public class ShopUIHandler
    {
        private readonly CachedSaveDataService cachedSave;
        private readonly TextMeshProUGUI goldText;
        private readonly Button backButton;
        private readonly DetailsPanel detailsPanel;

        public ShopUIHandler(
            CachedSaveDataService cachedSave,
            TextMeshProUGUI goldText,
            Button backButton,
            DetailsPanel detailsPanel)
        {
            this.cachedSave = cachedSave;
            this.goldText = goldText;
            this.backButton = backButton;
            this.detailsPanel = detailsPanel;
        }

        public void UpdateGoldDisplay()
        {
            if (goldText != null && cachedSave != null)
            {
                goldText.text = cachedSave.TotalGold.ToString();
            }
        }

        public void OnBackButtonClicked()
        {
            var mainMenu = Object.FindFirstObjectByType<MainMenuManager>();
            if (mainMenu != null)
            {
                mainMenu.ReturnToMainMenu();
            }
        }

        public void ShowGoldPackDetails(AdPackCard card)
        {
            if (detailsPanel != null && card != null)
            {
                detailsPanel.ShowGoldPackDetails(
                    card.GetIcon(),
                    card.GetTitle(),
                    card.GetDescription()
                );
            }
        }


    }
}
