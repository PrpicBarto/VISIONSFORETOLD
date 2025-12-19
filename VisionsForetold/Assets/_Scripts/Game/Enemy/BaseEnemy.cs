using UnityEngine;
using UnityEngine.AI;

//base klasa - svi bad guys inheritaju ovo

public abstract class BaseEnemy : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected Transform player;
    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] protected Health health;
    [SerializeField] protected Animator animator;

    [Header("Base Stats")] 
    [SerializeField] protected float detectionRange = 10f;
    [SerializeField] protected float moveSpeed = 3f;

    [Header("Animation Settings")]
    [SerializeField] protected float animationSmoothTime = 0.1f;
    [SerializeField] protected bool useAnimations = true;

    [Header("Audio Settings")]
    [SerializeField] protected AudioClip attackSound;
    [SerializeField] protected AudioClip hurtSound;
    [SerializeField] protected AudioClip deathSound;
    [SerializeField] protected bool enterCombatOnDetection = true;

    [Header("Ragdoll Settings")]
    [SerializeField] protected bool useRagdoll = true;
    [SerializeField] protected float ragdollDelay = 0f;
    [SerializeField] protected float ragdollForce = 300f;
    [SerializeField] protected float ragdollDuration = 10f;
    [SerializeField] protected bool autoDetectRagdoll = true;

    // Animation parameter hashes (shared by all enemies)
    protected static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    protected static readonly int AttackHash = Animator.StringToHash("Attack");
    protected static readonly int SpeedHash = Animator.StringToHash("Speed");
    protected static readonly int DeathHash = Animator.StringToHash("Death");
    protected static readonly int SpawnHash = Animator.StringToHash("Spawn");
    protected static readonly int HealHash = Animator.StringToHash("Heal");

    protected bool isDead = false;
    protected float currentAnimationSpeed = 0f;
    protected bool hasEnteredCombat = false;
    
    // Ragdoll components
    protected Rigidbody[] ragdollRigidbodies;
    protected Collider[] ragdollColliders;
    protected CharacterJoint[] ragdollJoints;
    protected bool ragdollInitialized = false;

    protected virtual void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (health == null) health = GetComponent<Health>();
        if (animator == null) animator = GetComponent<Animator>();

        // Set NavMeshAgent speed and ensure it's enabled
        if (agent != null)
        {
            agent.enabled = true; // Explicitly enable
            agent.speed = moveSpeed;
            Debug.Log($"[{name}] NavMeshAgent initialized - Speed: {agent.speed}, Enabled: {agent.enabled}");
        }
        else
        {
            Debug.LogError($"[{name}] NavMeshAgent component is missing!");
        }

        FindPlayer();

        if (health != null)
        {
            health.OnDeath.AddListener(OnDeath);
            health.OnDamageTaken.AddListener(OnDamageTaken);
        }

        // Initialize ragdoll
        if (useRagdoll && autoDetectRagdoll)
        {
            InitializeRagdoll();
        }
    }

    protected virtual void FindPlayer()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    protected virtual void Update()
    {
        if (isDead || player == null || (health != null && health.IsDead))
        {
            if (player == null)
                Debug.LogWarning($"[{name}] Player reference is NULL!");
            return;
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= detectionRange)
        {
            // Enter combat when player is detected
            if (!hasEnteredCombat && enterCombatOnDetection)
            {
                Debug.Log($"[{name}] Player detected at distance {distanceToPlayer:F2}!");
                OnPlayerDetected();
                hasEnteredCombat = true;
            }
            
            UpdateBehavior(distanceToPlayer);
        }
        else if (hasEnteredCombat)
        {
            // Player lost
            OnPlayerLost();
            hasEnteredCombat = false;
        }

        // Update animations every frame
        if (useAnimations)
        {
            UpdateAnimations();
        }
    }
    
    protected abstract void UpdateBehavior(float distanceToPlayer);
    
    #region Combat Detection
    
    /// <summary>
    /// Called when player is detected (enters detection range)
    /// Override in child classes for custom behavior
    /// </summary>
    protected virtual void OnPlayerDetected()
    {
        // Enter combat music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.EnterCombat();
        }
    }

    /// <summary>
    /// Called when player is lost (exits detection range)
    /// Override in child classes for custom behavior
    /// </summary>
    protected virtual void OnPlayerLost()
    {
        // Check if this was the last enemy tracking player
        CheckCombatExit();
    }

    /// <summary>
    /// Checks if combat should end (no more enemies tracking player)
    /// </summary>
    protected virtual void CheckCombatExit()
    {
        BaseEnemy[] allEnemies = FindObjectsByType<BaseEnemy>(FindObjectsSortMode.None);
        
        foreach (var enemy in allEnemies)
        {
            if (enemy != this && !enemy.isDead && enemy.hasEnteredCombat)
            {
                return; // Other enemies still in combat
            }
        }
        
        // No other enemies in combat - exit
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ExitCombat();
        }
    }

    /// <summary>
    /// Called when enemy takes damage
    /// </summary>
    protected virtual void OnDamageTaken(int damage)
    {
        // Play hurt sound
        if (hurtSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(hurtSound);
        }

        // Enter combat if not already
        if (!hasEnteredCombat && enterCombatOnDetection)
        {
            OnPlayerDetected();
            hasEnteredCombat = true;
        }
    }

    #endregion

    #region Ragdoll System

    /// <summary>
    /// Initialize ragdoll components (finds all ragdoll rigidbodies and colliders)
    /// </summary>
    protected virtual void InitializeRagdoll()
    {
        // Get all rigidbodies in children (these are ragdoll bones)
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();
        ragdollJoints = GetComponentsInChildren<CharacterJoint>();

        if (ragdollRigidbodies.Length == 0)
        {
            Debug.LogWarning($"[{name}] No ragdoll rigidbodies found! Make sure to set up ragdoll on this character.");
            useRagdoll = false;
            return;
        }

        // Disable ragdoll initially
        SetRagdollState(false);
        ragdollInitialized = true;

        Debug.Log($"[{name}] Ragdoll initialized with {ragdollRigidbodies.Length} rigidbodies");
    }

    /// <summary>
    /// Enable or disable ragdoll physics
    /// </summary>
    protected virtual void SetRagdollState(bool enabled)
    {
        if (ragdollRigidbodies == null || ragdollRigidbodies.Length == 0)
        {
            return;
        }

        // Get the main collider on the root GameObject
        Collider mainCollider = GetComponent<Collider>();

        // Enable/disable all ragdoll rigidbodies
        foreach (var rb in ragdollRigidbodies)
        {
            if (rb != null)
            {
                rb.isKinematic = !enabled;
                rb.detectCollisions = enabled;
            }
        }

        // Enable/disable ragdoll colliders
        foreach (var col in ragdollColliders)
        {
            if (col != null)
            {
                // Check if this is the main collider (on root GameObject)
                if (col == mainCollider)
                {
                    // Disable main collider when ragdoll is active (opposite behavior)
                    col.enabled = !enabled;
                }
                else
                {
                    // Enable ragdoll bone colliders when ragdoll is active
                    col.enabled = enabled;
                }
            }
        }

        // Disable animator when ragdoll is active
        if (animator != null)
        {
            animator.enabled = !enabled;
        }
    }

    /// <summary>
    /// Activate ragdoll on death
    /// </summary>
    protected virtual void ActivateRagdoll()
    {
        if (!useRagdoll || !ragdollInitialized)
        {
            return;
        }

        // Disable NavMeshAgent
        if (agent != null)
        {
            agent.enabled = false;
        }

        // Enable ragdoll
        SetRagdollState(true);

        // Apply force to ragdoll (optional - simulates impact)
        ApplyRagdollForce();

        // Schedule cleanup
        if (ragdollDuration > 0)
        {
            Destroy(gameObject, ragdollDuration);
        }

        Debug.Log($"[{name}] Ragdoll activated");
    }

    /// <summary>
    /// Apply force to ragdoll for impact effect
    /// </summary>
    protected virtual void ApplyRagdollForce()
    {
        if (ragdollForce <= 0 || player == null)
        {
            return;
        }

        // Calculate direction from player to enemy
        Vector3 forceDirection = (transform.position - player.position).normalized;
        forceDirection.y = 0.5f; // Add upward component

        // Apply force to all ragdoll rigidbodies
        foreach (var rb in ragdollRigidbodies)
        {
            if (rb != null)
            {
                rb.AddForce(forceDirection * ragdollForce, ForceMode.Impulse);
            }
        }
    }

    /// <summary>
    /// Apply force to ragdoll from specific direction (e.g., from projectile impact)
    /// </summary>
    public virtual void ApplyRagdollForceFromDirection(Vector3 direction, float force)
    {
        if (!useRagdoll || !ragdollInitialized || ragdollRigidbodies == null)
        {
            return;
        }

        direction.y = 0.5f; // Add upward component

        foreach (var rb in ragdollRigidbodies)
        {
            if (rb != null)
            {
                rb.AddForce(direction.normalized * force, ForceMode.Impulse);
            }
        }
    }

    #endregion

    #region Animation

    /// <summary>
    /// Updates animation parameters based on enemy movement
    /// Called automatically every frame if useAnimations is true
    /// </summary>
    protected virtual void UpdateAnimations()
    {
        if (animator == null || !useAnimations) return;

        // Check if AnimatorController is assigned
        if (animator.runtimeAnimatorController == null)
        {
            return;
        }

        // Calculate movement speed
        bool isMoving = false;
        float targetSpeed = 0f;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            // Check if agent is moving
            isMoving = agent.velocity.magnitude > 0.1f;
            
            if (isMoving)
            {
                // Normalize speed (0-1 range based on max speed)
                targetSpeed = agent.velocity.magnitude / agent.speed;
            }
        }

        // Smooth speed transition
        currentAnimationSpeed = Mathf.Lerp(
            currentAnimationSpeed, 
            targetSpeed, 
            Time.deltaTime / animationSmoothTime
        );

        // Update animator parameters
        animator.SetBool(IsMovingHash, isMoving);
        animator.SetFloat(SpeedHash, currentAnimationSpeed);
    }

    /// <summary>
    /// Triggers the attack animation
    /// Call this from child classes when attacking
    /// </summary>
    protected virtual void TriggerAttackAnimation()
    {
        if (animator != null && useAnimations)
        {
            animator.ResetTrigger(AttackHash);
            animator.SetTrigger(AttackHash);
        }

        // Play attack sound
        if (attackSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(attackSound);
        }
    }

    /// <summary>
    /// Triggers the death animation
    /// Called automatically when enemy dies
    /// </summary>
    protected virtual void TriggerDeathAnimation()
    {
        if (animator != null && useAnimations && !useRagdoll)
        {
            animator.ResetTrigger(DeathHash);
            animator.SetTrigger(DeathHash);
        }

        // Play death sound
        if (deathSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(deathSound);
        }
    }

    /// <summary>
    /// Triggers the spawn animation (for Lich summoning)
    /// Call this from child classes when spawning units
    /// </summary>
    protected virtual void TriggerSpawnAnimation()
    {
        if (animator != null && useAnimations)
        {
            animator.ResetTrigger(SpawnHash);
            animator.SetTrigger(SpawnHash);
        }
    }

    /// <summary>
    /// Triggers the heal animation (for Revenant healing)
    /// Call this from child classes when healing allies
    /// </summary>
    protected virtual void TriggerHealAnimation()
    {
        if (animator != null && useAnimations)
        {
            animator.ResetTrigger(HealHash);
            animator.SetTrigger(HealHash);
        }
    }

    #endregion
    
    protected virtual void OnDeath()
    {
        isDead = true;
        
        // Play death sound
        if (deathSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(deathSound);
        }

        // Check if combat should end
        CheckCombatExit();

        // Activate ragdoll or play death animation
        if (useRagdoll && ragdollInitialized)
        {
            if (ragdollDelay > 0)
            {
                // Play death animation first, then activate ragdoll
                TriggerDeathAnimation();
                Invoke(nameof(ActivateRagdoll), ragdollDelay);
            }
            else
            {
                // Immediate ragdoll
                ActivateRagdoll();
            }
        }
        else
        {
            // No ragdoll - use death animation
            TriggerDeathAnimation();
            
            // Disable agent
            if (agent != null) agent.enabled = false;
            
            // Destroy after animation
            if (ragdollDuration > 0)
            {
                Destroy(gameObject, ragdollDuration);
            }
        }
    }

    protected void LookAtPlayer()
    {
        Vector3 direction = (player.position - transform.position);
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
