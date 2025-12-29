using UnityEngine;
using System.Collections.Generic;

namespace PixelVanguard.Core
{
    /// <summary>
    /// Singleton Audio Manager - Event-Driven Architecture.
    /// Dynamically subscribes to weapon fire events when weapons are equipped.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private Data.SFXLibrary sfxLibrary;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;

        [Header("Settings")]
        [SerializeField] private float sfxVolume = 1f;
        [SerializeField] private float musicVolume = 0.5f;
        [SerializeField] private bool pitchRandomization = true;
        [SerializeField] private float pitchVariationRange = 0.1f; // ±10%

        // Track subscribed weapons to prevent memory leaks
        private Dictionary<Gameplay.WeaponBase, Data.WeaponType> subscribedWeapons = new Dictionary<Gameplay.WeaponBase, Data.WeaponType>();

        private void Awake()
        {
            // Singleton with DontDestroyOnLoad
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Auto-create audio sources
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }

            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            sfxSource.volume = sfxVolume;
            musicSource.volume = musicVolume;
        }

        private void OnEnable()
        {
            // Subscribe to game events
            GameEvents.OnXPGained += HandleXPPickup;
            GameEvents.OnGoldCollected += HandleGoldPickup;
            GameEvents.OnPlayerLevelUp += HandleLevelUp;
            GameEvents.OnHealthPotionPickup += HandleHealthPotion;
            GameEvents.OnPlayerDamaged += HandlePlayerDamage;
            GameEvents.OnWeaponEquipped += HandleWeaponEquipped;
            GameEvents.OnWeaponSpawned += HandleWeaponSpawn;
            GameEvents.OnGameOver += HandleGameOver;
            GameEvents.OnUpgradeSelected += HandleUpgradeSelected;
            
            // Subscribe to scene changes to control music
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            // Unsubscribe from game events
            GameEvents.OnXPGained -= HandleXPPickup;
            GameEvents.OnGoldCollected -= HandleGoldPickup;
            GameEvents.OnPlayerLevelUp -= HandleLevelUp;
            GameEvents.OnHealthPotionPickup -= HandleHealthPotion;
            GameEvents.OnPlayerDamaged -= HandlePlayerDamage;
            GameEvents.OnWeaponEquipped -= HandleWeaponEquipped;
            GameEvents.OnWeaponSpawned -= HandleWeaponSpawn;
            GameEvents.OnGameOver -= HandleGameOver;
            GameEvents.OnUpgradeSelected -= HandleUpgradeSelected;
            
            // Unsubscribe from all weapon events
            UnsubscribeFromAllWeapons();
            
            // Unsubscribe from scene changes
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Start()
        {
            // Auto-start background music ONLY in GameScene
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            Debug.Log($"Current scene: {currentScene}");
            if (currentScene == "GameScene" && sfxLibrary != null && sfxLibrary.backgroundMusic != null)
            {
                PlayMusic(sfxLibrary.backgroundMusic);
            }
            
            // Subscribe to starter weapons (weapons equipped before AudioManager exists)
            SubscribeToStarterWeapons();
        }
        
        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            if (scene.name == "GameScene" && sfxLibrary != null && sfxLibrary.backgroundMusic != null)
            {
                PlayMusic(sfxLibrary.backgroundMusic);
            }
            else
            {
                StopMusic();
            }
        }

        #region Event Handlers

        private void HandleXPPickup(float amount) => PlaySFX(sfxLibrary?.xpPickup);
        private void HandleGoldPickup(int amount) => PlaySFX(sfxLibrary?.goldPickup);
        private void HandleLevelUp() => PlaySFX(sfxLibrary?.levelUp);
        private void HandleHealthPotion(float healAmount) => PlaySFX(sfxLibrary?.healthPotion);
        private void HandlePlayerDamage(float damage) => PlaySFX(sfxLibrary?.playerDamage);
        private void HandleWeaponSpawn() => PlaySFX(sfxLibrary?.magicOrbitalSpawn); // Generic weapon spawn
        private void HandleGameOver(GameOverReason reason) => PlaySFX(sfxLibrary?.gameOver);
        private void HandleUpgradeSelected() => PlaySFX(sfxLibrary?.upgradeSelect);

        /// <summary>
        /// When weapon is equipped, find its instance and subscribe to fire event.
        /// </summary>
        private void HandleWeaponEquipped(string weaponID)
        {
            // Find all weapons in scene (handles edge case of multiple same-type weapons)
            var allWeapons = FindObjectsByType<Gameplay.WeaponBase>(FindObjectsSortMode.None);
            
            foreach (var weapon in allWeapons)
            {
                // Skip if already subscribed
                if (subscribedWeapons.ContainsKey(weapon))
                    continue;
                
                // Get weapon data
                var weaponData = GetWeaponData(weapon);
                if (weaponData == null || weaponData.weaponID != weaponID)
                    continue;
                
                // Subscribe to weapon fire event
                weapon.OnWeaponFired += () => HandleWeaponFire(weaponData.type);
                subscribedWeapons[weapon] = weaponData.type;
                
                Debug.Log($"[AudioManager] Subscribed to {weaponID} fire event");
            }
        }

        /// <summary>
        /// Handle weapon fire - map weapon type to correct sound.
        /// </summary>
        private void HandleWeaponFire(Data.WeaponType weaponType)
        {
            switch (weaponType)
            {
                case Data.WeaponType.Greatsword:
                    PlaySFX(sfxLibrary?.greatswordSwing);
                    break;
                case Data.WeaponType.Crossbow:
                    PlaySFX(sfxLibrary?.crossbowFire);
                    break;
                // Add more weapons as needed
                default:
                    break;
            }
        }

        #endregion

        #region Weapon Subscription Management

        /// <summary>
        /// Subscribe to starter weapons (weapons that exist before AudioManager starts).
        /// </summary>
        private void SubscribeToStarterWeapons()
        {
            var allWeapons = FindObjectsByType<Gameplay.WeaponBase>(FindObjectsSortMode.None);
            
            foreach (var weapon in allWeapons)
            {
                var weaponData = GetWeaponData(weapon);
                if (weaponData == null) continue;
                
                weapon.OnWeaponFired += () => HandleWeaponFire(weaponData.type);
                subscribedWeapons[weapon] = weaponData.type;
                
                Debug.Log($"[AudioManager] Subscribed to starter weapon: {weaponData.weaponID}");
            }
        }

        /// <summary>
        /// Unsubscribe from all weapons (cleanup).
        /// </summary>
        private void UnsubscribeFromAllWeapons()
        {
            subscribedWeapons.Clear();
        }

        /// <summary>
        /// Get WeaponData from a weapon instance.
        /// </summary>
        private Data.WeaponData GetWeaponData(Gameplay.WeaponBase weapon)
        {
            // Use reflection to access protected weaponData field
            var field = typeof(Gameplay.WeaponBase).GetField("weaponData", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            return field?.GetValue(weapon) as Data.WeaponData;
        }

        #endregion

        #region SFX Playback

        private void PlaySFX(AudioClip clip, bool randomizePitch = true)
        {
            if (clip == null) return;

            sfxSource.pitch = 1f;

            if (pitchRandomization && randomizePitch)
            {
                float pitchVariation = Random.Range(-pitchVariationRange, pitchVariationRange);
                sfxSource.pitch = 1f + pitchVariation;
            }

            sfxSource.PlayOneShot(clip);
        }

        #endregion

        #region Public API (UI Only)

        /// <summary>
        /// Play upgrade select (called by UI).
        /// </summary>
        public void PlayUpgradeSelect() => PlaySFX(sfxLibrary?.upgradeSelect);

        /// <summary>
        /// Play button click (called by UI).
        /// </summary>
        public void PlayButtonClick() => PlaySFX(sfxLibrary?.buttonClick, false);

        #endregion

        #region Music Playback

        public void PlayMusic(AudioClip musicClip)
        {
            if (musicClip == null) return;
            musicSource.clip = musicClip;
            musicSource.Play();
        }

        public void StopMusic() => musicSource.Stop();
        public void PauseMusic() => musicSource.Pause();
        public void ResumeMusic() => musicSource.UnPause();

        #endregion

        #region Volume Control

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            sfxSource.volume = sfxVolume;
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            musicSource.volume = musicVolume;
        }

        #endregion
    }
}
