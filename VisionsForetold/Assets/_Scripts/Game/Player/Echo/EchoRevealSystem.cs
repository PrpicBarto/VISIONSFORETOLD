using UnityEngine;
using System.Collections.Generic;

namespace VisionsForetold.Game.Player.Echo
{
    /// <summary>
    /// Makes objects visible when echolocation pulse intersects them
    /// Objects are revealed by removing fog overlay in their area
    /// </summary>
    public class EchoRevealSystem : MonoBehaviour
    {
        [Header("Reveal Settings")]
        [Tooltip("Layers to detect and reveal")]
        [SerializeField] private LayerMask detectionLayers = ~0;
        
        [Tooltip("How many raycasts per frame (more = better coverage)")]
        [SerializeField] private int raycastsPerFrame = 64;
        
        [Tooltip("How close to pulse edge to trigger reveal")]
        [SerializeField] private float detectionThreshold = 3f;
        
        [Tooltip("How long objects stay revealed after pulse hits")]
        [SerializeField] private float revealDuration = 5f;
        
        [Tooltip("Enable vertical raycasting for 3D objects")]
        [SerializeField] private bool detect3D = false;
        
        [Tooltip("Number of vertical rays")]
        [SerializeField] private int verticalRayCount = 5;
        
        [Tooltip("Height range for vertical detection")]
        [SerializeField] private float verticalRange = 10f;

        [Header("Shader Communication")]
        [Tooltip("Maximum revealed object positions to send to shader")]
        [SerializeField] private int maxRevealedObjects = 50;
        
        [Tooltip("Radius around revealed object center to show")]
        [SerializeField] private float revealRadius = 5f;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        [SerializeField] private bool showDebugGizmos = true;

        // Runtime data
        private EcholocationController echoController;
        private Transform player;
        private Dictionary<GameObject, RevealData> revealedObjects = new Dictionary<GameObject, RevealData>();
        
        // Shader data arrays
        private Vector4[] revealPositions;
        private float[] revealRadii;
        private float[] revealStrengths;
        
        // Shader property IDs
        private static readonly int RevealCountID = Shader.PropertyToID("_RevealCount");
        private static readonly int RevealPositionsID = Shader.PropertyToID("_RevealPositions");
        private static readonly int RevealRadiiID = Shader.PropertyToID("_RevealRadii");
        private static readonly int RevealStrengthsID = Shader.PropertyToID("_RevealStrengths");

        private class RevealData
        {
            public GameObject gameObject;
            public Vector3 center;
            public float radius;
            public float revealTime;
            public float endTime;
            public Bounds bounds;
        }

        #region Unity Lifecycle

        private void Awake()
        {
            echoController = GetComponent<EcholocationController>();
            if (echoController == null)
            {
                Debug.LogError("[EchoReveal] EcholocationController not found!");
                enabled = false;
                return;
            }

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            player = playerObj != null ? playerObj.transform : transform;

            // Initialize shader data arrays
            revealPositions = new Vector4[maxRevealedObjects];
            revealRadii = new float[maxRevealedObjects];
            revealStrengths = new float[maxRevealedObjects];
        }

        private void Update()
        {
            if (echoController.IsPulsing)
            {
                DetectObjectsAtPulseEdge();
            }

            UpdateRevealedObjects();
            SendDataToShader();
        }

        #endregion

        #region Detection

        private void DetectObjectsAtPulseEdge()
        {
            float pulseRadius = echoController.CurrentPulseRadius;
            Vector3 pulseCenter = player.position;

            for (int i = 0; i < raycastsPerFrame; i++)
            {
                float angle = (i / (float)raycastsPerFrame) * 360f;
                Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;

                if (detect3D)
                {
                    CastVerticalRays(pulseCenter, direction, pulseRadius);
                }
                else
                {
                    CastHorizontalRay(pulseCenter, direction, pulseRadius);
                }
            }
        }

        private void CastHorizontalRay(Vector3 origin, Vector3 direction, float radius)
        {
            if (Physics.Raycast(origin, direction, out RaycastHit hit, radius + detectionThreshold, detectionLayers))
            {
                float distFromEdge = Mathf.Abs(hit.distance - radius);
                
                if (distFromEdge <= detectionThreshold)
                {
                    RevealObject(hit.collider.gameObject, hit.point);
                }
            }
        }

