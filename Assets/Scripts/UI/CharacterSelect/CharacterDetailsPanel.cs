using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PixelVanguard.Services;
using PixelVanguard.Data;

namespace PixelVanguard.UI.CharacterSelect
{
    /// <summary>
    /// Character details panel showing character stats and weapon info.
    /// Displays stats WITH shop upgrades applied for unlocked characters.
    /// </summary>
    public class CharacterDetailsPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image characterIconImage;
        [SerializeField] private Image weaponIconImage;
        [SerializeField] private TextMeshProUGUI infoText;      // Combined weapon + stats text

        /// <summary>
        /// Show character details with calculated stats.
        /// </summary>
        public void ShowCharacterDetails(CharacterData character, SaveData saveData, bool isLocked)
        {
            if (character == null)
            {
                Debug.LogWarning("[CharacterDetailsPanel] Character data is null!");
                return;
            }

            // Set character icon
            if (character.portrait != null && characterIconImage != null)
            {
                characterIconImage.sprite = character.portrait;
            }

            // Set weapon icon
            if (character.starterWeapon != null && character.starterWeapon.icon != null && weaponIconImage != null)
            {
                weaponIconImage.sprite = character.starterWeapon.icon;
            }

            // Build combined info text (weapon + stats)
            if (infoText != null)
            {
                // Localize weapon name based on weapon ID
                string weaponKey = $"ui.weapon.{character.starterWeapon.weaponID}";
                string weaponName = Core.LocalizationManager.Get(weaponKey);

                if (isLocked)
                {
                    // Show base stats only for locked characters
                    string lockedText = Core.LocalizationManager.Get("ui.common.locked");
                    string purchaseText = Core.LocalizationManager.Get("ui.common.purchase_to_unlock");
                    string weaponLabel = Core.LocalizationManager.Get("ui.character.stats.weapon");
                    string healthLabel = Core.LocalizationManager.Get("ui.character.stats.base_health");
                    string speedLabel = Core.LocalizationManager.Get("ui.character.stats.base_speed");
                    string damageLabel = Core.LocalizationManager.Get("ui.character.stats.damage");
                    
                    infoText.text = $"<color=#888888>{lockedText} - {purchaseText}</color>\n\n" +
                                    $"{weaponLabel}: {weaponName}\n" +
                                    $"{healthLabel}: {character.maxHealth}\n" +
                                    $"{speedLabel}: {character.moveSpeed:F1}\n" +
                                    $"{damageLabel}: {character.baseDamageMultiplier:F1}x";
                }
                else
                {
                    // Show final stats with shop upgrades applied
                    int baseHP = (int)character.maxHealth;
                    int vitalityLevel = saveData.GetStatLevel("vitality");
                    int finalHP = baseHP + (vitalityLevel * 10);

                    float baseSpeed = character.moveSpeed;
                    int greavesLevel = saveData.GetStatLevel("greaves");
                    float finalSpeed = baseSpeed * (1f + greavesLevel * 0.05f);

                    float baseDamage = character.baseDamageMultiplier;
                    int mightLevel = saveData.GetStatLevel("might");
                    float finalDamage = baseDamage * (1f + mightLevel * 0.10f);

                    // Build text with localized labels
                    string weaponLabel = Core.LocalizationManager.Get("ui.character.stats.weapon");
                    string healthLabel = Core.LocalizationManager.Get("ui.character.stats.health");
                    string speedLabel = Core.LocalizationManager.Get("ui.character.stats.speed");
                    string damageLabel = Core.LocalizationManager.Get("ui.character.stats.damage");
                    
                    infoText.text = $"{weaponLabel}: {weaponName}\n\n";
                    
                    infoText.text += $"{healthLabel}: <color=#00FF00>{finalHP}</color>";
                    if (vitalityLevel > 0)
                    {
                        infoText.text += $" <size=80%>({baseHP} +{vitalityLevel * 10})</size>";
                    }

                    infoText.text += $"\n{speedLabel}: <color=#00BFFF>{finalSpeed:F1}</color>";
                    if (greavesLevel > 0)
                    {
                        infoText.text += $" <size=80%>({baseSpeed:F1} +{greavesLevel * 5}%)</size>";
                    }

                    infoText.text += $"\n{damageLabel}: <color=#FF4500>{finalDamage:F2}x</color>";
                    if (mightLevel > 0)
                    {
                        infoText.text += $" <size=80%>({baseDamage:F1}x +{mightLevel * 10}%)</size>";
                    }
                }
            }
        }
    }
}
