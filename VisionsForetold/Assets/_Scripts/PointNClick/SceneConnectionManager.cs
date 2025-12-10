using UnityEngine;
using UnityEngine.SceneManagement;

namespace VisionsForetold.PointNClick
{
    /// <summary>
    /// Manages scene connections and area unlocking
    /// Place this in each gameplay scene to handle returning to map and unlocking areas
    /// </summary>
    public class SceneConnectionManager : MonoBehaviour
    {
        public static SceneConnectionManager Instance { get; private set; }

        [Header("Connection Settings")]
        [Tooltip("Scene connection data asset")]
        [SerializeField] private SceneConnectionData connectionData;

        [Tooltip("Current scene name (auto-detected if empty)")]
        [SerializeField] private string currentSceneName;

        [Tooltip("Unlock areas on scene load")]
        [SerializeField] private bool unlockOnStart = false;

        [Tooltip("Unlock areas on scene completion (call CompleteScene())")]
        [SerializeField] private bool unlockOnCompletion = true;

        [Header("Return to Map")]
        [Tooltip("Show return to map UI hint")]
        [SerializeField] private bool showReturnUI = true;

        [Tooltip("Keyboard key to press to return to map")]
        [SerializeField] private KeyCode returnKey = KeyCode.M;

        [Header("Debug")]
        [SerializeField] private bool showDebug = true;

        private bool sceneCompleted = false;

        #region Unity Lifecycle

        private void Awake()
        {
            // Singleton
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Auto-detect scene name
            if (string.IsNullOrEmpty(currentSceneName))
            {
                currentSceneName = SceneManager.GetActiveScene().name;
            }
        }

        private void Start()
        {
            if (connectionData == null)
            {
                Debug.LogWarning("[SceneConnectionManager] No connection data assigned!");
                return;
            }

            // Unlock areas on start if configured
            if (unlockOnStart)
            {
                UnlockAreas();
            }
        }

        private void Update()
        {
            // Handle return to map input (keyboard only - avoid Input Manager)
            if (showReturnUI)
            {
                if (Input.GetKeyDown(returnKey))
                {
                    ReturnToMap();
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Mark scene as completed and unlock areas
        /// </summary>
        public void CompleteScene()
        {
            if (sceneCompleted) return;

            sceneCompleted = true;

            if (unlockOnCompletion)
            {
                UnlockAreas();
            }

            if (showDebug)
            {
                Debug.Log($"[SceneConnectionManager] Scene '{currentSceneName}' completed!");
            }
        }

        /// <summary>
        /// Unlock areas defined in connection data
        /// </summary>
        public void UnlockAreas()
        {
            if (connectionData == null)
            {
                Debug.LogWarning("[SceneConnectionManager] Cannot unlock areas - no connection data!");
                return;
            }

            var connection = connectionData.GetConnection(currentSceneName);
            if (connection == null)
            {
                if (showDebug)
                {
                    Debug.Log($"[SceneConnectionManager] No connection data for scene '{currentSceneName}'");
                }
                return;
            }

            // Unlock areas
            foreach (var area in connection.unlockedAreas)
            {
                if (area != null)
                {
                    area.isUnlocked = true;
                    area.isDiscovered = true;

                    if (showDebug)
                    {
                        Debug.Log($"[SceneConnectionManager] Unlocked area: {area.areaName}");
                    }
                }
            }

            // Lock areas (optional)
            foreach (var area in connection.lockedAreas)
            {
                if (area != null)
                {
                    area.isUnlocked = false;

                    if (showDebug)
                    {
                        Debug.Log($"[SceneConnectionManager] Locked area: {area.areaName}");
                    }
                }
            }
        }

        /// <summary>
        /// Return to the map scene
        /// </summary>
        public void ReturnToMap()
        {
            if (connectionData == null)
            {
                Debug.LogError("[SceneConnectionManager] Cannot return to map - no connection data!");
                return;
            }

            string mapScene = connectionData.GetReturnMapScene(currentSceneName);

            if (string.IsNullOrEmpty(mapScene))
            {
                Debug.LogError("[SceneConnectionManager] No return map scene defined!");
                return;
            }

            if (showDebug)
            {
                Debug.Log($"[SceneConnectionManager] Returning to map: {mapScene}");
            }

            SceneManager.LoadScene(mapScene);
        }

        /// <summary>
        /// Load a specific scene (helper method)
        /// </summary>
        public void LoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[SceneConnectionManager] Scene name is empty!");
                return;
            }

            if (showDebug)
            {
                Debug.Log($"[SceneConnectionManager] Loading scene: {sceneName}");
            }

            SceneManager.LoadScene(sceneName);
        }

        /// <summary>
        /// Get the return map scene name for the current scene
        /// </summary>
        public string GetReturnMapScene()
        {
            if (connectionData == null)
            {
                Debug.LogWarning("[SceneConnectionManager] No connection data, using default map scene");
                return "MapScene";
            }

            return connectionData.GetReturnMapScene(currentSceneName);
        }

        #endregion

        #region Debug GUI

        private void OnGUI()
        {
            if (!showReturnUI) return;

            GUIStyle style = new GUIStyle(GUI.skin.box)
            {
                fontSize = 14,
                alignment = TextAnchor.UpperLeft
            };
            style.normal.textColor = Color.white;

            string helpText = $"Press [{returnKey}] to return to map";
            Vector2 size = style.CalcSize(new GUIContent(helpText));
            
            GUI.Box(new Rect(10, Screen.height - 50, size.x + 20, 40), helpText, style);
        }

        #endregion
    }
}
