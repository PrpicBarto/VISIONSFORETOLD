using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class PlayerHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private PlayerXP playerXP;
    
    [Header("Health Bar")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float lowHealthThreshold = 0.3f;
    
    [Header("XP Bar")]
    [SerializeField] private Image xpBarFill;
    [SerializeField] private TMP_Text xpText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Color xpBarColor = new Color(0.3f, 0.5f, 1f); // Blue
    
    [Header("Animation")]
    [SerializeField] private bool smoothTransition = true;
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Events")] 
    
    
    private float targetHealthFill;
    private float targetXpFill;

    private void Start()
    {
        // Auto-find player if not assigned
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<Health>();
                playerXP = player.GetComponent<PlayerXP>();
                Debug.Log("[PlayerHUD] Auto-found player components");
            }
            else
            {
                Debug.LogError("[PlayerHUD] No GameObject with 'Player' tag found!");
            }
        }
        
        // Subscribe to health events
        if (playerHealth != null)
        {
            Debug.Log($"[PlayerHUD] Subscribing to Health events. Current HP: {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}");
            playerHealth.OnHealthChanged.AddListener(UpdateHealthBar);
            UpdateHealthBar(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }
        else
        {
            Debug.LogError("[PlayerHUD] playerHealth is NULL! Health bar will not update.");
        }
        
        // Subscribe to XP events
        if (playerXP != null)
        {
            Debug.Log("[PlayerHUD] Subscribing to XP events");
            playerXP.OnXPChanged.AddListener(UpdateXPBar);
            playerXP.OnLevelUp.AddListener(UpdateLevel);
            UpdateXPBar(playerXP.CurrentXP, playerXP.XPToNextLevel);
            UpdateLevel(playerXP.Level);
        }
        else
        {
            Debug.LogWarning("[PlayerHUD] playerXP is NULL! XP bar will not update.");
        }
        
        // Set XP bar color
        if (xpBarFill != null)
        {
            xpBarFill.color = xpBarColor;
        }
        
        // Verify UI elements
        if (healthBarFill == null)
            Debug.LogError("[PlayerHUD] healthBarFill is not assigned!");
        if (healthText == null)
            Debug.LogWarning("[PlayerHUD] healthText is not assigned!");
    }

    private void Update()
    {
        // Failsafe: Continuously verify health display is correct
        if (playerHealth != null && healthBarFill != null)
        {
            float actualHealthPercent = (float)playerHealth.CurrentHealth / playerHealth.MaxHealth;
            
            // If there's a significant difference between displayed and actual health, force update
            if (Mathf.Abs(targetHealthFill - actualHealthPercent) > 0.01f)
            {
                Debug.LogWarning($"[PlayerHUD] Health mismatch detected! Target: {targetHealthFill:F2}, Actual: {actualHealthPercent:F2} - Forcing update");
                UpdateHealthBar(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }
        }
        
        if (smoothTransition)
        {
            if (healthBarFill != null && !Mathf.Approximately(healthBarFill.fillAmount, targetHealthFill))
            {
                healthBarFill.fillAmount = Mathf.Lerp(
                    healthBarFill.fillAmount, 
                    targetHealthFill, 
                    Time.deltaTime * smoothSpeed
                );
            }
            
            if (xpBarFill != null && !Mathf.Approximately(xpBarFill.fillAmount, targetXpFill))
            {
                xpBarFill.fillAmount = Mathf.Lerp(
                    xpBarFill.fillAmount, 
                    targetXpFill, 
                    Time.deltaTime * smoothSpeed
                );
            }
        }
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        Debug.Log($"[PlayerHUD] UpdateHealthBar called: {currentHealth}/{maxHealth}");
        
        // Safety check
        if (maxHealth <= 0)
        {
            Debug.LogError("[PlayerHUD] maxHealth is 0 or negative!");
            return;
        }

        float healthPercent = (float)currentHealth / maxHealth;
        targetHealthFill = healthPercent;
        
        Debug.Log($"[PlayerHUD] Health percent: {healthPercent:F2}, Target fill: {targetHealthFill:F2}");
        
        if (!smoothTransition && healthBarFill != null)
        {
            healthBarFill.fillAmount = healthPercent;
        }
        
        // Update color
        if (healthBarFill != null)
        {
            healthBarFill.color = Color.Lerp(lowHealthColor, fullHealthColor, 
                healthPercent / lowHealthThreshold);
        }
        else
        {
            Debug.LogError("[PlayerHUD] healthBarFill is NULL in UpdateHealthBar!");
        }
        
        // Update text
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }

    private void UpdateXPBar(int currentXP, int xpToNextLevel)
    {
        float xpPercent = (float)currentXP / xpToNextLevel;
        targetXpFill = xpPercent;
        
        if (!smoothTransition && xpBarFill != null)
        {
            xpBarFill.fillAmount = xpPercent;
        }
        
        // Update text
        if (xpText != null)
        {
            xpText.text = $"XP: {currentXP}/{xpToNextLevel}";
        }
    }

    private void UpdateLevel(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"Level {level}";
        }
    }
}