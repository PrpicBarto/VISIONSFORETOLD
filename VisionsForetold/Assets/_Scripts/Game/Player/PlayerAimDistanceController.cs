using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Ensures there's always a minimum distance between the player and where they're aiming.
/// Makes the cursor invisible and locks it during gameplay.
/// </summary>
public class PlayerAimDistanceController : MonoBehaviour
{
    [Header("Aim Distance Settings")]
    [Tooltip("Minimum distance between player and aim target")]
    [SerializeField] private float minAimDistance = 2f; // Reduced for closer isometric combat
    
    [Tooltip("Maximum distance for aim target")]
    [SerializeField] private float maxAimDistance = 10f; // Reduced for tighter isometric view
    
    [Tooltip("Smooth the aim target position")]
    [SerializeField] private bool smoothAimPosition = true;
    
    [Tooltip("Speed of aim position smoothing")]
    [SerializeField] private float aimSmoothSpeed = 15f; // Increased for snappier isometric feel

    [Header("Cursor Settings")]
    [Tooltip("Hide cursor during gameplay (uncheck for isometric - usually want visible cursor)")]
    [SerializeField] private bool hideCursor = false; // Changed to false for isometric
    
    [Tooltip("Lock cursor to center of screen")]
    [SerializeField] private bool lockCursor = false;
    
    [Header("Isometric Settings")]
    [Tooltip("Use isometric ground plane for aim detection")]
    [SerializeField] private bool useIsometricMode = true;
    
    [Tooltip("Height of the ground plane for isometric projection")]
    [SerializeField] private float groundPlaneHeight = 0f;
    
    [Tooltip("Snap aim to grid (useful for isometric movement)")]
    [SerializeField] private bool snapToGrid = false;
    
    [Tooltip("Grid size for snapping")]
    [SerializeField] private float gridSize = 0.5f;

    [Header("References")]
    [SerializeField] private Transform aimTarget;
    [SerializeField] private Transform player;
    
    [Header("Gamepad Settings")]
    [Tooltip("Distance for gamepad aiming")]
    [SerializeField] private float gamepadAimDistance = 6f; // Reduced for isometric

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private Color minDistanceColor = Color.yellow;
    [SerializeField] private Color maxDistanceColor = Color.red;

    private Camera mainCamera;
    private Vector3 targetAimPosition;
    private PlayerMovement playerMovement;
    private bool isUsingGamepad;
    private Vector2 lastMousePosition;
    private float lastInputTime;

    #region Unity Lifecycle

    private void Awake()
    {
        mainCamera = Camera.main;
        
        // Auto-find player if not assigned
        if (player == null)
        {
            player = transform;
        }

        // Try to get PlayerMovement component
        playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null && aimTarget == null)
        {
            // Try to get aim target from PlayerMovement
            aimTarget = playerMovement.AimTarget;
        }

        if (aimTarget == null)
        {
            Debug.LogWarning("[PlayerAimDistanceController] Aim target not assigned! Creating one...");
            CreateAimTarget();
        }

