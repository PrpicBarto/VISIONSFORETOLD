using UnityEngine;
using UnityEngine.SceneManagement;

namespace VisionsForetold.Game.SaveSystem
{
    /// <summary>
    /// Manages player spawning at save stations when returning from map
    /// Automatically positions player at last save location
    /// </summary>
    public class PlayerSpawnManager : MonoBehaviour
    {
        public static PlayerSpawnManager Instance { get; private set; }

        [Header("Spawn Settings")]
        [SerializeField] private float spawnDelay = 0.2f; // Increased delay to ensure Health.Start() completes
        [SerializeField] private bool debugMode = true;

        private bool hasSpawned = false;
        private bool isRestoringHealth = false;

        #region Unity Lifecycle

        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            // Wait a frame to ensure everything is initialized
            Invoke(nameof(CheckAndSpawnPlayer), spawnDelay);
        }

        #endregion

        #region Spawn Logic

        /// <summary>
        /// Checks if player needs to be spawned at save location and does so
        /// </summary>
        private void CheckAndSpawnPlayer()
        {
            if (hasSpawned)
                return;

            // Get SaveManager instance
            SaveManager saveManager = SaveManager.Instance;
            if (saveManager == null)
            {
                if (debugMode)
                    Debug.LogWarning("[PlayerSpawnManager] SaveManager not found!");
                return;
            }

            // Get current save data
            SaveData saveData = saveManager.GetCurrentSaveData();
            if (saveData == null)
            {
                if (debugMode)
                    Debug.Log("[PlayerSpawnManager] No save data found - using default spawn");
                return;
            }

            // Check if we're returning from map (saveStationPosition is set)
            if (saveData.saveStationPosition == Vector3.zero)
            {
                if (debugMode)
                    Debug.Log("[PlayerSpawnManager] No save station position - using default spawn");
                return;
            }

            // Find player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("[PlayerSpawnManager] Player not found in scene!");
                return;
            }

