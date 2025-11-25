using UnityEngine;

namespace VisionsForetold.Game.Player.Echo
{
    /// <summary>
    /// Echolocation Fog of War System
    /// Creates a fog overlay that reveals areas through expanding pulse waves.
    /// Uses a simple transparent plane approach for reliable rendering without complex post-processing.
    /// </summary>
    public class EcholocationController : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private bool enableEcholocation = true;
        [Tooltip("Material using the Custom/URP/Echolocation shader")]
        [SerializeField] private Material fogMaterial;
        [SerializeField] private Transform player;
        
        [Header("Fog Plane Configuration")]
        [SerializeField] private Vector3 planeSize = new Vector3(200, 1, 200);
        [Tooltip("Distance from camera to place fog plane")]
        [SerializeField] private float planeDistanceFromCamera = 50f;
        [Tooltip("Use camera-facing billboard mode (recommended for 3D)")]
        [SerializeField] private bool useCameraBillboard = true;
        
        [Header("Pulse Settings")]
        [Tooltip("How fast the pulse expands (units per second)")]
        [SerializeField] private float pulseSpeed = 20f;
        [Tooltip("Maximum radius the pulse reaches before stopping")]
        [SerializeField] private float maxPulseRadius = 40f;
        [Tooltip("Time between automatic pulses")]
        [SerializeField] private float pulseInterval = 2.5f;
        [Tooltip("Width of the visible pulse ring")]
        [SerializeField] private float pulseWidth = 5f;
        [SerializeField] private bool autoPulse = true;

        [Header("Fog & Reveal Settings")]
        [Tooltip("Radius around player that stays visible")]
        [SerializeField] private float permanentRevealRadius = 0f; // Disabled by default for global fog
        [Tooltip("How long it takes for fog to return after pulse")]
        [SerializeField] private float revealDuration = 3f;
        [Tooltip("Curve controlling how fog fades back in")]
        [SerializeField] private AnimationCurve revealFadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        [Tooltip("Color of the fog (dark = more hidden)")]
        [SerializeField] private Color fogColor = new Color(0f, 0f, 0f, 1f); // Pure black
        [Tooltip("How opaque the fog is (0 = transparent, 1 = solid)")]
        [SerializeField, Range(0f, 1f)] private float fogDensity = 1.0f; // Fully opaque
        
        [Header("Distance-Based Fog Density")]
        [Tooltip("Distance at which fog reaches maximum density")]
        [SerializeField] private float fogDistanceFalloff = 100f;
        [Tooltip("Minimum fog density near player (0-1)")]
        [SerializeField, Range(0f, 1f)] private float fogMinDensity = 0.95f; // Very dense everywhere
        [Tooltip("Maximum fog density far from player (0-1)")]
        [SerializeField, Range(0f, 1f)] private float fogMaxDensity = 1.0f; // Pitch black far away

        [Header("Visual Settings")]
        [Tooltip("Color of the pulse ring edge glow")]
        [SerializeField] private Color edgeColor = new Color(0.3f, 0.6f, 1f, 1f);
        [Tooltip("Intensity of the pulse ring glow effect")]
        [SerializeField] private float edgeIntensity = 1.5f;

        [Header("Debug")]
        [SerializeField] private bool showDebug = true;
        [SerializeField] private bool showGizmos = true;

        // Runtime state
        private GameObject fogPlane;
        private float currentPulseRadius;
        private float timeSinceLastPulse;
        private bool isPulsing;
        private float revealFade = 0f;
        private float timeSinceReveal;

