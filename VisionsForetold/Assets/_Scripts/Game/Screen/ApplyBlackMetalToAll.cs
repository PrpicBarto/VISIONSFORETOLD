using UnityEngine;

/// <summary>
/// Automatically applies the Black Metal outline shader to all renderers in the scene
/// Attach to any GameObject in your scene (or create an empty GameObject for it)
/// </summary>
public class ApplyBlackMetalToAll : MonoBehaviour
{
    [Header("Shader Settings")]
    [Tooltip("The Black Metal outline shader material to apply")]
    [SerializeField] private Material blackMetalMaterial;

    [Header("Auto-Create Material")]
    [Tooltip("Auto-create material if not assigned")]
    [SerializeField] private bool autoCreateMaterial = true;
    
    [Tooltip("Main color for objects")]
    [SerializeField] private Color mainColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    
    [Tooltip("Outline color")]
    [SerializeField] private Color outlineColor = Color.black;
    
    [Tooltip("Outline width")]
    [SerializeField] [Range(0.001f, 0.02f)] private float outlineWidth = 0.005f;
    
    [Tooltip("Brightness")]
    [SerializeField] [Range(0f, 2f)] private float brightness = 0.8f;

    [Header("Application Settings")]
    [Tooltip("Apply on Start")]
    [SerializeField] private bool applyOnStart = true;
    
    [Tooltip("Include inactive objects")]
    [SerializeField] private bool includeInactive = false;
    
    [Tooltip("Override existing materials")]
    [SerializeField] private bool overrideExisting = true;

    [Header("Filter Settings")]
    [Tooltip("Only apply to objects with these tags (leave empty for all)")]
    [SerializeField] private string[] includeTags = new string[0];
    
    [Tooltip("Don't apply to objects with these tags")]
    [SerializeField] private string[] excludeTags = new string[] { "UI", "MainCamera" };
    
    [Tooltip("Exclude objects by name (contains check)")]
    [SerializeField] private string[] excludeNames = new string[] { "Camera", "Light" };

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private int objectsProcessed = 0;
    private int objectsSkipped = 0;

    void Start()
    {
        if (applyOnStart)
        {
            ApplyToAllObjects();
        }
    }

    /// <summary>
    /// Manually trigger application of shader to all objects
    /// Can be called from Inspector or code
    /// </summary>
    [ContextMenu("Apply Black Metal Shader to All Objects")]
    public void ApplyToAllObjects()
    {
        objectsProcessed = 0;
        objectsSkipped = 0;

        // Create or verify material
        if (blackMetalMaterial == null && autoCreateMaterial)
        {
            CreateBlackMetalMaterial();
        }

        if (blackMetalMaterial == null)
        {
            Debug.LogError("[ApplyBlackMetalToAll] No material assigned and auto-create failed! Assign a material with BlackMetalOutline shader.");
            return;
        }

        // Find ALL renderer types in scene
        
        // 1. MeshRenderers (standard objects)
        MeshRenderer[] meshRenderers = includeInactive 
            ? FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None)
            : FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        if (showDebugLogs)
        {
            Debug.Log($"[ApplyBlackMetalToAll] Found {meshRenderers.Length} MeshRenderers");
        }

        foreach (MeshRenderer renderer in meshRenderers)
        {
            if (ShouldApplyToObject(renderer.gameObject))
            {
                ApplyMaterialToRenderer(renderer);
                objectsProcessed++;
            }
            else
            {
                objectsSkipped++;
            }
        }

        // 2. SkinnedMeshRenderers (characters, animated models)
        SkinnedMeshRenderer[] skinnedRenderers = includeInactive
            ? FindObjectsByType<SkinnedMeshRenderer>(FindObjectsSortMode.None)
            : FindObjectsByType<SkinnedMeshRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        if (showDebugLogs)
        {
            Debug.Log($"[ApplyBlackMetalToAll] Found {skinnedRenderers.Length} SkinnedMeshRenderers");
        }

        foreach (SkinnedMeshRenderer renderer in skinnedRenderers)
        {
            if (ShouldApplyToObject(renderer.gameObject))
            {
                ApplyMaterialToRenderer(renderer);
                objectsProcessed++;
            }
            else
            {
                objectsSkipped++;
            }
        }
        
        // 3. LOD Groups (Level of Detail objects, often used for trees)
        LODGroup[] lodGroups = includeInactive
            ? FindObjectsByType<LODGroup>(FindObjectsSortMode.None)
            : FindObjectsByType<LODGroup>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        
        if (showDebugLogs)
        {
            Debug.Log($"[ApplyBlackMetalToAll] Found {lodGroups.Length} LOD Groups");
        }
        
        foreach (LODGroup lodGroup in lodGroups)
        {
            if (ShouldApplyToObject(lodGroup.gameObject))
            {
                // Get all LOD levels
                LOD[] lods = lodGroup.GetLODs();
                foreach (LOD lod in lods)
                {
                    foreach (Renderer renderer in lod.renderers)
                    {
                        if (renderer != null)
                        {
                            ApplyMaterialToRenderer(renderer);
                            objectsProcessed++;
                        }
                    }
                }
            }
        }
        
        // 4. Terrain trees (if any)
        Terrain[] terrains = includeInactive
            ? FindObjectsByType<Terrain>(FindObjectsSortMode.None)
            : FindObjectsByType<Terrain>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        
        if (showDebugLogs && terrains.Length > 0)
        {
            Debug.Log($"[ApplyBlackMetalToAll] Found {terrains.Length} Terrains (tree prototypes will be updated)");
        }
        
