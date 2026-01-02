using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using PixelVanguard.Services;
using PixelVanguard.Data;

namespace PixelVanguard.UI.CharacterSelect
{
    public class CharacterSelectController : MonoBehaviour
    {
        [Header("Character Setup")]
        [SerializeField] private CharacterData[] availableCharacters;

        [Header("UI References")]
        [SerializeField] private Transform characterCardsContainer;
        [SerializeField] private GameObject characterCardPrefab;
        [SerializeField] private CharacterDetailsPanel detailsPanel;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private Button backButton;

        [Header("Action Button States")]
        [SerializeField] private Button actionButton;
        [SerializeField] private GameObject confirmContainer;
        [SerializeField] private GameObject playContainer;
        [SerializeField] private GameObject buyContainer;

        [Header("Buy Container Elements")]
        [SerializeField] private TextMeshProUGUI buyPriceText;

        private List<CharacterCard> characterCards = new List<CharacterCard>();
        private CharacterData currentlyViewedCharacter;
        private CharacterData selectedCharacter;
        private SaveData saveData;
        private ISaveService saveService;

        private void Awake()
        {
            CreateCharacterCards();
        }

        private void Start()
        {
            saveService = Core.ServiceLocator.Get<ISaveService>();
            LoadSaveData();

            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);

            if (actionButton != null)
                actionButton.onClick.AddListener(OnActionClicked);

            RefreshUI();
        }

        private void OnEnable()
        {
            // Automatically refresh when panel is enabled (SetActive(true))
            RefreshGoldAndUI();
        }

        /// <summary>
        /// Public method to refresh gold and UI when panel is shown.
        /// Called by MainMenuManager when opening character select.
        /// </summary>
        public void RefreshGoldAndUI()
        {
            if (saveService != null)
            {
                saveData = saveService.LoadData();

                if (goldText != null)
                {
                    goldText.text = saveData.totalGold.ToString();
                }
                else
                {
                    Debug.LogError("[CharacterSelect] Gold Text component is MISSING!");
                }

                // Refresh all character cards to show correct affordability
                RefreshUI();
            }
        }

        private void CreateCharacterCards()
        {
            foreach (var character in availableCharacters)
            {
                GameObject cardObj = Instantiate(characterCardPrefab, characterCardsContainer);
                CharacterCard card = cardObj.GetComponent<CharacterCard>();
                card.OnCardClicked += () => OnCharacterCardClicked(character);
                characterCards.Add(card);
            }
        }

        private void LoadSaveData()
        {
            if (saveService != null)
            {
                saveData = saveService.LoadData();
                selectedCharacter = FindCharacterById(saveData.selectedCharacterID);

                if (selectedCharacter == null && availableCharacters.Length > 0)
                    selectedCharacter = availableCharacters[0];
            }
        }

        private void RefreshUI()
        {
            if (saveData == null) return;

            if (goldText != null)
                goldText.text = saveData.totalGold.ToString();

            for (int i = 0; i < availableCharacters.Length && i < characterCards.Count; i++)
            {
                var character = availableCharacters[i];
                bool isLocked = !saveData.IsCharacterUnlocked(character.characterID);
                characterCards[i].Initialize(character, isLocked);
            }

            if (selectedCharacter != null && currentlyViewedCharacter == null)
            {
                currentlyViewedCharacter = selectedCharacter;
                ShowCharacterDetails(selectedCharacter);
                UpdateActionButton();
            }
        }

        private void OnCharacterCardClicked(CharacterData character)
        {
            currentlyViewedCharacter = character;
            ShowCharacterDetails(character);
            UpdateActionButton();
        }

        private void ShowCharacterDetails(CharacterData character)
        {
            if (detailsPanel == null || character == null) return;
            bool isLocked = !saveData.IsCharacterUnlocked(character.characterID);
            detailsPanel.ShowCharacterDetails(character, saveData, isLocked);
        }

        private void UpdateActionButton()
        {
            if (currentlyViewedCharacter == null) return;

            bool isLocked = !saveData.IsCharacterUnlocked(currentlyViewedCharacter.characterID);

            if (confirmContainer != null) confirmContainer.SetActive(false);
            if (playContainer != null) playContainer.SetActive(false);
            if (buyContainer != null) buyContainer.SetActive(false);

            if (isLocked)
            {
                if (buyContainer != null)
                {
                    buyContainer.SetActive(true);
                    if (buyPriceText != null)
                        buyPriceText.text = currentlyViewedCharacter.goldCost.ToString();

                    bool canAfford = saveData.totalGold >= currentlyViewedCharacter.goldCost;
                    if (actionButton != null)
                        actionButton.interactable = canAfford;
                }
            }
            else
            {
                bool isSelected = string.Equals(
                    currentlyViewedCharacter.characterID,
                    selectedCharacter?.characterID,
                    System.StringComparison.OrdinalIgnoreCase
                );

                if (isSelected)
                {
                    if (playContainer != null)
                        playContainer.SetActive(true);
                }
                else
                {
                    if (confirmContainer != null)
                        confirmContainer.SetActive(true);
                }

                if (actionButton != null)
                    actionButton.interactable = true;
            }
        }

        private void OnActionClicked()
        {
            if (currentlyViewedCharacter == null) return;

            bool isLocked = !saveData.IsCharacterUnlocked(currentlyViewedCharacter.characterID);

            if (isLocked)
            {
                PurchaseCharacter(currentlyViewedCharacter);
            }
            else
            {
                if (confirmContainer != null && confirmContainer.activeSelf)
                {
                    ConfirmSelection();
                }
                else if (playContainer != null && playContainer.activeSelf)
                {
                    StartGame();
                }
            }
        }

        private void PurchaseCharacter(CharacterData character)
        {
            int cost = character.goldCost;
            if (saveData.totalGold < cost) return;

            saveData.totalGold -= cost;
            saveData.UnlockCharacter(character.characterID);
            saveService.SaveData(saveData);

            if (goldText != null)
                goldText.text = saveData.totalGold.ToString();

            // Update the purchased character's card visual state (opacity 59% → 100%)
            for (int i = 0; i < availableCharacters.Length && i < characterCards.Count; i++)
            {
                if (availableCharacters[i].characterID == character.characterID)
                {
                    characterCards[i].Initialize(character, false); // Now unlocked
                    break;
                }
            }

            UpdateActionButton();
        }

        private void ConfirmSelection()
        {
            selectedCharacter = currentlyViewedCharacter;
            saveData.selectedCharacterID = selectedCharacter.characterID;
            saveService.SaveData(saveData);

            Core.CharacterManager.SelectedCharacter = selectedCharacter;

            UpdateActionButton();
        }

        private void StartGame()
        {
            SceneManager.LoadScene("GameScene");
        }

        private CharacterData FindCharacterById(string characterId)
        {
            foreach (var character in availableCharacters)
            {
                if (character.characterID.ToLower() == characterId.ToLower())
                    return character;
            }
            return null;
        }

        private void OnBackClicked()
        {
            var mainMenu = FindFirstObjectByType<MainMenuManager>();
            if (mainMenu != null)
                mainMenu.ReturnToMainMenu();
            else
                SceneManager.LoadScene("MainMenu");
        }
    }
}