        // Shader property IDs (cached for performance)
        private static readonly int FogColorID = Shader.PropertyToID("_FogColor");
        private static readonly int FogDensityID = Shader.PropertyToID("_FogDensity");
        private static readonly int FogDistanceFalloffID = Shader.PropertyToID("_FogDistanceFalloff");
        private static readonly int FogMinDensityID = Shader.PropertyToID("_FogMinDensity");
        private static readonly int FogMaxDensityID = Shader.PropertyToID("_FogMaxDensity");
        private static readonly int PulseCenterID = Shader.PropertyToID("_PulseCenter");
        private static readonly int PulseRadiusID = Shader.PropertyToID("_PulseRadius");
        private static readonly int PulseWidthID = Shader.PropertyToID("_PulseWidth");
        private static readonly int PulseIntensityID = Shader.PropertyToID("_PulseIntensity");
        private static readonly int RevealRadiusID = Shader.PropertyToID("_RevealRadius");
        private static readonly int RevealFadeID = Shader.PropertyToID("_RevealFade");
        private static readonly int EdgeColorID = Shader.PropertyToID("_EdgeColor");
        private static readonly int EdgeIntensityID = Shader.PropertyToID("_EdgeIntensity");

        #region Unity Lifecycle

        private void Start()
        {
            if (!ValidateSetup())
            {
                enabled = false;
                return;
            }

            FindPlayerIfNeeded();
            CreateFogPlane();
            UpdateShaderProperties();
            
            if (autoPulse)
            {
                InvokeRepeating(nameof(TriggerPulse), 1f, pulseInterval);
            }
            
            Debug.Log("[Echolocation] System initialized successfully!");
        }

        private void Update()
        {
            if (!enableEcholocation || fogPlane == null) return;

            UpdateFogPlanePosition();
            UpdatePulseAnimation();
            UpdateRevealFade();
            UpdateShaderProperties();
        }

        private void OnDestroy()
        {
            if (fogPlane != null)
            {
                Destroy(fogPlane);
            }
        }

        #endregion

        #region Setup & Validation

        private bool ValidateSetup()
        {
            if (!enableEcholocation)
            {
                Debug.LogWarning("[Echolocation] System is disabled.");
                return false;
            }

            if (fogMaterial == null)
            {
                Debug.LogError("[Echolocation] Fog Material is not assigned!");
                Debug.LogError("[Echolocation] Create a material with shader 'Custom/URP/Echolocation' and assign it in the Inspector.");
                return false;
            }

            if (fogMaterial.shader.name != "Custom/URP/Echolocation")
            {
                Debug.LogWarning($"[Echolocation] Material is using shader '{fogMaterial.shader.name}' instead of 'Custom/URP/Echolocation'. This may not work correctly.");
            }

            return true;
        }

