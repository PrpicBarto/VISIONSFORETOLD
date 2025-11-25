using UnityEngine;
using System.Collections.Generic;

namespace VisionsForetold.Game.Player.Echo
{
    /// <summary>
    /// Data structure representing a detected object hit by echo pulse
    /// </summary>
    [System.Serializable]
    public class EchoHit
    {
        public GameObject hitObject;
        public Vector3 hitPosition;
        public float distanceFromPlayer;
        public float hitTime;
        public EchoObjectType objectType;
        public Renderer objectRenderer;
        
        public EchoHit(GameObject obj, Vector3 pos, float dist, float time, EchoObjectType type, Renderer renderer)
        {
            hitObject = obj;
            hitPosition = pos;
            distanceFromPlayer = dist;
            hitTime = time;
            objectType = type;
            objectRenderer = renderer;
        }
    }

    /// <summary>
    /// Types of objects that can be detected by echolocation
    /// </summary>
    public enum EchoObjectType
    {
        Wall,
        Enemy,
        Item,
        Interactive,
        Hazard,
        Unknown
    }

    /// <summary>
    /// Detects intersections between echo pulses and objects in the world
    /// Works in conjunction with EcholocationController to reveal and highlight objects
    /// </summary>
    public class EchoIntersectionDetector : MonoBehaviour
    {
        [Header("Detection Settings")]
        [Tooltip("Layers to detect with echolocation")]
        [SerializeField] private LayerMask detectionLayers = ~0;
        
        [Tooltip("How many raycasts to perform around the pulse (more = better detection)")]
        [SerializeField] private int raycastResolution = 32;
        
        [Tooltip("Only detect objects this close to the pulse ring edge")]
        [SerializeField] private float detectionThreshold = 2f;
        
        [Tooltip("Enable vertical raycasting (for 3D environments)")]
        [SerializeField] private bool detect3D = false;
        
        [Tooltip("Number of vertical rays (if 3D detection enabled)")]
        [SerializeField] private int verticalRayCount = 5;
        
        [Tooltip("Height range for vertical rays")]
        [SerializeField] private float verticalRayHeight = 10f;

        [Header("Object Classification")]
        [Tooltip("Tag for wall objects")]
        [SerializeField] private string wallTag = "Wall";
        
        [Tooltip("Tag for enemy objects")]
        [SerializeField] private string enemyTag = "Enemy";
        
        [Tooltip("Tag for item objects")]
        [SerializeField] private string itemTag = "Item";
        
        [Tooltip("Tag for interactive objects")]
        [SerializeField] private string interactiveTag = "Interactive";
        
        [Tooltip("Tag for hazard objects")]
        [SerializeField] private string hazardTag = "Hazard";

        [Header("Highlight Settings")]
        [Tooltip("Enable visual highlighting of detected objects")]
        [SerializeField] private bool enableHighlighting = true;
        
        [Tooltip("How long objects stay highlighted after detection")]
        [SerializeField] private float highlightDuration = 3f;
        
        [Tooltip("Emission color for highlighted objects")]
        [SerializeField] private Color highlightColor = new Color(0.3f, 0.6f, 1f, 1f);
        
        [Tooltip("Intensity of highlight emission")]
        [SerializeField] private float highlightIntensity = 2f;

        [Header("Events & Callbacks")]
        [Tooltip("Enable debug logging for detections")]
        [SerializeField] private bool logDetections = true;

        [Header("Performance")]
        [Tooltip("Maximum objects to track simultaneously")]
        [SerializeField] private int maxTrackedObjects = 100;
        
        [Tooltip("Update detection every N frames")]
        [SerializeField] private int detectionFrameInterval = 1;

        // Runtime state
        private EcholocationController echoController;
        private Transform player;
        private List<EchoHit> currentHits = new List<EchoHit>();
        private Dictionary<GameObject, HighlightData> highlightedObjects = new Dictionary<GameObject, HighlightData>();
        private int frameCounter;

        // Shader property IDs
        private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");
        private static readonly int EmissiveIntensityID = Shader.PropertyToID("_EmissiveIntensity");

        // Events
        public System.Action<EchoHit> OnObjectDetected;
        public System.Action<List<EchoHit>> OnPulseComplete;

        #region Unity Lifecycle

        private void Awake()
        {
            echoController = GetComponent<EcholocationController>();
            if (echoController == null)
            {
                Debug.LogError("[EchoIntersection] EcholocationController not found! This component requires EcholocationController.");
                enabled = false;
                return;
            }

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                player = transform;
                Debug.LogWarning("[EchoIntersection] Player not found. Using this transform as center.");
            }
        }

        private void Update()
        {
            frameCounter++;
            
            if (frameCounter % detectionFrameInterval != 0)
                return;

            if (echoController.IsPulsing)
            {
                PerformIntersectionDetection();
            }

            UpdateHighlights();
        }