            // Spawn player at save station location
            SpawnPlayerAtSaveLocation(player, saveData);
            hasSpawned = true;
        }

        /// <summary>
        /// Spawns player at the saved save station location
        /// </summary>
        private void SpawnPlayerAtSaveLocation(GameObject player, SaveData saveData)
        {
            // Set player position to save station position
            player.transform.position = saveData.saveStationPosition;
            
            // Optionally restore rotation
            if (saveData.playerRotation != Quaternion.identity)
            {
                player.transform.rotation = saveData.playerRotation;
            }

            if (debugMode)
            {
                Debug.Log($"[PlayerSpawnManager] ? Spawned player at save station: {saveData.saveStationPosition}");
                
                // Find nearest save station for visual confirmation
                SaveStation nearestStation = FindNearestSaveStation(saveData.saveStationPosition);
                if (nearestStation != null)
                {
                    float distance = Vector3.Distance(player.transform.position, nearestStation.transform.position);
                    Debug.Log($"[PlayerSpawnManager] Nearest save station distance: {distance:F2}m");
                }
            }

            // Apply saved health and stats
            ApplyPlayerStats(player, saveData);
        }

        /// <summary>
        /// Applies saved player stats (health, XP, skills, etc.)
        /// </summary>
        private void ApplyPlayerStats(GameObject player, SaveData saveData)
        {
            isRestoringHealth = true; // Flag to prevent issues
            
            // Apply health - CRITICAL: Do this carefully to avoid death trigger
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                if (debugMode)
                    Debug.Log($"[PlayerSpawnManager] Current health before restore: {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}, IsDead: {playerHealth.IsDead}");

                // IMPORTANT: Reset death state first!
                if (playerHealth.IsDead)
                {
                    playerHealth.ResetHealth();
                    if (debugMode)
                        Debug.Log("[PlayerSpawnManager] Reset death state before applying saved health");
                }

                // Set max health first
                playerHealth.SetMaxHealth(saveData.playerMaxHealth, false);
                
                // Then set current health
                // Only enforce minimum if saved health was invalid (0 or negative)
                int healthToRestore = saveData.playerHealth;
                if (healthToRestore <= 0)
                {
                    healthToRestore = saveData.playerMaxHealth; // Restore to full if saved health was invalid
                    if (debugMode)
                        Debug.LogWarning($"[PlayerSpawnManager] Saved health was {saveData.playerHealth}, restoring to full health instead");
                }
                
                // CRITICAL: Use SetHealth with checkDeath=false to bypass death trigger when loading
                if (healthToRestore > 0)
                {
                    playerHealth.SetHealth(healthToRestore, false); // Don't check death when restoring from save
                }
                else
                {
                    // Emergency fallback
                    playerHealth.ResetHealth();
                }
                
                if (debugMode)
                    Debug.Log($"[PlayerSpawnManager] Restored health: {healthToRestore}/{saveData.playerMaxHealth}");
            }

            // Apply XP and Level (PlayerXP component)
            PlayerXP playerXP = player.GetComponent<PlayerXP>();
            if (playerXP != null && saveData.currentLevel > 0)
            {
                playerXP.LoadXPData(saveData.currentXP, saveData.currentLevel, saveData.xpToNextLevel);
                
                if (debugMode)
                    Debug.Log($"[PlayerSpawnManager] Restored XP: {saveData.currentXP} | Level: {saveData.currentLevel}");
            }

            // Apply skills
            var skillManager = VisionsForetold.Game.SkillSystem.SkillManager.Instance;
            if (skillManager != null && saveData.skills != null)
            {
                skillManager.LoadSkillData(saveData.skills);
                
                if (debugMode)
                    Debug.Log($"[PlayerSpawnManager] Restored skills - Level: {saveData.skills.level}");
            }

            // Ensure player components are enabled (in case they were disabled)
            EnsurePlayerComponentsEnabled(player);
            
            isRestoringHealth = false;
        }

        /// <summary>
        /// Ensures all player components are enabled after spawning
        /// </summary>
        private void EnsurePlayerComponentsEnabled(GameObject player)
        {
            // Re-enable PlayerInput
            var playerInput = player.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (playerInput != null && !playerInput.enabled)
            {
                playerInput.enabled = true;
                if (debugMode)
                    Debug.Log("[PlayerSpawnManager] Re-enabled PlayerInput");
            }

            // Re-enable PlayerMovement
            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            if (movement != null && !movement.enabled)
            {
                movement.enabled = true;
                if (debugMode)
                    Debug.Log("[PlayerSpawnManager] Re-enabled PlayerMovement");
            }

            // Re-enable PlayerAttack
            PlayerAttack attack = player.GetComponent<PlayerAttack>();
            if (attack != null && !attack.enabled)
            {
                attack.enabled = true;
                if (debugMode)
                    Debug.Log("[PlayerSpawnManager] Re-enabled PlayerAttack");
            }
        }

        /// <summary>
        /// Finds the nearest save station to a position (for debugging)
        /// </summary>
        private SaveStation FindNearestSaveStation(Vector3 position)
        {
            SaveStation[] stations = FindObjectsOfType<SaveStation>();
            if (stations.Length == 0)
                return null;

            SaveStation nearest = null;
            float minDistance = Mathf.Infinity;

            foreach (SaveStation station in stations)
            {
                float distance = Vector3.Distance(position, station.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = station;
                }
            }

            return nearest;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Manually trigger player spawn (useful for testing)
        /// </summary>
        public void ForceSpawn()
        {
            hasSpawned = false;
            CheckAndSpawnPlayer();
        }

        /// <summary>
        /// Reset spawn flag (useful when reloading scene)
        /// </summary>
        public void ResetSpawnFlag()
        {
            hasSpawned = false;
        }

        #endregion

        #region Debug

        private void OnDrawGizmosSelected()
        {
            if (!debugMode)
                return;

            SaveManager saveManager = SaveManager.Instance;
            if (saveManager == null)
                return;

            SaveData saveData = saveManager.GetCurrentSaveData();
            if (saveData == null || saveData.saveStationPosition == Vector3.zero)
                return;

            // Draw spawn location
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(saveData.saveStationPosition, 1f);
            Gizmos.DrawLine(saveData.saveStationPosition, saveData.saveStationPosition + Vector3.up * 3f);
        }

        #endregion
    }
}
