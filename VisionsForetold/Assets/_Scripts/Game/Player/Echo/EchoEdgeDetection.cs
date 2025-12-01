using UnityEngine;
using System.Collections.Generic;

namespace VisionsForetold.Game.Player.Echo
{
    /// <summary>
    /// Renders edge outlines of objects when echolocation pulse intersects them
    /// Creates true sonar effect by showing object silhouettes/contours
    /// </summary>
    public class EchoEdgeDetection : MonoBehaviour
    {
        [Header("Detection Settings")]
        [Tooltip("Layers to detect and outline")]
        [SerializeField] private LayerMask detectionLayers = ~0;
        
        [Tooltip("How many raycasts to perform around pulse")]
        [SerializeField] private int raycastResolution = 64;
        
        [Tooltip("Detection threshold from pulse edge")]
        [SerializeField] private float detectionThreshold = 2f;
        
        [Tooltip("Enable vertical raycasting")]
        [SerializeField] private bool detect3D = false;
        
        [Tooltip("Number of vertical rays")]
        [SerializeField] private int verticalRayCount = 5;
        
        [Tooltip("Vertical detection range")]
        [SerializeField] private float verticalRange = 10f;

        [Header("Edge Rendering")]
        [Tooltip("Material for rendering edges (use edge detection shader)")]
        [SerializeField] private Material edgeMaterial;
        
        [Tooltip("Edge outline color")]
        [SerializeField] private Color edgeColor = new Color(0.3f, 0.8f, 1f, 1f);
        
        [Tooltip("Edge thickness")]
        [SerializeField, Range(0.001f, 0.1f)] private float edgeThickness = 0.01f;
        
        [Tooltip("How long edges stay visible after pulse hits")]
        [SerializeField] private float edgeVisibilityDuration = 2f;
        
        [Tooltip("Edge fade out duration")]
        [SerializeField] private float edgeFadeOutDuration = 0.5f;

        [Header("Performance")]
        [Tooltip("Maximum objects to track")]
        [SerializeField] private int maxTrackedObjects = 50;
        
        [Tooltip("Update detection every N frames")]
        [SerializeField] private int detectionFrameInterval = 1;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        [SerializeField] private bool showDebugGizmos = true;

        // Runtime data
        private EcholocationController echoController;
        private Transform player;
        private Dictionary<GameObject, EdgeData> detectedObjects = new Dictionary<GameObject, EdgeData>();
        private int frameCounter;
        private List<GameObject> objectsToRemove = new List<GameObject>(10);
        private Dictionary<GameObject, MeshFilter> meshFilterCache = new Dictionary<GameObject, MeshFilter>(50);
        private Dictionary<GameObject, Renderer> rendererCache = new Dictionary<GameObject, Renderer>(50);

        private class EdgeData
        {
            public GameObject gameObject;
            public Renderer renderer;
            public Material[] originalMaterials;
            public GameObject edgeObject;
            public float detectionTime;
            public float endTime;
            public Vector3 hitPosition;
        }

        #region Unity Lifecycle

        private void Awake()
        {
            echoController = GetComponent<EcholocationController>();
            if (echoController == null)
            {
                Debug.LogError("[EchoEdgeDetection] EcholocationController not found!");
                enabled = false;
                return;
            }

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            player = playerObj != null ? playerObj.transform : transform;

            if (edgeMaterial == null)
            {
                Debug.LogWarning("[EchoEdgeDetection] Edge material not assigned! Edges will not render.");
            }
        }

        private void Update()
        {
            frameCounter++;
            
            if (frameCounter % detectionFrameInterval != 0)
                return;

            if (echoController.IsPulsing)
            {
                DetectObjectsAtPulseEdge();
            }

            UpdateEdgeVisibility();
        }

        private void OnDestroy()
        {
            // Clean up all edge objects and materials
            foreach (var kvp in detectedObjects)
            {
                if (kvp.Value.edgeObject != null)
                {
                    Renderer edgeRenderer = kvp.Value.edgeObject.GetComponent<Renderer>();
                    if (edgeRenderer != null && edgeRenderer.material != null)
                    {
                        Destroy(edgeRenderer.material);
                    }
                    Destroy(kvp.Value.edgeObject);
                }
            }
            detectedObjects.Clear();
            objectsToRemove.Clear();
            meshFilterCache.Clear();
            rendererCache.Clear();
        }