        private void OnDestroy()
        {
            // Clean up all highlights
            foreach (var kvp in highlightedObjects)
            {
                RemoveHighlight(kvp.Key);
            }
            highlightedObjects.Clear();
        }

        #endregion

        #region Intersection Detection

        /// <summary>
        /// Perform raycasts around the pulse ring to detect objects
        /// </summary>
        private void PerformIntersectionDetection()
        {
            float pulseRadius = echoController.CurrentPulseRadius;
            Vector3 pulseCenter = player.position;

            // Clear previous frame's hits
            currentHits.Clear();

            // Perform circular raycasts
            for (int i = 0; i < raycastResolution; i++)
            {
                float angle = (i / (float)raycastResolution) * 360f;
                Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;

                if (detect3D)
                {
                    // 3D detection with vertical rays
                    PerformVerticalRaycast(pulseCenter, direction, pulseRadius);
                }
                else
                {
                    // 2D detection (horizontal plane only)
                    PerformHorizontalRaycast(pulseCenter, direction, pulseRadius);
                }
            }

            // Invoke completion callback
            if (currentHits.Count > 0)
            {
                OnPulseComplete?.Invoke(new List<EchoHit>(currentHits));
            }
        }

        /// <summary>
        /// Perform a single horizontal raycast from pulse center
        /// </summary>
        private void PerformHorizontalRaycast(Vector3 origin, Vector3 direction, float radius)
        {
            Vector3 rayOrigin = origin;
            Vector3 rayEnd = origin + direction * radius;

            // Cast ray
            if (Physics.Raycast(rayOrigin, direction, out RaycastHit hit, radius, detectionLayers))
            {
                // Check if hit is near pulse ring edge
                float distFromRing = Mathf.Abs(hit.distance - radius);
                
                if (distFromRing <= detectionThreshold)
                {
                    ProcessHit(hit, radius);
                }
            }

            // Debug visualization
            if (logDetections)
            {
                Debug.DrawRay(rayOrigin, direction * radius, Color.yellow, 0.1f);
            }
        }

        /// <summary>
        /// Perform vertical raycasts at given angle
        /// </summary>
        private void PerformVerticalRaycast(Vector3 origin, Vector3 direction, float radius)
        {
            Vector3 targetPoint = origin + direction * radius;

            for (int v = 0; v < verticalRayCount; v++)
            {
                float heightOffset = (v / (float)(verticalRayCount - 1)) * verticalRayHeight - (verticalRayHeight * 0.5f);
                Vector3 rayOrigin = origin + Vector3.up * heightOffset;
                Vector3 rayDirection = (targetPoint - rayOrigin).normalized;
                float rayDistance = Vector3.Distance(rayOrigin, targetPoint);

                if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, rayDistance, detectionLayers))
                {
                    float distFromRing = Mathf.Abs(hit.distance - radius);
                    
                    if (distFromRing <= detectionThreshold)
                    {
                        ProcessHit(hit, radius);
                    }
                }

