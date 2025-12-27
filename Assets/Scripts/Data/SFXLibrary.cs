using UnityEngine;

namespace PixelVanguard.Data
{
    /// <summary>
    /// ScriptableObject that holds all SFX AudioClips.
    /// Create via: Assets > Create > Audio > SFX Library
    /// </summary>
    [CreateAssetMenu(fileName = "SFXLibrary", menuName = "Audio/SFX Library")]
    public class SFXLibrary : ScriptableObject
    {
        [Header("Weapons")]
        public AudioClip greatswordSwing;
        public AudioClip crossbowFire;

        [Header("Progression")]
        public AudioClip xpPickup;
        public AudioClip levelUp;
        public AudioClip upgradeSelect;

        [Header("Collectibles")]
        public AudioClip goldPickup;
        public AudioClip healthPotion;

        [Header("UI")]
        public AudioClip buttonClick;

        [Header("Music")]
        public AudioClip backgroundMusic;
    }
}
