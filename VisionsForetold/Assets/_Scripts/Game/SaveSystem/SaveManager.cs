using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VisionsForetold.Game.SaveSystem
{
    /// <summary>
    /// Manages game saving and loading operations
    /// Singleton pattern for global access
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        [Header("Save Settings")]
        [SerializeField] private int maxSaveSlots = 3;
        [SerializeField] private bool autoSave = false;
        [SerializeField] private float autoSaveInterval = 300f; // 5 minutes

        private SaveData currentSaveData;
        private string saveDirectory;
        private float autoSaveTimer;
        private bool isSaving;

        // Events
        public event Action<SaveData> OnGameSaved;
        public event Action<SaveData> OnGameLoaded;
        public event Action<string> OnSaveError;

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSaveSystem();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            if (autoSave && currentSaveData != null)
            {
                autoSaveTimer += Time.deltaTime;
                if (autoSaveTimer >= autoSaveInterval)
                {
                    AutoSaveGame();
                    autoSaveTimer = 0f;
                }
            }
        }

        #endregion

        #region Initialization

        private void InitializeSaveSystem()
        {
            try
            {
                saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
                
                if (!Directory.Exists(saveDirectory))
                {
                    Directory.CreateDirectory(saveDirectory);
                    Debug.Log($"[SaveManager] Created save directory: {saveDirectory}");
                }
                else
                {
                    Debug.Log($"[SaveManager] Save directory exists: {saveDirectory}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to initialize save directory: {e.Message}");
                OnSaveError?.Invoke($"Failed to initialize save system: {e.Message}");
            }
        }

        #endregion

        #region Save Operations

        /// <summary>
        /// Saves the current game state to specified slot
        /// </summary>
        public void SaveGame(int slotIndex, string saveName = null)
        {
            if (isSaving)
            {
                Debug.LogWarning("[SaveManager] Save already in progress! Please wait.");
                OnSaveError?.Invoke("Save already in progress");
                return;
            }

            if (slotIndex < 0 || slotIndex >= maxSaveSlots)
            {
                Debug.LogError($"[SaveManager] Invalid save slot: {slotIndex}. Valid range: 0-{maxSaveSlots - 1}");
                OnSaveError?.Invoke($"Invalid save slot: {slotIndex}");
                return;
            }

            try
            {
                isSaving = true;

                // Create or update save data
                if (currentSaveData == null)
                {
                    currentSaveData = new SaveData();
                    Debug.Log("[SaveManager] Created new save data");
                }

                // Update save metadata
                currentSaveData.saveSlotIndex = slotIndex;
                currentSaveData.saveName = !string.IsNullOrWhiteSpace(saveName) ? saveName : $"Save {slotIndex + 1}";
                currentSaveData.saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                currentSaveData.currentSceneName = SceneManager.GetActiveScene().name;

                Debug.Log($"[SaveManager] Saving to slot {slotIndex}: '{currentSaveData.saveName}' in scene '{currentSaveData.currentSceneName}'");

                // Save map info if in gameplay scene
                SaveMapInfo();

                // Collect player data
                if (!CollectPlayerData())
                {
                    Debug.LogWarning("[SaveManager] Failed to collect complete player data, saving partial state");
                }

                // Save to file
                string filePath = GetSaveFilePath(slotIndex);
                string json = JsonUtility.ToJson(currentSaveData, true);
                
                // Write to file with backup
                WriteSaveFile(filePath, json);

                Debug.Log($"[SaveManager] ? Game saved successfully to slot {slotIndex}: {filePath}");
                OnGameSaved?.Invoke(currentSaveData);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] ? Failed to save game: {e.Message}\nStack trace: {e.StackTrace}");
                OnSaveError?.Invoke($"Save failed: {e.Message}");
            }
            finally
            {
                isSaving = false;
            }
        }

        /// <summary>
        /// Writes save file with backup protection
        /// </summary>
        private void WriteSaveFile(string filePath, string json)
        {
            // Create backup if file exists
            if (File.Exists(filePath))
            {
                string backupPath = filePath + ".backup";
                File.Copy(filePath, backupPath, true);
            }

            // Write to file
            File.WriteAllText(filePath, json);
            
            Debug.Log($"[SaveManager] Wrote {json.Length} bytes to {filePath}");
        }

        /// <summary>
        /// Auto-saves to the current slot
        /// </summary>
        private void AutoSaveGame()
        {
            if (currentSaveData != null)
            {
                SaveGame(currentSaveData.saveSlotIndex, currentSaveData.saveName);
                Debug.Log("[SaveManager] Auto-saved game");
            }
        }

        /// <summary>
        /// Collects all player data for saving
        /// </summary>
        /// <returns>True if all data collected successfully, false if partial</returns>
        private bool CollectPlayerData()
        {
            bool success = true;
            
            try
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                
                if (player == null)
                {
                    Debug.LogWarning("[SaveManager] Player not found! Cannot save player data.");
                    return false;
                }

                // Save position and rotation
                currentSaveData.playerPosition = player.transform.position;
                currentSaveData.playerRotation = player.transform.rotation;
                Debug.Log($"[SaveManager] Saved player position: {currentSaveData.playerPosition}");

                // Save health
                Health playerHealth = player.GetComponent<Health>();
                if (playerHealth != null)
                {
                    currentSaveData.playerHealth = playerHealth.CurrentHealth;
                    currentSaveData.playerMaxHealth = playerHealth.MaxHealth;
                    Debug.Log($"[SaveManager] Saved player health: {currentSaveData.playerHealth}/{currentSaveData.playerMaxHealth}");
                }
                else
                {
                    Debug.LogWarning("[SaveManager] Health component not found on player");
                    success = false;
                }

                // Save XP and level (PlayerXP component)
                PlayerXP playerXP = player.GetComponent<PlayerXP>();
                if (playerXP != null)
                {
                    currentSaveData.currentXP = playerXP.CurrentXP;
                    currentSaveData.currentLevel = playerXP.Level;
                    currentSaveData.xpToNextLevel = playerXP.XPToNextLevel;
                    Debug.Log($"[SaveManager] Saved XP: {currentSaveData.currentXP} | Level: {currentSaveData.currentLevel}");
                }
                else
                {
                    Debug.LogWarning("[SaveManager] PlayerXP component not found - XP not saved");
                    success = false;
                }

                // Save skills from SkillManager
                var skillManager = VisionsForetold.Game.SkillSystem.SkillManager.Instance;
                if (skillManager != null)
                {
                    currentSaveData.skills = skillManager.GetSkillSaveData();
                    Debug.Log($"[SaveManager] Saved skills - Level: {currentSaveData.skills.level}, Points: {currentSaveData.skills.skillPoints}");
                }
                else
                {
                    Debug.LogWarning("[SaveManager] SkillManager not found - skills not saved");
                    success = false;
                }

                // Add more player data collection here as needed
                // Example: inventory, quests, etc.
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Error collecting player data: {e.Message}");
                success = false;
            }
            
            return success;
        }

        /// <summary>
        /// Save map-related information for returning to correct area
        /// </summary>
        private void SaveMapInfo()
        {
            // Find SceneConnectionManager to get return map info
            var connectionManager = VisionsForetold.PointNClick.SceneConnectionManager.Instance;
            if (connectionManager != null)
            {
                currentSaveData.lastMapScene = connectionManager.GetReturnMapScene();
            }

            // Save current scene as the area to return to
            currentSaveData.returnAreaId = currentSaveData.currentSceneName;
            
            // Save save station position if available
            var saveStation = FindObjectOfType<SaveStation>();
            if (saveStation != null)
            {
                currentSaveData.saveStationPosition = saveStation.transform.position;
            }
        }

        #endregion

        #region Load Operations

        /// <summary>
        /// Loads game from specified slot
        /// </summary>
        public void LoadGame(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxSaveSlots)
            {
                Debug.LogError($"[SaveManager] Invalid save slot: {slotIndex}");
                OnSaveError?.Invoke("Invalid save slot");
                return;
            }

            string filePath = GetSaveFilePath(slotIndex);

            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[SaveManager] No save file found at slot {slotIndex}");
                OnSaveError?.Invoke("No save file found");
                return;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                currentSaveData = JsonUtility.FromJson<SaveData>(json);

                Debug.Log($"[SaveManager] Loaded game from slot {slotIndex}");

                // Load the saved scene
                SceneManager.sceneLoaded += OnSceneLoadedForGame;
                SceneManager.LoadScene(currentSaveData.currentSceneName);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to load game: {e.Message}");
                OnSaveError?.Invoke($"Load failed: {e.Message}");
            }
        }

        /// <summary>
        /// Called when scene is loaded after loading a save
        /// </summary>
        private void OnSceneLoadedForGame(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnSceneLoadedForGame;
            ApplyPlayerData();
            OnGameLoaded?.Invoke(currentSaveData);
        }

        /// <summary>
        /// Applies loaded data to the player
        /// </summary>
        private void ApplyPlayerData()
        {
            if (currentSaveData == null)
                return;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // Apply position and rotation
                // Use saveStationPosition if available (returning from map), otherwise use playerPosition
                Vector3 spawnPosition = currentSaveData.saveStationPosition != Vector3.zero 
                    ? currentSaveData.saveStationPosition 
                    : currentSaveData.playerPosition;
                    
                player.transform.position = spawnPosition;
                player.transform.rotation = currentSaveData.playerRotation;
                
                Debug.Log($"[SaveManager] Respawned player at: {spawnPosition}");

                // Apply health - CRITICAL: Reset death state first!
                Health playerHealth = player.GetComponent<Health>();
                if (playerHealth != null)
                {
                    // Reset death state if player was dead
                    if (playerHealth.IsDead)
                    {
                        playerHealth.ResetHealth();
                        Debug.Log("[SaveManager] Reset death state before applying saved health");
                    }

                    // Set max health first
                    playerHealth.SetMaxHealth(currentSaveData.playerMaxHealth, false);
                    
                    // Then set current health
                    // Only enforce full health if saved health was invalid (0 or negative)
                    int healthToRestore = currentSaveData.playerHealth;
                    if (healthToRestore <= 0)
                    {
                        healthToRestore = currentSaveData.playerMaxHealth; // Restore to full if saved health was invalid
                        Debug.LogWarning($"[SaveManager] Saved health was {currentSaveData.playerHealth}, restoring to full health instead");
                    }
                    
                    // Use SetHealth with checkDeath=false to prevent death trigger during load
                    playerHealth.SetHealth(healthToRestore, false);
                    
                    Debug.Log($"[SaveManager] Restored health: {healthToRestore}/{currentSaveData.playerMaxHealth}");
                }

                // Apply XP and Level (PlayerXP component)
                PlayerXP playerXP = player.GetComponent<PlayerXP>();
                if (playerXP != null && currentSaveData.currentLevel > 0)
                {
                    playerXP.LoadXPData(currentSaveData.currentXP, currentSaveData.currentLevel, currentSaveData.xpToNextLevel);
                    Debug.Log($"[SaveManager] Loaded XP: {currentSaveData.currentXP} | Level: {currentSaveData.currentLevel}");
                }

                // Apply skills from SkillManager
                var skillManager = VisionsForetold.Game.SkillSystem.SkillManager.Instance;
                if (skillManager != null && currentSaveData.skills != null)
                {
                    skillManager.LoadSkillData(currentSaveData.skills);
                    Debug.Log($"[SaveManager] Loaded skills - Level: {currentSaveData.skills.level}");
                }
                else if (skillManager == null)
                {
                    Debug.LogWarning("[SaveManager] SkillManager not found - skills not loaded");
                }

                // Ensure all player components are enabled
                EnsurePlayerComponentsEnabled(player);

                // Apply other data as needed
            }
            else
            {
                Debug.LogWarning("[SaveManager] Player not found in scene! Cannot apply player data.");
            }
        }

        /// <summary>
        /// Ensures all player components are enabled after loading
        /// </summary>
        private void EnsurePlayerComponentsEnabled(GameObject player)
        {
            // Re-enable PlayerInput
            var playerInput = player.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (playerInput != null && !playerInput.enabled)
            {
                playerInput.enabled = true;
                Debug.Log("[SaveManager] Re-enabled PlayerInput");
            }

            // Re-enable PlayerMovement
            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            if (movement != null && !movement.enabled)
            {
                movement.enabled = true;
                Debug.Log("[SaveManager] Re-enabled PlayerMovement");
            }

            // Re-enable PlayerAttack
            PlayerAttack attack = player.GetComponent<PlayerAttack>();
            if (attack != null && !attack.enabled)
            {
                attack.enabled = true;
                Debug.Log("[SaveManager] Re-enabled PlayerAttack");
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Gets the file path for a save slot
        /// </summary>
        private string GetSaveFilePath(int slotIndex)
        {
            return Path.Combine(saveDirectory, $"save_{slotIndex}.json");
        }

        /// <summary>
        /// Checks if a save exists in the specified slot
        /// </summary>
        public bool DoesSaveExist(int slotIndex)
        {
            return File.Exists(GetSaveFilePath(slotIndex));
        }

        /// <summary>
        /// Gets save data from a slot without loading it
        /// </summary>
        public SaveData GetSaveData(int slotIndex)
        {
            if (!DoesSaveExist(slotIndex))
                return null;

            try
            {
                string json = File.ReadAllText(GetSaveFilePath(slotIndex));
                return JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to read save data: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Deletes a save from the specified slot
        /// </summary>
        public void DeleteSave(int slotIndex)
        {
            string filePath = GetSaveFilePath(slotIndex);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log($"[SaveManager] Deleted save from slot {slotIndex}");
            }
        }

        /// <summary>
        /// Deletes ALL save files (use with caution!)
        /// </summary>
        public void DeleteAllSaves()
        {
            for (int i = 0; i < maxSaveSlots; i++)
            {
                DeleteSave(i);
            }
            
            // Also clear current save data
            currentSaveData = null;
            
            Debug.Log("[SaveManager] Deleted all save files");
        }

        /// <summary>
        /// Opens the save directory in file explorer (Editor only)
        /// </summary>
        [ContextMenu("Open Save Directory")]
        public void OpenSaveDirectory()
        {
            if (Directory.Exists(saveDirectory))
            {
                #if UNITY_EDITOR
                UnityEditor.EditorUtility.RevealInFinder(saveDirectory);
                #else
                System.Diagnostics.Process.Start(saveDirectory);
                #endif
                Debug.Log($"[SaveManager] Opened save directory: {saveDirectory}");
            }
            else
            {
                Debug.LogWarning($"[SaveManager] Save directory doesn't exist: {saveDirectory}");
            }
        }

        /// <summary>
        /// Creates a new save in the specified slot
        /// </summary>
        public void CreateNewSave(int slotIndex)
        {
            currentSaveData = new SaveData
            {
                saveSlotIndex = slotIndex,
                saveName = $"Save {slotIndex + 1}",
                saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
        }

        /// <summary>
        /// Gets the current save data
        /// </summary>
        public SaveData GetCurrentSaveData()
        {
            return currentSaveData;
        }

        #endregion
    }
}