        #endregion

        #region Detection

        private void DetectObjectsAtPulseEdge()
        {
            float pulseRadius = echoController.CurrentPulseRadius;
            if (pulseRadius < 0.1f) return;
            
            Vector3 pulseCenter = player.position;
            float angleStep = 360f / raycastResolution;

            for (int i = 0; i < raycastResolution; i++)
            {
                float angle = i * angleStep;
                Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;

                if (detect3D)
                {
                    CastVerticalRays(pulseCenter, direction, pulseRadius);
                }
                else
                {
                    CastHorizontalRay(pulseCenter, direction, pulseRadius);
                }
                
                // Early exit if max tracked
                if (detectedObjects.Count >= maxTrackedObjects) break;
            }
        }

        private void CastHorizontalRay(Vector3 origin, Vector3 direction, float radius)
        {
            if (Physics.Raycast(origin, direction, out RaycastHit hit, radius + detectionThreshold, detectionLayers))
            {
                float distFromEdge = Mathf.Abs(hit.distance - radius);
                
                if (distFromEdge <= detectionThreshold)
                {
                    ProcessHit(hit);
                }
            }
        }

        private void CastVerticalRays(Vector3 origin, Vector3 direction, float radius)
        {
            Vector3 targetPoint = origin + direction * radius;
            float verticalStep = verticalRange / (verticalRayCount - 1);
            float startHeight = -verticalRange * 0.5f;

            for (int v = 0; v < verticalRayCount; v++)
            {
                float heightOffset = startHeight + v * verticalStep;
                Vector3 rayOrigin = origin + Vector3.up * heightOffset;
                Vector3 rayDirection = (targetPoint - rayOrigin).normalized;
                float rayDistance = Vector3.Distance(rayOrigin, targetPoint);

                if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, rayDistance + detectionThreshold, detectionLayers))
                {
                    float distFromEdge = Mathf.Abs(hit.distance - radius);
                    
                    if (distFromEdge <= detectionThreshold)
                    {
                        ProcessHit(hit);
                        if (detectedObjects.Count >= maxTrackedObjects) break;
                    }
                }
            }
        }

        private void ProcessHit(RaycastHit hit)
        {
            GameObject hitObject = hit.collider.gameObject;

            // Skip if already detected
            if (detectedObjects.ContainsKey(hitObject))
            {
                // Refresh timer
                detectedObjects[hitObject].endTime = Time.time + edgeVisibilityDuration;
                return;
            }

            // Skip if max tracked
            if (detectedObjects.Count >= maxTrackedObjects)
                return;

            // Get renderer with caching
            if (!rendererCache.TryGetValue(hitObject, out Renderer renderer))
            {
                renderer = hit.collider.GetComponent<Renderer>();
                if (renderer == null)
                    renderer = hitObject.GetComponentInChildren<Renderer>();
                    
                rendererCache[hitObject] = renderer;
            }

            if (renderer == null)
                return;

            // Create edge data
            EdgeData data = new EdgeData
            {
                gameObject = hitObject,
                renderer = renderer,
                originalMaterials = renderer.materials,
                detectionTime = Time.time,
                endTime = Time.time + edgeVisibilityDuration,
                hitPosition = hit.point
            };

            // Create edge outline object
            if (edgeMaterial != null)
            {
                data.edgeObject = CreateEdgeOutline(hitObject, renderer);
            }

            detectedObjects[hitObject] = data;

            if (showDebugLogs)
            {
                Debug.Log($"[EchoEdgeDetection] Detected edge: {hitObject.name} at {hit.point}");
            }
        }

        #endregion

        #region Edge Rendering

        private GameObject CreateEdgeOutline(GameObject target, Renderer targetRenderer)
        {
            // Create duplicate object for edge rendering
            GameObject edgeObj = new GameObject($"{target.name}_EdgeOutline");
            edgeObj.transform.SetParent(target.transform, false);
            edgeObj.transform.localPosition = Vector3.zero;
            edgeObj.transform.localRotation = Quaternion.identity;
            edgeObj.transform.localScale = Vector3.one;

            // Get mesh filter with caching
            if (!meshFilterCache.TryGetValue(target, out MeshFilter targetMeshFilter))
            {
                targetMeshFilter = target.GetComponent<MeshFilter>();
                if (targetMeshFilter == null)
                    targetMeshFilter = target.GetComponentInChildren<MeshFilter>();
                    
                meshFilterCache[target] = targetMeshFilter;
            }

            if (targetMeshFilter == null || targetMeshFilter.sharedMesh == null)
            {
                Destroy(edgeObj);
                return null;
            }

            MeshFilter edgeMeshFilter = edgeObj.AddComponent<MeshFilter>();
            edgeMeshFilter.sharedMesh = targetMeshFilter.sharedMesh;

            // Add renderer with edge material
            MeshRenderer edgeRenderer = edgeObj.AddComponent<MeshRenderer>();
            Material edgeMat = new Material(edgeMaterial);
            edgeRenderer.material = edgeMat;
            edgeRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            edgeRenderer.receiveShadows = false;

            // Set edge properties
            edgeMat.SetColor("_EdgeColor", edgeColor);
            edgeMat.SetFloat("_EdgeThickness", edgeThickness);
            edgeMat.SetFloat("_Alpha", 1f);
            edgeMat.SetColor("_BackgroundColor", new Color(0, 0, 0, 0));

            return edgeObj;
        }

        private void UpdateEdgeVisibility()
        {
            objectsToRemove.Clear();
            float currentTime = Time.time;

            foreach (var kvp in detectedObjects)
            {
                EdgeData data = kvp.Value;
                
                // Check if expired
                if (currentTime >= data.endTime)
                {
                    objectsToRemove.Add(kvp.Key);
                    continue;
                }

                // Update fade
                if (data.edgeObject != null)
                {
                    float timeRemaining = data.endTime - currentTime;
                    float alpha = timeRemaining < edgeFadeOutDuration ? timeRemaining / edgeFadeOutDuration : 1f;

                    // Update material properties
                    Renderer edgeRenderer = data.edgeObject.GetComponent<Renderer>();
                    if (edgeRenderer != null && edgeRenderer.material != null)
                    {
                        Material mat = edgeRenderer.material;
                        mat.SetFloat("_Alpha", alpha);
                        mat.SetColor("_EdgeColor", edgeColor);
                        mat.SetFloat("_EdgeThickness", edgeThickness);
                    }
                }
            }

            // Remove expired
            for (int i = 0; i < objectsToRemove.Count; i++)
            {
                RemoveEdge(objectsToRemove[i]);
            }
        }

        private void RemoveEdge(GameObject obj)
        {
            if (!detectedObjects.ContainsKey(obj))
                return;

            EdgeData data = detectedObjects[obj];

            if (data.edgeObject != null)
            {
                // Destroy material instance first
                Renderer edgeRenderer = data.edgeObject.GetComponent<Renderer>();
                if (edgeRenderer != null && edgeRenderer.material != null)
                {
                    Destroy(edgeRenderer.material);
                }
                
                Destroy(data.edgeObject);
            }

            detectedObjects.Remove(obj);

            if (showDebugLogs)
            {
                Debug.Log($"[EchoEdgeDetection] Removed edge: {obj.name}");
            }
        }

        #endregion

        #region Public API

        public int GetDetectedObjectCount()
        {
            return detectedObjects.Count;
        }

        public List<GameObject> GetDetectedObjects()
        {
            return new List<GameObject>(detectedObjects.Keys);
        }

        public void ClearAllEdges()
        {
            List<GameObject> all = new List<GameObject>(detectedObjects.Keys);
            foreach (var obj in all)
            {
                RemoveEdge(obj);
            }
        }

        #endregion

        #region Debug

        private void OnDrawGizmos()
        {
            if (!showDebugGizmos || !Application.isPlaying)
                return;

            // Draw detected hit points
            Gizmos.color = Color.cyan;
            foreach (var data in detectedObjects.Values)
            {
                Gizmos.DrawWireSphere(data.hitPosition, 0.2f);
            }
        }

        #endregion
    }
}
