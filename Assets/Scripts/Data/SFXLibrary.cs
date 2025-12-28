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

        [Header("Combat")]
        public AudioClip playerDamage;

        [Header("Weapon Spawns")]
        public AudioClip magicOrbitalSpawn;
        public AudioClip holyWaterThrow;

        [Header("UI")]
        public AudioClip buttonClick;
        public AudioClip gameOver;

        [Header("Music")]
        public AudioClip backgroundMusic;
    }
}
