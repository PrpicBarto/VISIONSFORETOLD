using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Makes player and enemies visible through walls using X-Ray shader
/// Shows them as glowing silhouettes when occluded
/// </summary>
public class CharacterXRaySystem : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Player to make visible through walls")]
    [SerializeField] private Transform player;
    
    [Tooltip("Camera to check from")]
    [SerializeField] private Camera mainCamera;
    
    [Tooltip("Include enemies in x-ray vision")]
    [SerializeField] private bool includeEnemies = true;
    
    [Tooltip("Tag for enemies")]
    [SerializeField] private string enemyTag = "Enemy";

    [Header("X-Ray Settings")]
    [Tooltip("X-Ray material (uses CharacterXRay shader)")]
    [SerializeField] private Material xrayMaterial;
    
    [Tooltip("Color for x-ray silhouette")]
    [SerializeField] private Color xrayColor = new Color(0.2f, 0.8f, 1.0f, 0.8f);
    
    [Tooltip("X-Ray visibility strength")]
    [SerializeField, Range(0f, 1f)] private float xrayStrength = 0.8f;
    
    [Tooltip("Rim light power")]
    [SerializeField, Range(0.1f, 8f)] private float rimPower = 3f;

    [Header("Detection")]
    [Tooltip("Layers that can block line of sight")]
    [SerializeField] private LayerMask obstructionLayers = ~0;
    
    [Tooltip("How often to check (seconds)")]
    [SerializeField] private float checkInterval = 0.1f;
    
    [Tooltip("Use sphere cast for better detection")]
    [SerializeField] private bool useSphereCast = true;
    
    [Tooltip("Sphere cast radius")]
    [SerializeField] private float sphereRadius = 0.3f;

    [Header("Debug")]
    [SerializeField] private bool showDebugRays = false;

    // Runtime data
    private Dictionary<Renderer, XRayData> trackedCharacters = new Dictionary<Renderer, XRayData>();
    private List<Transform> enemyTargets = new List<Transform>();
    private float lastCheckTime;
    private float lastEnemyUpdateTime;
    private float enemyUpdateInterval = 0.5f;
    
    private class XRayData
    {
        public Material[] originalMaterials;
        public Material[] xrayMaterials;
        public bool isOccluded;
    }

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeComponents();
        CreateXRayMaterial();
    }

    private void Start()
    {
        SetupPlayer();
    }

    private void Update()
    {
        // Update enemy list
        if (includeEnemies && Time.time >= lastEnemyUpdateTime + enemyUpdateInterval)
        {
            UpdateEnemyList();
            lastEnemyUpdateTime = Time.time;
        }

        // Check occlusion
        if (Time.time >= lastCheckTime + checkInterval)
        {
            CheckOcclusion();
            lastCheckTime = Time.time;
        }
    }

    private void OnDestroy()
    {
        RestoreAllMaterials();
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("[CharacterXRay] No player found!");
                enabled = false;
                return;
            }
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("[CharacterXRay] No camera found!");
                enabled = false;
                return;
            }
        }
    }

    private void CreateXRayMaterial()
    {
        if (xrayMaterial == null)
        {
            Shader xrayShader = Shader.Find("Custom/CharacterXRay");
            
            if (xrayShader != null)
            {
                xrayMaterial = new Material(xrayShader);
                Debug.Log("[CharacterXRay] Created X-Ray material with custom shader");
            }
            else
            {
                Debug.LogError("[CharacterXRay] CharacterXRay shader not found! Character won't be visible through walls.");
                enabled = false;
                return;
            }
        }
        
        // Set properties
        if (xrayMaterial != null)
        {
            xrayMaterial.SetColor("_XRayColor", xrayColor);
            xrayMaterial.SetFloat("_XRayStrength", xrayStrength);
            xrayMaterial.SetFloat("_RimPower", rimPower);
        }
    }

    private void SetupPlayer()
    {
        if (player == null) return;

        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null && renderer.enabled)
            {
                RegisterCharacter(renderer);
            }
        }
        
        Debug.Log($"[CharacterXRay] Registered {renderers.Length} renderers for player");
    }

    #endregion

    #region Character Management

    private void RegisterCharacter(Renderer renderer)
    {
        if (renderer == null)
            return;
            
        if (trackedCharacters.ContainsKey(renderer))
            return;

        // Check if xrayMaterial is assigned
        if (xrayMaterial == null)
        {
            Debug.LogError("[CharacterXRay] X-Ray material is not assigned! Please assign it in the Inspector or ensure the shader exists.");
            return;
        }

        XRayData data = new XRayData();
        data.originalMaterials = renderer.sharedMaterials;
        
        if (data.originalMaterials == null || data.originalMaterials.Length == 0)
        {
            Debug.LogWarning($"[CharacterXRay] Renderer on {renderer.name} has no materials. Skipping.");
            return;
        }
        
        data.xrayMaterials = new Material[data.originalMaterials.Length];
        
        // Create x-ray materials for each material slot
        for (int i = 0; i < data.originalMaterials.Length; i++)
        {
            // Skip null materials
            if (data.originalMaterials[i] == null)
            {
                Debug.LogWarning($"[CharacterXRay] Material at index {i} is null on {renderer.name}. Skipping this slot.");
                data.xrayMaterials[i] = null;
                continue;
            }

            try
            {
                data.xrayMaterials[i] = new Material(xrayMaterial);
                
                // Copy main texture if it exists
                if (data.originalMaterials[i].HasProperty("_MainTex"))
                {
                    Texture mainTex = data.originalMaterials[i].GetTexture("_MainTex");
                    if (mainTex != null && data.xrayMaterials[i] != null)
                    {
                        data.xrayMaterials[i].SetTexture("_MainTex", mainTex);
                    }
                }
                
                // Copy main color if it exists
                if (data.originalMaterials[i].HasProperty("_Color"))
                {
                    Color mainColor = data.originalMaterials[i].GetColor("_Color");
                    if (data.xrayMaterials[i] != null)
                    {
                        data.xrayMaterials[i].SetColor("_Color", mainColor);
                    }
                }
                // Try BaseColor for URP materials
                else if (data.originalMaterials[i].HasProperty("_BaseColor"))
                {
                    Color baseColor = data.originalMaterials[i].GetColor("_BaseColor");
                    if (data.xrayMaterials[i] != null)
                    {
                        data.xrayMaterials[i].SetColor("_Color", baseColor);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[CharacterXRay] Failed to create x-ray material for {renderer.name} at index {i}: {e.Message}");
                data.xrayMaterials[i] = null;
            }
        }
        
        data.isOccluded = false;
        trackedCharacters[renderer] = data;
    }

    private void UpdateEnemyList()
    {
        if (!includeEnemies) return;
        
        enemyTargets.Clear();
        
        try
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
            
            foreach (GameObject enemy in enemies)
            {
                if (enemy != null && enemy.activeInHierarchy)
                {
                    enemyTargets.Add(enemy.transform);
                    
                    // Register enemy renderers
                    Renderer[] renderers = enemy.GetComponentsInChildren<Renderer>();
                    if (renderers != null && renderers.Length > 0)
                    {
                        foreach (Renderer renderer in renderers)
                        {
                            if (renderer != null && renderer.enabled && !trackedCharacters.ContainsKey(renderer))
                            {
                                RegisterCharacter(renderer);
                            }
                        }
                    }
                }
            }
        }
        catch (UnityException)
        {
            // Tag doesn't exist - this is normal if no enemies are tagged yet
            Debug.LogWarning($"[CharacterXRay] Tag '{enemyTag}' not found. Create the tag or assign it to enemy objects.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[CharacterXRay] Error updating enemy list: {e.Message}");
        }
    }

    #endregion

    #region Occlusion Detection

    private void CheckOcclusion()
    {
        if (mainCamera == null) return;

        // Check player
        if (player != null)
        {
            bool playerOccluded = IsOccluded(player);
            UpdateCharacterMaterials(player, playerOccluded);
        }

        // Check enemies
        if (includeEnemies)
        {
            foreach (Transform enemy in enemyTargets)
            {
                if (enemy != null && enemy.gameObject.activeInHierarchy)
                {
                    bool enemyOccluded = IsOccluded(enemy);
                    UpdateCharacterMaterials(enemy, enemyOccluded);
                }
            }
        }
    }

    private bool IsOccluded(Transform character)
    {
        if (character == null || mainCamera == null)
            return false;

        Vector3 direction = character.position - mainCamera.transform.position;
        float distance = direction.magnitude;
        direction.Normalize();

        RaycastHit[] hits;
        
        if (useSphereCast)
        {
            hits = Physics.SphereCastAll(
                mainCamera.transform.position,
                sphereRadius,
                direction,
                distance,
                obstructionLayers
            );
        }
        else
        {
            hits = Physics.RaycastAll(
                mainCamera.transform.position,
                direction,
                distance,
                obstructionLayers
            );
        }

        if (showDebugRays)
        {
            Color debugColor = hits.Length > 0 ? Color.red : Color.green;
            Debug.DrawRay(mainCamera.transform.position, direction * distance, debugColor);
        }

        // Check if any hit is NOT the character itself
        foreach (RaycastHit hit in hits)
        {
            if (!hit.transform.IsChildOf(character) && hit.transform != character)
            {
                return true; // Something is blocking
            }
        }

        return false; // Clear line of sight
    }

    private void UpdateCharacterMaterials(Transform character, bool isOccluded)
    {
        if (character == null)
            return;
            
        Renderer[] renderers = character.GetComponentsInChildren<Renderer>();
        
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null || !trackedCharacters.ContainsKey(renderer))
                continue;

            XRayData data = trackedCharacters[renderer];
            
            if (data == null || data.xrayMaterials == null || data.originalMaterials == null)
            {
                Debug.LogWarning($"[CharacterXRay] Invalid data for renderer {renderer.name}. Skipping.");
                continue;
            }
            
            if (isOccluded != data.isOccluded)
            {
                data.isOccluded = isOccluded;
                
                try
                {
                    if (isOccluded)
                    {
                        // Switch to x-ray materials (filter out nulls)
                        Material[] validXRayMaterials = new Material[data.xrayMaterials.Length];
                        for (int i = 0; i < data.xrayMaterials.Length; i++)
                        {
                            // Use x-ray material if available, otherwise fall back to original
                            validXRayMaterials[i] = data.xrayMaterials[i] != null 
                                ? data.xrayMaterials[i] 
                                : data.originalMaterials[i];
                        }
                        renderer.materials = validXRayMaterials;
                    }
                    else
                    {
                        // Restore original materials
                        renderer.sharedMaterials = data.originalMaterials;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[CharacterXRay] Failed to update materials for {renderer.name}: {e.Message}");
                }
            }
        }
    }

    #endregion

    #region Cleanup

    private void RestoreAllMaterials()
    {
        foreach (var kvp in trackedCharacters)
        {
            if (kvp.Key != null && kvp.Value != null && kvp.Value.originalMaterials != null)
            {
                try
                {
                    kvp.Key.sharedMaterials = kvp.Value.originalMaterials;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[CharacterXRay] Failed to restore materials for {kvp.Key.name}: {e.Message}");
                }
            }
        }
        trackedCharacters.Clear();
    }

    #endregion

    #region Public API

    public void SetXRayColor(Color color)
    {
        xrayColor = color;
        if (xrayMaterial != null)
        {
            xrayMaterial.SetColor("_XRayColor", color);
        }
    }

    public void SetXRayStrength(float strength)
    {
        xrayStrength = Mathf.Clamp01(strength);
        if (xrayMaterial != null)
        {
            xrayMaterial.SetFloat("_XRayStrength", xrayStrength);
        }
    }

    public void SetRimPower(float power)
    {
        rimPower = Mathf.Clamp(power, 0.1f, 8f);
        if (xrayMaterial != null)
        {
            xrayMaterial.SetFloat("_RimPower", rimPower);
        }
    }

    #endregion

    #region Debug

    private void OnDrawGizmosSelected()
    {
        if (player == null || mainCamera == null)
            return;

        // Draw line from camera to player
        Gizmos.color = IsOccluded(player) ? Color.red : Color.green;
        Gizmos.DrawLine(mainCamera.transform.position, player.position);

        // Draw sphere at player
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(player.position, 0.5f);

        // Draw enemies
        if (includeEnemies)
        {
            foreach (Transform enemy in enemyTargets)
            {
                if (enemy != null)
                {
                    Gizmos.color = IsOccluded(enemy) ? Color.yellow : Color.blue;
                    Gizmos.DrawLine(mainCamera.transform.position, enemy.position);
                }
            }
        }
    }

    #endregion
}
