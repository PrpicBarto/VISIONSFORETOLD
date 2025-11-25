using UnityEngine;
using VisionsForetold.Game.Player.Echo;
using System.Collections.Generic;

/// <summary>
/// Example script demonstrating how to use EchoIntersectionDetector
/// Attach this to the same GameObject as EcholocationController and EchoIntersectionDetector
/// </summary>
public class EchoInteractionExample : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject enemyWarningUI;
    [SerializeField] private GameObject itemIndicatorPrefab;
    
    [Header("Audio")]
    [SerializeField] private AudioClip enemyDetectedSound;
    [SerializeField] private AudioClip itemDetectedSound;
    
    [Header("Settings")]
    [SerializeField] private float warningDuration = 2f;
    
    private EchoIntersectionDetector detector;
    private List<GameObject> itemIndicators = new List<GameObject>();
    private float warningTimer;

    #region Unity Lifecycle

    private void Start()
    {
        // Get the detector component
        detector = GetComponent<EchoIntersectionDetector>();
        
        if (detector == null)
        {
            Debug.LogError("[EchoExample] EchoIntersectionDetector not found!");
            enabled = false;
            return;
        }

        // Subscribe to detection events
        detector.OnObjectDetected += HandleObjectDetected;
        detector.OnPulseComplete += HandlePulseComplete;
        
        // Hide warning UI initially
        if (enemyWarningUI != null)
        {
            enemyWarningUI.SetActive(false);
        }
        
        Debug.Log("[EchoExample] Initialized! Listening for echo detections...");
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (detector != null)
        {
            detector.OnObjectDetected -= HandleObjectDetected;
            detector.OnPulseComplete -= HandlePulseComplete;
        }
        
        // Clean up item indicators
        foreach (var indicator in itemIndicators)
        {
            if (indicator != null)
                Destroy(indicator);
        }
        itemIndicators.Clear();
    }

    private void Update()
    {
        // Update warning timer
        if (warningTimer > 0f)
        {
            warningTimer -= Time.deltaTime;
            
            if (warningTimer <= 0f && enemyWarningUI != null)
            {
                enemyWarningUI.SetActive(false);
            }
        }
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Called when ANY object is detected by the echo pulse
    /// </summary>
    private void HandleObjectDetected(EchoHit hit)
    {
        Debug.Log($"[EchoExample] Detected: {hit.objectType} '{hit.hitObject.name}' at {hit.distanceFromPlayer:F1}m");

        // React based on object type
        switch (hit.objectType)
        {
            case EchoObjectType.Enemy:
                HandleEnemyDetected(hit);
                break;

            case EchoObjectType.Item:
                HandleItemDetected(hit);
                break;

            case EchoObjectType.Interactive:
                HandleInteractiveDetected(hit);
                break;

            case EchoObjectType.Hazard:
                HandleHazardDetected(hit);
                break;

            case EchoObjectType.Wall:
                // Walls are highlighted automatically, no additional action needed
                break;
        }
    }

    /// <summary>
    /// Called when a pulse completes (after detecting all objects)
    /// </summary>
    private void HandlePulseComplete(List<EchoHit> allHits)
    {
        // Count detections by type
        int wallCount = 0;
        int enemyCount = 0;
        int itemCount = 0;
        int interactiveCount = 0;
        int hazardCount = 0;

        foreach (var hit in allHits)
        {
            switch (hit.objectType)
            {
                case EchoObjectType.Wall: wallCount++; break;
                case EchoObjectType.Enemy: enemyCount++; break;
                case EchoObjectType.Item: itemCount++; break;
                case EchoObjectType.Interactive: interactiveCount++; break;
                case EchoObjectType.Hazard: hazardCount++; break;
            }
        }

        // Log summary
        Debug.Log($"[EchoExample] Pulse complete! Detected {allHits.Count} objects:");
        Debug.Log($"  Walls: {wallCount}, Enemies: {enemyCount}, Items: {itemCount}, Interactive: {interactiveCount}, Hazards: {hazardCount}");

        // Example: Alert if multiple enemies detected
        if (enemyCount >= 3)
        {
            Debug.LogWarning("[EchoExample] WARNING: Multiple enemies nearby!");
            // Play alarm sound, show special UI, etc.
        }
    }

    #endregion

    #region Type-Specific Handlers

    private void HandleEnemyDetected(EchoHit hit)
    {
        Debug.LogWarning($"[EchoExample] ?? ENEMY detected: {hit.hitObject.name}");

        // Show warning UI
        if (enemyWarningUI != null)
        {
            enemyWarningUI.SetActive(true);
            warningTimer = warningDuration;
        }

        // Play warning sound
        if (enemyDetectedSound != null)
        {
            AudioSource.PlayClipAtPoint(enemyDetectedSound, hit.hitPosition);
        }

        // Example: Mark enemy on minimap
        // MinimapManager.Instance.AddEnemyMarker(hit.hitPosition);

        // Example: Trigger stealth alert
        // if (PlayerStealth.Instance.IsStealthActive)
        // {
        //     PlayerStealth.Instance.ShowStealthRisk(hit.distanceFromPlayer);
        // }
    }

    private void HandleItemDetected(EchoHit hit)
    {
        Debug.Log($"[EchoExample] ?? ITEM detected: {hit.hitObject.name}");

        // Play item detection sound
        if (itemDetectedSound != null)
        {
            AudioSource.PlayClipAtPoint(itemDetectedSound, hit.hitPosition);
        }

        // Spawn world-space indicator for item
        if (itemIndicatorPrefab != null)
        {
            GameObject indicator = Instantiate(itemIndicatorPrefab, hit.hitPosition + Vector3.up * 2f, Quaternion.identity);
            itemIndicators.Add(indicator);
            
            // Auto-destroy after a few seconds
            Destroy(indicator, 5f);
        }

        // Example: Auto-collect if close enough
        // if (hit.distanceFromPlayer < 2f)
        // {
        //     Inventory.Instance.CollectItem(hit.hitObject);
        // }
    }

    private void HandleInteractiveDetected(EchoHit hit)
    {
        Debug.Log($"[EchoExample] ?? INTERACTIVE detected: {hit.hitObject.name}");

        // Example: Show interaction prompt if close
        if (hit.distanceFromPlayer < 3f)
        {
            // InteractionUI.Instance.ShowPrompt("Press E to interact", hit.hitObject);
        }

        // Example: Add to quest tracker
        // QuestManager.Instance.CheckObjectiveDetected(hit.hitObject);
    }

    private void HandleHazardDetected(EchoHit hit)
    {
        Debug.LogWarning($"[EchoExample] ?? HAZARD detected: {hit.hitObject.name}");

        // Example: Show danger indicator
        // DangerIndicatorUI.Instance.ShowWarning(hit.hitPosition);

        // Example: Play warning sound if very close
        if (hit.distanceFromPlayer < 5f)
        {
            // AudioManager.Instance.PlayWarning("DangerClose");
        }
    }

    #endregion

    #region Public Methods (for testing/debugging)

    /// <summary>
    /// Test method to print all currently highlighted objects
    /// </summary>
    [ContextMenu("Show All Highlighted Objects")]
    public void ShowHighlightedObjects()
    {
        if (detector == null) return;

        List<GameObject> highlighted = detector.GetHighlightedObjects();
        
        Debug.Log($"[EchoExample] Currently highlighted: {highlighted.Count} objects");
        foreach (var obj in highlighted)
        {
            Debug.Log($"  - {obj.name}");
        }
    }

    /// <summary>
    /// Test method to clear all highlights
    /// </summary>
    [ContextMenu("Clear All Highlights")]
    public void ClearHighlights()
    {
        if (detector != null)
        {
            detector.ClearAllHighlights();
            Debug.Log("[EchoExample] Cleared all highlights");
        }
    }

    /// <summary>
    /// Test method to toggle highlighting on/off
    /// </summary>
    [ContextMenu("Toggle Highlighting")]
    public void ToggleHighlighting()
    {
        if (detector != null)
        {
            // Note: This would require adding a getter for enableHighlighting
            // or tracking the state separately
            Debug.Log("[EchoExample] Highlighting toggled");
        }
    }

    #endregion
}