        private void FindPlayerIfNeeded()
        {
            if (player != null) return;

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log($"[Echolocation] Auto-found player: {player.name}");
            }
            else
            {
                player = transform;
                Debug.LogWarning("[Echolocation] No Player tag found. Using this GameObject as center.");
            }
        }

        #endregion

        #region Fog Plane Creation

        private void CreateFogPlane()
        {
            // Create quad primitive (1x1 by default)
            fogPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
            fogPlane.name = "EcholocationFogPlane";
            
            // Remove collider (we don't need physics)
            Destroy(fogPlane.GetComponent<Collider>());
            
            // Configure renderer
            MeshRenderer renderer = fogPlane.GetComponent<MeshRenderer>();
            renderer.material = fogMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sortingOrder = 1000; // Render on top
            
            // Position and scale - will be updated in Update()
            fogPlane.transform.localScale = new Vector3(planeSize.x * 10, planeSize.z * 10, 1);
            
            // Set layer to ignore raycasts
            fogPlane.layer = LayerMask.NameToLayer("Ignore Raycast");
            
            if (showDebug)
            {
                Debug.Log($"[Echolocation] Fog plane created:");
                Debug.Log($"  Scale: {fogPlane.transform.localScale}");
                Debug.Log($"  Billboard Mode: {useCameraBillboard}");
            }
        }

        #endregion

        #region Update Methods

        private void UpdateFogPlanePosition()
        {
            if (useCameraBillboard)
            {
                // Billboard mode: Always face camera
                Camera mainCam = Camera.main;
                if (mainCam == null) return;

                // Position plane in front of camera
                Vector3 camForward = mainCam.transform.forward;
                fogPlane.transform.position = mainCam.transform.position + camForward * planeDistanceFromCamera;
                
                // Face camera (billboard)
                fogPlane.transform.rotation = Quaternion.LookRotation(-camForward);
            }
            else
            {
                // Legacy mode: Horizontal plane following player
                Vector3 fogPos = player.position;
                fogPos.y = player.position.y; // Same height as player
                fogPlane.transform.position = fogPos;
                fogPlane.transform.rotation = Quaternion.Euler(90, 0, 0); // Face down
            }
        }

        private void UpdatePulseAnimation()
        {
            if (!isPulsing) return;

            currentPulseRadius += pulseSpeed * Time.deltaTime;
            
            if (currentPulseRadius >= maxPulseRadius)
            {
                EndPulse();
            }
        }

        private void UpdateRevealFade()
        {
            if (isPulsing) return;

            // Gradually fade fog back in after pulse
            timeSinceReveal += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(timeSinceReveal / revealDuration);
            revealFade = revealFadeCurve.Evaluate(normalizedTime);
        }

        private void UpdateShaderProperties()
        {
            if (fogMaterial == null) return;

            // Update all shader properties
            fogMaterial.SetColor(FogColorID, fogColor);
            fogMaterial.SetFloat(FogDensityID, fogDensity);
            fogMaterial.SetFloat(FogDistanceFalloffID, fogDistanceFalloff);
            fogMaterial.SetFloat(FogMinDensityID, fogMinDensity);
            fogMaterial.SetFloat(FogMaxDensityID, fogMaxDensity);
            fogMaterial.SetVector(PulseCenterID, player.position);
            fogMaterial.SetFloat(PulseRadiusID, isPulsing ? currentPulseRadius : 0f);
            fogMaterial.SetFloat(PulseWidthID, pulseWidth);
            fogMaterial.SetFloat(PulseIntensityID, isPulsing ? 1f : 0f);
            fogMaterial.SetFloat(RevealRadiusID, permanentRevealRadius);
            fogMaterial.SetFloat(RevealFadeID, revealFade);
            fogMaterial.SetColor(EdgeColorID, edgeColor);
            fogMaterial.SetFloat(EdgeIntensityID, edgeIntensity);
        }

        #endregion

        #region Pulse Control

        /// <summary>
        /// Manually trigger an echolocation pulse
        /// </summary>
        public void TriggerPulse()
        {
            if (!enableEcholocation) return;

            isPulsing = true;
            currentPulseRadius = 0f;
            timeSinceLastPulse = 0f;
            timeSinceReveal = 0f;
            revealFade = 0f;

            if (showDebug)
            {
                Debug.Log($"[Echolocation] ?? Pulse triggered at {player.position}");
            }
        }

        private void EndPulse()
        {
            isPulsing = false;
            currentPulseRadius = 0f;
            timeSinceReveal = 0f;

            if (showDebug)
            {
                Debug.Log("[Echolocation] Pulse completed");
            }
        }

        /// <summary>
        /// Stop automatic pulsing
        /// </summary>
        public void StopAutoPulse()
        {
            autoPulse = false;
            CancelInvoke(nameof(TriggerPulse));
        }

        /// <summary>
        /// Start automatic pulsing
        /// </summary>
        public void StartAutoPulse()
        {
            autoPulse = true;
            InvokeRepeating(nameof(TriggerPulse), 0f, pulseInterval);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Enable or disable the echolocation system
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            enableEcholocation = enabled;
            
            if (fogPlane != null)
            {
                fogPlane.SetActive(enabled);
            }

            if (enabled && autoPulse)
            {
                StartAutoPulse();
            }
            else
            {
                StopAutoPulse();
            }
        }

        /// <summary>
        /// Get whether a pulse is currently active
        /// </summary>
        public bool IsPulsing => isPulsing;

        /// <summary>
        /// Get the current pulse radius
        /// </summary>
        public float CurrentPulseRadius => currentPulseRadius;

        /// <summary>
        /// Get time until next pulse
        /// </summary>
        public float TimeUntilNextPulse => pulseInterval - timeSinceLastPulse;

        /// <summary>
        /// Get the fog material (for external systems to update shader properties)
        /// </summary>
        public Material GetFogMaterial()
        {
            return fogMaterial;
        }

        #endregion

        #region Debug & Gizmos

        private void OnDrawGizmosSelected()
        {
            if (!showGizmos || player == null) return;

            // Draw fog plane position
            if (useCameraBillboard && Camera.main != null)
            {
                Gizmos.color = Color.magenta;
                Vector3 planePos = Camera.main.transform.position + Camera.main.transform.forward * planeDistanceFromCamera;
                Gizmos.DrawWireCube(planePos, new Vector3(10, 10, 0.1f));
            }
            else
            {
                Gizmos.color = Color.magenta;
                Vector3 planePos = player.position;
                Gizmos.DrawWireCube(planePos, new Vector3(5, 0.1f, 5));
            }

            // Draw permanent reveal radius (green circle)
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(player.position, permanentRevealRadius);

            // Draw max pulse radius (yellow circle)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.position, maxPulseRadius);

            // Draw current pulse (cyan circle)
            if (Application.isPlaying && isPulsing)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(player.position, currentPulseRadius);
            }
        }

        private void OnGUI()
        {
            if (!showDebug) return;

            GUIStyle style = new GUIStyle(GUI.skin.box)
            {
                fontSize = 14,
                alignment = TextAnchor.UpperLeft
            };
            style.normal.textColor = Color.white;

            string status = "=== ECHOLOCATION ===\n";
            status += $"Enabled: {enableEcholocation}\n";
            status += $"Auto-Pulse: {autoPulse}\n";
            status += $"Pulsing: {(isPulsing ? "YES ?" : "NO")}\n";
            
            if (isPulsing)
            {
                status += $"Radius: {currentPulseRadius:F1}/{maxPulseRadius}\n";
            }
            else
            {
                status += $"Next Pulse: {TimeUntilNextPulse:F1}s\n";
            }
            
            status += $"Reveal Fade: {revealFade:F2}\n";
            status += $"Fog Density: {fogDensity:F2}\n";
            
            if (fogPlane != null)
            {
                status += $"Plane Active: {fogPlane.activeSelf}\n";
            }
            else
            {
                status += "? Plane: NULL\n";
            }

            GUI.Box(new Rect(10, 10, 280, 180), status, style);
        }

        private void OnValidate()
        {
            // Clamp values to valid ranges
            pulseSpeed = Mathf.Max(0.1f, pulseSpeed);
            maxPulseRadius = Mathf.Max(1f, maxPulseRadius);
            pulseInterval = Mathf.Max(0.1f, pulseInterval);
            pulseWidth = Mathf.Max(0.1f, pulseWidth);
            permanentRevealRadius = Mathf.Max(0f, permanentRevealRadius);
            revealDuration = Mathf.Max(0.1f, revealDuration);
            edgeIntensity = Mathf.Max(0f, edgeIntensity);
            fogDensity = Mathf.Clamp01(fogDensity);
            fogDistanceFalloff = Mathf.Max(1f, fogDistanceFalloff);
            fogMinDensity = Mathf.Clamp01(fogMinDensity);
            fogMaxDensity = Mathf.Clamp01(fogMaxDensity);
            
            // Ensure min <= max
            if (fogMinDensity > fogMaxDensity)
            {
                fogMaxDensity = fogMinDensity;
            }
            
            // Warn about very dark fog
            if (fogColor.r < 0.1f && fogColor.g < 0.1f && fogColor.b < 0.1f)
            {
                Debug.LogWarning("[Echolocation] Fog color is very dark. Scene may appear mostly black.");
            }
        }

        #endregion
    }
}