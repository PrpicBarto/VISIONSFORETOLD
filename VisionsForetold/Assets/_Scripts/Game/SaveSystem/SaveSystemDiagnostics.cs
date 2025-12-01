using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VisionsForetold.Game.SaveSystem
{
    /// <summary>
    /// Diagnostic tool for troubleshooting save system issues
    /// </summary>
    public class SaveSystemDiagnostics : MonoBehaviour
    {
#if UNITY_EDITOR
        [MenuItem("Tools/Save System/Run Diagnostics")]
        public static void RunDiagnostics()
        {
            Debug.Log("=== SAVE SYSTEM DIAGNOSTICS ===\n");
            
            bool allPassed = true;

            // Check 1: SaveManager exists
            SaveManager saveManager = FindObjectOfType<SaveManager>();
            if (saveManager == null)
            {
                Debug.LogError("? SaveManager not found in scene! Please add SaveManager to your scene.");
                allPassed = false;
            }
            else
            {
                Debug.Log("? SaveManager found");
            }

            // Check 2: SaveStations exist
            SaveStation[] saveStations = FindObjectsOfType<SaveStation>();
            if (saveStations.Length == 0)
            {
                Debug.LogWarning("? No SaveStations found in scene. Add SaveStation components to interaction objects.");
            }
            else
            {
                Debug.Log($"? Found {saveStations.Length} SaveStation(s)");
                
                // Validate each save station
                for (int i = 0; i < saveStations.Length; i++)
                {
                    SaveStation station = saveStations[i];
                    Debug.Log($"\n  SaveStation {i + 1}: {station.gameObject.name}");
                    
                    // Check collider
                    Collider col = station.GetComponent<Collider>();
                    if (col == null)
                    {
                        Debug.LogError($"    ? Missing Collider component!");
                        allPassed = false;
                    }
                    else if (!col.isTrigger)
                    {
                        Debug.LogWarning($"    ? Collider is not set to Trigger!");
                    }
                    else
                    {
                        Debug.Log($"    ? Collider configured correctly");
                    }
                    
                    // Check SaveStationMenu reference
                    if (station.GetType().GetField("saveStationMenu", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(station) == null)
                    {
                        Debug.LogError($"    ? SaveStationMenu not assigned!");
                        allPassed = false;
                    }
                    else
                    {
                        Debug.Log($"    ? SaveStationMenu assigned");
                    }
                }
            }

            // Check 3: Player exists and is tagged
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("? No GameObject with tag 'Player' found!");
                allPassed = false;
            }
            else
            {
                Debug.Log($"? Player found: {player.name}");
                
                // Check PlayerInput
                var playerInput = player.GetComponent<UnityEngine.InputSystem.PlayerInput>();
                if (playerInput == null)
                {
                    Debug.LogWarning("  ? PlayerInput component not found on player. Input system integration may not work.");
                }
                else
                {
                    Debug.Log("  ? PlayerInput component found");
                    
                    // Check for Interact action
                    var interactAction = playerInput.actions.FindAction("Interact");
                    if (interactAction == null)
                    {
                        Debug.LogError("  ? 'Interact' action not found in Input Actions!");
                        Debug.LogError("    Please add an 'Interact' action to your Input Actions asset.");
                        allPassed = false;
                    }
                    else
                    {
                        Debug.Log("  ? 'Interact' action configured");
                    }
                }
                
                // Check Health component
                var health = player.GetComponent<Health>();
                if (health == null)
                {
                    Debug.LogWarning("  ? Health component not found on player. Health won't be saved.");
                }
                else
                {
                    Debug.Log("  ? Health component found");
                }
            }

            // Check 4: Save directory
            if (saveManager != null)
            {
                string saveDir = System.IO.Path.Combine(Application.persistentDataPath, "Saves");
                if (System.IO.Directory.Exists(saveDir))
                {
                    Debug.Log($"? Save directory exists: {saveDir}");
                    
                    var files = System.IO.Directory.GetFiles(saveDir, "*.json");
                    Debug.Log($"  Found {files.Length} save file(s)");
                }
                else
                {
                    Debug.Log($"? Save directory doesn't exist yet (will be created on first save): {saveDir}");
                }
            }

            // Final summary
            Debug.Log("\n=== DIAGNOSTICS COMPLETE ===");
            if (allPassed)
            {
                Debug.Log("? All critical checks passed! Save system should work correctly.");
            }
            else
            {
                Debug.LogError("? Some checks failed. Please review the errors above and fix them.");
            }
        }

        [MenuItem("Tools/Save System/Show Save Directory")]
        public static void ShowSaveDirectory()
        {
            string saveDir = System.IO.Path.Combine(Application.persistentDataPath, "Saves");
            
            if (System.IO.Directory.Exists(saveDir))
            {
                EditorUtility.RevealInFinder(saveDir);
                Debug.Log($"Opened save directory: {saveDir}");
            }
            else
            {
                Debug.LogWarning($"Save directory doesn't exist yet: {saveDir}\nIt will be created when you first save the game.");
            }
        }

        [MenuItem("Tools/Save System/Clear All Saves")]
        public static void ClearAllSaves()
        {
            if (EditorUtility.DisplayDialog("Clear All Saves", 
                "Are you sure you want to delete ALL save files? This cannot be undone!", 
                "Yes, Delete All", 
                "Cancel"))
            {
                string saveDir = System.IO.Path.Combine(Application.persistentDataPath, "Saves");
                
                if (System.IO.Directory.Exists(saveDir))
                {
                    var files = System.IO.Directory.GetFiles(saveDir);
                    int deleted = 0;
                    
                    foreach (var file in files)
                    {
                        try
                        {
                            System.IO.File.Delete(file);
                            deleted++;
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError($"Failed to delete {file}: {e.Message}");
                        }
                    }
                    
                    Debug.Log($"Deleted {deleted} save file(s)");
                }
                else
                {
                    Debug.Log("No save directory found - nothing to delete");
                }
            }
        }
#endif

        [ContextMenu("Test Save")]
        public void TestSave()
        {
            SaveManager saveManager = SaveManager.Instance;
            if (saveManager != null)
            {
                Debug.Log("[Diagnostics] Testing save to slot 0...");
                saveManager.SaveGame(0, "Test Save");
            }
            else
            {
                Debug.LogError("[Diagnostics] SaveManager not found!");
            }
        }

        [ContextMenu("Test Load")]
        public void TestLoad()
        {
            SaveManager saveManager = SaveManager.Instance;
            if (saveManager != null)
            {
                if (saveManager.DoesSaveExist(0))
                {
                    Debug.Log("[Diagnostics] Testing load from slot 0...");
                    saveManager.LoadGame(0);
                }
                else
                {
                    Debug.LogWarning("[Diagnostics] No save found in slot 0");
                }
            }
            else
            {
                Debug.LogError("[Diagnostics] SaveManager not found!");
            }
        }
    }
}