                // Debug visualization
                if (logDetections)
                {
                    Debug.DrawRay(rayOrigin, rayDirection * rayDistance, Color.cyan, 0.1f);
                }
            }
        }

        /// <summary>
        /// Process a detected hit
        /// </summary>
        private void ProcessHit(RaycastHit hit, float currentRadius)
        {
            GameObject hitObject = hit.collider.gameObject;

            // Skip if already detected this pulse
            if (currentHits.Exists(h => h.hitObject == hitObject))
                return;

            // Skip if max tracked objects reached
            if (currentHits.Count >= maxTrackedObjects)
                return;

            // Classify object type
            EchoObjectType objectType = ClassifyObject(hitObject);

            // Get renderer for highlighting
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            if (renderer == null)
                renderer = hitObject.GetComponentInChildren<Renderer>();

            // Create hit data
            float distanceFromPlayer = Vector3.Distance(player.position, hit.point);
            EchoHit echoHit = new EchoHit(
                hitObject,
                hit.point,
                distanceFromPlayer,
                Time.time,
                objectType,
                renderer
            );

            currentHits.Add(echoHit);

            // Apply highlight
            if (enableHighlighting && renderer != null)
            {
                ApplyHighlight(hitObject, renderer, objectType);
            }

            // Invoke detection callback
            OnObjectDetected?.Invoke(echoHit);

            // Log detection
            if (logDetections)
            {
                Debug.Log($"[EchoIntersection] Detected {objectType}: {hitObject.name} at {distanceFromPlayer:F1}m");
            }
        }

        #endregion

        #region Object Classification

        /// <summary>
        /// Classify detected object by tag
        /// </summary>
        private EchoObjectType ClassifyObject(GameObject obj)
        {
            if (obj.CompareTag(wallTag))
                return EchoObjectType.Wall;
            
            if (obj.CompareTag(enemyTag))
                return EchoObjectType.Enemy;
            
            if (obj.CompareTag(itemTag))
                return EchoObjectType.Item;
            
            if (obj.CompareTag(interactiveTag))
                return EchoObjectType.Interactive;
            
            if (obj.CompareTag(hazardTag))
                return EchoObjectType.Hazard;
            
            return EchoObjectType.Unknown;
        }

        /// <summary>
        /// Get highlight color based on object type
        /// </summary>
        private Color GetHighlightColorForType(EchoObjectType type)
        {
            switch (type)
            {
                case EchoObjectType.Wall:
                    return new Color(0.5f, 0.5f, 0.5f); // Gray
                case EchoObjectType.Enemy:
                    return new Color(1f, 0.2f, 0.2f); // Red
                case EchoObjectType.Item:
                    return new Color(1f, 1f, 0.2f); // Yellow
                case EchoObjectType.Interactive:
                    return new Color(0.2f, 1f, 0.2f); // Green
                case EchoObjectType.Hazard:
                    return new Color(1f, 0.5f, 0f); // Orange
                default:
                    return highlightColor; // Default cyan
            }
        }

        #endregion

        #region Highlighting System

        private class HighlightData
        {
            public Renderer renderer;
            public Material[] originalMaterials;
            public Material[] highlightMaterials;
            public float highlightEndTime;
            public EchoObjectType objectType;
        }

        /// <summary>
        /// Apply highlight effect to detected object
        /// </summary>
        private void ApplyHighlight(GameObject obj, Renderer renderer, EchoObjectType objectType)
        {
            // Skip if already highlighted
            if (highlightedObjects.ContainsKey(obj))
            {
                // Refresh highlight duration
                highlightedObjects[obj].highlightEndTime = Time.time + highlightDuration;
                return;
            }

            // Store original materials
            Material[] originalMats = renderer.materials;
            Material[] highlightMats = new Material[originalMats.Length];

            Color typeColor = GetHighlightColorForType(objectType);

            for (int i = 0; i < originalMats.Length; i++)
            {
                // Create new material instance
                highlightMats[i] = new Material(originalMats[i]);
                
                // Enable emission
                highlightMats[i].EnableKeyword("_EMISSION");
                highlightMats[i].SetColor(EmissionColorID, typeColor * highlightIntensity);
            }

            // Apply highlight materials
            renderer.materials = highlightMats;

            // Track highlighted object
            HighlightData data = new HighlightData
            {
                renderer = renderer,
                originalMaterials = originalMats,
                highlightMaterials = highlightMats,
                highlightEndTime = Time.time + highlightDuration,
                objectType = objectType
            };

            highlightedObjects[obj] = data;
        }

        /// <summary>
        /// Update all active highlights
        /// </summary>
        private void UpdateHighlights()
        {
            List<GameObject> toRemove = new List<GameObject>();

            foreach (var kvp in highlightedObjects)
            {
                if (Time.time >= kvp.Value.highlightEndTime)
                {
                    toRemove.Add(kvp.Key);
                }
            }

            // Remove expired highlights
            foreach (var obj in toRemove)
            {
                RemoveHighlight(obj);
            }
        }

        /// <summary>
        /// Remove highlight from object
        /// </summary>
        private void RemoveHighlight(GameObject obj)
        {
            if (!highlightedObjects.ContainsKey(obj))
                return;

            HighlightData data = highlightedObjects[obj];

            // Restore original materials
            if (data.renderer != null)
            {
                data.renderer.materials = data.originalMaterials;
            }

            // Destroy highlight materials
            foreach (var mat in data.highlightMaterials)
            {
                if (mat != null)
                {
                    Destroy(mat);
                }
            }

            highlightedObjects.Remove(obj);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Get all objects detected in the current pulse
        /// </summary>
        public List<EchoHit> GetCurrentHits()
        {
            return new List<EchoHit>(currentHits);
        }

        /// <summary>
        /// Get all currently highlighted objects
        /// </summary>
        public List<GameObject> GetHighlightedObjects()
        {
            return new List<GameObject>(highlightedObjects.Keys);
        }

        /// <summary>
        /// Manually clear all highlights
        /// </summary>
        public void ClearAllHighlights()
        {
            List<GameObject> allObjects = new List<GameObject>(highlightedObjects.Keys);
            foreach (var obj in allObjects)
            {
                RemoveHighlight(obj);
            }
        }

        /// <summary>
        /// Set detection layers at runtime
        /// </summary>
        public void SetDetectionLayers(LayerMask layers)
        {
            detectionLayers = layers;
        }

        /// <summary>
        /// Enable/disable highlighting
        /// </summary>
        public void SetHighlightingEnabled(bool enabled)
        {
            enableHighlighting = enabled;
            
            if (!enabled)
            {
                ClearAllHighlights();
            }
        }

        #endregion

        #region Debug

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying || !echoController.IsPulsing)
                return;

            // Draw detected hits
            Gizmos.color = Color.red;
            foreach (var hit in currentHits)
            {
                Gizmos.DrawWireSphere(hit.hitPosition, 0.5f);
            }
        }

        #endregion
    }
}
