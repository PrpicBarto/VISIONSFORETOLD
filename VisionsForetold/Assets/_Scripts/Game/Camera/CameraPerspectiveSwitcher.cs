using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

/// <summary>
/// Camera perspective switcher - Toggle between isometric and third-person views
/// Supports both keyboard and gamepad input
/// Works with Cinemachine virtual cameras
/// </summary>
public class CameraPerspectiveSwitcher : MonoBehaviour
{
    public enum CameraPerspective
    {
        Isometric,      // Top-down isometric view (fixed angle)
        ThirdPerson     // Behind-player view (rotates with player)
    }

    [Header("Camera References")]
    [Tooltip("Isometric camera (top-down, fixed angle)")]
    [SerializeField] private CinemachineCamera isometricCamera;
    
    [Tooltip("Third-person camera (follows player rotation)")]
    [SerializeField] private CinemachineCamera thirdPersonCamera;
    
    [Tooltip("Player transform")]
    [SerializeField] private Transform playerTransform;

    [Header("Starting Perspective")]
    [SerializeField] private CameraPerspective defaultPerspective = CameraPerspective.Isometric;

    [Header("Input Settings")]
    [Tooltip("Key to switch perspective")]
    [SerializeField] private Key switchKey = Key.V;
    
    [Tooltip("Gamepad button name for switching (from Input Actions)")]
    [SerializeField] private string gamepadSwitchButton = "SwitchCamera";
    
    [Tooltip("Cooldown between perspective switches")]
    [SerializeField] private float switchCooldown = 0.5f;

    [Header("Transition Settings")]
    [Tooltip("Blend time when switching cameras")]
    [SerializeField] private float blendTime = 0.8f;
    
    [Tooltip("Smooth transition")]
    [SerializeField] private bool smoothTransition = true;

    [Header("Third Person Settings")]
    [Tooltip("Camera distance from player in third person")]
    [SerializeField] private float thirdPersonDistance = 5f;
    
    [Tooltip("Camera height offset in third person")]
    [SerializeField] private float thirdPersonHeight = 5f;
    
    [Tooltip("How fast camera rotates with player")]
    [SerializeField] private float cameraRotationSpeed = 10f;
    
    [Tooltip("Enable camera rotation with mouse/right stick")]
    [SerializeField] private bool enableManualCameraRotation = true;
    
    [Tooltip("Mouse sensitivity for camera rotation")]
    [SerializeField] private float mouseSensitivity = 2f;
    
    [Tooltip("Gamepad sensitivity for camera rotation")]
    [SerializeField] private float gamepadSensitivity = 100f;

    [Header("Isometric Settings")]
    [Tooltip("Fixed angle for isometric camera (degrees)")]
    [SerializeField] private float isometricAngle = 45f;
    
    [Tooltip("Distance from player in isometric view")]
    [SerializeField] private float isometricDistance = 10f;
    
    [Tooltip("Height of isometric camera")]
    [SerializeField] private float isometricHeight = 10f;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;

    // Current state
    private CameraPerspective currentPerspective;
    private float lastSwitchTime = -Mathf.Infinity;
    
    // Input System
    private PlayerInput playerInput;
    private InputAction switchCameraAction;
    private InputAction lookAction; // For manual camera rotation
    
    // Camera rotation (third person)
    private float currentCameraYaw = 0f;
    private float currentCameraPitch = 0f;
    
