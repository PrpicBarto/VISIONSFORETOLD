using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f; // Degrees per second

    [Header("Sprint Settings")]
    [SerializeField] private bool enableSprint = true;
    [SerializeField] private float sprintSpeedMultiplier = 1.8f;
    [SerializeField] private bool requireStamina = true;
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 20f; // Stamina per second while sprinting
    [SerializeField] private float staminaRegenRate = 15f; // Stamina per second when not sprinting
    [SerializeField] private float staminaRegenDelay = 1f; // Delay before stamina starts regenerating
    [SerializeField] private float minStaminaToSprint = 10f; // Minimum stamina required to start sprinting
    [SerializeField] private bool canSprintBackwards = false; // Allow sprinting while moving backwards
    [SerializeField] private bool canSprintStrafe = true; // Allow sprinting while strafing

    [Header("Dodge Settings")]
    [SerializeField] private bool enableDodge = true;
    [SerializeField] private float dodgeDistance = 5f;
    [SerializeField] private float dodgeDuration = 0.4f;
    [SerializeField] private float dodgeCooldown = 1.0f;
    [SerializeField] private AnimationCurve dodgeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool invulnerableDuringDodge = true;

    [Header("Dash Settings")]
    [SerializeField] private bool enableDash = true;
    [SerializeField] private float dashDistance = 8f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float dashCooldown = 2.0f;
    [SerializeField] private AnimationCurve dashCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private float dashStaminaCost = 25f;
    [SerializeField] private bool requireStaminaForDash = true;
    [SerializeField] private bool canDashInAir = false;
    [SerializeField] private bool invulnerableDuringDash = false;

    [Header("Aiming Settings")]
    [SerializeField] private Transform aimTarget;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float gamepadAimSensitivity = 5f;
    [SerializeField] private float gamepadAimRange = 8f; // How far the aim extends from player

    [Header("Rotation Behavior")]
    [SerializeField] private bool alwaysRotateTowardsAim = true;
    [SerializeField] private bool rotateTowardsMovementWhenNoAim = false;
    [SerializeField] private float minAimDistance = 0.1f;
    [SerializeField] private bool rotateToMovementByDefault = true; // New: Rotate to movement when not attacking
    [SerializeField] private float attackRotationDuration = 0.5f; // How long to keep aiming after attack

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
    [SerializeField] private float joystickDeadzone = 0.15f; // Deadzone for joystick input

    [Header("Animation Settings")]
    [SerializeField] private float animationSmoothTime = 0.1f;
    [SerializeField] private bool useAnimationEvents = false;

    // Input variables
    private Vector2 movementInput;
    private Vector2 aimInput;
    private Vector2 mousePosition;
    private Vector2 lastMousePosition;
    private bool sprintInput;

    // Input detection
    private bool isUsingGamepadAim;
    private bool isUsingMouse;
    private bool isUsingGamepadMovement;
    private float lastGamepadAimTime;
    private float lastMouseMoveTime;
    private float lastGamepadMovementTime;

    // Sprint state
    private bool isSprinting;
    private float currentStamina;
    private float lastStaminaUseTime;

    // Dodge state
    private bool isDodging;
    private float dodgeTimer;
    private float lastDodgeTime = -Mathf.Infinity;
    private Vector3 dodgeDirection;

    // Dash state
    private bool isDashing;
    private float dashTimer;
    private float lastDashTime = -Mathf.Infinity;
    private Vector3 dashDirection;

    // Rotation state
    private bool isAttacking;
    private float lastAttackTime = -Mathf.Infinity;

    // Animation variables
    private float currentAnimationSpeed;
    private bool wasMovingLastFrame;
    private bool isLowHealth;
    private float lastHealthPercentage = 1f;

    // Animation parameter hashes - Movement
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int DodgeHash = Animator.StringToHash("Dodge");
    private static readonly int IsSprintingHash = Animator.StringToHash("IsSprinting");
    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
    private static readonly int DirectionXHash = Animator.StringToHash("DirectionX");
    private static readonly int DirectionYHash = Animator.StringToHash("DirectionY");
    
    // Animation parameter hashes - Combat (Combo System)
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int Attack1Hash = Animator.StringToHash("Attack1");
    private static readonly int Attack2Hash = Animator.StringToHash("Attack2");
    private static readonly int Attack3Hash = Animator.StringToHash("Attack3");
    private static readonly int ComboStepHash = Animator.StringToHash("ComboStep");
    private static readonly int AttackBowHash = Animator.StringToHash("AttackBow");
    private static readonly int SpellFireballHash = Animator.StringToHash("SpellFireball");
    private static readonly int SpellIceHash = Animator.StringToHash("SpellIce");
    
    // Animation parameter hashes - States
    private static readonly int WalkHash = Animator.StringToHash("Walk");
    private static readonly int WalkHurtHash = Animator.StringToHash("WalkHurt");
    private static readonly int HurtHash = Animator.StringToHash("Hurt");
    private static readonly int IdleHash = Animator.StringToHash("Idle");
    private static readonly int DashHash = Animator.StringToHash("Dash");
    private static readonly int RunHash = Animator.StringToHash("Run");
    
    // Animation parameter hashes - Health
    private static readonly int IsLowHealthHash = Animator.StringToHash("IsLowHealth");
    private static readonly int HealthPercentageHash = Animator.StringToHash("HealthPercentage");

    // Camera reference
    private Camera mainCamera;

    // Public properties
    public Transform AimTarget => aimTarget;
    public bool IsDodging => isDodging;
    public bool IsDashing => isDashing;
    public bool IsSprinting => isSprinting;
    public bool IsAttacking => isAttacking;
    public bool IsRunning => currentAnimationSpeed > 1.2f && movementInput.magnitude > 0.1f;
    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;
    public float StaminaPercentage => maxStamina > 0 ? currentStamina / maxStamina : 0f;
    public bool IsLowHealth => isLowHealth;
    public float HealthPercentage => playerHealth != null ? playerHealth.HealthPercentage : 1f;

    private void Awake()
    {
        InitializeComponents();
        SetupRigidbody();
        SetupAnimator();
        CreateCameraTarget();
        InitializeAimTarget();
        InitializeStamina();
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

        playerRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        playerRigidbody.useGravity = true;
        playerRigidbody.isKinematic = false;
        playerRigidbody.linearDamping = 5f;
        playerRigidbody.angularDamping = 5f;

        if (playerRigidbody.mass < 1f)
            playerRigidbody.mass = 1f;
    }

    private void SetupAnimator()
    {
        if (animator == null) return;

        animator.applyRootMotion = false;
        animator.updateMode = AnimatorUpdateMode.Normal;

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
            aimTarget.position = transform.position + transform.forward * mouseSensitivity;
        }
    }

    private void InitializeStamina()
    {
        currentStamina = maxStamina;
    }

    private void FixedUpdate()
    {
        if (isDodging)
        {
            HandleDodgeMovement();
        }
        else if (isDashing)
        {
            HandleDashMovement();
        }
        else
        {
            HandleMovement();
            HandleAiming();
            HandleRotation();
        }
        
        UpdateCameraTarget();
    }

    private void Update()
    {
        DetectInputMethod();
        HandleSprint();
        UpdateStamina();
        UpdateAnimations();
    }

    #region Input Methods

    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
        
        // Apply deadzone for joystick
        if (movementInput.magnitude < joystickDeadzone)
        {
            movementInput = Vector2.zero;
        }
        else
        {
            // Normalize and remap to account for deadzone
            movementInput = movementInput.normalized * ((movementInput.magnitude - joystickDeadzone) / (1f - joystickDeadzone));
        }
        
        // Track gamepad movement
        if (movementInput.sqrMagnitude > 0.01f)
        {
            lastGamepadMovementTime = Time.time;
        }
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        aimInput = context.ReadValue<Vector2>();
        
        // Apply deadzone for joystick
        if (aimInput.magnitude < joystickDeadzone)
        {
            aimInput = Vector2.zero;
        }

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
    /// Sprint input - can be button (toggle) or hold
    /// Add this to your Input Actions asset:
    /// - Action Name: "Sprint"
    /// - Action Type: Button
    /// - Binding: Left Shift (Keyboard), Left Trigger (Gamepad)
    /// </summary>
    public void OnSprint(InputAction.CallbackContext context)
    {
        if (!enableSprint) return;

        // Hold to sprint (works for both keyboard and gamepad)
        sprintInput = context.ReadValueAsButton();
    }

    public void OnDodge(InputAction.CallbackContext context)
    {
        if (context.performed && enableDodge && !isDodging)
        {
            TryPerformDodge();
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed && enableDash && !isDashing)
        {
            TryPerformDash();
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
        bool gamepadAimActive = (Time.time - lastGamepadAimTime) < inputSwitchDelay;
        bool gamepadMoveActive = (Time.time - lastGamepadMovementTime) < inputSwitchDelay;
        bool mouseActive = (Time.time - lastMouseMoveTime) < inputSwitchDelay;

        // Update gamepad aim detection
        if (gamepadAimActive && aimInput.sqrMagnitude > 0.01f)
        {
            isUsingGamepadAim = true;
            isUsingMouse = false;
        }
        else if (mouseActive)
        {
            isUsingGamepadAim = false;
            isUsingMouse = true;
        }
        else if (!gamepadAimActive && !mouseActive)
        {
            isUsingMouse = true;
            isUsingGamepadAim = false;
        }

        // Update gamepad movement detection
        isUsingGamepadMovement = gamepadMoveActive || gamepadAimActive;
    }

    #endregion

    #region Sprint System

    private void HandleSprint()
    {
        if (!enableSprint || isDodging || !CanMove())
        {
            isSprinting = false;
            return;
        }

        bool wantsToSprint = sprintInput && movementInput.magnitude > 0.1f;

        // Check if player can sprint based on stamina
        if (requireStamina)
        {
            if (wantsToSprint)
            {
                // Can only start sprinting if we have minimum stamina
                if (!isSprinting && currentStamina < minStaminaToSprint)
                {
                    wantsToSprint = false;
                }
                // Stop sprinting if we run out of stamina
                else if (isSprinting && currentStamina <= 0)
                {
                    wantsToSprint = false;
                }
            }
        }

        // Check movement direction restrictions
        if (wantsToSprint)
        {
            Vector2 normalizedInput = movementInput.normalized;
            
            // Check if moving backwards (negative Y input)
            if (!canSprintBackwards && normalizedInput.y < -0.1f)
            {
                wantsToSprint = false;
            }
            
            // Check if strafing (significant X input with little Y)
            if (!canSprintStrafe && Mathf.Abs(normalizedInput.x) > 0.5f && Mathf.Abs(normalizedInput.y) < 0.5f)
            {
                wantsToSprint = false;
            }
        }

        isSprinting = wantsToSprint;
    }

    private void UpdateStamina()
    {
        if (!requireStamina) return;

        if (isSprinting)
        {
            // Drain stamina while sprinting
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(0, currentStamina);
            lastStaminaUseTime = Time.time;
        }
        else
        {
            // Regenerate stamina after delay
            if (Time.time - lastStaminaUseTime >= staminaRegenDelay)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(maxStamina, currentStamina);
            }
        }
    }

    /// <summary>
    /// Get the current movement speed including sprint multiplier
    /// </summary>
    private float GetCurrentMoveSpeed()
    {
        return isSprinting ? moveSpeed * sprintSpeedMultiplier : moveSpeed;
    }

    #endregion

    #region Movement
    
    private void HandleMovement()
    {
        if (!CanMove())
        {
            playerRigidbody.linearVelocity = new Vector3(0, playerRigidbody.linearVelocity.y, 0);
            return;
        }

        Vector2 normalizedInput = movementInput.normalized;
        Vector3 worldMovement = GetWorldSpaceMovement(normalizedInput);

        // Apply movement with sprint modifier
        float currentSpeed = GetCurrentMoveSpeed();
        Vector3 targetVelocity = worldMovement * currentSpeed;
        targetVelocity.y = playerRigidbody.linearVelocity.y;

        playerRigidbody.linearVelocity = Vector3.Lerp(
            playerRigidbody.linearVelocity,
            targetVelocity,
            Time.fixedDeltaTime * 10f
        );
    }

    private bool CanMove()
    {
        Health playerHealth = GetComponent<Health>();
        if (playerHealth != null && playerHealth.IsDead)
            return false;

        return true;
    }

    private Vector3 GetWorldSpaceMovement(Vector2 input)
    {
        if (mainCamera != null)
        {
            Vector3 cameraForward = mainCamera.transform.forward;
            Vector3 cameraRight = mainCamera.transform.right;

            cameraForward.y = 0;
            cameraRight.y = 0;

            cameraForward.Normalize();
            cameraRight.Normalize();

            return cameraForward * input.y + cameraRight * input.x;
        }

        return new Vector3(input.x, 0, input.y);
    }

    #endregion

    #region Dodge System

    private void TryPerformDodge()
    {
        float cooldownRemaining = (lastDodgeTime + dodgeCooldown) - Time.time;
        if (cooldownRemaining > 0)
        {
            return;
        }

        if (movementInput.magnitude > 0.1f)
        {
            dodgeDirection = GetWorldSpaceMovement(movementInput.normalized);
        }
        else
        {
            dodgeDirection = transform.forward;
        }

        StartDodge();
    }

    private void StartDodge()
    {
        isDodging = true;
        isSprinting = false; // Cancel sprint when dodging
        dodgeTimer = 0f;
        lastDodgeTime = Time.time;

        if (animator != null)
        {
            animator.SetTrigger(DodgeHash);
        }

        if (invulnerableDuringDodge && playerHealth != null)
        {
            // playerHealth.SetInvulnerable(true);
        }

        Debug.Log($"[PlayerMovement] Dodge started! Direction: {dodgeDirection}");
    }

    private void HandleDodgeMovement()
    {
        dodgeTimer += Time.fixedDeltaTime;
        float normalizedTime = dodgeTimer / dodgeDuration;

        if (normalizedTime >= 1f)
        {
            EndDodge();
            return;
        }

        float curveValue = dodgeCurve.Evaluate(normalizedTime);
        float dodgeSpeed = (dodgeDistance / dodgeDuration) * curveValue;

        Vector3 dodgeVelocity = dodgeDirection * dodgeSpeed;
        dodgeVelocity.y = playerRigidbody.linearVelocity.y;

        playerRigidbody.linearVelocity = dodgeVelocity;
    }

    private void EndDodge()
    {
        isDodging = false;
        dodgeTimer = 0f;

        if (invulnerableDuringDodge && playerHealth != null)
        {
            // playerHealth.SetInvulnerable(false);
        }

        Debug.Log("[PlayerMovement] Dodge ended!");
    }

    public float GetDodgeCooldownRemaining()
    {
        return Mathf.Max(0, (lastDodgeTime + dodgeCooldown) - Time.time);
    }

    #endregion

    #region Dash System

    private void TryPerformDash()
    {
        // Check cooldown
        float cooldownRemaining = (lastDashTime + dashCooldown) - Time.time;
        if (cooldownRemaining > 0)
        {
            Debug.Log($"[PlayerMovement] Dash on cooldown! {cooldownRemaining:F1}s remaining");
            return;
        }

        // Check if already dashing or dodging
        if (isDashing || isDodging)
        {
            return;
        }

        // Check stamina requirement
        if (requireStaminaForDash && currentStamina < dashStaminaCost)
        {
            Debug.Log("[PlayerMovement] Not enough stamina to dash!");
            return;
        }

        // Check if grounded (if required)
        if (!canDashInAir && !IsGrounded())
        {
            Debug.Log("[PlayerMovement] Cannot dash in air!");
            return;
        }

        // Determine dash direction
        if (movementInput.magnitude > 0.1f)
        {
            // Dash in movement direction
            dashDirection = GetWorldSpaceMovement(movementInput.normalized);
        }
        else
        {
            // Dash forward if no input
            dashDirection = transform.forward;
        }

        StartDash();
    }

    private void StartDash()
    {
        isDashing = true;
        isSprinting = false; // Cancel sprint when dashing
        dashTimer = 0f;
        lastDashTime = Time.time;

        // Consume stamina
        if (requireStaminaForDash)
        {
            currentStamina = Mathf.Max(0, currentStamina - dashStaminaCost);
            lastStaminaUseTime = Time.time;
        }

        // Trigger dash animation
        if (animator != null)
        {
            animator.ResetTrigger(DashHash);
            animator.SetTrigger(DashHash);
        }

        // Set invulnerability if enabled
        if (invulnerableDuringDash && playerHealth != null)
        {
            // playerHealth.SetInvulnerable(true);
        }

        Debug.Log($"[PlayerMovement] Dash started! Direction: {dashDirection}, Stamina: {currentStamina}/{maxStamina}");
    }

    private void HandleDashMovement()
    {
        dashTimer += Time.fixedDeltaTime;
        float normalizedTime = dashTimer / dashDuration;

        if (normalizedTime >= 1f)
        {
            EndDash();
            return;
        }

        // Use dash curve for speed variation (can be linear or curved)
        float curveValue = dashCurve.Evaluate(normalizedTime);
        float dashSpeed = (dashDistance / dashDuration) * curveValue;

        Vector3 dashVelocity = dashDirection * dashSpeed;
        dashVelocity.y = playerRigidbody.linearVelocity.y;

        playerRigidbody.linearVelocity = dashVelocity;
    }

    private void EndDash()
    {
        isDashing = false;
        dashTimer = 0f;

        // Remove invulnerability
        if (invulnerableDuringDash && playerHealth != null)
        {
            // playerHealth.SetInvulnerable(false);
        }

        Debug.Log("[PlayerMovement] Dash ended!");
    }

    public float GetDashCooldownRemaining()
    {
        return Mathf.Max(0, (lastDashTime + dashCooldown) - Time.time);
    }

    private bool IsGrounded()
    {
        // Simple ground check using raycast
        float rayDistance = 0.2f;
        return Physics.Raycast(transform.position, Vector3.down, rayDistance);
    }

    #endregion

    #region Rotation

    private void HandleRotation()
    {
        // With single run animation: Character always faces movement direction
        // This ensures the animation plays naturally in the direction of travel
        
        if (movementInput.magnitude > 0.1f)
        {
            // Always rotate to movement direction when moving
            Vector3 worldMovement = GetWorldSpaceMovement(movementInput.normalized);
            RotateTowardsMovement(worldMovement);
        }
        // When not moving, keep current rotation (or optionally face aim target)
    }

    private void RotateTowardsAim()
    {
        if (aimTarget == null) return;

        Vector3 aimDirection = aimTarget.position - transform.position;
        aimDirection.y = 0;

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
            Vector3 aimDirection = GetWorldSpaceMovement(aimInput).normalized;
            aimTarget.position = transform.position + aimDirection * gamepadAimRange;
        }
        else
        {
            aimTarget.position = transform.position + transform.forward * gamepadAimRange;
        }
    }

    private void HandleMouseAiming()
    {
        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            aimTarget.position = hitInfo.point;
        }
        else
        {
            Plane groundPlane = new Plane(Vector3.up, transform.position);
            if (groundPlane.Raycast(ray, out float distance))
            {
                aimTarget.position = ray.GetPoint(distance);
            }
        }
    }
    
    #endregion

    #region Camera
    
    private void UpdateCameraTarget()
    {
        if (cameraTarget == null || aimTarget == null) return;

        Vector3 playerPos = transform.position;
        Vector3 aimPos = aimTarget.position;
        Vector3 midpoint = (playerPos + aimPos) * 0.5f + cameraOffset;

        Vector3 directionFromPlayer = midpoint - playerPos;
        if (directionFromPlayer.magnitude > maxCameraDistance)
        {
            midpoint = playerPos + directionFromPlayer.normalized * maxCameraDistance + cameraOffset;
        }

        cameraTarget.position = Vector3.Lerp(
            cameraTarget.position,
            midpoint,
            cameraLerpSpeed * Time.fixedDeltaTime
        );

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

        // Check if AnimatorController is assigned
        if (animator.runtimeAnimatorController == null)
        {
            // Only log warning once instead of every frame
            if (Time.frameCount % 300 == 0) // Every ~5 seconds at 60fps
            {
                Debug.LogWarning("[PlayerMovement] No AnimatorController assigned! Please assign one to the Animator component on the Player GameObject.");
            }
            return;
        }

        // Don't update movement animations while dashing or dodging
        bool isMoving = movementInput.magnitude > 0.1f && !isDodging && !isDashing;
        
        if (isMoving)
        {
            // Calculate speed with sprint multiplier
            float targetSpeed = movementInput.magnitude;
            if (isSprinting)
            {
                targetSpeed *= sprintSpeedMultiplier;
            }

            currentAnimationSpeed = Mathf.Lerp(currentAnimationSpeed, targetSpeed, Time.deltaTime / animationSmoothTime);

            // Run animation plays whenever moving (single animation for all directions)
            bool isRunning = true;

            // Update movement animations
            animator.SetBool(IsMovingHash, true);
            animator.SetFloat(SpeedHash, currentAnimationSpeed);
            animator.SetBool(IsSprintingHash, isSprinting);
            animator.SetBool(IsRunningHash, isRunning);
            
            // Set directional parameters to 0 (not used with single animation)
            animator.SetFloat(DirectionXHash, 0f);
            animator.SetFloat(DirectionYHash, 0f);
        }
        else
        {
            // Not moving - return to idle
            currentAnimationSpeed = Mathf.Lerp(currentAnimationSpeed, 0f, Time.deltaTime / animationSmoothTime);

            animator.SetBool(IsMovingHash, false);
            animator.SetFloat(SpeedHash, 0f);
            animator.SetBool(IsSprintingHash, false);
            animator.SetBool(IsRunningHash, false);
            animator.SetFloat(DirectionXHash, 0f);
            animator.SetFloat(DirectionYHash, 0f);
        }

        // Update health-based animations
        UpdateHealthAnimations();

        wasMovingLastFrame = isMoving;
    }

    private void UpdateHealthAnimations()
    {
        if (playerHealth == null || animator == null) return;

        float healthPercentage = playerHealth.HealthPercentage;
        
        // Update health percentage for animator (useful for blend trees)
        animator.SetFloat(HealthPercentageHash, healthPercentage);

        // Check for low health state (below 30%)
        bool currentlyLowHealth = healthPercentage <= 0.3f;
        
        if (currentlyLowHealth != isLowHealth)
        {
            isLowHealth = currentlyLowHealth;
            animator.SetBool(IsLowHealthHash, isLowHealth);
            
            if (isLowHealth)
            {
                Debug.Log("[PlayerMovement] Entered low health state - switching to hurt animations");
            }
        }

        lastHealthPercentage = healthPercentage;
    }

    // Combat Animation Triggers
    
    /// <summary>
    /// Trigger melee attack animation (generic)
    /// Called from PlayerAttack script when performing melee attack
    /// </summary>
    public void TriggerAttack()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning("[PlayerMovement] No AnimatorController assigned!");
            return;
        }

        // Reset trigger first to ensure clean state
        animator.ResetTrigger(AttackHash);
        animator.SetTrigger(AttackHash);
        Debug.Log("[PlayerMovement] Triggered Attack animation");
    }

    /// <summary>
    /// Trigger specific combo attack animation
    /// Use this for 3-hit combo system with individual animations
    /// </summary>
    /// <param name="comboStep">Which combo step (1, 2, or 3)</param>
    public void TriggerComboAttack(int comboStep)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;
        
        // Mark as attacking for rotation
        SetAttackingState(true);
        
        // Update combo step parameter for blend trees/state machines
        animator.SetInteger(ComboStepHash, comboStep);
        
        // Trigger specific attack based on combo step
        switch (comboStep)
        {
            case 1:
                animator.ResetTrigger(Attack1Hash);
                animator.SetTrigger(Attack1Hash);
                Debug.Log("[PlayerMovement] Triggered Attack1 (Combo 1/3)");
                break;
            case 2:
                animator.ResetTrigger(Attack2Hash);
                animator.SetTrigger(Attack2Hash);
                Debug.Log("[PlayerMovement] Triggered Attack2 (Combo 2/3)");
                break;
            case 3:
                animator.ResetTrigger(Attack3Hash);
                animator.SetTrigger(Attack3Hash);
                Debug.Log("[PlayerMovement] Triggered Attack3 (Combo 3/3 - FINISHER)");
                break;
            default:
                // Fallback to generic attack
                animator.ResetTrigger(AttackHash);
                animator.SetTrigger(AttackHash);
                Debug.LogWarning($"[PlayerMovement] Invalid combo step {comboStep}, using generic attack");
                break;
        }
    }

    /// <summary>
    /// Reset combo state (call when combo times out or is interrupted)
    /// </summary>
    public void ResetCombo()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;
        
        animator.SetInteger(ComboStepHash, 0);
        Debug.Log("[PlayerMovement] Combo reset");
    }

    /// <summary>
    /// Trigger bow/ranged attack animation
    /// Called from PlayerAttack script when performing ranged attack
    /// </summary>
    public void TriggerAttackBow()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;

        // Mark as attacking for rotation
        SetAttackingState(true);

        // Reset trigger first to ensure clean state (fixes single-trigger issue)
        animator.ResetTrigger(AttackBowHash);
        
        // Then set the trigger
        animator.SetTrigger(AttackBowHash);
        Debug.Log("[PlayerMovement] Triggered AttackBow animation");
    }

    /// <summary>
    /// Trigger fireball spell animation
    /// Called from PlayerAttack script when casting fireball spell
    /// </summary>
    public void TriggerSpellFireball()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;

        // Mark as attacking for rotation
        SetAttackingState(true);

        animator.ResetTrigger(SpellFireballHash);
        animator.SetTrigger(SpellFireballHash);
        Debug.Log("[PlayerMovement] Triggered SpellFireball animation");
    }

    /// <summary>
    /// Trigger ice spell animation
    /// Called from PlayerAttack script when casting ice spell
    /// </summary>
    public void TriggerSpellIce()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;

        // Mark as attacking for rotation
        SetAttackingState(true);

        animator.ResetTrigger(SpellIceHash);
        animator.SetTrigger(SpellIceHash);
        Debug.Log("[PlayerMovement] Triggered SpellIce animation");
    }

    /// <summary>
    /// Trigger hurt/damage animation when player takes damage
    /// Should be called from Health script's TakeDamage method
    /// </summary>
    public void TriggerHurt()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;

        animator.ResetTrigger(HurtHash);
        animator.SetTrigger(HurtHash);
        Debug.Log("[PlayerMovement] Triggered Hurt animation");
    }

    /// <summary>
    /// Trigger dash animation (alternative to dodge roll)
    /// Can be used for faster, more aggressive movement
    /// </summary>
    public void TriggerDash()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;

        animator.ResetTrigger(DashHash);
        animator.SetTrigger(DashHash);
        Debug.Log("[PlayerMovement] Triggered Dash animation");
    }

    // Generic Animation Control Methods

    public void TriggerAnimation(string triggerName)
    {
        if (animator != null)
        {
            animator.SetTrigger(triggerName);
        }
    }

    public void SetAnimationBool(string paramName, bool value)
    {
        if (animator != null)
        {
            animator.SetBool(paramName, value);
        }
    }

    public void SetAnimationFloat(string paramName, float value)
    {
        if (animator != null)
        {
            animator.SetFloat(paramName, value);
        }
    }
    
    #endregion

    #region Public Methods for External Access

    public Vector3 GetAimDirection()
    {
        if (aimTarget == null) return transform.forward;

        Vector3 aimDirection = aimTarget.position - transform.position;
        aimDirection.y = 0;
        return aimDirection.normalized;
    }

    public Vector3 GetAimPosition()
    {
        return aimTarget != null ? aimTarget.position : transform.position + transform.forward * 5f;
    }

    /// <summary>
    /// Force sprint on/off (useful for cutscenes, abilities, etc.)
    /// </summary>
    public void SetSprintEnabled(bool enabled)
    {
        enableSprint = enabled;
        if (!enabled)
        {
            isSprinting = false;
        }
    }

    /// <summary>
    /// Add or remove stamina
    /// </summary>
    public void ModifyStamina(float amount)
    {
        currentStamina = Mathf.Clamp(currentStamina + amount, 0, maxStamina);
    }

    /// <summary>
    /// Fully restore stamina
    /// </summary>
    public void RestoreStamina()
    {
        currentStamina = maxStamina;
    }

    /// <summary>
    /// Force dash in a specific direction (useful for abilities)
    /// </summary>
    public void PerformDashInDirection(Vector3 direction)
    {
        if (!enableDash || isDashing || isDodging) return;

        dashDirection = direction.normalized;
        StartDash();
    }

    /// <summary>
    /// Cancel current dash
    /// </summary>
    public void CancelDash()
    {
        if (isDashing)
        {
            EndDash();
        }
    }

    /// <summary>
    /// Set the attacking state (for rotation control)
    /// Called automatically by attack triggers
    /// </summary>
    public void SetAttackingState(bool attacking)
    {
        isAttacking = attacking;
        if (attacking)
        {
            lastAttackTime = Time.time;
            Debug.Log("[PlayerMovement] Entered attacking state - rotating to aim target");
        }
        else
        {
            Debug.Log("[PlayerMovement] Exited attacking state - rotating to movement direction");
        }
    }

    /// <summary>
    /// Check if currently in attack rotation mode
    /// </summary>
    public bool IsInAttackRotationMode()
    {
        return isAttacking || (Time.time - lastAttackTime < attackRotationDuration);
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
        Gizmos.color = isSprinting ? Color.yellow : Color.green;
        Gizmos.DrawRay(transform.position, transform.forward * 3f);

        // Draw dodge direction when dodging
        if (Application.isPlaying && isDodging)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, dodgeDirection * dodgeDistance);
        }

        // Draw dash direction when dashing
        if (Application.isPlaying && isDashing)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, dashDirection * dashDistance);
        }
    }


    private void OnValidate()
    {
        moveSpeed = Mathf.Max(0.1f, moveSpeed);
        rotationSpeed = Mathf.Max(1f, rotationSpeed);
        animationSmoothTime = Mathf.Max(0.01f, animationSmoothTime);
        minAimDistance = Mathf.Max(0.01f, minAimDistance);
        dodgeDistance = Mathf.Max(0.1f, dodgeDistance);
        dodgeDuration = Mathf.Max(0.1f, dodgeDuration);
        dodgeCooldown = Mathf.Max(0f, dodgeCooldown);
        
        // Sprint validation
        sprintSpeedMultiplier = Mathf.Max(1f, sprintSpeedMultiplier);
        maxStamina = Mathf.Max(1f, maxStamina);
        staminaDrainRate = Mathf.Max(0f, staminaDrainRate);
        staminaRegenRate = Mathf.Max(0f, staminaRegenRate);
        staminaRegenDelay = Mathf.Max(0f, staminaRegenDelay);
        minStaminaToSprint = Mathf.Clamp(minStaminaToSprint, 0f, maxStamina);
        joystickDeadzone = Mathf.Clamp(joystickDeadzone, 0f, 0.9f);

        // Dash validation
        dashDistance = Mathf.Max(0.1f, dashDistance);
        dashDuration = Mathf.Max(0.1f, dashDuration);
        dashCooldown = Mathf.Max(0f, dashCooldown);
        dashStaminaCost = Mathf.Max(0f, dashStaminaCost);
    }
    
    #endregion
}