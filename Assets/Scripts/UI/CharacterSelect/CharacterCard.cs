using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace PixelVanguard.UI.CharacterSelect
{
    /// <summary>
    /// Individual character card UI component.
    /// Shows character icon, weapon icon, and handles locked/unlocked visual states.
    /// </summary>
    public class CharacterCard : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image characterIconImage;
        [SerializeField] private Image weaponIconImage;        // Bottom-right corner
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Button cardButton;

        [Header("Visual Settings")]
        [SerializeField] private Color lockedTint = new Color(1f, 1f, 1f, 0.59f); // 150/255 opacity (59%)
        [SerializeField] private Color unlockedTint = Color.white;                 // Full opacity

        private Data.CharacterData characterData;
        private bool isLocked;

        public Data.CharacterData CharacterData => characterData;
        public bool IsLocked => isLocked;
        public event Action OnCardClicked;

        private void Start()
        {
            if (cardButton != null)
            {
                cardButton.onClick.AddListener(() => OnCardClicked?.Invoke());
            }
        }

        /// <summary>
        /// Initialize card with character data and lock state.
        /// </summary>
        public void Initialize(Data.CharacterData data, bool locked)
        {
            characterData = data;
            isLocked = locked;

            if (data == null)
            {
                Debug.LogWarning("[CharacterCard] Initialized with null CharacterData!");
                return;
            }

            // Set character icon (use portrait sprite)
            if (data.portrait != null && characterIconImage != null)
            {
                characterIconImage.sprite = data.portrait;
            }

            // Set weapon icon (bottom-right)
            if (data.starterWeapon != null && data.starterWeapon.icon != null && weaponIconImage != null)
            {
                weaponIconImage.sprite = data.starterWeapon.icon;
            }

            // Set name
            if (nameText != null)
            {
                nameText.text = data.displayName;
            }

            // Apply locked/unlocked visual state
            UpdateVisualState();
        }

        /// <summary>
        /// Update visual appearance based on locked state (opacity only).
        /// </summary>
        private void UpdateVisualState()
        {
            if (isLocked)
            {
                // 59% opacity for all images
                if (characterIconImage != null)
                    characterIconImage.color = lockedTint;
                    
                if (weaponIconImage != null)
                    weaponIconImage.color = lockedTint;
            }
            else
            {
                // Full opacity
                if (characterIconImage != null)
                    characterIconImage.color = unlockedTint;
                    
                if (weaponIconImage != null)
                    weaponIconImage.color = unlockedTint;
            }
        }

        private void OnDestroy()
        {
            if (cardButton != null)
            {
                cardButton.onClick.RemoveAllListeners();
            }
        }
    }
}