        foreach (Terrain terrain in terrains)
        {
            if (ShouldApplyToObject(terrain.gameObject))
            {
                ApplyToTerrainTrees(terrain);
            }
        }

        // Summary
        Debug.Log($"<color=green>[ApplyBlackMetalToAll] Complete! Applied to {objectsProcessed} objects, skipped {objectsSkipped} objects</color>");
    }

    /// <summary>
    /// Create a Black Metal material with default settings
    /// </summary>
    private void CreateBlackMetalMaterial()
    {
        Shader shader = Shader.Find("Custom/BlackMetalOutline");
        
        if (shader == null)
        {
            Debug.LogError("[ApplyBlackMetalToAll] BlackMetalOutline shader not found! Make sure it exists at Assets/Shaders/BlackMetalOutline.shader");
            return;
        }

        blackMetalMaterial = new Material(shader);
        blackMetalMaterial.name = "BlackMetal_Auto";
        
        // Set properties
        blackMetalMaterial.SetColor("_Color", mainColor);
        blackMetalMaterial.SetColor("_OutlineColor", outlineColor);
        blackMetalMaterial.SetFloat("_OutlineWidth", outlineWidth);
        blackMetalMaterial.SetFloat("_Brightness", brightness);

        if (showDebugLogs)
        {
            Debug.Log("[ApplyBlackMetalToAll] Auto-created Black Metal material");
        }
    }

    /// <summary>
    /// Check if shader should be applied to this object
    /// </summary>
    private bool ShouldApplyToObject(GameObject obj)
    {
        // Check exclude tags
        foreach (string tag in excludeTags)
        {
            if (obj.CompareTag(tag))
            {
                if (showDebugLogs)
                    Debug.Log($"[ApplyBlackMetalToAll] Skipping {obj.name} - excluded tag: {tag}");
                return false;
            }
        }

        // Check exclude names
        foreach (string name in excludeNames)
        {
            if (obj.name.Contains(name))
            {
                if (showDebugLogs)
                    Debug.Log($"[ApplyBlackMetalToAll] Skipping {obj.name} - excluded name contains: {name}");
                return false;
            }
        }

        // If include tags specified, object must have one of them
        if (includeTags.Length > 0)
        {
            bool hasIncludeTag = false;
            foreach (string tag in includeTags)
            {
                if (obj.CompareTag(tag))
                {
                    hasIncludeTag = true;
                    break;
                }
            }

            if (!hasIncludeTag)
            {
                if (showDebugLogs)
                    Debug.Log($"[ApplyBlackMetalToAll] Skipping {obj.name} - doesn't have required tag");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Apply material to a renderer
    /// </summary>
    private void ApplyMaterialToRenderer(Renderer renderer)
    {
        if (!overrideExisting && renderer.sharedMaterials.Length > 0 && renderer.sharedMaterial != null)
        {
            // Don't override if material already exists
            if (showDebugLogs)
                Debug.Log($"[ApplyBlackMetalToAll] Skipping {renderer.gameObject.name} - already has material (override disabled)");
            return;
        }

        // Apply material to all material slots
        Material[] newMaterials = new Material[renderer.sharedMaterials.Length];
        for (int i = 0; i < newMaterials.Length; i++)
        {
            newMaterials[i] = blackMetalMaterial;
        }
        
        renderer.sharedMaterials = newMaterials;

        if (showDebugLogs)
            Debug.Log($"[ApplyBlackMetalToAll] Applied to: {renderer.gameObject.name}");
    }
    
    /// <summary>
    /// Apply shader to terrain tree prototypes
    /// </summary>
    private void ApplyToTerrainTrees(Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;
        if (terrainData == null) return;
        
        TreePrototype[] treePrototypes = terrainData.treePrototypes;
        bool modified = false;
        
        for (int i = 0; i < treePrototypes.Length; i++)
        {
            if (treePrototypes[i].prefab != null)
            {
                // Get all renderers in tree prefab
                Renderer[] renderers = treePrototypes[i].prefab.GetComponentsInChildren<Renderer>(true);
                
                foreach (Renderer renderer in renderers)
                {
                    ApplyMaterialToRenderer(renderer);
                    modified = true;
                    objectsProcessed++;
                }
            }
        }
        
        if (modified && showDebugLogs)
        {
            Debug.Log($"[ApplyBlackMetalToAll] Updated tree prototypes on terrain: {terrain.name}");
        }
    }

    /// <summary>
    /// Remove Black Metal shader from all objects (restore defaults)
    /// </summary>
    [ContextMenu("Remove Black Metal Shader from All Objects")]
    public void RemoveFromAllObjects()
    {
        int removed = 0;

        MeshRenderer[] renderers = FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        foreach (MeshRenderer renderer in renderers)
        {
            if (renderer.sharedMaterial == blackMetalMaterial)
            {
                renderer.sharedMaterial = null;
                removed++;
            }
        }

        SkinnedMeshRenderer[] skinnedRenderers = FindObjectsByType<SkinnedMeshRenderer>(FindObjectsSortMode.None);
        foreach (SkinnedMeshRenderer renderer in skinnedRenderers)
        {
            if (renderer.sharedMaterial == blackMetalMaterial)
            {
                renderer.sharedMaterial = null;
                removed++;
            }
        }

        Debug.Log($"<color=yellow>[ApplyBlackMetalToAll] Removed Black Metal shader from {removed} objects</color>");
    }
}
