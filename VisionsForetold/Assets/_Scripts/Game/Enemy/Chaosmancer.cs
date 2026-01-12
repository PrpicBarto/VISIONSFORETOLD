using System;
using System.Collections;
using UnityEngine;

public class Chaosmancer : MonoBehaviour
{
    [Header("Boss stats")] [SerializeField]
    private Health health;

    [SerializeField] private int maxHealth = 500;

    [Header("Proximity Activation")]
    [Tooltip("Enable/disable proximity-based activation")]
    [SerializeField] private bool useProximityActivation = true;
    
    [Tooltip("Distance at which boss becomes active")]
    [SerializeField] private float activationDistance = 30f;
    
    [Tooltip("Distance at which boss becomes inactive (should be > activationDistance)")]
    [SerializeField] private float deactivationDistance = 40f;
    
    [Tooltip("Check interval for proximity (in seconds, 0 = every frame)")]
    [SerializeField] private float proximityCheckInterval = 0.5f;

    [Header("Movement")] [SerializeField] private Transform player;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float minDistance = 8f;
    [SerializeField] private float maxDistance = 15f;

    [Header("Attack 1 - Tornado Projectile")] [SerializeField]
    private GameObject tornadoProjectilePrefab;

    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private int tornadoDamage = 25;
    [SerializeField] private float tornadoSpeed = 10f;
    [SerializeField] private float tornadoCooldown = 4f;

    [Header("Attack 2 - Tornado Transformation")] [SerializeField]
    private GameObject tornadoFormPrefab;

    [SerializeField] private float pullForce = 5f;
    [SerializeField] private float pullRadius = 10f;
    [SerializeField] private float transformDuration = 5f;
    [SerializeField] private int transformDamagePerTick = 10;
    [SerializeField] private float transformCooldown = 12f;

    [Header("Attack 3 - Ground Slam")] [SerializeField]
    private float slamRange = 6f;

    [SerializeField] private int slamDamage = 35;
    [SerializeField] private float knockupForce = 10f;
    [SerializeField] private GameObject slamEffectPrefab;
    [SerializeField] private float slamCooldown = 8f;

    [Header("Boss Behavior")] [SerializeField]
    private float phaseTransitionHealth = 0.5f;

    private bool isEnraged = false;

    [Header("Audio")] [SerializeField] private AudioClip tornadoSound;
    [SerializeField] private AudioClip transformSound;
    [SerializeField] private AudioClip slamSound;
    [SerializeField] private AudioClip roarSound;

    private float lastTornadoTime = -999f;
    private float lastTransformTime = -999f;
    private float lastSlamTime = -999f;
    private bool isTransformed;
    private bool isDead;
    private Rigidbody rb;
    private AudioSource audioSource;
    private GameObject tornadoFormDistance;

    private int attackCounter;
    private bool inPhase2;
    
    // Proximity activation state
    private bool isBossActive;
    private float lastProximityCheckTime;
    private Animator animator;
    private Collider[] bossColliders;

    private void Awake()
    {
        if (health == null) health = GetComponent<Health>();
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        bossColliders = GetComponentsInChildren<Collider>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        FindPlayer();
        
        // Start inactive if using proximity activation
        if (useProximityActivation)
        {
            SetBossActive(false);
            Debug.Log("[Chaosmancer] Starting inactive - will activate when player approaches");
        }
        else
        {
            isBossActive = true;
        }
    }

    private void Start()
    {
        if (health != null)  // FIXED: Changed from "== null" to "!= null"
        {
            health.SetMaxHealth(maxHealth, false);
            health.OnHealthChanged.AddListener(OnHealthChanged);
            health.OnDeath.AddListener(OnDeath);
        }
        else
        {
            Debug.LogError("[Chaosmancer] Health component is NULL! Boss will not take damage or die properly.");
        }

        if (projectileSpawnPoint == null)
        {
            projectileSpawnPoint = transform;
        }

        // Register boss for boss music system
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.RegisterBoss(transform);
            Debug.Log("[Chaosmancer] Registered as boss - boss music will play when player approaches!");
        }
        else
        {
            Debug.LogWarning("[Chaosmancer] AudioManager not found! Boss music will not play.");
        }

        PlaySound(roarSound);

