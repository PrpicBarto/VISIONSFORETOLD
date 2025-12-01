using UnityEngine;

namespace VisionsForetold.Game.Player.Echo
{
    /// <summary>
    /// Central manager for the echolocation system
    /// Coordinates all echo components and ensures proper setup
    /// Attach this to a GameObject in your scene (e.g., "EcholocationManager")
    /// </summary>
    public class EchoManager : MonoBehaviour
    {
        [Header("Required Components")]
        [Tooltip("The echolocation controller (manages fog and pulses)")]
        [SerializeField] private EcholocationController echoController;
        
        [Tooltip("Detects objects hit by echolocation")]
        [SerializeField] private EchoIntersectionDetector intersectionDetector;
        
        [Header("Optional Components")]
        [Tooltip("Reveals objects temporarily after pulse")]
        [SerializeField] private EchoRevealSystem revealSystem;
        
        [Tooltip("Renders edge outlines on detected objects")]
        [SerializeField] private EchoEdgeDetection edgeDetection;
        
        [Tooltip("Keeps pulse areas visible for a duration")]
        [SerializeField] private EchoPulseMemory pulseMemory;
        
        [Header("Player Reference")]
        [Tooltip("Player transform (auto-detected if not assigned)")]
        [SerializeField] private Transform player;
        
        [Header("Camera Settings")]
        [Tooltip("Main camera (auto-detected if not assigned)")]
        [SerializeField] private Camera mainCamera;
        
        [Tooltip("Camera render distance (affects fog plane size)")]
        [SerializeField] private float cameraFarClipPlane = 1000f;
        
        [Header("Scene Configuration")]
        [Tooltip("Ground/floor layer for detecting ground level")]
        [SerializeField] private LayerMask groundLayer;
        
        [Tooltip("Manually set ground level (if auto-detect fails)")]
        [SerializeField] private float manualGroundLevel = 0f;
        
        [Tooltip("Use manual ground level instead of auto-detect")]
        [SerializeField] private bool useManualGroundLevel = false;
        
        [Header("Material Setup")]
        [Tooltip("Material using the Custom/URP/Echolocation shader")]
        [SerializeField] private Material echolocationMaterial;
        
        [Header("Performance")]
        [Tooltip("Update interval for position tracking (0 = every frame)")]
        [SerializeField] private int updateInterval = 0;
        
        [Header("Debug")]
        [SerializeField] private bool showDebug = true;
        [SerializeField] private bool validateSetupOnStart = true;

        // Runtime state
        private int frameCounter = 0;
        private float detectedGroundLevel = 0f;
        private bool isInitialized = false;

        #region Unity Lifecycle

        private void Awake()
        {
            // Auto-find player if not assigned
            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    player = playerObj.transform;
                    if (showDebug)
                        Debug.Log($"[EchoManager] Auto-found player: {player.name}");
                }
                else
                {
                    Debug.LogError("[EchoManager] No player found! Please assign or tag player as 'Player'");
                }
            }