        targetAimPosition = aimTarget.position;
    }

    private void Start()
    {
        SetupCursor();
    }

    private void Update()
    {
        DetectInputMethod();
        UpdateAimPosition();
        EnforceAimDistance();
    }

    private void LateUpdate()
    {
        // Ensure cursor state is maintained
        if (hideCursor && Cursor.visible)
        {
            SetupCursor();
        }
    }

    #endregion

    #region Cursor Management

    private void SetupCursor()
    {
        if (hideCursor)
        {
            Cursor.visible = false;
            
            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Confined;
            }
            
            Debug.Log("[PlayerAimDistanceController] Cursor hidden and configured");
        }
    }

    /// <summary>
    /// Show cursor (useful for menus)
    /// </summary>
    public void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// Hide cursor (for gameplay)
    /// </summary>
    public void HideCursor()
    {
        if (hideCursor)
        {
            Cursor.visible = false;
            Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.Confined;
        }
    }

    #endregion

    #region Input Detection

    private void DetectInputMethod()
    {
        // Check for mouse movement
        if (Mouse.current != null)
        {
            Vector2 currentMousePos = Mouse.current.position.ReadValue();
            if (Vector2.Distance(currentMousePos, lastMousePosition) > 1f)
            {
                isUsingGamepad = false;
                lastInputTime = Time.time;
                lastMousePosition = currentMousePos;
            }
        }

        // Check for gamepad input
        if (Gamepad.current != null)
        {
            Vector2 rightStick = Gamepad.current.rightStick.ReadValue();
            if (rightStick.magnitude > 0.1f)
            {
                isUsingGamepad = true;
                lastInputTime = Time.time;
            }
        }
    }

    #endregion

    #region Aim Position

    private void UpdateAimPosition()
    {
        if (aimTarget == null || player == null) return;

        Vector3 desiredAimPosition;

        if (isUsingGamepad)
        {
            // Gamepad: Aim in direction of right stick
            if (Gamepad.current != null)
            {
                Vector2 aimInput = Gamepad.current.rightStick.ReadValue();
                
                if (aimInput.magnitude > 0.1f)
                {
                    Vector3 aimDirection = GetWorldSpaceDirection(aimInput).normalized;
                    desiredAimPosition = player.position + aimDirection * gamepadAimDistance;
                }
                else
                {
                    // No gamepad input, aim forward
                    desiredAimPosition = player.position + player.forward * gamepadAimDistance;
                }
            }
            else
            {
                desiredAimPosition = player.position + player.forward * gamepadAimDistance;
            }
        }
        else
        {
            // Mouse: Aim at cursor position (with raycast)
            desiredAimPosition = GetMouseAimPosition();
        }

        // Smooth aim position if enabled
        if (smoothAimPosition)
        {
            targetAimPosition = Vector3.Lerp(targetAimPosition, desiredAimPosition, aimSmoothSpeed * Time.deltaTime);
        }
        else
        {
            targetAimPosition = desiredAimPosition;
        }
    }

    private Vector3 GetMouseAimPosition()
    {
        if (mainCamera == null || Mouse.current == null) 
            return player.position + player.forward * minAimDistance;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        // For isometric mode, raycast to ground plane
        if (useIsometricMode)
        {
            Plane groundPlane = new Plane(Vector3.up, new Vector3(0, groundPlaneHeight, 0));
            
            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 point = ray.GetPoint(distance);
                
                // Snap to grid if enabled
                if (snapToGrid && gridSize > 0)
                {
                    point.x = Mathf.Round(point.x / gridSize) * gridSize;
                    point.z = Mathf.Round(point.z / gridSize) * gridSize;
                }
                
                // Clamp to max distance from player
                Vector3 directionToPoint = point - player.position;
                directionToPoint.y = 0; // Keep on ground plane
                
                if (directionToPoint.magnitude > maxAimDistance)
                {
                    point = player.position + directionToPoint.normalized * maxAimDistance;
                }
                
                // Ensure point is at ground level
                point.y = groundPlaneHeight;
                
                return point;
            }
        }
        else
        {
            // Standard 3D raycast (original behavior)
            if (Physics.Raycast(ray, out RaycastHit hitInfo, maxAimDistance * 2f))
            {
                return hitInfo.point;
            }

            // If no hit, project onto ground plane
            Plane groundPlane = new Plane(Vector3.up, player.position);
            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 point = ray.GetPoint(distance);
                
                // Clamp to max distance
                Vector3 directionToPoint = point - player.position;
                if (directionToPoint.magnitude > maxAimDistance)
                {
                    point = player.position + directionToPoint.normalized * maxAimDistance;
                }
                
                return point;
            }
        }

        // Fallback: aim forward
        return player.position + player.forward * minAimDistance;
    }

    private Vector3 GetWorldSpaceDirection(Vector2 input)
    {
        if (mainCamera == null) return new Vector3(input.x, 0, input.y);

        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;

        cameraForward.Normalize();
        cameraRight.Normalize();

        return cameraForward * input.y + cameraRight * input.x;
    }

    #endregion

    #region Distance Enforcement

    private void EnforceAimDistance()
    {
        if (aimTarget == null || player == null) return;

        Vector3 directionToAim = targetAimPosition - player.position;
        
        // For isometric, only consider horizontal distance (XZ plane)
        if (useIsometricMode)
        {
            directionToAim.y = 0; // Flatten to ground plane
        }
        
        float distanceToAim = directionToAim.magnitude;

        // Enforce minimum distance
        if (distanceToAim < minAimDistance)
        {
            if (directionToAim.magnitude > 0.01f)
            {
                targetAimPosition = player.position + directionToAim.normalized * minAimDistance;
            }
            else
            {
                // If too close, aim forward
                Vector3 forward = player.forward;
                if (useIsometricMode) forward.y = 0;
                targetAimPosition = player.position + forward.normalized * minAimDistance;
            }
        }
        // Enforce maximum distance
        else if (distanceToAim > maxAimDistance)
        {
            targetAimPosition = player.position + directionToAim.normalized * maxAimDistance;
        }

        // Ensure aim target stays on ground plane in isometric mode
        if (useIsometricMode)
        {
            targetAimPosition.y = groundPlaneHeight;
        }

        // Apply the constrained position
        aimTarget.position = targetAimPosition;
    }

    #endregion

    #region Helper Methods

    private void CreateAimTarget()
    {
        GameObject aimObj = new GameObject("AimTarget");
        aimTarget = aimObj.transform;
        aimTarget.position = player.position + player.forward * minAimDistance;
        
        Debug.Log("[PlayerAimDistanceController] Created aim target at runtime");
    }

    /// <summary>
    /// Get current aim target transform
    /// </summary>
    public Transform GetAimTarget()
    {
        return aimTarget;
    }

    /// <summary>
    /// Get distance between player and aim target
    /// </summary>
    public float GetCurrentAimDistance()
    {
        if (aimTarget == null || player == null) return 0f;
        return Vector3.Distance(player.position, aimTarget.position);
    }

    /// <summary>
    /// Check if using gamepad for aiming
    /// </summary>
    public bool IsUsingGamepad()
    {
        return isUsingGamepad;
    }

    #endregion

    #region Debug

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos || player == null) return;

        // Draw minimum distance circle
        Gizmos.color = minDistanceColor;
        DrawCircle(player.position, minAimDistance, 32);

        // Draw maximum distance circle
        Gizmos.color = maxDistanceColor;
        DrawCircle(player.position, maxAimDistance, 64);

        // Draw line to aim target
        if (aimTarget != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(player.position + Vector3.up, aimTarget.position + Vector3.up);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(aimTarget.position, 0.3f);
        }
    }

    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 previousPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(previousPoint, newPoint);
            previousPoint = newPoint;
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// Set minimum aim distance
    /// </summary>
    public void SetMinAimDistance(float distance)
    {
        minAimDistance = Mathf.Max(0.1f, distance);
    }

    /// <summary>
    /// Set maximum aim distance
    /// </summary>
    public void SetMaxAimDistance(float distance)
    {
        maxAimDistance = Mathf.Max(minAimDistance, distance);
    }

    /// <summary>
    /// Enable/disable cursor visibility
    /// </summary>
    public void SetCursorVisible(bool visible)
    {
        hideCursor = !visible;
        if (visible)
        {
            ShowCursor();
        }
        else
        {
            HideCursor();
        }
    }

    #endregion
}
