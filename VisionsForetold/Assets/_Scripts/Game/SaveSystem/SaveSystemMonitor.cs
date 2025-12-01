using UnityEngine;

namespace VisionsForetold.Game.SaveSystem
{
    /// <summary>
    /// Runtime monitor for save system - displays status and helps debug issues
    /// Add this to SaveManager GameObject for real-time diagnostics
    /// </summary>
    [RequireComponent(typeof(SaveManager))]
    public class SaveSystemMonitor : MonoBehaviour
    {
        [Header("Display Settings")]
        [SerializeField] private bool showRuntimeInfo = true;
        [SerializeField] private KeyCode toggleKey = KeyCode.F12;
        
        private SaveManager saveManager;
        private GUIStyle style;
        private bool initialized;

        private void Start()
        {
            saveManager = GetComponent<SaveManager>();
            InitializeGUI();
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                showRuntimeInfo = !showRuntimeInfo;
                Debug.Log($"[SaveSystemMonitor] Runtime info: {(showRuntimeInfo ? "ON" : "OFF")}");
            }
        }

        private void InitializeGUI()
        {
            style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 12;
            style.padding = new RectOffset(10, 10, 10, 10);
            initialized = true;
        }

        private void OnGUI()
        {
            if (!showRuntimeInfo || !initialized)
                return;

            // Background
            GUI.Box(new Rect(10, 10, 350, 250), "");

            // Title
            GUIStyle titleStyle = new GUIStyle(style);
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.fontSize = 14;
            GUI.Label(new Rect(20, 15, 330, 20), "=== SAVE SYSTEM MONITOR ===", titleStyle);

            int yPos = 40;

            // SaveManager Status
            if (saveManager != null)
            {
                GUI.Label(new Rect(20, yPos, 330, 20), "? SaveManager: Active", style);
                yPos += 20;

                SaveData currentSave = saveManager.GetCurrentSaveData();
                if (currentSave != null)
                {
                    GUI.Label(new Rect(20, yPos, 330, 20), $"  Save Name: {currentSave.saveName}", style);
                    yPos += 20;
                    GUI.Label(new Rect(20, yPos, 330, 20), $"  Slot: {currentSave.saveSlotIndex}", style);
                    yPos += 20;
                    GUI.Label(new Rect(20, yPos, 330, 20), $"  Scene: {currentSave.currentSceneName}", style);
                    yPos += 20;
                    GUI.Label(new Rect(20, yPos, 330, 20), $"  Last Saved: {currentSave.saveDate}", style);
                    yPos += 20;
                }
                else
                {
                    GUI.Label(new Rect(20, yPos, 330, 20), "  No active save", style);
                    yPos += 20;
                }
            }
            else
            {
                GUIStyle errorStyle = new GUIStyle(style);
                errorStyle.normal.textColor = Color.red;
                GUI.Label(new Rect(20, yPos, 330, 20), "? SaveManager: Missing!", errorStyle);
                yPos += 20;
            }

            yPos += 10;

            // Player Status
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                GUI.Label(new Rect(20, yPos, 330, 20), "? Player: Found", style);
                yPos += 20;
                GUI.Label(new Rect(20, yPos, 330, 20), $"  Position: {player.transform.position.ToString("F1")}", style);
                yPos += 20;

                Health health = player.GetComponent<Health>();
                if (health != null)
                {
                    GUI.Label(new Rect(20, yPos, 330, 20), $"  Health: {health.CurrentHealth}/{health.MaxHealth}", style);
                    yPos += 20;
                }
            }
            else
            {
                GUIStyle warnStyle = new GUIStyle(style);
                warnStyle.normal.textColor = Color.yellow;
                GUI.Label(new Rect(20, yPos, 330, 20), "? Player: Not Found", warnStyle);
                yPos += 20;
            }

            yPos += 10;

            // Save Directory Info
            string saveDir = System.IO.Path.Combine(Application.persistentDataPath, "Saves");
            if (System.IO.Directory.Exists(saveDir))
            {
                var files = System.IO.Directory.GetFiles(saveDir, "*.json");
                GUI.Label(new Rect(20, yPos, 330, 20), $"? Save Files: {files.Length}", style);
                yPos += 20;
            }
            else
            {
                GUI.Label(new Rect(20, yPos, 330, 20), "? Save Directory: Not Created Yet", style);
                yPos += 20;
            }

            // Help text
            GUIStyle helpStyle = new GUIStyle(style);
            helpStyle.fontSize = 10;
            helpStyle.normal.textColor = Color.gray;
            GUI.Label(new Rect(20, yPos + 10, 330, 20), $"Press {toggleKey} to toggle this display", helpStyle);
        }

        private void OnDrawGizmos()
        {
            // Draw SaveManager indicator
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up, "Save Manager", new GUIStyle()
            {
                normal = { textColor = Color.green },
                fontSize = 12
            });
            #endif
        }
    }
}
