using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private bool initializeHealthOnStart = true;

    [Header("Death Settings")]
    [SerializeField] private GameObject ragdollPrefab;
    [SerializeField] private bool isPlayer = false;
    [SerializeField] private float ragdollLifetime = 10f;

    [Header("Player Death Scene Settings")]
    [SerializeField] private bool loadSceneOnPlayerDeath = true;
    [SerializeField] private string deathSceneName = "MainMenu";
    [SerializeField] private float deathSceneLoadDelay = 2f;
    [SerializeField] private bool showDeathMessage = true;
    [SerializeField] private string deathMessage = "You Died!";

    [Header("Health Regeneration")]
    [SerializeField] private bool enableHealthRegeneration = false;
    [SerializeField] private float healthRegenRate = 1f; // Health per second
    [SerializeField] private float healthRegenDelay = 3f; // Delay after taking damage
    [SerializeField] private int healthRegenThreshold = 50; // Only regen below this percentage

    [Header("Events")]
    public UnityEvent<int, int> OnHealthChanged; // (currentHealth, maxHealth)
    public UnityEvent<int> OnDamageTaken; // (damageAmount)
    public UnityEvent<int> OnHealthRestored; // (healAmount)
    public UnityEvent OnDeath;
    public UnityEvent OnFullHealth;

    // Private variables for optimization
    private float lastDamageTime = -Mathf.Infinity;
    public bool isDead = false;
    private float healthRegenTimer = 0f;

    // Properties for external access
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public float HealthPercentage => (float)currentHealth / maxHealth;
    public bool IsDead => isDead;
    public bool IsAtFullHealth => currentHealth >= maxHealth;

    #region Unity Lifecycle

    private void Awake()
    {
        // Initialize health if not set in inspector
        if (currentHealth <= 0 && initializeHealthOnStart)
        {
            currentHealth = maxHealth;
        }
    }

    private void Start()
    {
        // Invoke initial health state
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (IsAtFullHealth)
        {
            OnFullHealth?.Invoke();
        }
    }

    private void Update()
    {
        if (enableHealthRegeneration && !isDead)
        {
            HandleHealthRegeneration();
        }
    }

    #endregion

    #region Damage and Healing Methods

    /// <summary>
    /// Deals damage to this health component (compatible with PlayerAttack)
    /// </summary>
    /// <param name="damage">Amount of damage to deal</param>
    public void DealDamage(int damage)
    {
        TakeDamage(damage);
    }

    /// <summary>
    /// Takes damage - preferred method name (matches EnemyHealth pattern)
    /// </summary>
    /// <param name="damage">Amount of damage to take</param>
    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0) return;

        int previousHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - damage);
        lastDamageTime = Time.time;

        // Reset health regen timer when taking damage
        healthRegenTimer = 0f;

        // Trigger hurt animation for player
        if (isPlayer)
        {
            PlayerMovement playerMovement = GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.TriggerHurt();
            }
        }

        // Invoke events
        OnDamageTaken?.Invoke(damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");

        // Check for death
        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    /// <summary>
    /// Heals the health component (compatible with PlayerAttack heal spell)
    /// </summary>
    /// <param name="healAmount">Amount of health to restore</param>
    public void Heal(int healAmount)
    {
        AddHealth(healAmount);
    }

    /// <summary>
    /// Adds health to the current health
    /// </summary>
    /// <param name="amount">Amount of health to add</param>
    public void AddHealth(int amount)
    {
        if (isDead || amount <= 0) return;

        int previousHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);

        // Only invoke events if health actually changed
        if (currentHealth != previousHealth)
        {
            OnHealthRestored?.Invoke(amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            Debug.Log($"{gameObject.name} healed {amount} health. Health: {currentHealth}/{maxHealth}");

            // Check if at full health
            if (IsAtFullHealth)
            {
                OnFullHealth?.Invoke();
            }
        }
    }

    /// <summary>
    /// Restores health to full
    /// </summary>
    public void RestoreToFullHealth()
    {
        int healAmount = maxHealth - currentHealth;
        if (healAmount > 0)
        {
            AddHealth(healAmount);
        }
    }

    /// <summary>
    /// Sets health to a specific value
    /// </summary>
    /// <param name="newHealth">New health value</param>
    public void SetHealth(int newHealth)
    {
        SetHealth(newHealth, true);
    }

    /// <summary>
    /// Sets health to a specific value with option to check for death
    /// </summary>
    /// <param name="newHealth">New health value</param>
    /// <param name="checkDeath">Whether to check for death (false when loading from save)</param>
    public void SetHealth(int newHealth, bool checkDeath)
    {
        newHealth = Mathf.Clamp(newHealth, 0, maxHealth);

        if (newHealth != currentHealth)
        {
            int difference = newHealth - currentHealth;
            currentHealth = newHealth;

            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (difference > 0)
            {
                OnHealthRestored?.Invoke(difference);
            }
            else if (difference < 0)
            {
                OnDamageTaken?.Invoke(-difference);
            }

            if (checkDeath && currentHealth <= 0 && !isDead)
            {
                Die();
            }
            else if (IsAtFullHealth)
            {
                OnFullHealth?.Invoke();
            }
        }
    }

    #endregion

    #region Health Regeneration

    private void HandleHealthRegeneration()
    {
        // Only regenerate if below threshold and enough time has passed since last damage
        if (HealthPercentage >= (healthRegenThreshold / 100f) ||
            Time.time < lastDamageTime + healthRegenDelay)
        {
            return;
        }

        healthRegenTimer += Time.deltaTime;

        if (healthRegenTimer >= 1f / healthRegenRate)
        {
            AddHealth(1);
            healthRegenTimer = 0f;
        }
    }

    #endregion

    #region Death System

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log($"{gameObject.name} has died.");

        // Invoke death event
        OnDeath?.Invoke();

        // Handle ragdoll instantiation (legacy system - only if ragdollPrefab assigned)
        if (ragdollPrefab != null)
        {
            SpawnRagdoll();
        }

        // Handle player-specific death logic
        if (isPlayer)
        {
            HandlePlayerDeath();
        }
        // NOTE: Removed automatic Destroy for non-player entities
        // Let the enemy's own death system (BaseEnemy) handle destruction timing
        // This allows ragdoll systems to work properly
    }

    private void SpawnRagdoll()
    {
        GameObject ragdollInstance = Instantiate(ragdollPrefab, transform.position, transform.rotation);

        // Copy velocity if this object has a rigidbody
        Rigidbody originalRb = GetComponent<Rigidbody>();
        if (originalRb != null)
        {
            Rigidbody[] ragdollRigidbodies = ragdollInstance.GetComponentsInChildren<Rigidbody>();
            foreach (var rb in ragdollRigidbodies)
            {
                rb.linearVelocity = originalRb.linearVelocity;
                rb.angularVelocity = originalRb.angularVelocity;
            }
        }

        // Auto-destroy ragdoll after specified time
        if (ragdollLifetime > 0)
        {
            Destroy(ragdollInstance, ragdollLifetime);
        }
    }

    private void HandlePlayerDeath()
    {
        // Player-specific death logic
        // Disable player controls
        var playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = false;
        }

        // Disable movement
        var playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // Disable attack
        var playerAttack = GetComponent<PlayerAttack>();
        if (playerAttack != null)
        {
            playerAttack.enabled = false;
        }

        // Exit combat music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ExitCombat();
        }

        // Show death message if enabled
        if (showDeathMessage)
        {
            Debug.Log($"<color=red>{deathMessage}</color>");
        }

        // Load death scene (main menu) after delay
        if (loadSceneOnPlayerDeath)
        {
            if (deathSceneLoadDelay > 0)
            {
                // Load scene after delay (allows death animation/effects to play)
                Invoke(nameof(LoadDeathScene), deathSceneLoadDelay);
                Debug.Log($"Player died! Loading '{deathSceneName}' scene in {deathSceneLoadDelay} seconds...");
            }
            else
            {
                // Load scene immediately
                LoadDeathScene();
            }
        }
        else
        {
            Debug.Log("Player died! Scene loading is disabled. Enable 'Load Scene On Player Death' in Inspector.");
        }
    }

    /// <summary>
    /// Loads the death scene (usually main menu)
    /// </summary>
    private void LoadDeathScene()
    {
        Debug.Log($"Loading death scene: {deathSceneName}");
        
        // IMPORTANT: Clean up before loading main menu to prevent conflicts
        // Reset time scale
        Time.timeScale = 1f;
        
        // Destroy any DontDestroyOnLoad objects from gameplay
        CleanupGameplayObjects();
        
        try
        {
            SceneManager.LoadScene(deathSceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load scene '{deathSceneName}': {e.Message}");
            Debug.LogError("Make sure the scene is added to Build Settings (File ? Build Settings ? Add Open Scenes)");
        }
    }
    
    /// <summary>
    /// Cleanup gameplay objects before returning to main menu
    /// </summary>
    private void CleanupGameplayObjects()
    {
        // Destroy the player GameObject entirely
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
        
        // Find and destroy any other player-related objects that might persist
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject obj in rootObjects)
        {
            // Destroy player and player-related objects
            if (obj.CompareTag("Player") || 
                obj.name.Contains("Player") ||
                obj.GetComponent<PlayerMovement>() != null ||
                obj.GetComponent<PlayerAttack>() != null)
            {
                Destroy(obj);
            }
        }
        
        Debug.Log("[Health] Cleaned up gameplay objects before loading death scene");
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Resets health component to initial state (useful for respawning)
    /// </summary>
    public void ResetHealth()
    {
        isDead = false;
        currentHealth = maxHealth;
        lastDamageTime = -Mathf.Infinity;
        healthRegenTimer = 0f;

        // Re-enable components if they were disabled
        if (isPlayer)
        {
            var playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (playerInput != null) playerInput.enabled = true;

            var playerMovement = GetComponent<PlayerMovement>();
            if (playerMovement != null) playerMovement.enabled = true;

            var playerAttack = GetComponent<PlayerAttack>();
            if (playerAttack != null) playerAttack.enabled = true;
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnFullHealth?.Invoke();
    }

    /// <summary>
    /// Sets the maximum health (useful for upgrades)
    /// </summary>
    /// <param name="newMaxHealth">New maximum health value</param>
    /// <param name="adjustCurrentHealth">Whether to adjust current health proportionally</param>
    public void SetMaxHealth(int newMaxHealth, bool adjustCurrentHealth = false)
    {
        if (newMaxHealth <= 0) return;

        if (adjustCurrentHealth)
        {
            float healthRatio = HealthPercentage;
            maxHealth = newMaxHealth;
            currentHealth = Mathf.RoundToInt(maxHealth * healthRatio);
        }
        else
        {
            maxHealth = newMaxHealth;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    #endregion

    #region Debug and Validation

    private void OnValidate()
    {
        // Ensure health values are valid in the inspector
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        healthRegenRate = Mathf.Max(0.1f, healthRegenRate);
        healthRegenDelay = Mathf.Max(0f, healthRegenDelay);
        healthRegenThreshold = Mathf.Clamp(healthRegenThreshold, 0, 100);
    }

    #endregion
}