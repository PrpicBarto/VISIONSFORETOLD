using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Diagnostic tool to track down Invalid AABB errors
/// Attach to a GameObject in your scene to monitor all UI elements
/// </summary>
public class AABBDiagnostic : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool logEveryFrame = false;
    [SerializeField] private bool autoScanOnEnable = true;
    [SerializeField] private float scanInterval = 1f;

    private float lastScanTime;
    private List<Graphic> allGraphics = new List<Graphic>();
    private Dictionary<Graphic, string> graphicPaths = new Dictionary<Graphic, string>();

    private void OnEnable()
    {
        if (autoScanOnEnable)
        {
            ScanAllGraphics();
        }
    }

    private void Update()
    {
        if (Time.time - lastScanTime > scanInterval)
        {
            CheckAllGraphics();
            lastScanTime = Time.time;
        }
    }

    [ContextMenu("Scan All Graphics")]
    public void ScanAllGraphics()
    {
        allGraphics.Clear();
        graphicPaths.Clear();

        // Find all Graphic components in scene
        Graphic[] graphics = FindObjectsByType<Graphic>(FindObjectsSortMode.None);
        
        Debug.Log($"[AABB Diagnostic] Found {graphics.Length} Graphic components");

        foreach (var graphic in graphics)
        {
            if (graphic != null)
            {
                allGraphics.Add(graphic);
                graphicPaths[graphic] = GetGameObjectPath(graphic.gameObject);
            }
        }

        Debug.Log($"[AABB Diagnostic] Scan complete. Monitoring {allGraphics.Count} graphics.");
    }

    [ContextMenu("Check All Graphics")]
    public void CheckAllGraphics()
    {
        if (allGraphics.Count == 0)
        {
            ScanAllGraphics();
        }

        int invalidCount = 0;
        int playerUICount = 0;
        List<Graphic> toRemove = new List<Graphic>();

        foreach (var graphic in allGraphics)
        {
            // Check if graphic is null (destroyed)
            if (graphic == null)
            {
                toRemove.Add(graphic);
                continue;
            }

            // Check if this is on the player
            bool isPlayerUI = graphicPaths[graphic].Contains("Player/") || 
                             graphicPaths[graphic].Contains("player/") ||
                             graphic.transform.root.CompareTag("Player");

            if (isPlayerUI)
            {
                playerUICount++;
                Debug.Log($"[AABB] Player UI detected: {graphicPaths[graphic]}", graphic.gameObject);
            }

            // Check if active
            if (!graphic.isActiveAndEnabled)
            {
                if (logEveryFrame)
                    Debug.Log($"[AABB] Inactive: {graphicPaths[graphic]}");
                continue;
            }

            // Check RectTransform
            RectTransform rectTransform = graphic.rectTransform;
            if (rectTransform == null)
            {
                Debug.LogError($"[AABB] NULL RectTransform: {graphicPaths[graphic]}", graphic.gameObject);
                invalidCount++;
                continue;
            }

            // Check rect dimensions
            Rect rect = rectTransform.rect;
            if (float.IsNaN(rect.width) || float.IsNaN(rect.height) ||
                float.IsInfinity(rect.width) || float.IsInfinity(rect.height))
            {
                Debug.LogError($"[AABB] INVALID RECT: {graphicPaths[graphic]} - Width: {rect.width}, Height: {rect.height}", graphic.gameObject);
                if (isPlayerUI)
                    Debug.LogError($"[AABB] ^^^ THIS IS ON THE PLAYER! Check your player mesh for UI elements! ^^^");
                invalidCount++;
                continue;
            }

            if (rect.width < 0 || rect.height < 0)
            {
                Debug.LogWarning($"[AABB] NEGATIVE RECT: {graphicPaths[graphic]} - Width: {rect.width}, Height: {rect.height}", graphic.gameObject);
                if (isPlayerUI)
                    Debug.LogWarning($"[AABB] ^^^ THIS IS ON THE PLAYER! ^^^");
                invalidCount++;
                continue;
            }

            // Check position
            Vector3 pos = rectTransform.position;
            if (float.IsNaN(pos.x) || float.IsNaN(pos.y) || float.IsNaN(pos.z) ||
                float.IsInfinity(pos.x) || float.IsInfinity(pos.y) || float.IsInfinity(pos.z))
            {
                Debug.LogError($"[AABB] INVALID POSITION: {graphicPaths[graphic]} - Position: {pos}", graphic.gameObject);
                if (isPlayerUI)
                    Debug.LogError($"[AABB] ^^^ THIS IS ON THE PLAYER! ^^^");
                invalidCount++;
                continue;
            }

            // Check Canvas
            Canvas canvas = graphic.canvas;
            if (canvas == null)
            {
                Debug.LogWarning($"[AABB] NULL Canvas: {graphicPaths[graphic]}", graphic.gameObject);
                if (isPlayerUI)
                    Debug.LogWarning($"[AABB] ^^^ THIS IS ON THE PLAYER! ^^^");
                invalidCount++;
                continue;
            }

            // Check if CanvasRenderer is valid
            CanvasRenderer canvasRenderer = graphic.canvasRenderer;
            if (canvasRenderer == null)
            {
                Debug.LogError($"[AABB] NULL CanvasRenderer: {graphicPaths[graphic]}", graphic.gameObject);
                if (isPlayerUI)
                    Debug.LogError($"[AABB] ^^^ THIS IS ON THE PLAYER! ^^^");
                invalidCount++;
                continue;
            }

            // Check for Image with invalid sprite
            Image image = graphic as Image;
            if (image != null && image.sprite == null && image.type != Image.Type.Simple)
            {
                Debug.LogWarning($"[AABB] Image with NULL sprite: {graphicPaths[graphic]}", graphic.gameObject);
                if (isPlayerUI)
                    Debug.LogWarning($"[AABB] ^^^ THIS IS ON THE PLAYER! Check if this is from your new player mesh! ^^^");
            }

            // Log if everything seems fine but we want verbose logging
            if (logEveryFrame)
            {
                Debug.Log($"[AABB] OK: {graphicPaths[graphic]} - Rect: {rect.width}x{rect.height}");
            }
        }

        // Clean up null references
        foreach (var graphic in toRemove)
        {
            allGraphics.Remove(graphic);
            graphicPaths.Remove(graphic);
        }

        if (playerUICount > 0)
        {
            Debug.LogWarning($"[AABB Diagnostic] Found {playerUICount} UI elements on the PLAYER! These might be from your new player mesh.");
        }

        if (invalidCount > 0)
        {
            Debug.LogError($"[AABB Diagnostic] Found {invalidCount} invalid graphics! Check messages above for details.");
        }
        else if (!logEveryFrame)
        {
            Debug.Log($"[AABB Diagnostic] All {allGraphics.Count} graphics are valid.");
        }
    }

    [ContextMenu("Inspect Player UI")]
    public void InspectPlayerUI()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("[AABB] Player not found! Make sure player has 'Player' tag.");
            return;
        }

        Debug.Log($"[AABB] === INSPECTING PLAYER UI ===");
        Debug.Log($"[AABB] Player: {player.name}");

        // Find all Canvas components
        Canvas[] canvases = player.GetComponentsInChildren<Canvas>(true);
        Debug.Log($"[AABB] Found {canvases.Length} Canvas components on player:");
        foreach (var canvas in canvases)
        {
            string path = GetGameObjectPath(canvas.gameObject);
            Debug.Log($"[AABB]   Canvas: {path}");
            Debug.Log($"[AABB]     - Render Mode: {canvas.renderMode}");
            Debug.Log($"[AABB]     - Active: {canvas.gameObject.activeInHierarchy}");
            Debug.Log($"[AABB]     - Enabled: {canvas.enabled}");
            
            RectTransform rect = canvas.GetComponent<RectTransform>();
            if (rect != null)
            {
                Debug.Log($"[AABB]     - Size: {rect.rect.width} x {rect.rect.height}");
                Debug.Log($"[AABB]     - Position: {rect.position}");
            }
        }

        // Find all Graphic components (Images, Text, etc.)
        Graphic[] graphics = player.GetComponentsInChildren<Graphic>(true);
        Debug.Log($"[AABB] Found {graphics.Length} UI Graphic components on player:");
        foreach (var graphic in graphics)
        {
            string path = GetGameObjectPath(graphic.gameObject);
            string type = graphic.GetType().Name;
            Debug.Log($"[AABB]   {type}: {path}");
            Debug.Log($"[AABB]     - Active: {graphic.gameObject.activeInHierarchy}");
            Debug.Log($"[AABB]     - Enabled: {graphic.enabled}");

            // Check if it's an Image
            Image image = graphic as Image;
            if (image != null)
            {
                Debug.Log($"[AABB]     - Sprite: {(image.sprite != null ? image.sprite.name : "NULL")}");
                Debug.Log($"[AABB]     - Image Type: {image.type}");
            }

            // Check RectTransform
            RectTransform rect = graphic.rectTransform;
            if (rect != null)
            {
                Debug.Log($"[AABB]     - Size: {rect.rect.width} x {rect.rect.height}");
                if (float.IsNaN(rect.rect.width) || float.IsNaN(rect.rect.height))
                {
                    Debug.LogError($"[AABB]     ^^^ INVALID SIZE! This is causing the AABB error! ^^^");
                }
            }
        }

        Debug.Log($"[AABB] === END PLAYER UI INSPECTION ===");
    }

    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;

        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }

        return path;
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, Screen.height - 120, 400, 110));
        GUILayout.Box("AABB Diagnostic");
        GUILayout.Label($"Monitoring: {allGraphics.Count} graphics");
        
        if (GUILayout.Button("Scan All Graphics"))
        {
            ScanAllGraphics();
        }
        
        if (GUILayout.Button("Check All Graphics"))
        {
            CheckAllGraphics();
        }
        
        GUILayout.EndArea();
    }
}
