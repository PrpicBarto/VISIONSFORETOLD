using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

/// <summary>
/// Dynamic camera system that shifts to show what the player is aiming at
/// Works with both mouse and gamepad input
/// Integrates with existing PlayerMovement aim system
/// </summary>
public class AimOffsetCamera : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The player's transform")]
    [SerializeField] private Transform playerTransform;
    
    [Tooltip("The player's aim target from PlayerMovement")]
    [SerializeField] private Transform aimTarget;
    
    [Tooltip("Cinemachine virtual camera to control")]
    [SerializeField] private CinemachineCamera virtualCamera;
    
    [Tooltip("PlayerMovement script reference (auto-found if null)")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Camera Offset Settings")]
    [Tooltip("How far the camera shifts based on aim direction")]
    [SerializeField, Range(0f, 10f)] private float maxCameraOffset = 3f;
    
    [Tooltip("How quickly camera moves to new position")]
    [SerializeField, Range(1f, 20f)] private float cameraSmoothSpeed = 5f;
    
    [Tooltip("Minimum aim distance to trigger camera shift")]
    [SerializeField] private float minAimDistance = 1f;
    
    [Tooltip("Percentage of screen to shift camera (0-1)")]
    [SerializeField, Range(0f, 1f)] private float screenOffsetPercentage = 0.3f;

    [Header("Vertical Offset")]
    [Tooltip("Additional vertical offset when aiming")]
    [SerializeField] private float verticalAimOffset = 0.5f;
    
    [Tooltip("Apply vertical offset")]
    [SerializeField] private bool useVerticalOffset = true;

    [Header("Dead Zone")]
    [Tooltip("Dead zone where camera doesn't move (percentage of max offset)")]
    [SerializeField, Range(0f, 0.5f)] private float deadZone = 0.1f;

    [Header("Zoom Settings")]
    [Tooltip("Enable zoom when aiming far")]
    [SerializeField] private bool enableAimZoom = false;
    
    [Tooltip("Field of view when aiming far")]
    [SerializeField, Range(20f, 60f)] private float aimedFOV = 50f;
    
    [Tooltip("Normal field of view")]
    [SerializeField, Range(40f, 80f)] private float normalFOV = 60f;
    
    [Tooltip("Distance threshold to trigger zoom")]
    [SerializeField] private float zoomDistanceThreshold = 8f;

    [Header("Look Ahead")]
    [Tooltip("Show more area in the aim direction")]
    [SerializeField] private bool enableLookAhead = true;
    
    [Tooltip("How far ahead to look")]
    [SerializeField, Range(0f, 15f)] private float lookAheadDistance = 5f;

    [Header("Debug")]
    [Tooltip("Show debug gizmos in Scene view")]
    [SerializeField] private bool showDebugGizmos = true;
    
    [Tooltip("Show debug UI in Game view")]
    [SerializeField] private bool showDebugUI = false;

    // Runtime state
    private Vector3 targetOffset;
    private Vector3 currentOffset;
    private Vector3 baseOffset;
    private float targetFOV;
    private float currentFOV;
    private CinemachineFollow followComponent;
    private CinemachinePositionComposer positionComposer;

    // Input tracking
    private Camera mainCamera;
    private Vector2 lastAimPosition;

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        InitializeCamera();
    }

    private void LateUpdate()
    {
        UpdateCameraOffset();
        UpdateCameraZoom();
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        // Auto-find components if not assigned
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (playerTransform == null)
            {
                Debug.LogError("[AimOffsetCamera] Player transform not found! Assign manually or tag player as 'Player'");
                enabled = false;
                return;
            }
        }

        if (playerMovement == null)
        {
            playerMovement = playerTransform.GetComponent<PlayerMovement>();
        }

        if (aimTarget == null && playerMovement != null)
        {
            aimTarget = playerMovement.AimTarget;
        }

        if (virtualCamera == null)
        {
            virtualCamera = GetComponent<CinemachineCamera>();
        }

        mainCamera = Camera.main;
    }

    private void InitializeCamera()
    {
        if (virtualCamera == null)
        {
            Debug.LogError("[AimOffsetCamera] No CinemachineCamera found!");
            enabled = false;
            return;
        }

        // Get Cinemachine components
        followComponent = virtualCamera.GetComponent<CinemachineFollow>();
        positionComposer = virtualCamera.GetComponent<CinemachinePositionComposer>();

        // Store base offset
        if (followComponent != null)
        {
            baseOffset = followComponent.FollowOffset;
        }

        // Initialize FOV
        if (mainCamera != null)
        {
            normalFOV = mainCamera.fieldOfView;
            currentFOV = normalFOV;
            targetFOV = normalFOV;
        }
    }

    #endregion

    #region Camera Update

    private void UpdateCameraOffset()
    {
        if (aimTarget == null || playerTransform == null)
            return;

        // Calculate aim direction
        Vector3 aimDirection = aimTarget.position - playerTransform.position;
        float aimDistance = aimDirection.magnitude;
        
        // Only apply offset if aim is far enough
        if (aimDistance < minAimDistance)
        {
            targetOffset = baseOffset;
        }
        else
        {
            // Normalize aim direction
            aimDirection /= aimDistance;
            aimDirection.y = 0; // Keep on horizontal plane

            // Calculate offset based on aim direction
            Vector3 lateralOffset = aimDirection * Mathf.Min(aimDistance, maxCameraOffset);
            
            // Apply dead zone
            float offsetMagnitude = lateralOffset.magnitude;
            float deadZoneThreshold = maxCameraOffset * deadZone;
            
            if (offsetMagnitude > deadZoneThreshold)
            {
                // Scale offset outside dead zone
                float scaledMagnitude = (offsetMagnitude - deadZoneThreshold) / (maxCameraOffset - deadZoneThreshold);
                lateralOffset = lateralOffset.normalized * scaledMagnitude * maxCameraOffset;
            }
            else
            {
                lateralOffset = Vector3.zero;
            }

            // Add vertical offset if enabled
            Vector3 verticalOffset = Vector3.zero;
            if (useVerticalOffset)
            {
                verticalOffset = Vector3.up * verticalAimOffset;
            }

            // Combine offsets
            targetOffset = baseOffset + lateralOffset + verticalOffset;

            // Look ahead - shift camera in aim direction
            if (enableLookAhead)
            {
                Vector3 lookAheadOffset = aimDirection * (lookAheadDistance * (aimDistance / 10f));
                targetOffset += lookAheadOffset;
            }
        }

        // Smoothly interpolate to target offset
        currentOffset = Vector3.Lerp(currentOffset, targetOffset, Time.deltaTime * cameraSmoothSpeed);

        // Apply offset to camera
        if (followComponent != null)
        {
            followComponent.FollowOffset = currentOffset;
        }
        else if (positionComposer != null)
        {
            // For PositionComposer, use different approach
            Vector3 screenOffset = CalculateScreenSpaceOffset(aimDirection);
            positionComposer.TargetOffset = screenOffset;
        }
    }

    private Vector3 CalculateScreenSpaceOffset(Vector3 aimDirection)
    {
        // Convert world direction to screen space offset
        Vector3 screenOffset = aimDirection * screenOffsetPercentage;
        screenOffset.y = useVerticalOffset ? verticalAimOffset * screenOffsetPercentage : 0f;
        return screenOffset;
    }

    private void UpdateCameraZoom()
    {
        if (!enableAimZoom || aimTarget == null || playerTransform == null)
            return;

        // Calculate aim distance
        float aimDistance = Vector3.Distance(playerTransform.position, aimTarget.position);

        // Determine target FOV based on aim distance
        if (aimDistance > zoomDistanceThreshold)
        {
            // Zoomed in when aiming far
            float zoomFactor = Mathf.Clamp01((aimDistance - zoomDistanceThreshold) / zoomDistanceThreshold);
            targetFOV = Mathf.Lerp(normalFOV, aimedFOV, zoomFactor);
        }
        else
        {
            // Normal FOV
            targetFOV = normalFOV;
        }

        // Smoothly interpolate FOV
        currentFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime * cameraSmoothSpeed);

        // Apply FOV to camera
        if (mainCamera != null)
        {
            mainCamera.fieldOfView = currentFOV;
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// Set the maximum camera offset distance
    /// </summary>
    public void SetMaxOffset(float offset)
    {
        maxCameraOffset = Mathf.Max(0f, offset);
    }

    /// <summary>
    /// Set camera smoothing speed
    /// </summary>
    public void SetSmoothSpeed(float speed)
    {
        cameraSmoothSpeed = Mathf.Clamp(speed, 1f, 20f);
    }

    /// <summary>
    /// Enable or disable aim zoom
    /// </summary>
    public void SetAimZoomEnabled(bool enabled)
    {
        enableAimZoom = enabled;
    }

    /// <summary>
    /// Enable or disable look ahead
    /// </summary>
    public void SetLookAheadEnabled(bool enabled)
    {
        enableLookAhead = enabled;
    }

    /// <summary>
    /// Reset camera to base position
    /// </summary>
    public void ResetCamera()
    {
        currentOffset = baseOffset;
        targetOffset = baseOffset;
        
        if (followComponent != null)
        {
            followComponent.FollowOffset = baseOffset;
        }
    }

    /// <summary>
    /// Get current camera offset from base position
    /// </summary>
    public Vector3 GetCurrentOffset()
    {
        return currentOffset - baseOffset;
    }

    #endregion

    #region Debug

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos || !Application.isPlaying)
            return;

        if (playerTransform == null || aimTarget == null)
            return;

        // Draw aim line
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(playerTransform.position, aimTarget.position);

        // Draw aim target
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(aimTarget.position, 0.5f);

        // Draw dead zone
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(playerTransform.position, maxCameraOffset * deadZone);

        // Draw max offset range
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(playerTransform.position, maxCameraOffset);

        // Draw current camera offset
        if (followComponent != null)
        {
            Vector3 cameraTargetPos = playerTransform.position + currentOffset;
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(cameraTargetPos, 0.3f);
            Gizmos.DrawLine(playerTransform.position, cameraTargetPos);
        }

        // Draw look ahead direction
        if (enableLookAhead)
        {
            Vector3 aimDir = (aimTarget.position - playerTransform.position).normalized;
            aimDir.y = 0;
            Vector3 lookAheadPos = playerTransform.position + aimDir * lookAheadDistance;
            
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(playerTransform.position, lookAheadPos);
            Gizmos.DrawWireSphere(lookAheadPos, 0.4f);
        }
    }

    private void OnGUI()
    {
        if (!showDebugUI)
            return;

        GUIStyle style = new GUIStyle(GUI.skin.box)
        {
            fontSize = 12,
            alignment = TextAnchor.UpperLeft
        };
        style.normal.textColor = Color.white;

        string debugInfo = "=== AIM OFFSET CAMERA ===\n";
        
        if (playerTransform != null && aimTarget != null)
        {
            float aimDistance = Vector3.Distance(playerTransform.position, aimTarget.position);
            debugInfo += $"Aim Distance: {aimDistance:F2}m\n";
        }
        
        debugInfo += $"Camera Offset: {GetCurrentOffset().magnitude:F2}m\n";
        debugInfo += $"Current FOV: {currentFOV:F1}°\n";
        debugInfo += $"Target FOV: {targetFOV:F1}°\n";
        debugInfo += $"Look Ahead: {(enableLookAhead ? "ON" : "OFF")}\n";
        debugInfo += $"Aim Zoom: {(enableAimZoom ? "ON" : "OFF")}";

        GUI.Box(new Rect(10, 150, 250, 140), debugInfo, style);
    }

    private void OnValidate()
    {
        maxCameraOffset = Mathf.Max(0f, maxCameraOffset);
        cameraSmoothSpeed = Mathf.Clamp(cameraSmoothSpeed, 1f, 20f);
        minAimDistance = Mathf.Max(0.1f, minAimDistance);
        lookAheadDistance = Mathf.Max(0f, lookAheadDistance);
        zoomDistanceThreshold = Mathf.Max(1f, zoomDistanceThreshold);
        normalFOV = Mathf.Clamp(normalFOV, 40f, 80f);
        aimedFOV = Mathf.Clamp(aimedFOV, 20f, 60f);
    }

    #endregion
}
