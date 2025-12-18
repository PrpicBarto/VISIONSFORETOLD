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

    // Animation parameter hashes (shared by all enemies)
    protected static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    protected static readonly int AttackHash = Animator.StringToHash("Attack");
    protected static readonly int SpeedHash = Animator.StringToHash("Speed");
    protected static readonly int DeathHash = Animator.StringToHash("Death");
    protected static readonly int SpawnHash = Animator.StringToHash("Spawn");
    protected static readonly int HealHash = Animator.StringToHash("Heal");

    protected bool isDead = false;
    protected float currentAnimationSpeed = 0f;

    protected virtual void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (health == null) health = GetComponent<Health>();
        if (animator == null) animator = GetComponent<Animator>();

        // Set NavMeshAgent speed
        if (agent != null)
        {
            agent.speed = moveSpeed;
        }

        FindPlayer();

        if (health != null)
        {
            health.OnDeath.AddListener(OnDeath);
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
            return;
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= detectionRange)
        {
            UpdateBehavior(distanceToPlayer);
        }

        // Update animations every frame
        if (useAnimations)
        {
            UpdateAnimations();
        }
    }
    
    protected abstract void UpdateBehavior(float distanceToPlayer);
    
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
    }

    /// <summary>
    /// Triggers the death animation
    /// Called automatically when enemy dies
    /// </summary>
    protected virtual void TriggerDeathAnimation()
    {
        if (animator != null && useAnimations)
        {
            animator.ResetTrigger(DeathHash);
            animator.SetTrigger(DeathHash);
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
    
    protected virtual void OnDeath()
    {
        isDead = true;
        
        // Trigger death animation
        TriggerDeathAnimation();
        
        if (agent != null) agent.enabled = false;
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
