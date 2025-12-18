using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerAttack : MonoBehaviour
{
    public enum AttackMode
    {
        Melee,
        Ranged,
        SpellWielding
    }

    public enum SpellType
    {
        Fireball,
        Lightning,
        IceBlast,
        Heal
    }

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2.0f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private AttackMode currentAttackMode = AttackMode.Melee;
    [SerializeField] private TMP_Text attackModeText;

    [Header("Melee Attack Settings")]
    [Tooltip("Angle of the melee attack cone in degrees")]
    [SerializeField] private float meleeAttackAngle = 90f;
    
    [Tooltip("Use sphere overlap for detection (false = use cone check)")]
    [SerializeField] private bool useSimpleSphereDetection = false;
    
    [Tooltip("Maximum number of enemies hit per melee attack")]
    [SerializeField] private int maxMeleeTargets = 5;

    [Header("Melee Combo Settings")]
    [SerializeField] private int comboCount = 3; // Number of hits in combo
    [SerializeField] private float comboWindow = 1.5f; // Time window to continue combo
    [SerializeField] private float comboResetDelay = 0.5f; // Delay before combo can restart
    [SerializeField] private float finalHitDamageMultiplier = 2.5f; // Damage multiplier for final hit
    [SerializeField] private TMP_Text comboText; // UI text to show combo progress

    private int currentComboStep = 0; // 0 = no combo, 1 = first hit, 2 = second hit, 3 = third hit
    private float lastComboHitTime = -Mathf.Infinity;
    private bool comboResetInProgress = false;

    [Header("Projectile Settings")]
    [SerializeField] private GameObject arrowProjectilePrefab;
    [SerializeField] private GameObject fireballProjectilePrefab;
    [SerializeField] private GameObject iceBlastProjectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private float projectileLifetime = 5f;
    [SerializeField] private float arrowFireDelay = 0.5f; // Delay before arrow fires (sync with animation)
    [SerializeField] private float spellCastDelay = 0.3f; // Delay before spell fires

    [Header("Aiming Settings")]
    [SerializeField] private bool useAimTarget = true; // Whether to use aim target or player forward
    [SerializeField] private float aimHeightOffset = 1.0f; // Height adjustment for projectile spawn
    [SerializeField] private LayerMask aimingLayerMask = -1; // What layers to consider for aiming

    [Header("Ranged Camera Zoom Settings")]
    [SerializeField] private bool enableRangedZoom = true; // Enable camera zoom in ranged mode
    [SerializeField] private float zoomedFOV = 45f; // Field of view when aiming (lower = more zoom)
    [SerializeField] private float normalFOV = 60f; // Normal field of view
    [SerializeField] private float zoomSpeed = 8f; // How fast camera zooms in/out
    [SerializeField] private float zoomSmoothTime = 0.15f; // Smoothing for zoom transition

    private float targetFOV;
    private float currentFOVVelocity; // For SmoothDamp
    private bool wasInRangedMode = false;

    [Header("Spell Settings")]
    [SerializeField] private SpellType currentSpell = SpellType.Fireball;
    [SerializeField] private TMP_Text currentSpellText;
    [SerializeField] private float spellCastRange = 15.0f;
    [SerializeField] private Transform spellCastPoint; // Where spells are cast from

    [Header("Spell Cooldowns")]
    [SerializeField] private float fireballCooldown = 2.0f;
    [SerializeField] private float lightningCooldown = 3.0f;
    [SerializeField] private float iceBlastCooldown = 2.5f;
    [SerializeField] private float healCooldown = 5.0f;

    [Header("Mode Switch Settings")]
    [SerializeField] private float modeSwitchCooldown = 0.2f; // Reduced for scroll wheel responsiveness
    [SerializeField] private float spellSwitchCooldown = 0.3f;
    [SerializeField] private float scrollSensitivity = 0.1f; // Minimum scroll delta to register

    private float lastModeSwitchTime = -Mathf.Infinity;
    private float lastSpellSwitchTime = -Mathf.Infinity;

    private float lastAttackTime = -Mathf.Infinity;
    private float lastFireballTime = -Mathf.Infinity;
    private float lastLightningTime = -Mathf.Infinity;
    private float lastIceBlastTime = -Mathf.Infinity;
    private float lastHealTime = -Mathf.Infinity;

    // Input System
    private PlayerInput playerInput;
    private InputAction attackAction;
    private InputAction scrollWheelAction;
    private InputAction nextSpellAction;
    private InputAction previousSpellAction;
    private InputAction gamePadModeSwitchAction; // Gamepad alternative for mode switching

    // Aim target reference (from PlayerMovement)
    private PlayerMovement playerMovement;
    private Transform aimTarget;

    // Camera reference for better aiming
    private Camera mainCamera;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerMovement = GetComponent<PlayerMovement>();
        mainCamera = Camera.main;

        // Find input actions
        attackAction = playerInput.actions.FindAction("Attack");
        scrollWheelAction = playerInput.actions.FindAction("ScrollWheel");
        nextSpellAction = playerInput.actions.FindAction("NextSpell");
        previousSpellAction = playerInput.actions.FindAction("PreviousSpell");
        gamePadModeSwitchAction = playerInput.actions.FindAction("GamepadModeSwitch");

        // Set default projectile spawn point if not assigned
        if (projectileSpawnPoint == null)
        {
            projectileSpawnPoint = transform;
        }

        // Set default spell cast point if not assigned
        if (spellCastPoint == null)
        {
            spellCastPoint = transform;
        }

        // Initialize camera zoom
        if (mainCamera != null)
        {
            normalFOV = mainCamera.fieldOfView; // Use current FOV as normal
            targetFOV = normalFOV;
        }

        // Get aim target directly from PlayerMovement
        GetAimTargetFromPlayerMovement();
    }

    /// <summary>
    /// Get the aim target from PlayerMovement component
    /// </summary>
    private void GetAimTargetFromPlayerMovement()
    {
        if (playerMovement != null)
        {
            aimTarget = playerMovement.AimTarget;
            if (aimTarget != null)
            {
                Debug.Log($"Successfully found aim target: {aimTarget.name}");
            }
            else
            {
                Debug.LogWarning("PlayerMovement.AimTarget is null! Make sure it's assigned in the inspector.");
            }
        }
        else
        {
            Debug.LogError("PlayerMovement component not found!");
        }
    }

    private void OnEnable()
    {
        // Subscribe to input events
        if (scrollWheelAction != null) scrollWheelAction.performed += OnScrollWheel;
        if (nextSpellAction != null) nextSpellAction.performed += OnNextSpell;
        if (previousSpellAction != null) previousSpellAction.performed += OnPreviousSpell;
        if (gamePadModeSwitchAction != null) gamePadModeSwitchAction.performed += OnGamepadModeSwitch;
    }

    private void OnDisable()
    {
        // Unsubscribe from input events
        if (scrollWheelAction != null) scrollWheelAction.performed -= OnScrollWheel;
        if (nextSpellAction != null) nextSpellAction.performed -= OnNextSpell;
        if (previousSpellAction != null) previousSpellAction.performed -= OnGamepadModeSwitch;
        if (gamePadModeSwitchAction != null) gamePadModeSwitchAction.performed -= OnGamepadModeSwitch;
    }

    private void Start()
    {
        UpdateAttackModeText();
        UpdateSpellText();
        UpdateComboText();
    }

    #region Input Handlers

    public void PerformAttack(InputAction.CallbackContext context)
    {
        if (!context.performed || Time.time < lastAttackTime + attackCooldown)
            return;

        switch (currentAttackMode)
        {
            case AttackMode.Melee:
                PerformMeleeAttack();
                break;
            case AttackMode.Ranged:
                PerformRangedAttack();
                break;
            case AttackMode.SpellWielding:
                CastSpell();
                break;
        }
        lastAttackTime = Time.time;
    }

    private void OnScrollWheel(InputAction.CallbackContext context)
    {
        if (Time.time < lastModeSwitchTime + modeSwitchCooldown)
            return;

        float scrollDelta = context.ReadValue<Vector2>().y;

        if (Mathf.Abs(scrollDelta) > scrollSensitivity)
        {
            int direction = scrollDelta > 0 ? 1 : -1;
            CycleAttackMode(direction);
            lastModeSwitchTime = Time.time;
            UpdateAttackModeText();
        }
    }

    private void OnGamepadModeSwitch(InputAction.CallbackContext context)
    {
        if (Time.time < lastModeSwitchTime + modeSwitchCooldown)
            return;

        CycleAttackMode(1); // Cycle forward on gamepad
        lastModeSwitchTime = Time.time;
        UpdateAttackModeText();
    }

    private void OnNextSpell(InputAction.CallbackContext context)
    {
        if (currentAttackMode != AttackMode.SpellWielding ||
            Time.time < lastSpellSwitchTime + spellSwitchCooldown)
            return;

        CycleSpell(1);
        lastSpellSwitchTime = Time.time;
        UpdateSpellText();
    }

    private void OnPreviousSpell(InputAction.CallbackContext context)
    {
        if (currentAttackMode != AttackMode.SpellWielding ||
            Time.time < lastSpellSwitchTime + spellSwitchCooldown)
            return;

        CycleSpell(-1);
        lastSpellSwitchTime = Time.time;
        UpdateSpellText();
    }

    #endregion

    #region Attack Methods

    private void PerformMeleeAttack()
    {
        // Check if combo window has expired
        if (Time.time > lastComboHitTime + comboWindow && currentComboStep > 0)
        {
            // Combo window expired, reset combo
            ResetCombo();
        }

        // Advance combo step
        currentComboStep++;
        if (currentComboStep > comboCount)
        {
            currentComboStep = 1; // Restart combo
        }

        lastComboHitTime = Time.time;

        // Trigger specific combo animation (check if player can attack)
        if (playerMovement != null && !playerMovement.IsDodging)
        {
            playerMovement.TriggerComboAttack(currentComboStep);
        }

        // Calculate damage based on combo step
        int damage = attackDamage;
        bool isFinalHit = (currentComboStep == comboCount);

        if (isFinalHit)
        {
            damage = Mathf.RoundToInt(attackDamage * finalHitDamageMultiplier);
            Debug.Log($"FINAL HIT! Combo {currentComboStep}/{comboCount} - Damage: {damage} (x{finalHitDamageMultiplier})");
        }
        else
        {
            Debug.Log($"Combo {currentComboStep}/{comboCount} - Damage: {damage}");
        }

        // Update combo UI
        UpdateComboText();

        // Perform cone attack
        int enemiesHit = PerformConeAttack(damage, isFinalHit);

        if (enemiesHit == 0)
        {
            Debug.Log("Melee attack missed - no targets in cone");
        }
        else
        {
            Debug.Log($"Melee attack hit {enemiesHit} enem{(enemiesHit == 1 ? "y" : "ies")}!");
        }

        // Schedule combo reset if this was the final hit
        if (isFinalHit)
        {
            Invoke(nameof(ResetCombo), comboResetDelay);
            comboResetInProgress = true;
        }
    }

    /// <summary>
    /// Perform a cone-shaped melee attack in front of the player
    /// </summary>
    /// <param name="damage">Damage to deal to each enemy</param>
    /// <param name="isCritical">Is this a critical/final hit?</param>
    /// <returns>True if position is in cone</returns>
    private int PerformConeAttack(int damage, bool isCritical)
    {
        Vector3 attackOrigin = transform.position + Vector3.up * 0.5f;
        Vector3 attackDirection = transform.forward;
        int enemiesHit = 0;

        // Find all colliders in range
        Collider[] hitColliders = Physics.OverlapSphere(attackOrigin, attackRange, aimingLayerMask);

        // Track unique enemies (in case they have multiple colliders)
        HashSet<GameObject> hitEnemies = new HashSet<GameObject>();

        foreach (Collider col in hitColliders)
        {
            // Skip if it's the player
            if (col.gameObject == gameObject) continue;

            // Skip if we've already hit this enemy
            GameObject enemyRoot = col.transform.root.gameObject;
            if (hitEnemies.Contains(enemyRoot)) continue;

            // Check if target is in the attack cone
            if (!IsInAttackCone(attackOrigin, attackDirection, col.transform.position))
                continue;

            // Check if max targets reached
            if (enemiesHit >= maxMeleeTargets)
                break;

            // Try to damage the target
            Health targetHealth = col.GetComponent<Health>();
            if (targetHealth == null)
            {
                // Try root object if collider doesn't have Health
                targetHealth = enemyRoot.GetComponent<Health>();
            }

            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
                hitEnemies.Add(enemyRoot);
                enemiesHit++;

                // Show damage number
                if (DamageNumberManager.Instance != null)
                {
                    DamageNumberManager.Instance.ShowDamage(col.bounds.center + Vector3.up, damage);
                }

                // Visual feedback for critical hit
                if (isCritical)
                {
                    Debug.Log($"<color=red>CRITICAL HIT on {col.gameObject.name}!</color>");
                }
            }
        }

        return enemiesHit;
    }

    /// <summary>
    /// Check if a position is within the attack cone
    /// </summary>
    /// <param name="origin">Attack origin point</param>
    /// <param name="direction">Attack direction</param>
    /// <param name="targetPosition">Position to check</param>
    /// <returns>True if position is in cone</returns>
    private bool IsInAttackCone(Vector3 origin, Vector3 direction, Vector3 targetPosition)
    {
        // If using simple sphere detection, skip cone check
        if (useSimpleSphereDetection)
            return true;

        // Calculate direction to target
        Vector3 toTarget = targetPosition - origin;
        toTarget.y = 0; // Ignore vertical difference
        
        if (toTarget == Vector3.zero)
            return true; // Target is at origin

        // Calculate angle between attack direction and target
        float angle = Vector3.Angle(direction, toTarget.normalized);

        // Check if within cone angle
        return angle <= meleeAttackAngle * 0.5f;
    }

    private void ResetCombo()
    {
        currentComboStep = 0;
        comboResetInProgress = false;
        
        // Reset combo in animation system
        if (playerMovement != null)
        {
            playerMovement.ResetCombo();
        }
        
        UpdateComboText();
        Debug.Log("Combo reset");
    }

    private void PerformRangedAttack()
    {
        // Trigger bow attack animation (check if player can attack)
        if (playerMovement != null && !playerMovement.IsDodging)
        {
            playerMovement.TriggerAttackBow();
        }

        if (arrowProjectilePrefab == null)
        {
            Debug.LogWarning("Arrow projectile prefab not assigned!");
            return;
        }

        // Delay arrow firing to sync with animation
        StartCoroutine(FireArrowDelayed(arrowFireDelay));
    }

    /// <summary>
    /// Fires arrow after a delay to sync with bow animation
    /// </summary>
    private System.Collections.IEnumerator FireArrowDelayed(float delay)
    {
        // Store the aim direction at the moment of the attack
        Vector3 shootDirection = GetShootDirection();
        
        // Wait for animation to reach firing point
        yield return new WaitForSeconds(delay);
        
        // Fire the arrow
        FireProjectile(arrowProjectilePrefab, shootDirection, projectileSpeed, ProjectileDamage.ProjectileType.Arrow);
        Debug.Log($"Player shot an arrow after {delay}s delay!");
    }

    private void CastSpell()
    {
        if (!IsSpellReady(currentSpell))
        {
            Debug.Log($"{currentSpell} is still on cooldown!");
            return;
        }

        switch (currentSpell)
        {
            case SpellType.Fireball:
                CastFireball();
                lastFireballTime = Time.time;
                break;
            case SpellType.Lightning:
                CastLightning();
                lastLightningTime = Time.time;
                break;
            case SpellType.IceBlast:
                CastIceBlast();
                lastIceBlastTime = Time.time;
                break;
            case SpellType.Heal:
                CastHeal();
                lastHealTime = Time.time;
                break;
        }

        Debug.Log($"Player cast {currentSpell}!");
    }

    #endregion

    #region Projectile System

    private Vector3 GetShootDirection()
    {
        if (useAimTarget && aimTarget != null)
        {
            // Calculate direction to aim target with height adjustment
            Vector3 spawnPosition = GetProjectileSpawnPosition();
            Vector3 targetPosition = aimTarget.position;

            // Adjust target position to be at a reasonable height
            targetPosition.y = spawnPosition.y;

            Vector3 direction = (targetPosition - spawnPosition).normalized;

            Debug.DrawRay(spawnPosition, direction * 10f, Color.red, 0.5f);
            return direction;
        }
        else
        {
            // Use player's forward direction as fallback
            Debug.DrawRay(GetProjectileSpawnPosition(), transform.forward * 10f, Color.blue, 0.5f);
            return transform.forward;
        }
    }

    private Vector3 GetProjectileSpawnPosition()
    {
        Vector3 spawnPos = projectileSpawnPoint.position;
        spawnPos.y += aimHeightOffset;
        return spawnPos;
    }

    private void FireProjectile(GameObject projectilePrefab, Vector3 direction, float speed, ProjectileDamage.ProjectileType projectileType = ProjectileDamage.ProjectileType.Arrow)
    {
        if (projectilePrefab == null) return;

        Vector3 spawnPosition = GetProjectileSpawnPosition();
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.LookRotation(direction));

        // Initialize projectile damage component
        ProjectileDamage projectileDamage = projectile.GetComponent<ProjectileDamage>();
        if (projectileDamage != null)
        {
            projectileDamage.Initialize(attackDamage, gameObject);
            projectileDamage.SetProjectileType(projectileType);
        }

        // Add velocity to projectile
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }

        // Destroy projectile after lifetime
        Destroy(projectile, projectileLifetime);
    }

    #endregion

    #region Spell Casting

    private bool IsSpellReady(SpellType spell)
    {
        return spell switch
        {
            SpellType.Fireball => Time.time >= lastFireballTime + fireballCooldown,
            SpellType.Lightning => Time.time >= lastLightningTime + lightningCooldown,
            SpellType.IceBlast => Time.time >= lastIceBlastTime + iceBlastCooldown,
            SpellType.Heal => Time.time >= lastHealTime + healCooldown,
            _ => true
        };
    }

    private void CastFireball()
    {
        // Trigger fireball spell animation (check if player can cast)
        if (playerMovement != null && !playerMovement.IsDodging)
        {
            playerMovement.TriggerSpellFireball();
        }

        if (fireballProjectilePrefab != null)
        {
            // Delay projectile firing to sync with cast animation
            StartCoroutine(FireSpellDelayed(fireballProjectilePrefab, spellCastDelay, projectileSpeed * 0.8f, ProjectileDamage.ProjectileType.Fireball));
        }
        else
        {
            // Fallback to raycast-based fireball (also delayed)
            StartCoroutine(CastFireballRaycastDelayed(spellCastDelay));
        }
        Debug.Log("Casting Fireball - dealing fire damage!");
    }

    /// <summary>
    /// Fires spell projectile after a delay to sync with animation
    /// </summary>
    private System.Collections.IEnumerator FireSpellDelayed(GameObject projectilePrefab, float delay, float speed, ProjectileDamage.ProjectileType type)
    {
        // Store aim direction at cast time
        Vector3 castDirection = GetShootDirection();
        
        // Wait for animation to reach casting point
        yield return new WaitForSeconds(delay);
        
        // Fire the projectile
        FireProjectile(projectilePrefab, castDirection, speed, type);
        Debug.Log($"Spell projectile fired after {delay}s delay!");
    }

    /// <summary>
    /// Raycast-based fireball with delay
    /// </summary>
    private System.Collections.IEnumerator CastFireballRaycastDelayed(float delay)
    {
        Vector3 castDirection = GetShootDirection();
        Vector3 castOrigin = GetProjectileSpawnPosition();
        
        yield return new WaitForSeconds(delay);

        if (Physics.Raycast(castOrigin, castDirection, out RaycastHit hit, spellCastRange, aimingLayerMask))
        {
            // Apply area damage at hit point
            DealAreaDamage(hit.point, 3f, attackDamage * 2);
        }
        else
        {
            // Cast at maximum range
            Vector3 targetPoint = castOrigin + castDirection * spellCastRange;
            DealAreaDamage(targetPoint, 3f, attackDamage * 2);
        }
    }

    private void CastLightning()
    {
        Vector3 castDirection = GetShootDirection();
        Vector3 castOrigin = GetProjectileSpawnPosition();

        if (Physics.Raycast(castOrigin, castDirection, out RaycastHit hit, spellCastRange, aimingLayerMask))
        {
            // Try new Health system first
            var targetHealth = hit.collider.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(attackDamage * 3);
            }
            else
            {
                // Fallback to old system
                var enemyHealth = hit.collider.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(attackDamage * 3);
                }
            }

            // TODO: Add lightning visual effect from cast point to hit point
            Debug.DrawLine(castOrigin, hit.point, Color.cyan, 1f);
        }
        Debug.Log("Casting Lightning - instant electrical damage!");
    }

    private void CastIceBlast()
    {
        // Trigger ice spell animation (check if player can cast)
        if (playerMovement != null && !playerMovement.IsDodging)
        {
            playerMovement.TriggerSpellIce();
        }

        if (iceBlastProjectilePrefab != null)
        {
            // Delay projectile firing to sync with cast animation
            StartCoroutine(FireSpellDelayed(iceBlastProjectilePrefab, spellCastDelay, projectileSpeed * 0.6f, ProjectileDamage.ProjectileType.IceBlast));
        }
        else
        {
            // Fallback to area effect at target location (also delayed)
            StartCoroutine(CastIceBlastAreaDelayed(spellCastDelay));
        }
        Debug.Log("Casting Ice Blast - freezing area damage!");
    }

    /// <summary>
    /// Area-based ice blast with delay
    /// </summary>
    private System.Collections.IEnumerator CastIceBlastAreaDelayed(float delay)
    {
        Vector3 castDirection = GetShootDirection();
        Vector3 castOrigin = GetProjectileSpawnPosition();
        Vector3 targetPosition = castOrigin + castDirection * spellCastRange;
        
        yield return new WaitForSeconds(delay);

        DealAreaDamage(targetPosition, 4f, attackDamage);
    }

    private void CastHeal()
    {
        // Heal the player
        var playerHealth = GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.Heal(attackDamage * 2);
        }

        // TODO: Add healing visual effects
        Debug.Log("Casting Heal - restoring health!");
    }

    private void DealAreaDamage(Vector3 center, float radius, int damage)
    {
        Collider[] enemies = Physics.OverlapSphere(center, radius, aimingLayerMask);
        foreach (var enemy in enemies)
        {
            // Skip if it's the player
            if (enemy.gameObject == gameObject) continue;

            // Try new Health system first
            var targetHealth = enemy.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
            }
            else
            {
                // Fallback to old system
                var enemyHealth = enemy.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage);
                }
            }
        }
    }

    #endregion

    #region Mode and Spell Cycling

    private void CycleAttackMode(int direction)
    {
        var modes = System.Enum.GetValues(typeof(AttackMode));
        int modeCount = modes.Length;
        int currentIndex = (int)currentAttackMode;
        int newIndex = (currentIndex + direction + modeCount) % modeCount;
        currentAttackMode = (AttackMode)modes.GetValue(newIndex);

        // Update camera zoom when entering/leaving ranged mode
        UpdateCameraZoom();

        Debug.Log($"Switched to {currentAttackMode} mode");
    }

    private void CycleSpell(int direction)
    {
        var spells = System.Enum.GetValues(typeof(SpellType));
        int spellCount = spells.Length;
        int currentIndex = (int)currentSpell;
        int newIndex = (currentIndex + direction + spellCount) % spellCount;
        currentSpell = (SpellType)spells.GetValue(newIndex);

        Debug.Log($"Selected spell: {currentSpell}");
    }

    #endregion

    #region Camera Zoom

    private void UpdateCameraZoom()
    {
        if (!enableRangedZoom || mainCamera == null) return;

        // Set target FOV based on current attack mode
        if (currentAttackMode == AttackMode.Ranged)
        {
            targetFOV = zoomedFOV;
        }
        else
        {
            targetFOV = normalFOV;
        }
    }

    private void ApplyCameraZoom()
    {
        if (!enableRangedZoom || mainCamera == null) return;

        // Smoothly interpolate to target FOV
        mainCamera.fieldOfView = Mathf.SmoothDamp(
            mainCamera.fieldOfView,
            targetFOV,
            ref currentFOVVelocity,
            zoomSmoothTime,
            zoomSpeed * 100f // Max speed
        );
    }

    #endregion

    #region UI Updates

    private void UpdateAttackModeText()
    {
        if (attackModeText != null)
        {
            attackModeText.text = $"Mode: {currentAttackMode}";
        }
    }

    private void UpdateSpellText()
    {
        if (currentSpellText != null)
        {
            if (currentAttackMode == AttackMode.SpellWielding)
            {
                float cooldownRemaining = GetSpellCooldownRemaining(currentSpell);
                string cooldownText = cooldownRemaining > 0 ? $" ({cooldownRemaining:F1}s)" : " (Ready)";
                currentSpellText.text = $"Spell: {currentSpell}{cooldownText}";
            }
            else
            {
                currentSpellText.text = "";
            }
        }
    }

    private void UpdateComboText()
    {
        if (comboText != null)
        {
            if (currentAttackMode == AttackMode.Melee && currentComboStep > 0)
            {
                string comboDisplay = "";
                for (int i = 1; i <= comboCount; i++)
                {
                    if (i <= currentComboStep)
                    {
                        comboDisplay += i == comboCount ? "? " : "? "; // Star for final hit
                    }
                    else
                    {
                        comboDisplay += "? ";
                    }
                }
                
                comboText.text = $"Combo: {comboDisplay}({currentComboStep}/{comboCount})";
                
                // Add visual indicator for final hit ready
                if (currentComboStep == comboCount - 1)
                {
                    comboText.text += " <color=yellow>[FINAL HIT READY!]</color>";
                }
            }
            else
            {
                comboText.text = "";
            }
        }
    }

    private float GetSpellCooldownRemaining(SpellType spell)
    {
        return spell switch
        {
            SpellType.Fireball => Mathf.Max(0, fireballCooldown - (Time.time - lastFireballTime)),
            SpellType.Lightning => Mathf.Max(0, lightningCooldown - (Time.time - lastLightningTime)),
            SpellType.IceBlast => Mathf.Max(0, iceBlastCooldown - (Time.time - lastIceBlastTime)),
            SpellType.Heal => Mathf.Max(0, healCooldown - (Time.time - lastHealTime)),
            _ => 0
        };
    }

    #endregion

    #region Debug

    private void OnDrawGizmosSelected()
    {
        // Draw attack range for melee (cone visualization)
        if (currentAttackMode == AttackMode.Melee || !Application.isPlaying)
        {
            Vector3 origin = transform.position + Vector3.up * 0.5f;
            Vector3 forward = transform.forward;
            
            // Draw attack range sphere
            Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
            Gizmos.DrawWireSphere(origin, attackRange);
            
            // Draw cone visualization
            if (!useSimpleSphereDetection)
            {
                // Calculate cone edges
                float halfAngle = meleeAttackAngle * 0.5f;
                Vector3 leftBoundary = Quaternion.Euler(0, -halfAngle, 0) * forward * attackRange;
                Vector3 rightBoundary = Quaternion.Euler(0, halfAngle, 0) * forward * attackRange;
                
                // Draw cone lines
                Gizmos.color = Color.red;
                Gizmos.DrawLine(origin, origin + leftBoundary);
                Gizmos.DrawLine(origin, origin + rightBoundary);
                Gizmos.DrawLine(origin, origin + forward * attackRange);
                
                // Draw cone arc
                Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
                int segments = 20;
                Vector3 previousPoint = origin + leftBoundary;
                
                for (int i = 1; i <= segments; i++)
                {
                    float angle = -halfAngle + (meleeAttackAngle * i / segments);
                    Vector3 point = origin + Quaternion.Euler(0, angle, 0) * forward * attackRange;
                    Gizmos.DrawLine(previousPoint, point);
                    previousPoint = point;
                }
            }
        }

        // Draw shooting direction for ranged
        if (Application.isPlaying && currentAttackMode == AttackMode.Ranged)
        {
            Vector3 shootDir = GetShootDirection();
            Vector3 spawnPos = GetProjectileSpawnPosition();

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(spawnPos, shootDir * 10f);

            // Draw aim target if available
            if (aimTarget != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(aimTarget.position, 0.5f);
                Gizmos.DrawLine(spawnPos, aimTarget.position);
            }
        }
    }

    #endregion

    private void Update()
    {
        // Update spell text to show cooldown timers
        if (currentAttackMode == AttackMode.SpellWielding)
        {
            UpdateSpellText();
        }

        // Update combo text in real-time
        if (currentAttackMode == AttackMode.Melee)
        {
            // Check if combo window expired
            if (currentComboStep > 0 && Time.time > lastComboHitTime + comboWindow && !comboResetInProgress)
            {
                ResetCombo();
            }
            
            UpdateComboText();
        }

        // Apply camera zoom smoothly
        ApplyCameraZoom();

        // Detect mode changes to update zoom
        if (currentAttackMode == AttackMode.Ranged && !wasInRangedMode)
        {
            UpdateCameraZoom();
            wasInRangedMode = true;
        }
        else if (currentAttackMode != AttackMode.Ranged && wasInRangedMode)
        {
            UpdateCameraZoom();
            wasInRangedMode = false;
        }

        // Try to get aim target if we don't have it
        if (aimTarget == null && playerMovement != null)
        {
            GetAimTargetFromPlayerMovement();
        }
    }
}