        private void CastVerticalRays(Vector3 origin, Vector3 direction, float radius)
        {
            Vector3 targetPoint = origin + direction * radius;

            for (int v = 0; v < verticalRayCount; v++)
            {
                float heightOffset = (v / (float)(verticalRayCount - 1)) * verticalRange - (verticalRange * 0.5f);
                Vector3 rayOrigin = origin + Vector3.up * heightOffset;
                Vector3 rayDirection = (targetPoint - rayOrigin).normalized;
                float rayDistance = Vector3.Distance(rayOrigin, targetPoint);

                if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, rayDistance + detectionThreshold, detectionLayers))
                {
                    float distFromEdge = Mathf.Abs(hit.distance - radius);
                    
                    if (distFromEdge <= detectionThreshold)
                    {
                        RevealObject(hit.collider.gameObject, hit.point);
                    }
                }
            }
        }

        #endregion

        #region Object Revealing

        private void RevealObject(GameObject obj, Vector3 hitPoint)
        {
            // Skip if already revealed (refresh timer)
            if (revealedObjects.ContainsKey(obj))
            {
                revealedObjects[obj].endTime = Time.time + revealDuration;
                return;
            }

            // Get object bounds
            Renderer renderer = obj.GetComponent<Renderer>();
            Bounds bounds;
            
            if (renderer != null)
            {
                bounds = renderer.bounds;
            }
            else
            {
                // Use collider bounds as fallback
                Collider col = obj.GetComponent<Collider>();
                bounds = col != null ? col.bounds : new Bounds(obj.transform.position, Vector3.one);
            }

            // Calculate reveal radius (use object size or fixed radius)
            float objectRadius = Mathf.Max(bounds.extents.magnitude, revealRadius);

            // Create reveal data
            RevealData data = new RevealData
            {
                gameObject = obj,
                center = bounds.center,
                radius = objectRadius,
                revealTime = Time.time,
                endTime = Time.time + revealDuration,
                bounds = bounds
            };

            revealedObjects[obj] = data;

            if (showDebugLogs)
            {
                Debug.Log($"[EchoReveal] Revealed: {obj.name} at {bounds.center} (radius: {objectRadius:F1})");
            }
        }

        private void UpdateRevealedObjects()
        {
            List<GameObject> toRemove = new List<GameObject>();
            float currentTime = Time.time;

            foreach (var kvp in revealedObjects)
            {
                if (currentTime >= kvp.Value.endTime)
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var obj in toRemove)
            {
                if (showDebugLogs)
                {
                    Debug.Log($"[EchoReveal] Hiding: {obj.name}");
                }
                revealedObjects.Remove(obj);
            }
        }

        #endregion

        #region Shader Communication

        private void SendDataToShader()
        {
            if (echoController == null) return;

            int count = 0;

            // Fill arrays with revealed object data
            foreach (var data in revealedObjects.Values)
            {
                if (count >= maxRevealedObjects) break;

                // Calculate fade based on time
                float timeAlive = Time.time - data.revealTime;
                float timeToDeath = data.endTime - Time.time;
                float fadeIn = Mathf.Clamp01(timeAlive / 0.5f); // Fade in over 0.5s
                float fadeOut = Mathf.Clamp01(timeToDeath / 1f); // Fade out over 1s
                float strength = Mathf.Min(fadeIn, fadeOut);

                revealPositions[count] = new Vector4(data.center.x, data.center.y, data.center.z, 1f);
                revealRadii[count] = data.radius;
                revealStrengths[count] = strength;

                count++;
            }

            // Clear remaining slots
            for (int i = count; i < maxRevealedObjects; i++)
            {
                revealPositions[i] = Vector4.zero;
                revealRadii[i] = 0f;
                revealStrengths[i] = 0f;
            }

            // Send to shader material
            Material fogMaterial = echoController.GetFogMaterial();
            if (fogMaterial != null)
            {
                fogMaterial.SetInt(RevealCountID, count);
                fogMaterial.SetVectorArray(RevealPositionsID, revealPositions);
                fogMaterial.SetFloatArray(RevealRadiiID, revealRadii);
                fogMaterial.SetFloatArray(RevealStrengthsID, revealStrengths);
            }
        }

        #endregion

        #region Public API

        public int GetRevealedObjectCount()
        {
            return revealedObjects.Count;
        }

        public List<GameObject> GetRevealedObjects()
        {
            return new List<GameObject>(revealedObjects.Keys);
        }

        public void ClearAllReveals()
        {
            revealedObjects.Clear();
        }

        #endregion

        #region Debug

        private void OnDrawGizmos()
        {
            if (!showDebugGizmos || !Application.isPlaying) return;

            // Draw revealed object spheres
            Gizmos.color = Color.green;
            foreach (var data in revealedObjects.Values)
            {
                Gizmos.DrawWireSphere(data.center, data.radius);
                
                // Draw bounds
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(data.bounds.center, data.bounds.size);
            }
        }

        #endregion
    }
}