        // Removed duplicate CheckProximity coroutine - proximity is handled in Update()
        // if (useProximityActivation)
        // {
        //     StartCoroutine(CheckProximity());
        // }
    }

    private void FindPlayer()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    private void Update()
    {
        if (isDead || player == null)
        {
            // Don't update if boss is dead or player not found
            return;
        }

        // Handle proximity-based activation
        if (useProximityActivation)
        {
            CheckProximityActivation();
            
            // Don't update boss logic if inactive
            if (!isBossActive)
            {
                return;
            }
        }

        if (health == null || health.isDead)
        {
            return;
        }

        // FREQUENT DEBUG LOGS (for testing) - Remove later!
        if (Time.frameCount % 60 == 0) // Every second
        {
            Debug.Log(
                $"<color=cyan>[BOSS STATUS]</color> HP: {health.CurrentHealth}/{health.MaxHealth} | Phase: {(inPhase2 ? "2" : "1")} | Enraged: {isEnraged} | Pos: {transform.position}");
        }

        if (!isTransformed)
        {
            HandleMovement();
            DecideNextAttack();
        }
    }

    private void HandleMovement()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        Vector3 moveDirection = Vector3.zero;

        if (distanceToPlayer < minDistance)
        {
            // Retreat from player
            moveDirection = (transform.position - player.position).normalized;
        }
        else if (distanceToPlayer > maxDistance)
        {
            // Approach player
            moveDirection = (player.position - transform.position).normalized;
        }

        // Move using Rigidbody (physics-based movement)
        if (moveDirection != Vector3.zero && rb != null)
        {
            Vector3 targetPosition = transform.position + moveDirection * (moveSpeed * Time.deltaTime);
            rb.MovePosition(targetPosition);
        }

        LookAtPlayer();
    }

    private void LookAtPlayer()
    {
        Vector3 direction = (player.position - transform.position);
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void DecideNextAttack()
    {
        float currentTime = Time.time;

        // FIXED: Use correct last attack times for each attack
        bool canTornado = currentTime - lastTornadoTime > tornadoCooldown;
        bool canTransform = currentTime - lastTransformTime > transformCooldown;
        bool canSlam = currentTime - lastSlamTime > slamCooldown;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (canTransform && attackCounter >= 3)
        {
            StartCoroutine(TransformAttack());
            attackCounter = 0;
        }
        
        else if (canSlam && distanceToPlayer <= slamRange)  // FIXED: Use slamRange, not slamDamage
        {
            GroundSlamAttack();
            attackCounter = 0;
        }
        
        else if (canTornado)
        {
            TornadoProjectileAttack();
            attackCounter++;
        }
    }

    private void TornadoProjectileAttack()
    {
        // FIXED: Changed from "tornadoProjectilePrefab == null" to "!= null"
        if (tornadoProjectilePrefab != null)
        {
            lastTornadoTime = Time.time;
            LookAtPlayer();

            Vector3 direction = (player.position - projectileSpawnPoint.position).normalized;
            GameObject tornado = Instantiate(tornadoProjectilePrefab, projectileSpawnPoint.position,
                Quaternion.LookRotation(direction));

            TornadoProjectile tornadoScript = tornado.GetComponent<TornadoProjectile>();
            if (tornadoScript != null)
            {
                tornadoScript.Initialize(tornadoDamage, tornadoSpeed);
            }
            else
            {
                ProjectileDamage projDamage = tornado.GetComponent<ProjectileDamage>();
                if (projDamage != null)
                {
                    projDamage.Initialize(tornadoDamage, gameObject, "Player");
                }
                
                Rigidbody tornadoRb = tornado.GetComponent<Rigidbody>();
                if (tornadoRb != null)
                {
                    tornadoRb.linearVelocity = direction * tornadoSpeed;
                }
            }
            PlaySound(tornadoSound);
            Debug.Log("Chaosmancer fired tornado!");
        }
        else
        {
            Debug.LogWarning("[Chaosmancer] Tornado projectile prefab is not assigned!");
            lastTornadoTime = Time.time; // Set cooldown anyway to prevent spam
        }
    }

    private IEnumerator TransformAttack()
    {
        isTransformed = true;
        lastTransformTime = Time.time;

        if (tornadoFormPrefab != null)
        {
            tornadoFormDistance = Instantiate(tornadoFormPrefab, transform.position, Quaternion.identity);
            tornadoFormDistance.transform.SetParent(transform);
        }

        MeshRenderer renderer = GetComponentInChildren<MeshRenderer>();
        if (renderer != null) renderer.enabled = false;
        
        PlaySound(transformSound);
        Debug.Log($"Chaosmancer transformed into tornado!");
        
        float elapsedTime = 0f;
        float damageTickRate = 0.5f;
        float lastDamageTick = 0f;

        while (elapsedTime < transformDuration)
        {
            PullPlayerTowardBoss();

            if (elapsedTime - lastDamageTick >= damageTickRate)
            {
                DealTransformDamage();
                lastDamageTick = elapsedTime;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (tornadoFormDistance != null)
        {
            Destroy(tornadoFormDistance);
        }
        
        if(renderer != null) renderer.enabled = true;
        
        isTransformed = false;
        Debug.Log($"Chaosmancer transformation ended!");
    }

    private void PullPlayerTowardBoss()
    {
        if(player==null) return;
        
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= pullRadius)
        {
            Vector3 pullDir = (transform.position - player.position).normalized;
            
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.AddForce(pullDir * pullForce, ForceMode.Force);
            }
            else
            {
                player.position += pullDir * (pullForce * Time.deltaTime * 0.1f);
            }
        }
    }

    private void DealTransformDamage()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= pullRadius)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(transformDamagePerTick);

                if (DamageNumberManager.Instance != null)
                {
                    DamageNumberManager.Instance.ShowDamage(player.position + Vector3.up * 2f, transformDamagePerTick);
                }
            }
        }
    }

    private void GroundSlamAttack()
    {
        lastSlamTime = Time.time;
        
        StartCoroutine(PerformGroundSlam());
    }

    private IEnumerator PerformGroundSlam()
    {
        yield return new WaitForSeconds(0.5f);

        PlaySound(slamSound);

        if (slamEffectPrefab != null)
        {
            GameObject slamEffect = Instantiate(slamEffectPrefab, transform.position, Quaternion.identity);
            Destroy(slamEffect, 2f);
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= slamRange)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(slamDamage);

                if (DamageNumberManager.Instance != null)
                {
                    DamageNumberManager.Instance.ShowDamage(player.position + Vector3.up * 2f, slamDamage);
                }
            }
        }
        
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            playerRb.AddForce(Vector3.up * knockupForce, ForceMode.Impulse);
        }

        Debug.Log($"Chaosmancer slammed the ground! Player knocked up!");
    }

    private void OnHealthChanged(int currentHealth, int maxHealth)
    {
        float healthPercent = (float)currentHealth / maxHealth;

        if (!inPhase2 && healthPercent < phaseTransitionHealth)
        {
            EnterPhase2();
        }
    }

    private void EnterPhase2()
    {
        inPhase2 = true;
        isEnraged = true;

        tornadoCooldown *= 0.7f;
        transformCooldown *= 0.7f;
        slamCooldown *= 0.7f;

        moveSpeed *= 1.3f;

        PlaySound(roarSound);
        Debug.Log($"Chaosmancer entered Phase 2! ENRAGED!!!");
    }

    private void OnDeath()
    {
        if (isDead) return; // Prevent multiple death calls
        
        isDead = true;
        
        Debug.Log("[Chaosmancer] Boss defeated!");
        
        // Unregister boss and end boss music IMMEDIATELY
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.EndBossFight();
            AudioManager.Instance.UnregisterBoss();
            Debug.Log("[Chaosmancer] Boss music ended and unregistered!");
        }

        // Stop all coroutines safely
        StopAllCoroutines();

        // Clean up tornado form
        if (tornadoFormDistance != null)
        {
            Destroy(tornadoFormDistance);
            tornadoFormDistance = null;
        }
        
        // Disable this component to prevent further updates
        this.enabled = false;
        
        // Show "To Be Continued" screen after a short delay
        StartCoroutine(ShowToBeContinuedAfterDelay(2f));
    }
    
    /// <summary>
    /// Show the "To Be Continued" screen after boss defeat
    /// </summary>
    private IEnumerator ShowToBeContinuedAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Show "To Be Continued" screen
        if (ToBeContinuedManager.Instance != null)
        {
            ToBeContinuedManager.Instance.ShowToBeContinued();
            Debug.Log("[Chaosmancer] Showing 'To Be Continued' screen!");
        }
        else
        {
            Debug.LogError("[Chaosmancer] ToBeContinuedManager not found! Add it to the scene.");
        }
        
        // Optional: Destroy the boss GameObject after showing the screen
        // Destroy(gameObject, 1f);
    }

    private void OnDestroy()
    {
        // Cleanup boss music registration if destroyed without death callback
        if (!isDead && AudioManager.Instance != null)
        {
            AudioManager.Instance.UnregisterBoss();
            Debug.Log("[Chaosmancer] Unregistered on destroy");
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    #region Proximity Activation

    /// <summary>
    /// Check if player is within activation/deactivation distance
    /// </summary>
    private void CheckProximityActivation()
    {
        // Only check at intervals to save performance
        if (Time.time - lastProximityCheckTime < proximityCheckInterval)
        {
            return;
        }

        lastProximityCheckTime = Time.time;

        if (player == null)
        {
            FindPlayer();
            if (player == null) return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Use hysteresis to prevent rapid toggling
        if (!isBossActive && distanceToPlayer <= activationDistance)
        {
            // Player entered activation range - activate boss
            SetBossActive(true);
            Debug.Log($"[Chaosmancer] Player entered range ({distanceToPlayer:F1}m) - ACTIVATING boss!");
        }
        else if (isBossActive && distanceToPlayer >= deactivationDistance)
        {
            // Player left deactivation range - deactivate boss
            SetBossActive(false);
            Debug.Log($"[Chaosmancer] Player left range ({distanceToPlayer:F1}m) - DEACTIVATING boss!");
        }
    }

    /// <summary>
    /// Set boss active or inactive state
    /// </summary>
    private void SetBossActive(bool active)
    {
        isBossActive = active;

        // DON'T disable this script! It needs to run to check proximity and re-enable itself
        // this.enabled = active;  // ? REMOVED: This prevents Update() from running

        // Enable/disable animator
        if (animator != null)
        {
            animator.enabled = active;
        }

        // Enable/disable rigidbody
        if (rb != null)
        {
            rb.isKinematic = !active;
            
            if (!active)
            {
                // Stop all physics movement when deactivating
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        // Enable/disable colliders (optional - keep colliders active for player collision)
        // Uncomment if you want to disable collision when inactive
        /*
        if (bossColliders != null)
        {
            foreach (var col in bossColliders)
            {
                if (col != null)
                {
                    col.enabled = active;
                }
            }
        }
        */

        // Stop all coroutines when deactivating (but NOT the script itself!)
        if (!active)
        {
            // Stop attack coroutines
            StopAllCoroutines();
            
            // Clean up tornado form if active
            if (tornadoFormDistance != null)
            {
                Destroy(tornadoFormDistance);
                tornadoFormDistance = null;
                isTransformed = false;
            }
        }

        // Log state change
        string state = active ? "ACTIVE" : "INACTIVE";
        Debug.Log($"[Chaosmancer] Boss is now {state}");
    }

    /// <summary>
    /// Force activate boss (for cutscenes, etc.)
    /// </summary>
    public void ForceActivate()
    {
        if (useProximityActivation)
        {
            SetBossActive(true);
            Debug.Log("[Chaosmancer] Force activated!");
        }
    }

    /// <summary>
    /// Force deactivate boss
    /// </summary>
    public void ForceDeactivate()
    {
        if (useProximityActivation)
        {
            SetBossActive(false);
            Debug.Log("[Chaosmancer] Force deactivated!");
        }
    }

    /// <summary>
    /// Check if boss is currently active
    /// </summary>
    public bool IsActive()
    {
        return isBossActive;
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        // Draw attack ranges
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, slamRange);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pullRadius);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minDistance);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, maxDistance);
        
        // Draw proximity activation ranges
        if (useProximityActivation)
        {
            // Activation range (green)
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, activationDistance);
            
            // Deactivation range (orange)
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, deactivationDistance);
            
            // Draw line to player if in editor
            if (Application.isPlaying && player != null)
            {
                float distance = Vector3.Distance(transform.position, player.position);
                Gizmos.color = isBossActive ? Color.green : Color.red;
                Gizmos.DrawLine(transform.position, player.position);
                
                // Draw label
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(
                    transform.position + Vector3.up * 3f,
                    $"Boss: {(isBossActive ? "ACTIVE" : "INACTIVE")}\nDistance: {distance:F1}m"
                );
                #endif
            }
        }
    }
}