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
        
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<Health>();
                playerXP = player.GetComponent<PlayerXP>();
            }
        }
        
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.AddListener(UpdateHealthBar);
            UpdateHealthBar(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }
        
        if (playerXP != null)
        {
            playerXP.OnXPChanged.AddListener(UpdateXPBar);
            playerXP.OnLevelUp.AddListener(UpdateLevel);
            UpdateXPBar(playerXP.CurrentXP, playerXP.XPToNextLevel);
            UpdateLevel(playerXP.Level);
        }
        
        if (xpBarFill != null)
        {
            xpBarFill.color = xpBarColor;
        }
    }

    private void Update()
    {
        if (smoothTransition)
        {
            if (healthBarFill != null)
            {
                healthBarFill.fillAmount = Mathf.Lerp(
                    healthBarFill.fillAmount, 
                    targetHealthFill, 
                    Time.deltaTime * smoothSpeed
                );
            }
            
            if (xpBarFill != null)
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
        float healthPercent = (float)currentHealth / maxHealth;
        targetHealthFill = healthPercent;
        
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