            // Auto-find camera if not assigned
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera != null && showDebug)
                {
                    Debug.Log($"[EchoManager] Auto-found camera: {mainCamera.name}");
                }
            }

            // Auto-find echo controller if not assigned
            if (echoController == null)
            {
                echoController = GetComponent<EcholocationController>();
            }

            // Auto-find intersection detector if not assigned
            if (intersectionDetector == null)
            {
                intersectionDetector = GetComponent<EchoIntersectionDetector>();
            }
        }

        private void Start()
        {
            if (validateSetupOnStart)
            {
                ValidateAndSetup();
            }

            DetectGroundLevel();
            isInitialized = true;

            if (showDebug)
            {
                Debug.Log("[EchoManager] Initialization complete!");
                LogSetupStatus();
            }
        }

        private void Update()
        {
            if (!isInitialized) return;

            frameCounter++;
            
            if (updateInterval > 0 && frameCounter % updateInterval != 0)
                return;

            // Keep ground level updated if player moves vertically
            if (!useManualGroundLevel && player != null)
            {
                DetectGroundLevel();
            }
        }

        #endregion

        #region Setup & Validation

        private void ValidateAndSetup()
        {
            bool hasErrors = false;

            // Check required components
            if (echoController == null)
            {
                Debug.LogError("[EchoManager] EcholocationController is missing! Please assign it in the inspector or add it to this GameObject.");
                hasErrors = true;
            }

            if (intersectionDetector == null)
            {
                Debug.LogWarning("[EchoManager] EchoIntersectionDetector is not assigned. Object detection will not work.");
            }

            // Check material
            if (echolocationMaterial == null)
            {
                Debug.LogError("[EchoManager] Echolocation material is not assigned! Create a material with 'Custom/URP/Echolocation' shader.");
                hasErrors = true;
            }
            else if (echolocationMaterial.shader.name != "Custom/URP/Echolocation")
            {
                Debug.LogWarning($"[EchoManager] Material is using wrong shader: '{echolocationMaterial.shader.name}'. Expected 'Custom/URP/Echolocation'");
            }

            // Check player
            if (player == null)
            {
                Debug.LogError("[EchoManager] Player transform is not assigned! Echolocation needs a player reference.");
                hasErrors = true;
            }

            // Check camera
            if (mainCamera == null)
            {
                Debug.LogWarning("[EchoManager] Main camera not found. Fog plane positioning may fail.");
            }
            else
            {
                // Update camera far clip plane if needed
                if (mainCamera.farClipPlane < cameraFarClipPlane)
                {
                    mainCamera.farClipPlane = cameraFarClipPlane;
                    if (showDebug)
                        Debug.Log($"[EchoManager] Updated camera far clip plane to {cameraFarClipPlane}");
                }
            }

            if (hasErrors)
            {
                Debug.LogError("[EchoManager] Setup validation failed! Please fix the errors above.");
                enabled = false;
            }
        }

        private void DetectGroundLevel()
        {
            if (player == null) return;

            if (useManualGroundLevel)
            {
                detectedGroundLevel = manualGroundLevel;
                return;
            }

            // Raycast down from player to find ground
            Vector3 rayOrigin = player.position + Vector3.up * 0.5f;
            
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 100f, groundLayer))
            {
                detectedGroundLevel = hit.point.y;
            }
            else
            {
                // Fallback to player Y position
                detectedGroundLevel = player.position.y;
            }
        }

        private void LogSetupStatus()
        {
            Debug.Log("=== ECHO MANAGER SETUP ===");
            Debug.Log($"Player: {(player != null ? player.name : "NULL")}");
            Debug.Log($"Camera: {(mainCamera != null ? mainCamera.name : "NULL")}");
            Debug.Log($"Ground Level: {detectedGroundLevel:F2}");
            Debug.Log($"Material: {(echolocationMaterial != null ? echolocationMaterial.name : "NULL")}");
            Debug.Log("");
            Debug.Log("Components:");
            Debug.Log($"  EcholocationController: {(echoController != null ? "???" : "???")}");
            Debug.Log($"  IntersectionDetector: {(intersectionDetector != null ? "???" : "???")}");
            Debug.Log($"  RevealSystem: {(revealSystem != null ? "???" : "-")}");
            Debug.Log($"  EdgeDetection: {(edgeDetection != null ? "???" : "-")}");
            Debug.Log($"  PulseMemory: {(pulseMemory != null ? "???" : "-")}");
            Debug.Log("==========================");
        }

        #endregion

        #region Public API

        /// <summary>
        /// Manually trigger an echolocation pulse
        /// </summary>
        public void TriggerPulse()
        {
            if (echoController != null)
            {
                echoController.TriggerPulse();
            }
        }

        /// <summary>
        /// Enable or disable the entire echolocation system
        /// </summary>
        public void SetEcholocationEnabled(bool enabled)
        {
            if (echoController != null)
            {
                echoController.SetEnabled(enabled);
            }

            // Enable/disable optional components
            if (intersectionDetector != null)
                intersectionDetector.enabled = enabled;
                
            if (revealSystem != null)
                revealSystem.enabled = enabled;
                
            if (edgeDetection != null)
                edgeDetection.enabled = enabled;
                
            if (pulseMemory != null)
                pulseMemory.enabled = enabled;
        }

        /// <summary>
        /// Get the echolocation controller
        /// </summary>
        public EcholocationController GetController()
        {
            return echoController;
        }

        /// <summary>
        /// Get the intersection detector
        /// </summary>
        public EchoIntersectionDetector GetIntersectionDetector()
        {
            return intersectionDetector;
        }

        /// <summary>
        /// Get detected ground level
        /// </summary>
        public float GetGroundLevel()
        {
            return detectedGroundLevel;
        }

        /// <summary>
        /// Force ground level re-detection
        /// </summary>
        public void RefreshGroundLevel()
        {
            DetectGroundLevel();
        }

        #endregion

        #region Debug

        private void OnDrawGizmosSelected()
        {
            if (player == null) return;

            // Draw ground level
            Gizmos.color = Color.green;
            Vector3 groundCenter = player.position;
            groundCenter.y = useManualGroundLevel ? manualGroundLevel : detectedGroundLevel;
            Gizmos.DrawWireCube(groundCenter, new Vector3(30, 0.1f, 30));

            // Draw player position
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(player.position, 1f);
        }

        #endregion
    }
}
