using UnityEngine;

namespace PixelVanguard.Core
{
    /// <summary>
    /// Manages the selected character for the current game session.
    /// Stores the selected character and spawns the player prefab.
    /// </summary>
    public class CharacterManager : MonoBehaviour
    {
        [Header("Default Character")]
        [Tooltip("Character to use if no selection made (The Knight)")]
        [SerializeField] private Data.CharacterData defaultCharacter;

        [Header("All Characters")]
        [Tooltip("All available characters (assign same list as CharacterSelectController)")]
        [SerializeField] private Data.CharacterData[] allCharacters;

        [Header("Spawn Settings")]
        [Tooltip("Where to spawn the player (leave empty for (0,0,0))")]
        [SerializeField] private Transform spawnPoint;

        [Tooltip("Cinemachine camera to follow player (optional - auto-detects if empty)")]
        [SerializeField] private MonoBehaviour cinemachineCamera;

        /// <summary>
        /// Currently selected character. Set by Main Menu or defaults to Knight.
        /// </summary>
        public static Data.CharacterData SelectedCharacter { get; set; }

        /// <summary>
        /// Reference to spawned player GameObject.
        /// </summary>
        public static GameObject SpawnedPlayer { get; private set; }

        private void Awake()
        {
            // If no character selected via static variable, load from SaveData
            if (SelectedCharacter == null)
            {
                LoadSelectedCharacterFromSave();
            }

            // Final fallback to default character
            if (SelectedCharacter == null)
            {
                if (defaultCharacter != null)
                {
                    SelectedCharacter = defaultCharacter;
                }
                else
                {
                    Debug.LogError("[CharacterManager] No default character assigned!");
                    return;
                }
            }

            // Spawn the player
            SpawnPlayer();
        }

        private void LoadSelectedCharacterFromSave()
        {
            var saveService = ServiceLocator.Get<Services.ISaveService>();
            if (saveService == null || allCharacters == null || allCharacters.Length == 0)
                return;

            var saveData = saveService.LoadData();
            string selectedID = saveData.selectedCharacterID;

            // Find the character by ID from the Inspector-assigned list
            foreach (var character in allCharacters)
            {
                if (character != null && character.characterID.ToLower() == selectedID.ToLower())
                {
                    SelectedCharacter = character;
                    return;
                }
            }
        }

        private void SpawnPlayer()
        {
            if (SelectedCharacter == null)
            {
                Debug.LogError("[CharacterManager] Cannot spawn player - no character selected!");
                return;
            }

            // Determine spawn position
            Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;

            GameObject playerPrefab = SelectedCharacter.characterPrefab;

            // If no prefab assigned, try to find existing player in scene (fallback)
            if (playerPrefab == null)
            {
                Debug.LogWarning("[CharacterManager] No character prefab assigned. Looking for existing Player in scene...");
                SpawnedPlayer = GameObject.FindGameObjectWithTag("Player");
                if (SpawnedPlayer != null)
                {
                    SetupCameraFollow(SpawnedPlayer.transform);
                    return;
                }
                else
                {
                    Debug.LogError("[CharacterManager] No character prefab AND no existing Player found!");
                    return;
                }
            }

            // Instantiate player prefab
            SpawnedPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            SpawnedPlayer.name = $"Player ({SelectedCharacter.displayName})";

            // Set camera to follow the spawned player
            SetupCameraFollow(SpawnedPlayer.transform);

        }

        private void SetupCameraFollow(Transform playerTransform)
        {
            // If user assigned a camera in Inspector, use that
            if (cinemachineCamera != null)
            {
                SetCameraTarget(cinemachineCamera, playerTransform);
                return;
            }

            // Otherwise, auto-detect Cinemachine camera in scene
            MonoBehaviour[] allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            
            foreach (var component in allMonoBehaviours)
            {
                string typeName = component.GetType().Name;
                
                if (typeName.Contains("Cinemachine") && typeName.Contains("Camera"))
                {
                    if (SetCameraTarget(component, playerTransform))
                    {
                        return;
                    }
                }
            }

        }

        private bool SetCameraTarget(MonoBehaviour camera, Transform target)
        {
            var type = camera.GetType();
            
            // Try common Cinemachine property names
            string[] properties = { "Follow", "m_Follow", "TrackingTarget" };
            
            foreach (string propName in properties)
            {
                var prop = type.GetProperty(propName);
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(camera, target);
                    return true;
                }
                
                var field = type.GetField(propName);
                if (field != null)
                {
                    field.SetValue(camera, target);
                    return true;
                }
            }
            
            return false;
        }



        /// <summary>
        /// Reset character selection (call when returning to main menu).
        /// </summary>
        public static void ResetSelection()
        {
            SelectedCharacter = null;
            if (SpawnedPlayer != null)
            {
                Destroy(SpawnedPlayer);
                SpawnedPlayer = null;
            }
        }
    }
}
