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
        // ✅ REMOVED: selectedCharacter - using Single Source of Truth (cachedSave.Data.selectedCharacterID)
        private CachedSaveDataService cachedSave;

        private void Awake()
        {
            CreateCharacterCards();
        }

        private void Start()
        {
            cachedSave = Core.ServiceLocator.Get<CachedSaveDataService>();

            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);

            if (actionButton != null)
                actionButton.onClick.AddListener(OnActionClicked);

            // Note: Don't call SelectInitialCharacter here - wait until OnEnable when panel is shown
        }
        
        private void SelectInitialCharacter()
        {
            // Safety check - cards must be initialized first
            if (characterCards == null || characterCards.Count == 0) return;
            if (cachedSave == null) return;
            
            // ✅ SINGLE SOURCE OF TRUTH: Get from CachedSaveDataService only
            CharacterData selectedChar = GetSelectedCharacter();
            
            if (selectedChar != null)
            {
                // Select the saved character
                OnCharacterCardClicked(selectedChar);
            }
            else if (characterCards.Count > 0 && characterCards[0]?.CharacterData != null)
            {
                // Fallback to first character if none selected
                OnCharacterCardClicked(characterCards[0].CharacterData);
            }
        }

        private void OnEnable()
        {
            // Ensure cachedSave is initialized (OnEnable can run before Start)
            if (cachedSave == null)
                cachedSave = Core.ServiceLocator.Get<CachedSaveDataService>();
            
            // RefreshUI first to ensure cards are initialized with data
            RefreshGoldAndUI();
            
            // Then select initial character (needs cards to be initialized)
            SelectInitialCharacter();
            
            Core.LocalizationManager.OnLanguageChanged += OnLanguageChanged;
        }

        private void OnDisable()
        {
            Core.LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
        }

        private void OnLanguageChanged()
        {
            RefreshUI();
        }

        /// <summary>
        /// Public method to refresh gold and UI when panel is shown.
        /// Called by MainMenuManager when opening character select.
        /// </summary>
        public void RefreshGoldAndUI()
        {
            if (cachedSave != null)
            {

                if (goldText != null)
                {
                    goldText.text = cachedSave.Data.totalGold.ToString();
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
                
                if (card != null)
                {
                    // Subscribe to card click
                    card.OnCardClicked += () => OnCharacterCardClicked(card);
                    characterCards.Add(card);
                }
            }
        }

        private void RefreshUI()
        {
            if (cachedSave.Data == null) return;

            if (goldText != null)
                goldText.text = cachedSave.Data.totalGold.ToString();

            for (int i = 0; i < availableCharacters.Length && i < characterCards.Count; i++)
            {
                var character = availableCharacters[i];
                bool isLocked = !cachedSave.Data.IsCharacterUnlocked(character.characterID);
                characterCards[i].Initialize(character, isLocked);
            }

            // ✅ Use GetSelectedCharacter() for Single Source of Truth
            if (GetSelectedCharacter() != null && currentlyViewedCharacter == null)
            {
                currentlyViewedCharacter = GetSelectedCharacter();
                ShowCharacterDetails(GetSelectedCharacter());
                UpdateActionButton();
            }
        }

        private void OnCharacterCardClicked(CharacterData character)
        {
            currentlyViewedCharacter = character;
            ShowCharacterDetails(character);
            UpdateActionButton();
        }
        
        // Overload for card click events
        private void OnCharacterCardClicked(CharacterCard card)
        {
            if (card != null && card.CharacterData != null)
            {
                OnCharacterCardClicked(card.CharacterData);
            }
        }

        private void ShowCharacterDetails(CharacterData character)
        {
            if (detailsPanel == null || character == null) return;
            bool isLocked = !cachedSave.Data.IsCharacterUnlocked(character.characterID);
            detailsPanel.ShowCharacterDetails(character, cachedSave.Data, isLocked);
        }

        private void UpdateActionButton()
        {
            if (currentlyViewedCharacter == null) return;

            bool isLocked = !cachedSave.Data.IsCharacterUnlocked(currentlyViewedCharacter.characterID);

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

                    bool canAfford = cachedSave.Data.totalGold >= currentlyViewedCharacter.goldCost;
                    if (actionButton != null)
                        actionButton.interactable = canAfford;
                }
            }
            else
            {
                // ✅ Use GetSelectedCharacter() for comparison
                CharacterData selectedChar = GetSelectedCharacter();
                bool isSelected = string.Equals(
                    currentlyViewedCharacter.characterID,
                    selectedChar?.characterID,
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

            bool isLocked = !cachedSave.Data.IsCharacterUnlocked(currentlyViewedCharacter.characterID);

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
            if (cachedSave.Data.totalGold < cost) return;

            cachedSave.Data.totalGold -= cost;
            cachedSave.Data.UnlockCharacter(character.characterID);
            cachedSave.Save();

            if (goldText != null)
                goldText.text = cachedSave.Data.totalGold.ToString();

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
            // ✅ SINGLE SOURCE OF TRUTH: Save to CachedSaveDataService only
            cachedSave.Data.selectedCharacterID = currentlyViewedCharacter.characterID;
            cachedSave.Save();

            // Update CharacterManager static property for backward compatibility
            Core.CharacterManager.SelectedCharacter = currentlyViewedCharacter;

            UpdateActionButton();
        }

        private void StartGame()
        {
            SceneManager.LoadScene("GameScene");
        }

        /// <summary>
        /// Get the currently selected character from save data (Single Source of Truth).
        /// </summary>
        private CharacterData GetSelectedCharacter()
        {
            if (cachedSave?.Data == null) return null;
            
            string selectedId = cachedSave.Data.selectedCharacterID;
            if (string.IsNullOrEmpty(selectedId)) return null;
            
            return FindCharacterById(selectedId);
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
