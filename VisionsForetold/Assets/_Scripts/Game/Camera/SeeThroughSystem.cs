using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Makes objects between camera and target transparent/see-through
/// Attach to the camera or player
/// Automatically applies see-through shader to obstructing objects
/// </summary>
public class SeeThroughSystem : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Primary target to keep visible (usually player)")]
    [SerializeField] private Transform target;
    
    [Tooltip("Camera to check from (auto-finds if null)")]
    [SerializeField] private Camera mainCamera;
    
    [Tooltip("Also make enemies visible through walls")]
    [SerializeField] private bool includeEnemies = true;
    
    [Tooltip("Tag for enemy GameObjects")]
    [SerializeField] private string enemyTag = "Enemy";

    [Header("See-Through Settings")]
    [Tooltip("See-through material to apply")]
    [SerializeField] private Material seeThroughMaterial;
    
    [Tooltip("Color when object is see-through")]
    [SerializeField] private Color seeThroughColor = new Color(0.5f, 0.8f, 1.0f, 0.5f);
    
    [Tooltip("Transparency amount (0-1)")]
    [SerializeField, Range(0f, 1f)] private float transparencyAmount = 0.5f;

    [Header("Detection Settings")]
    [Tooltip("Layers to check for obstruction (exclude Ground/Terrain)")]
    [SerializeField] private LayerMask obstructionLayers = ~0;
    
    [Tooltip("Distance from target to check")]
    [SerializeField] private float checkRadius = 0.5f;
    
    [Tooltip("How often to check for obstructions (in seconds)")]
    [SerializeField] private float checkInterval = 0.1f;
    
    [Tooltip("Smooth transition time")]
    [SerializeField] private float transitionTime = 0.2f;

    [Header("Advanced")]
    [Tooltip("Use spherecast instead of raycast (better for large objects)")]
    [SerializeField] private bool useSphereCast = true;
    
    [Tooltip("Sphere radius for spherecast")]
    [SerializeField] private float sphereRadius = 0.1f;
    
    [Tooltip("Ignore objects with these tags (Player/Enemy are handled separately)")]
    [SerializeField] private List<string> ignoreTags = new List<string>();

    [Header("Debug")]
    [SerializeField] private bool showDebugRays = false;

    // Internal state
    private Dictionary<Renderer, MaterialData> affectedRenderers = new Dictionary<Renderer, MaterialData>();
    private float lastCheckTime;
    private HashSet<Renderer> currentlyObstructing = new HashSet<Renderer>();
    private List<Transform> enemyTargets = new List<Transform>();
    private float enemyUpdateInterval = 0.5f;
    private float lastEnemyUpdateTime;

    private class MaterialData
    {
        public Material[] originalMaterials;
        public Material[] seeThroughMaterials;
        public float currentAlpha = 0f;
        public float targetAlpha = 0f;
    }

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeComponents();
        CreateSeeThroughMaterial();
    }

    private void Update()
    {
        // Update enemy list periodically
        if (includeEnemies && Time.time >= lastEnemyUpdateTime + enemyUpdateInterval)
        {
            UpdateEnemyList();
            lastEnemyUpdateTime = Time.time;
        }

        // Check for obstructions periodically
        if (Time.time >= lastCheckTime + checkInterval)
        {
            CheckForObstructions();
            lastCheckTime = Time.time;
        }

        // Update material transparency
        UpdateMaterials();
    }

    private void OnDestroy()
    {
        // Restore all materials
        RestoreAllMaterials();
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        // Auto-find target if not assigned
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                Debug.LogError("[SeeThroughSystem] No target assigned and no Player found!");
                enabled = false;
                return;
            }
        }

        // Auto-find camera
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("[SeeThroughSystem] No camera found!");
                enabled = false;
                return;
            }
        }
    }

    private void CreateSeeThroughMaterial()
    {
        if (seeThroughMaterial == null)
        {
            // Try to load the shader
            Shader seeThroughShader = Shader.Find("Custom/SeeThrough");
            
            if (seeThroughShader != null)
            {
                seeThroughMaterial = new Material(seeThroughShader);
                seeThroughMaterial.SetColor("_SeeThroughColor", seeThroughColor);
                seeThroughMaterial.SetFloat("_SeeThroughAmount", transparencyAmount);
            }
            else
            {
                Debug.LogWarning("[SeeThroughSystem] Custom/SeeThrough shader not found! Using fallback.");
                
                // Fallback to standard transparent shader
                seeThroughMaterial = new Material(Shader.Find("Standard"));
                seeThroughMaterial.SetFloat("_Mode", 3); // Transparent mode
                seeThroughMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                seeThroughMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                seeThroughMaterial.SetInt("_ZWrite", 0);
                seeThroughMaterial.DisableKeyword("_ALPHATEST_ON");
                seeThroughMaterial.EnableKeyword("_ALPHABLEND_ON");
                seeThroughMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                seeThroughMaterial.renderQueue = 3000;
                seeThroughMaterial.SetColor("_Color", seeThroughColor);
            }
        }
    }

    #endregion

    #region Obstruction Detection

    private void UpdateEnemyList()
    {
        enemyTargets.Clear();
        
        if (string.IsNullOrEmpty(enemyTag))
            return;

        // Find all enemies by tag
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null && enemy.activeInHierarchy)
            {
                enemyTargets.Add(enemy.transform);
            }
        }
    }

    private void CheckForObstructions()
    {
        if (mainCamera == null)
            return;

        // Clear current obstruction set
        currentlyObstructing.Clear();

        // Check player
        if (target != null)
        {
            CheckObstructionsForTarget(target);
        }

        // Check enemies
        if (includeEnemies)
        {
            foreach (Transform enemy in enemyTargets)
            {
                if (enemy != null && enemy.gameObject.activeInHierarchy)
                {
                    CheckObstructionsForTarget(enemy);
                }
            }
        }

        // Remove see-through effect from objects no longer obstructing
        RemoveSeeThroughFromNonObstructing();
    }

    private void CheckObstructionsForTarget(Transform targetTransform)
    {
        // Direction from camera to target
        Vector3 directionToTarget = targetTransform.position - mainCamera.transform.position;
        float distanceToTarget = directionToTarget.magnitude;
        directionToTarget.Normalize();

        // Perform raycast or spherecast
        RaycastHit[] hits;
        
        if (useSphereCast)
        {
            hits = Physics.SphereCastAll(
                mainCamera.transform.position,
                sphereRadius,
                directionToTarget,
                distanceToTarget,
                obstructionLayers
            );
        }
        else
        {
            hits = Physics.RaycastAll(
                mainCamera.transform.position,
                directionToTarget,
                distanceToTarget,
                obstructionLayers
            );
        }

        // Debug visualization
        if (showDebugRays)
        {
            Debug.DrawRay(mainCamera.transform.position, directionToTarget * distanceToTarget, Color.yellow);
        }

        // Process hits
        foreach (RaycastHit hit in hits)
        {
            // Skip if it's the target itself
            if (hit.transform == targetTransform || hit.transform.IsChildOf(targetTransform))
                continue;

            // Skip ignored tags
            if (ShouldIgnore(hit.transform))
                continue;

            // Get all renderers on the hit object
            Renderer[] renderers = hit.transform.GetComponentsInChildren<Renderer>();
            
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null && renderer.enabled)
                {
                    ApplySeeThroughEffect(renderer);
                    currentlyObstructing.Add(renderer);
                }
            }
        }
    }

    private bool ShouldIgnore(Transform obj)
    {
        // Always ignore player
        if (obj.CompareTag("Player"))
            return true;

        // Ignore enemies only if not including them
        if (!includeEnemies && !string.IsNullOrEmpty(enemyTag) && obj.CompareTag(enemyTag))
            return true;

        // Check custom ignore tags
        foreach (string tag in ignoreTags)
        {
            if (obj.CompareTag(tag))
                return true;
        }
        
        return false;
    }

    #endregion

    #region Material Management

    private void ApplySeeThroughEffect(Renderer renderer)
    {
        if (!affectedRenderers.ContainsKey(renderer))
        {
            // Store original materials
            MaterialData data = new MaterialData();
            data.originalMaterials = renderer.materials;
            data.seeThroughMaterials = new Material[data.originalMaterials.Length];
            
            // Create see-through material instances
            for (int i = 0; i < data.originalMaterials.Length; i++)
            {
                data.seeThroughMaterials[i] = new Material(seeThroughMaterial);
                
                // Copy main texture from original
                if (data.originalMaterials[i].HasProperty("_MainTex"))
                {
                    data.seeThroughMaterials[i].SetTexture("_MainTex", 
                        data.originalMaterials[i].GetTexture("_MainTex"));
                }
            }
            
            affectedRenderers[renderer] = data;
        }

        // Set target alpha to transparent
        affectedRenderers[renderer].targetAlpha = 1f;
    }

    private void RemoveSeeThroughFromNonObstructing()
    {
        List<Renderer> toRemove = new List<Renderer>();

        foreach (var kvp in affectedRenderers)
        {
            if (!currentlyObstructing.Contains(kvp.Key))
            {
                // Set target alpha to opaque
                kvp.Value.targetAlpha = 0f;
                
                // If fully opaque, mark for removal
                if (kvp.Value.currentAlpha <= 0.01f)
                {
                    toRemove.Add(kvp.Key);
                }
            }
        }

        // Restore materials and remove from tracking
        foreach (Renderer renderer in toRemove)
        {
            if (renderer != null)
            {
                renderer.materials = affectedRenderers[renderer].originalMaterials;
            }
            affectedRenderers.Remove(renderer);
        }
    }

    private void UpdateMaterials()
    {
        foreach (var kvp in affectedRenderers)
        {
            Renderer renderer = kvp.Key;
            MaterialData data = kvp.Value;

            if (renderer == null)
                continue;

            // Smoothly interpolate alpha
            data.currentAlpha = Mathf.Lerp(
                data.currentAlpha,
                data.targetAlpha,
                Time.deltaTime / transitionTime
            );

            // Apply materials based on current alpha
            if (data.currentAlpha > 0.01f)
            {
                // Apply see-through materials
                renderer.materials = data.seeThroughMaterials;
                
                // Update alpha
                foreach (Material mat in data.seeThroughMaterials)
                {
                    if (mat.HasProperty("_SeeThroughAmount"))
                    {
                        mat.SetFloat("_SeeThroughAmount", data.currentAlpha * transparencyAmount);
                    }
                    else if (mat.HasProperty("_Color"))
                    {
                        Color col = seeThroughColor;
                        col.a = data.currentAlpha * transparencyAmount;
                        mat.SetColor("_Color", col);
                    }
                }
            }
            else
            {
                // Restore original materials
                renderer.materials = data.originalMaterials;
            }
        }
    }

    private void RestoreAllMaterials()
    {
        foreach (var kvp in affectedRenderers)
        {
            if (kvp.Key != null)
            {
                kvp.Key.materials = kvp.Value.originalMaterials;
            }
        }
        affectedRenderers.Clear();
    }

    #endregion

    #region Public API

    /// <summary>
    /// Set the target to keep visible
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// Set the see-through color
    /// </summary>
    public void SetSeeThroughColor(Color color)
    {
        seeThroughColor = color;
        if (seeThroughMaterial != null)
        {
            seeThroughMaterial.SetColor("_SeeThroughColor", color);
        }
    }

    /// <summary>
    /// Set transparency amount
    /// </summary>
    public void SetTransparency(float amount)
    {
        transparencyAmount = Mathf.Clamp01(amount);
        if (seeThroughMaterial != null)
        {
            seeThroughMaterial.SetFloat("_SeeThroughAmount", transparencyAmount);
        }
    }

    /// <summary>
    /// Enable or disable the system
    /// </summary>
    public void SetEnabled(bool enable)
    {
        enabled = enable;
        
        if (!enable)
        {
            RestoreAllMaterials();
        }
    }

    #endregion

    #region Debug

    private void OnDrawGizmosSelected()
    {
        if (target == null || mainCamera == null)
            return;

        // Draw line from camera to target
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(mainCamera.transform.position, target.position);

        // Draw sphere at target
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(target.position, checkRadius);

        // Draw affected renderers
        Gizmos.color = Color.yellow;
        foreach (var kvp in affectedRenderers)
        {
            if (kvp.Key != null && kvp.Value.currentAlpha > 0.01f)
            {
                Gizmos.DrawWireCube(kvp.Key.bounds.center, kvp.Key.bounds.size);
            }
        }
    }

    #endregion
}