    // Cinemachine components
    private CinemachineFollow isometricFollow;
    private CinemachineFollow thirdPersonFollow;

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeComponents();
        InitializeInputActions();
    }

    private void Start()
    {
        SetupCameras();
        SetPerspective(defaultPerspective, false); // Set initial perspective without blend
    }

    private void OnEnable()
    {
        if (switchCameraAction != null)
        {
            switchCameraAction.performed += OnSwitchCamera;
            switchCameraAction.Enable();
        }
    }

    private void OnDisable()
    {
        if (switchCameraAction != null)
        {
            switchCameraAction.performed -= OnSwitchCamera;
        }
    }

    private void Update()
    {
        HandleKeyboardInput();
        
        if (currentPerspective == CameraPerspective.ThirdPerson)
        {
            UpdateThirdPersonCamera();
        }
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        // Auto-find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogError("[CameraSwitcher] Player not found! Tag a GameObject as 'Player'");
                enabled = false;
                return;
            }
        }

        // Get PlayerInput
        playerInput = playerTransform.GetComponent<PlayerInput>();
        
        // Auto-create cameras if not assigned
        if (isometricCamera == null)
        {
            CreateIsometricCamera();
        }
        
        if (thirdPersonCamera == null)
        {
            CreateThirdPersonCamera();
        }
    }

    private void InitializeInputActions()
    {
        if (playerInput != null)
        {
            // Try to find switch camera action
            switchCameraAction = playerInput.actions.FindAction(gamepadSwitchButton);
            
            if (switchCameraAction == null)
            {
                Debug.LogWarning($"[CameraSwitcher] Input action '{gamepadSwitchButton}' not found. Create it in Input Actions.");
            }
            
            // Get look action for manual camera rotation
            lookAction = playerInput.actions.FindAction("Look");
        }
    }

    private void SetupCameras()
    {
        if (isometricCamera != null)
        {
            isometricFollow = isometricCamera.GetComponent<CinemachineFollow>();
            if (isometricFollow == null)
            {
                isometricFollow = isometricCamera.gameObject.AddComponent<CinemachineFollow>();
            }
            
            // Set isometric camera to follow player
            isometricCamera.Target.TrackingTarget = playerTransform;
            
            // Configure isometric follow offset
            isometricFollow.FollowOffset = CalculateIsometricOffset();
        }

        if (thirdPersonCamera != null)
        {
            // Use regular follow for third person
            thirdPersonFollow = thirdPersonCamera.GetComponent<CinemachineFollow>();
            if (thirdPersonFollow == null)
            {
                thirdPersonFollow = thirdPersonCamera.gameObject.AddComponent<CinemachineFollow>();
            }
            
            // Set third person camera to follow player
            thirdPersonCamera.Target.TrackingTarget = playerTransform;
            
            // Initial camera angle
            currentCameraYaw = playerTransform.eulerAngles.y;
            currentCameraPitch = 20f; // Slight downward angle
        }
    }

    private void CreateIsometricCamera()
    {
        GameObject camObj = new GameObject("IsometricCamera");
        isometricCamera = camObj.AddComponent<CinemachineCamera>();
        isometricCamera.Priority.Value = 10;
        
        Debug.Log("[CameraSwitcher] Created isometric camera automatically");
    }

    private void CreateThirdPersonCamera()
    {
        GameObject camObj = new GameObject("ThirdPersonCamera");
        thirdPersonCamera = camObj.AddComponent<CinemachineCamera>();
        thirdPersonCamera.Priority.Value = 10;
        
        Debug.Log("[CameraSwitcher] Created third-person camera automatically");
    }

    private Vector3 CalculateIsometricOffset()
    {
        // Calculate offset for isometric view
        float angleRad = isometricAngle * Mathf.Deg2Rad;
        
        Vector3 offset = new Vector3(
            0,
            isometricHeight,
            -isometricDistance * Mathf.Cos(angleRad)
        );
        
        return offset;
    }

    #endregion

    #region Input Handling

    private void HandleKeyboardInput()
    {
        // Check for keyboard input
        if (Keyboard.current != null && Keyboard.current[switchKey].wasPressedThisFrame)
        {
            TrySwitchPerspective();
        }
    }

    private void OnSwitchCamera(InputAction.CallbackContext context)
    {
        // Gamepad button pressed
        TrySwitchPerspective();
    }

    private void TrySwitchPerspective()
    {
        // Check cooldown
        if (Time.time < lastSwitchTime + switchCooldown)
            return;

        // Toggle perspective
        CameraPerspective newPerspective = currentPerspective == CameraPerspective.Isometric
            ? CameraPerspective.ThirdPerson
            : CameraPerspective.Isometric;

        SetPerspective(newPerspective, true);
        lastSwitchTime = Time.time;
    }

    #endregion

    #region Perspective Switching

    /// <summary>
    /// Set the camera perspective
    /// </summary>
    public void SetPerspective(CameraPerspective perspective, bool animated = true)
    {
        currentPerspective = perspective;

        switch (perspective)
        {
            case CameraPerspective.Isometric:
                ActivateIsometricCamera(animated);
                break;
            
            case CameraPerspective.ThirdPerson:
                ActivateThirdPersonCamera(animated);
                break;
        }

        if (showDebugInfo)
        {
            Debug.Log($"[CameraSwitcher] Switched to {perspective} perspective");
        }
    }

    private void ActivateIsometricCamera(bool animated)
    {
        if (isometricCamera == null || thirdPersonCamera == null)
            return;

        // Set priorities
        isometricCamera.Priority.Value = 20;
        thirdPersonCamera.Priority.Value = 10;

        // Blend time is handled automatically by Cinemachine in Unity 6
        // The cameras will blend based on their priority change
    }

    private void ActivateThirdPersonCamera(bool animated)
    {
        if (isometricCamera == null || thirdPersonCamera == null)
            return;

        // Set priorities
        thirdPersonCamera.Priority.Value = 20;
        isometricCamera.Priority.Value = 10;

        // Initialize camera angle to player's current rotation
        currentCameraYaw = playerTransform.eulerAngles.y;

        // Blend time is handled automatically by Cinemachine in Unity 6
        // The cameras will blend based on their priority change
    }

    #endregion

    #region Third Person Camera Update

    private void UpdateThirdPersonCamera()
    {
        if (thirdPersonCamera == null || playerTransform == null)
            return;

        // Handle manual camera rotation
        if (enableManualCameraRotation)
        {
            HandleCameraRotation();
        }
        else
        {
            // Auto-follow player rotation
            currentCameraYaw = Mathf.LerpAngle(currentCameraYaw, playerTransform.eulerAngles.y, 
                Time.deltaTime * cameraRotationSpeed);
        }

        // Apply camera position and rotation
        UpdateThirdPersonTransform();
    }

    private void HandleCameraRotation()
    {
        Vector2 lookInput = Vector2.zero;

        // Get input from mouse or gamepad
        if (Mouse.current != null && Mouse.current.rightButton.isPressed)
        {
            // Mouse look (right mouse button)
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            lookInput = mouseDelta * mouseSensitivity * 0.1f;
        }
        else if (lookAction != null && lookAction.enabled)
        {
            // Gamepad right stick
            lookInput = lookAction.ReadValue<Vector2>() * gamepadSensitivity * Time.deltaTime;
        }

        // Apply rotation
        if (lookInput.sqrMagnitude > 0.01f)
        {
            currentCameraYaw += lookInput.x;
            currentCameraPitch -= lookInput.y;
            
            // Clamp pitch
            currentCameraPitch = Mathf.Clamp(currentCameraPitch, -30f, 60f);
        }
        else if (!Mouse.current.rightButton.isPressed)
        {
            // Auto-follow player when not manually controlling
            currentCameraYaw = Mathf.LerpAngle(currentCameraYaw, playerTransform.eulerAngles.y,
                Time.deltaTime * cameraRotationSpeed * 0.5f);
        }
    }

    private void UpdateThirdPersonTransform()
    {
        // Manual follow offset calculation for third person
        if (thirdPersonFollow != null)
        {
            Quaternion rotation = Quaternion.Euler(currentCameraPitch, currentCameraYaw, 0);
            Vector3 offset = rotation * new Vector3(0, thirdPersonHeight, -thirdPersonDistance);
            
            thirdPersonFollow.FollowOffset = offset;
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// Get the current camera perspective
    /// </summary>
    public CameraPerspective GetCurrentPerspective()
    {
        return currentPerspective;
    }

    /// <summary>
    /// Check if in isometric view
    /// </summary>
    public bool IsIsometric()
    {
        return currentPerspective == CameraPerspective.Isometric;
    }

    /// <summary>
    /// Check if in third person view
    /// </summary>
    public bool IsThirdPerson()
    {
        return currentPerspective == CameraPerspective.ThirdPerson;
    }

    /// <summary>
    /// Switch to isometric view
    /// </summary>
    public void SwitchToIsometric()
    {
        SetPerspective(CameraPerspective.Isometric, true);
    }

    /// <summary>
    /// Switch to third person view
    /// </summary>
    public void SwitchToThirdPerson()
    {
        SetPerspective(CameraPerspective.ThirdPerson, true);
    }

    /// <summary>
    /// Toggle between perspectives
    /// </summary>
    public void TogglePerspective()
    {
        TrySwitchPerspective();
    }

    /// <summary>
    /// Set third person camera distance
    /// </summary>
    public void SetThirdPersonDistance(float distance)
    {
        thirdPersonDistance = Mathf.Max(2f, distance);
    }

    /// <summary>
    /// Set isometric camera distance
    /// </summary>
    public void SetIsometricDistance(float distance)
    {
        isometricDistance = Mathf.Max(5f, distance);
        
        if (isometricFollow != null)
        {
            isometricFollow.FollowOffset = CalculateIsometricOffset();
        }
    }

    #endregion

    #region Debug

    private void OnGUI()
    {
        if (!showDebugInfo)
            return;

        GUIStyle style = new GUIStyle(GUI.skin.box)
        {
            fontSize = 14,
            alignment = TextAnchor.UpperLeft
        };
        style.normal.textColor = Color.white;

        string debugInfo = "=== CAMERA PERSPECTIVE ===\n";
        debugInfo += $"Current: {currentPerspective}\n";
        debugInfo += $"Switch Key: {switchKey}\n";
        
        if (currentPerspective == CameraPerspective.ThirdPerson)
        {
            debugInfo += $"Yaw: {currentCameraYaw:F1}°\n";
            debugInfo += $"Pitch: {currentCameraPitch:F1}°\n";
            debugInfo += $"Distance: {thirdPersonDistance:F1}m\n";
        }
        else
        {
            debugInfo += $"Angle: {isometricAngle}°\n";
            debugInfo += $"Height: {isometricHeight:F1}m\n";
        }

        GUI.Box(new Rect(10, 380, 280, 140), debugInfo, style);
    }

    private void OnDrawGizmosSelected()
    {
        if (playerTransform == null)
            return;

        // Draw camera positions
        if (currentPerspective == CameraPerspective.Isometric)
        {
            // Draw isometric camera position
            Gizmos.color = Color.cyan;
            Vector3 isoPos = playerTransform.position + CalculateIsometricOffset();
            Gizmos.DrawWireSphere(isoPos, 0.5f);
            Gizmos.DrawLine(playerTransform.position, isoPos);
        }
        else
        {
            // Draw third person camera arc
            Gizmos.color = Color.magenta;
            Quaternion rotation = Quaternion.Euler(currentCameraPitch, currentCameraYaw, 0);
            Vector3 offset = rotation * new Vector3(0, thirdPersonHeight, -thirdPersonDistance);
            Vector3 camPos = playerTransform.position + offset;
            
            Gizmos.DrawWireSphere(camPos, 0.5f);
            Gizmos.DrawLine(playerTransform.position, camPos);
        }
    }

    private void OnValidate()
    {
        thirdPersonDistance = Mathf.Max(2f, thirdPersonDistance);
        thirdPersonHeight = Mathf.Max(0f, thirdPersonHeight);
        isometricDistance = Mathf.Max(5f, isometricDistance);
        isometricHeight = Mathf.Max(5f, isometricHeight);
        isometricAngle = Mathf.Clamp(isometricAngle, 0f, 89f);
        switchCooldown = Mathf.Max(0.1f, switchCooldown);
        blendTime = Mathf.Max(0f, blendTime);
        cameraRotationSpeed = Mathf.Max(1f, cameraRotationSpeed);
        mouseSensitivity = Mathf.Max(0.1f, mouseSensitivity);
        gamepadSensitivity = Mathf.Max(10f, gamepadSensitivity);
    }

    #endregion
}
