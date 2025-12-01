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
        [SerializeField] private bool useCameraBillboard = false;
        [Tooltip("Ground level Y position (for ground-based echolocation)")]
        [SerializeField] private float groundLevel = 0f;
        [Tooltip("Auto-detect ground level from player Y position")]
        [SerializeField] private bool autoDetectGroundLevel = true;
        [Tooltip("Height offset above ground level for fog plane")]
        [SerializeField] private float fogPlaneHeightOffset = 0.1f;
        
        [Header("Vertical Fog Coverage")]
        [Tooltip("Number of stacked fog planes for vertical coverage")]
        [SerializeField] private int verticalPlaneCount = 5;
        [Tooltip("Total vertical height to cover with fog")]
        [SerializeField] private float verticalCoverageHeight = 20f;
        [Tooltip("Use multiple planes (true) or single billboard (false)")]
        [SerializeField] private bool useMultiplePlanes = true;
        
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
        [SerializeField] private float permanentRevealRadius = 3f; // Increased from 0 to 3 meters
        [Tooltip("How long it takes for fog to return after pulse")]
        [SerializeField] private float revealDuration = 3f;
        [Tooltip("Curve controlling how fog fades back in")]
        [SerializeField] private AnimationCurve revealFadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        [Tooltip("Color of the fog (dark = more hidden)")]
        [SerializeField] private Color fogColor = new Color(0f, 0f, 0f, 0.85f); // Changed alpha from 1.0 to 0.85
        [Tooltip("How opaque the fog is (0 = transparent, 1 = solid)")]
        [SerializeField, Range(0f, 1f)] private float fogDensity = 0.85f; // Changed from 1.0 to 0.85
        
        [Header("Distance-Based Fog Density")]
        [Tooltip("Distance at which fog reaches maximum density")]
        [SerializeField] private float fogDistanceFalloff = 100f;
        [Tooltip("Minimum fog density near player (0-1)")]
        [SerializeField, Range(0f, 1f)] private float fogMinDensity = 0.7f; // Changed from 0.95 to 0.7
        [Tooltip("Maximum fog density far from player (0-1)")]
        [SerializeField, Range(0f, 1f)] private float fogMaxDensity = 0.95f; // Changed from 1.0 to 0.95
        
        [Header("Vertical Distance Settings")]
        [Tooltip("How much vertical distance affects fog (0=pure 2D/XZ only, 1=full 3D)")]
        [SerializeField, Range(0f, 1f)] private float verticalInfluence = 0.3f;

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
        private GameObject[] fogPlanes; // Multiple planes for vertical coverage
        private float currentPulseRadius;
        private float timeSinceLastPulse;
        private bool isPulsing;
        private float revealFade = 0f;
        private float timeSinceReveal;
        private Camera mainCamera;
        private bool shaderPropertiesDirty = true;

        // Shader property IDs (cached for performance)
        private static readonly int FogColorID = Shader.PropertyToID("_FogColor");
        private static readonly int FogDensityID = Shader.PropertyToID("_FogDensity");
        private static readonly int FogDistanceFalloffID = Shader.PropertyToID("_FogDistanceFalloff");
        private static readonly int FogMinDensityID = Shader.PropertyToID("_FogMinDensity");
        private static readonly int FogMaxDensityID = Shader.PropertyToID("_FogMaxDensity");
        private static readonly int VerticalInfluenceID = Shader.PropertyToID("_VerticalInfluence");
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
            mainCamera = Camera.main;
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

            // Re-cache camera if it becomes null
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null) return;
            }

            UpdateFogPlanePosition();
            
            bool wasPulsing = isPulsing;
            UpdatePulseAnimation();
            
            if (!isPulsing)
            {
                UpdateRevealFade();
            }
            
            // Always update when pulsing for smooth animation
            // Dirty flag optimization only applies when idle
            bool needsUpdate = isPulsing || wasPulsing != isPulsing || shaderPropertiesDirty;
            
            if (needsUpdate)
            {
                UpdateShaderProperties();
                
                // Only clear dirty flag if we're not pulsing
                // Keep updating every frame while pulsing
                if (!isPulsing)
                {
                    shaderPropertiesDirty = false;
                }
            }
        }

        private void OnDestroy()
        {
            if (fogPlane != null)
            {
                Destroy(fogPlane);
            }
            
            if (fogPlanes != null)
            {
                foreach (var plane in fogPlanes)
                {
                    if (plane != null)
                    {
                        // Destroy material instance
                        MeshRenderer renderer = plane.GetComponent<MeshRenderer>();
                        if (renderer != null && renderer.material != null)
                        {
                            Destroy(renderer.material);
                        }
                        Destroy(plane);
                    }
                }
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
            if (useMultiplePlanes)
            {
                CreateMultipleFogPlanes();
            }
            else
            {
                CreateSingleFogPlane();
            }
        }

        private void CreateMultipleFogPlanes()
        {
            // Create array to hold all planes
            fogPlanes = new GameObject[verticalPlaneCount];
            
            float heightStep = verticalCoverageHeight / Mathf.Max(1, verticalPlaneCount - 1);
            
            for (int i = 0; i < verticalPlaneCount; i++)
            {
                // Create quad primitive
                GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
                plane.name = $"EcholocationFogPlane_{i}";
                
                // Remove collider
                Destroy(plane.GetComponent<Collider>());
                
                // Configure renderer
                MeshRenderer renderer = plane.GetComponent<MeshRenderer>();
                renderer.material = new Material(fogMaterial); // Instance for each plane
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.sortingOrder = 1000 + i; // Stack properly
                
                // Scale plane
                plane.transform.localScale = new Vector3(planeSize.x * 10, planeSize.z * 10, 1);
                
                // Set layer
                plane.layer = LayerMask.NameToLayer("Ignore Raycast");
                
                fogPlanes[i] = plane;
            }
            
            // Keep reference to first plane for compatibility
            fogPlane = fogPlanes[0];
            
            if (showDebug)
            {
                Debug.Log($"[Echolocation] Created {verticalPlaneCount} stacked fog planes");
                Debug.Log($"  Coverage Height: {verticalCoverageHeight}m");
                Debug.Log($"  Height Step: {heightStep:F2}m");
            }
        }

        private void CreateSingleFogPlane()
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
            // Auto-detect ground level from player if enabled
            if (autoDetectGroundLevel)
            {
                groundLevel = player.position.y;
            }

            if (useMultiplePlanes && fogPlanes != null)
            {
                UpdateMultiplePlanePositions();
            }
            else if (fogPlane != null)
            {
                UpdateSinglePlanePosition();
            }
        }

        private void UpdateMultiplePlanePositions()
        {
            float heightStep = verticalCoverageHeight / Mathf.Max(1, verticalPlaneCount - 1);
            float startHeight = groundLevel + fogPlaneHeightOffset;
            
            for (int i = 0; i < fogPlanes.Length; i++)
            {
                if (fogPlanes[i] == null) continue;
                
                Vector3 planePos = player.position;
                planePos.y = startHeight + (i * heightStep);
                
                fogPlanes[i].transform.position = planePos;
                fogPlanes[i].transform.rotation = Quaternion.Euler(90, 0, 0); // Face down
                
                // Update material on each plane
                MeshRenderer renderer = fogPlanes[i].GetComponent<MeshRenderer>();
                if (renderer != null && renderer.material != null)
                {
                    UpdateMaterialProperties(renderer.material);
                }
            }
        }

        private void UpdateSinglePlanePosition()
        {
            if (useCameraBillboard)
            {
                if (mainCamera == null)
                {
                    mainCamera = Camera.main;
                    if (mainCamera == null) return;
                }

                // Position plane in front of camera
                Vector3 camForward = mainCamera.transform.forward;
                fogPlane.transform.position = mainCamera.transform.position + camForward * planeDistanceFromCamera;
                
                // Face camera (billboard)
                fogPlane.transform.rotation = Quaternion.LookRotation(-camForward);
            }
            else
            {
                // Ground-aligned mode: Horizontal plane at ground level following player XZ position
                Vector3 planePos = player.position;
                planePos.y = groundLevel + fogPlaneHeightOffset;
                fogPlane.transform.position = planePos;
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

            // Update main material
            UpdateMaterialProperties(fogMaterial);
        }

        private void UpdateMaterialProperties(Material material)
        {
            if (material == null) return;

            // Always update dynamic properties
            // Send ground-level position for proper XZ distance calculations
            Vector3 pulseCenter = player.position;
            pulseCenter.y = groundLevel; // Lock to ground level for shader calculations
            material.SetVector(PulseCenterID, pulseCenter);
            material.SetFloat(PulseRadiusID, isPulsing ? currentPulseRadius : 0f);
            material.SetFloat(PulseIntensityID, isPulsing ? 1f : 0f);
            
            // Only update static properties when dirty
            if (shaderPropertiesDirty)
            {
                material.SetColor(FogColorID, fogColor);
                material.SetFloat(FogDensityID, fogDensity);
                material.SetFloat(FogDistanceFalloffID, fogDistanceFalloff);
                material.SetFloat(FogMinDensityID, fogMinDensity);
                material.SetFloat(FogMaxDensityID, fogMaxDensity);
                material.SetFloat(VerticalInfluenceID, verticalInfluence);
                material.SetFloat(PulseWidthID, pulseWidth);
                material.SetFloat(RevealRadiusID, permanentRevealRadius);
                material.SetFloat(RevealFadeID, revealFade);
                material.SetColor(EdgeColorID, edgeColor);
                material.SetFloat(EdgeIntensityID, edgeIntensity);
            }
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
            shaderPropertiesDirty = true;

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
            
            if (useMultiplePlanes && fogPlanes != null)
            {
                foreach (var plane in fogPlanes)
                {
                    if (plane != null)
                    {
                        plane.SetActive(enabled);
                    }
                }
            }
            else if (fogPlane != null)
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

            // Draw ground level plane (green wireframe)
            Gizmos.color = Color.green;
            Vector3 groundPlaneCenter = player.position;
            groundPlaneCenter.y = autoDetectGroundLevel ? player.position.y : groundLevel;
            Gizmos.DrawWireCube(groundPlaneCenter, new Vector3(20, 0.1f, 20));
            
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
                Gizmos.DrawWireCube(groundPlaneCenter, new Vector3(15, 0.1f, 15));
            }

            // Draw permanent reveal radius (cyan circle at ground level)
            Gizmos.color = Color.cyan;
            DrawCircleOnGround(groundPlaneCenter, permanentRevealRadius);

            // Draw max pulse radius (yellow circle at ground level)
            Gizmos.color = Color.yellow;
            DrawCircleOnGround(groundPlaneCenter, maxPulseRadius);

            // Draw current pulse (white circle at ground level)
            if (Application.isPlaying && isPulsing)
            {
                Gizmos.color = Color.white;
                DrawCircleOnGround(groundPlaneCenter, currentPulseRadius);
            }
        }
        
        private void DrawCircleOnGround(Vector3 center, float radius)
        {
            int segments = 32;
            float angleStep = 360f / segments;
            Vector3 prevPoint = center + new Vector3(radius, 0, 0);
            
            for (int i = 1; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
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
            status += $"Billboard: {useCameraBillboard}\n";
            status += $"Ground Level: {(autoDetectGroundLevel ? player.position.y : groundLevel):F2}\n";
            status += $"Auto-Pulse: {autoPulse}\n";
            status += $"Pulsing: {(isPulsing ? "YES ?" : "NO")}\n";
            
            if (isPulsing)
            {
                status += $"Radius: {currentPulseRadius:F1}/{maxPulseRadius}\n";
                status += $"Speed: {pulseSpeed:F1} u/s\n";
            }
            else
            {
                status += $"Next Pulse: {TimeUntilNextPulse:F1}s\n";
            }
            
            status += $"Reveal Fade: {revealFade:F2}\n";
            status += $"Fog Density: {fogDensity:F2}\n";
            status += $"FPS: {(1f / Time.deltaTime):F0}\n";
            
            if (fogPlane != null)
            {
                status += $"Plane Active: {fogPlane.activeSelf}\n";
                status += $"Plane Y: {fogPlane.transform.position.y:F2}\n";
            }
            else
            {
                status += "? Plane: NULL\n";
            }
            
            if (mainCamera == null)
            {
                status += "? Camera: NULL\n";
            }

            GUI.Box(new Rect(10, 10, 300, 260), status, style);
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