using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f; // Degrees per second

    [Header("Dodge Settings")]
    [SerializeField] private bool enableDodge = true;
    [SerializeField] private float dodgeDistance = 5f;
    [SerializeField] private float dodgeDuration = 0.4f;
    [SerializeField] private float dodgeCooldown = 1.0f;
    [SerializeField] private AnimationCurve dodgeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool invulnerableDuringDodge = true;

    [Header("Aiming Settings")]
    [SerializeField] private Transform aimTarget;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float gamepadAimSensitivity = 5f;
    [SerializeField] private float gamepadAimRange = 8f; // How far the aim extends from player

    [Header("Rotation Behavior")]
    [SerializeField] private bool alwaysRotateTowardsAim = true; // NEW: Control rotation behavior
    [SerializeField] private bool rotateTowardsMovementWhenNoAim = false; // Fallback behavior
    [SerializeField] private float minAimDistance = 0.1f; // Minimum distance to consider aim valid

    [Header("Components")]
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private CinemachinePositionComposer positionComposer;
    [SerializeField] private Animator animator;
    [SerializeField] private Health playerHealth;

    [Header("Camera Tracking")]
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float cameraLerpSpeed = 5f;
    [SerializeField] private float maxCameraDistance = 10f;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 0, -2f);

    [Header("Input Detection")]
    [SerializeField] private float inputSwitchDelay = 0.1f;

    [Header("Animation Settings")]
    [SerializeField] private float animationSmoothTime = 0.1f;
    [SerializeField] private bool useAnimationEvents = false;

    // Input variables
    private Vector2 movementInput;
    private Vector2 aimInput;
    private Vector2 mousePosition;
    private Vector2 lastMousePosition;

    // Input detection
    private bool isUsingGamepadAim;
    private bool isUsingMouse;
    private float lastGamepadAimTime;
    private float lastMouseMoveTime;

    // Dodge state
    private bool isDodging;
    private float dodgeTimer;
    private float lastDodgeTime = -Mathf.Infinity;
    private Vector3 dodgeDirection;

    // Animation variables
    private float currentAnimationSpeed;
    private bool wasMovingLastFrame;

    // Animation parameter hashes
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int DodgeHash = Animator.StringToHash("Dodge");

    // Camera reference
    private Camera mainCamera;

    // Public properties
    public Transform AimTarget => aimTarget;
    public bool IsDodging => isDodging;

    private void Awake()
    {
        InitializeComponents();
        SetupRigidbody();
        SetupAnimator();
        CreateCameraTarget();
        InitializeAimTarget();
    }

    private void InitializeComponents()
    {
        if (playerRigidbody == null)
            playerRigidbody = GetComponent<Rigidbody>();
        if (animator == null)
            animator = GetComponent<Animator>();
        if (playerHealth == null)
            playerHealth = GetComponent<Health>();

        mainCamera = Camera.main;
    }

    private void SetupRigidbody()
    {
        if (playerRigidbody == null) return;

        playerRigidbody.constraints = RigidbodyConstraints.FreezeRotationX |
                                     RigidbodyConstraints.FreezeRotationZ;

        // Set interpolation for smoother movement
        playerRigidbody.interpolation = RigidbodyInterpolation.Interpolate;

        // Ensure the Rigidbody doesn't interfere with animations
        playerRigidbody.useGravity = true;
        playerRigidbody.isKinematic = false;

        // Set appropriate drag values to prevent sliding
        playerRigidbody.linearDamping = 5f;
        playerRigidbody.angularDamping = 5f;

        // Ensure mass is reasonable (too low can cause instability)
        if (playerRigidbody.mass < 1f)
            playerRigidbody.mass = 1f;
    }

    private void SetupAnimator()
    {
        if (animator == null) return;

        // Critical: Disable root motion to prevent conflicts with Rigidbody
        animator.applyRootMotion = false;

        // Set the Animator to not update position/rotation automatically
        animator.updateMode = AnimatorUpdateMode.Normal;

        // Ensure the Animator Controller is properly set up
        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning("PlayerMovement: No Animator Controller assigned to player!");
        }
    }

    private void CreateCameraTarget()
    {
        if (cameraTarget == null)
        {
            GameObject target = new GameObject("CameraTarget");
            cameraTarget = target.transform;
            cameraTarget.position = transform.position + cameraOffset;
        }
    }

    private void InitializeAimTarget()
    {
        // Create aim target if it doesn't exist
        if (aimTarget == null)
        {
            GameObject aimObj = new GameObject("AimTarget");
            aimTarget = aimObj.transform;
            aimTarget.SetParent(transform);
            aimTarget.localPosition = new Vector3(0, 0, mouseSensitivity);
            Debug.Log("[PlayerMovement] Created AimTarget automatically");
        }
        else
        {
            // Just set initial position if aim target already exists
            aimTarget.position = transform.position + transform.forward * mouseSensitivity;
        }
    }

    private void FixedUpdate()
    {
        if (isDodging)
        {
            HandleDodgeMovement();
        }
        else
        {
            HandleMovement();
            HandleAiming();
            HandleRotation(); // NEW: Separate rotation handling
        }
        
        UpdateCameraTarget();
    }

    private void Update()
    {
        DetectInputMethod();
        UpdateAnimations();
    }

    #region Input Methods

    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        aimInput = context.ReadValue<Vector2>();

        // Detect gamepad aiming
        if (aimInput.sqrMagnitude > 0.01f)
        {
            lastGamepadAimTime = Time.time;
        }
    }

    public void OnMousePosition(InputAction.CallbackContext context)
    {
        mousePosition = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// New Input System action for dodge
    /// Add this to your Input Actions asset:
    /// - Action Name: "Dodge"
    /// - Action Type: Button
    /// - Binding: Space (Keyboard), Button East/B (Gamepad)
    /// </summary>
    public void OnDodge(InputAction.CallbackContext context)
    {
        if (context.performed && enableDodge && !isDodging)
        {
            TryPerformDodge();
        }
    }

    #endregion

    #region Input Detection

    private void DetectInputMethod()
    {
        // Check for mouse movement
        if (Vector2.Distance(mousePosition, lastMousePosition) > 1f)
        {
            lastMouseMoveTime = Time.time;
            lastMousePosition = mousePosition;
        }

        // Determine which input method is currently active
        bool gamepadActive = (Time.time - lastGamepadAimTime) < inputSwitchDelay;
        bool mouseActive = (Time.time - lastMouseMoveTime) < inputSwitchDelay;

        if (gamepadActive && aimInput.sqrMagnitude > 0.01f)
        {
            isUsingGamepadAim = true;
            isUsingMouse = false;
        }
        else if (mouseActive)
        {
            isUsingGamepadAim = false;
            isUsingMouse = true;
        }
        // If neither is active recently, default to mouse for aiming
        else if (!gamepadActive && !mouseActive)
        {
            isUsingMouse = true;
            isUsingGamepadAim = false;
        }
    }

    #endregion

    #region Movement
    private void HandleMovement()
    {
        // Check if player should be able to move (not dead, not stunned, etc.)
        if (!CanMove())
        {
            // Stop movement but keep physics active
            playerRigidbody.linearVelocity = new Vector3(0, playerRigidbody.linearVelocity.y, 0);
            return;
        }

        // Handle movement (works with both WASD and gamepad automatically)
        Vector2 normalizedInput = movementInput.normalized;

        // Convert input to world space movement (accounting for isometric camera)
        Vector3 worldMovement = GetWorldSpaceMovement(normalizedInput);

        // Apply movement to rigidbody with smoother transitions
        Vector3 targetVelocity = worldMovement * moveSpeed;

        // Preserve Y velocity (gravity)
        targetVelocity.y = playerRigidbody.linearVelocity.y;

        // Apply velocity smoothly
        playerRigidbody.linearVelocity = Vector3.Lerp(
            playerRigidbody.linearVelocity,
            targetVelocity,
            Time.fixedDeltaTime * 10f
        );

        // Note: Rotation is now handled separately in HandleRotation()
    }

    private bool CanMove()
    {
        // Check if player is dead
        Health playerHealth = GetComponent<Health>();
        if (playerHealth != null && playerHealth.IsDead)
            return false;

        // Check if player is performing an action that should prevent movement
        // (You can add more conditions here as needed)

        return true;
    }

    private Vector3 GetWorldSpaceMovement(Vector2 input)
    {
        // For isometric camera, we need to adjust input based on camera orientation
        if (mainCamera != null)
        {
            // Get camera forward and right directions, but keep them on the ground plane
            Vector3 cameraForward = mainCamera.transform.forward;
            Vector3 cameraRight = mainCamera.transform.right;

            // Flatten to ground plane
            cameraForward.y = 0;
            cameraRight.y = 0;

            cameraForward.Normalize();
            cameraRight.Normalize();

            // Calculate movement direction
            return cameraForward * input.y + cameraRight * input.x;
        }

        // Fallback to basic movement
        return new Vector3(input.x, 0, input.y);
    }

    #endregion

    #region Dodge System

    /// <summary>
    /// Attempt to perform a dodge
    /// </summary>
    private void TryPerformDodge()
    {
        // Check cooldown
        float cooldownRemaining = (lastDodgeTime + dodgeCooldown) - Time.time;
        if (cooldownRemaining > 0)
        {
            return;
        }

        // Determine dodge direction
        if (movementInput.magnitude > 0.1f)
        {
            // Dodge in the direction we're moving
            dodgeDirection = GetWorldSpaceMovement(movementInput.normalized);
        }
        else
        {
            // Dodge forward if not moving
            dodgeDirection = transform.forward;
        }

        StartDodge();
    }

    /// <summary>
    /// Start the dodge sequence
    /// </summary>
    private void StartDodge()
    {
        isDodging = true;
        dodgeTimer = 0f;
        lastDodgeTime = Time.time;

        // Trigger dodge animation
        if (animator != null)
        {
            animator.SetTrigger(DodgeHash);
        }

        // Make player invulnerable if enabled
        if (invulnerableDuringDodge && playerHealth != null)
        {
            // You could implement: playerHealth.SetInvulnerable(true);
        }

        Debug.Log($"[PlayerMovement] Dodge started! Direction: {dodgeDirection}");
    }

    /// <summary>
    /// Handle dodge movement during dodge
    /// </summary>
    private void HandleDodgeMovement()
    {
        dodgeTimer += Time.fixedDeltaTime;
        float normalizedTime = dodgeTimer / dodgeDuration;

        if (normalizedTime >= 1f)
        {
            EndDodge();
            return;
        }

        // Use animation curve for dodge movement
        float curveValue = dodgeCurve.Evaluate(normalizedTime);
        float dodgeSpeed = (dodgeDistance / dodgeDuration) * curveValue;

        Vector3 dodgeVelocity = dodgeDirection * dodgeSpeed;
        dodgeVelocity.y = playerRigidbody.linearVelocity.y;

        playerRigidbody.linearVelocity = dodgeVelocity;
    }

    /// <summary>
    /// End the dodge sequence
    /// </summary>
    private void EndDodge()
    {
        isDodging = false;
        dodgeTimer = 0f;

        // Remove invulnerability
        if (invulnerableDuringDodge && playerHealth != null)
        {
            // You could implement: playerHealth.SetInvulnerable(false);
        }

        Debug.Log("[PlayerMovement] Dodge ended!");
    }

    /// <summary>
    /// Get remaining dodge cooldown time
    /// </summary>
    public float GetDodgeCooldownRemaining()
    {
        return Mathf.Max(0, (lastDodgeTime + dodgeCooldown) - Time.time);
    }

    #endregion

    #region Rotation

    /// <summary>
    /// NEW: Centralized rotation handling
    /// </summary>
    private void HandleRotation()
    {
        if (alwaysRotateTowardsAim && aimTarget != null)
        {
            RotateTowardsAim();
        }
        else if (rotateTowardsMovementWhenNoAim && movementInput.magnitude > 0.1f)
        {
            Vector3 worldMovement = GetWorldSpaceMovement(movementInput.normalized);
            RotateTowardsMovement(worldMovement);
        }
    }

    /// <summary>
    /// NEW: Rotate player to face the aim target
    /// </summary>
    private void RotateTowardsAim()
    {
        if (aimTarget == null) return;

        Vector3 aimDirection = aimTarget.position - transform.position;
        aimDirection.y = 0; // Keep rotation on horizontal plane only

        // Check if aim target is far enough to be valid
        if (aimDirection.sqrMagnitude > minAimDistance * minAimDistance)
        {
            Quaternion targetRotation = Quaternion.LookRotation(aimDirection.normalized);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );
        }
    }

    /// <summary>
    /// Rotate player to face movement direction (fallback behavior)
    /// </summary>
    private void RotateTowardsMovement(Vector3 movementDirection)
    {
        if (movementDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );
        }
    }

    #endregion

    #region Aiming
    private void HandleAiming()
    {
        if (aimTarget == null) return;

        if (isUsingGamepadAim)
        {
            HandleGamepadAiming();
        }
        else if (isUsingMouse)
        {
            HandleMouseAiming();
        }
    }

    private void HandleGamepadAiming()
    {
        if (aimInput.sqrMagnitude > 0.01f)
        {
            // Convert gamepad input to world space direction
            Vector3 aimDirection = GetWorldSpaceMovement(aimInput).normalized;

            // Set aim target position
            aimTarget.position = transform.position + aimDirection * gamepadAimRange;

            // Note: Player rotation is now handled in HandleRotation()
        }
        else
        {
            // No gamepad aim input, keep aim target in front of player
            aimTarget.position = transform.position + transform.forward * gamepadAimRange;
        }
    }

    private void HandleMouseAiming()
    {
        if (mainCamera == null) return;

        // Cast ray from camera through mouse position
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        // Raycast against ground plane or objects
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            aimTarget.position = hitInfo.point;
        }
        else
        {
            // If no hit, project onto a ground plane
            Plane groundPlane = new Plane(Vector3.up, transform.position);
            if (groundPlane.Raycast(ray, out float distance))
            {
                aimTarget.position = ray.GetPoint(distance);
            }
        }

        // Note: Player rotation is now handled in HandleRotation()
    }
    #endregion

    #region Camera
    private void UpdateCameraTarget()
    {
        if (cameraTarget == null || aimTarget == null) return;

        // Calculate midpoint between player and aim target
        Vector3 playerPos = transform.position;
        Vector3 aimPos = aimTarget.position;
        Vector3 midpoint = (playerPos + aimPos) * 0.5f + cameraOffset;

        // Clamp the midpoint to prevent camera from going too far
        Vector3 directionFromPlayer = midpoint - playerPos;
        if (directionFromPlayer.magnitude > maxCameraDistance)
        {
            midpoint = playerPos + directionFromPlayer.normalized * maxCameraDistance + cameraOffset;
        }

        // Smoothly move camera target to midpoint
        cameraTarget.position = Vector3.Lerp(
            cameraTarget.position,
            midpoint,
            cameraLerpSpeed * Time.fixedDeltaTime
        );

        // Update Cinemachine composer offset if available
        if (positionComposer != null)
        {
            Vector3 aimDirection = (aimPos - playerPos).normalized;
            positionComposer.TargetOffset = aimDirection * (mouseSensitivity * 0.3f);
        }
    }
    #endregion

    #region Animation
    private void UpdateAnimations()
    {
        if (animator == null) return;

        // Calculate movement speed for animations
        bool isMoving = movementInput.magnitude > 0.1f && !isDodging;
        float targetSpeed = isMoving ? movementInput.magnitude : 0f;

        // Smooth the animation speed changes
        currentAnimationSpeed = Mathf.Lerp(currentAnimationSpeed, targetSpeed, Time.deltaTime / animationSmoothTime);

        // Set animation parameters
        animator.SetBool(IsMovingHash, isMoving);
        animator.SetFloat(SpeedHash, currentAnimationSpeed);

        // Optional: Set movement direction for blend trees
        /*if (movementInput.magnitude > 0.1f)
        {
            animator.SetFloat("InputX", movementInput.x);
            animator.SetFloat("InputY", movementInput.y);
        }
        else
        {
            // Smoothly reduce input values to zero
            animator.SetFloat("InputX", Mathf.Lerp(animator.GetFloat("InputX"), 0, Time.deltaTime * 5f));
            animator.SetFloat("InputY", Mathf.Lerp(animator.GetFloat("InputY"), 0, Time.deltaTime * 5f));
        }*/

        // Track movement state changes
        wasMovingLastFrame = isMoving;
    }

    /// <summary>
    /// Call this method to trigger specific animations
    /// </summary>
    /// <param name="triggerName">Name of the animation trigger</param>
    public void TriggerAnimation(string triggerName)
    {
        if (animator != null)
        {
            animator.SetTrigger(triggerName);
        }
    }

    /// <summary>
    /// Sets an animation boolean parameter
    /// </summary>
    /// <param name="paramName">Parameter name</param>
    /// <param name="value">Boolean value</param>
    public void SetAnimationBool(string paramName, bool value)
    {
        if (animator != null)
        {
            animator.SetBool(paramName, value);
        }
    }
    #endregion

    #region Public Methods for External Access

    /// <summary>
    /// Get the current aim direction (used by PlayerAttack)
    /// </summary>
    /// <returns>Normalized aim direction vector</returns>
    public Vector3 GetAimDirection()
    {
        if (aimTarget == null) return transform.forward;

        Vector3 aimDirection = aimTarget.position - transform.position;
        aimDirection.y = 0;
        return aimDirection.normalized;
    }

    /// <summary>
    /// Get the current aim position
    /// </summary>
    /// <returns>World position of aim target</returns>
    public Vector3 GetAimPosition()
    {
        return aimTarget != null ? aimTarget.position : transform.position + transform.forward * 5f;
    }

    #endregion

    #region Debug
    private void OnDrawGizmosSelected()
    {
        // Draw aim direction
        if (aimTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, aimTarget.position);
            Gizmos.DrawWireSphere(aimTarget.position, 0.5f);
        }

        // Draw camera range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, maxCameraDistance);

        // Draw rotation direction
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.forward * 3f);

        // Draw dodge direction when dodging
        if (Application.isPlaying && isDodging)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, dodgeDirection * dodgeDistance);
        }
    }

    private void OnValidate()
    {
        // Ensure reasonable values in the inspector
        moveSpeed = Mathf.Max(0.1f, moveSpeed);
        rotationSpeed = Mathf.Max(1f, rotationSpeed);
        animationSmoothTime = Mathf.Max(0.01f, animationSmoothTime);
        minAimDistance = Mathf.Max(0.01f, minAimDistance);
        dodgeDistance = Mathf.Max(0.1f, dodgeDistance);
        dodgeDuration = Mathf.Max(0.1f, dodgeDuration);
        dodgeCooldown = Mathf.Max(0f, dodgeCooldown);
    }
    #endregion
}