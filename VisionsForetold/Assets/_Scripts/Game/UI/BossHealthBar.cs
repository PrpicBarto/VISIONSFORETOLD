using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Boss health bar that's part of the boss prefab
/// Automatically finds and connects to boss's Health component
/// </summary>
[RequireComponent(typeof(Canvas))]
public class BossHealthBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private TMP_Text bossNameText;
    [SerializeField] private TMP_Text healthText;
    
    [Header("Settings")]
    [SerializeField] private string bossName = "BOSS";
    [SerializeField] private bool hideWhenFull = false;
    [SerializeField] private bool showOnlyWhenDamaged = true;
    [SerializeField] private float hideDelay = 3f;
    
    [Header("Colors")]
    [SerializeField] private Color fullHealthColor = new Color(1f, 0f, 0f); // Red
    [SerializeField] private Color lowHealthColor = new Color(0.5f, 0f, 0f); // Dark red
    [SerializeField] private float lowHealthThreshold = 0.3f;
    
    [Header("Animation")]
    [SerializeField] private bool smoothTransition = true;
    [SerializeField] private float smoothSpeed = 5f;
    
    private Health bossHealth;
    private Canvas canvas;
    private Camera mainCamera;
    private float targetFillAmount = 1f;
    private float timeSinceLastDamage;
    private bool isVisible = true;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        mainCamera = Camera.main;
        
        // Auto-find boss health component on parent
        bossHealth = GetComponentInParent<Health>();
        
        if (bossHealth == null)
        {
            Debug.LogError($"BossHealthBar on {transform.parent.name}: No Health component found on parent!");
            return;
        }
        
        // Setup canvas
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = mainCamera;
        }
    }

    private void Start()
    {
        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged.AddListener(UpdateHealthBar);
            bossHealth.OnDeath.AddListener(OnBossDeath);
            UpdateHealthBar(bossHealth.CurrentHealth, bossHealth.MaxHealth);
        }
        
        if (bossNameText != null)
        {
            bossNameText.text = bossName;
        }
        
        if (hideWhenFull && bossHealth != null && bossHealth.IsAtFullHealth)
        {
            SetVisibility(false);
        }
    }

    private void Update()
    {
        // Always face camera
        FaceCamera();
        
        // Smooth fill animation
        if (smoothTransition && healthBarFill != null)
        {
            healthBarFill.fillAmount = Mathf.Lerp(
                healthBarFill.fillAmount,
                targetFillAmount,
                Time.deltaTime * smoothSpeed
            );
        }
        
        // Auto-hide after delay
        if (showOnlyWhenDamaged && isVisible && bossHealth != null && !bossHealth.IsDead)
        {
            timeSinceLastDamage += Time.deltaTime;
            
            if (timeSinceLastDamage > hideDelay && !bossHealth.IsAtFullHealth)
            {
                SetVisibility(false);
            }
        }
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (!isVisible && (showOnlyWhenDamaged || hideWhenFull))
        {
            SetVisibility(true);
        }
        
        timeSinceLastDamage = 0f;
        
        float healthPercent = (float)currentHealth / maxHealth;
        targetFillAmount = healthPercent;
        
        if (!smoothTransition && healthBarFill != null)
        {
            healthBarFill.fillAmount = healthPercent;
        }
        
        // Update color based on health
        if (healthBarFill != null)
        {
            if (healthPercent <= lowHealthThreshold)
            {
                healthBarFill.color = Color.Lerp(lowHealthColor, fullHealthColor, 
                    healthPercent / lowHealthThreshold);
            }
            else
            {
                healthBarFill.color = fullHealthColor;
            }
        }
        
        // Update text
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
        
        // Hide if at full health
        if (hideWhenFull && healthPercent >= 1f)
        {
            SetVisibility(false);
        }
    }

    private void FaceCamera()
    {
        if (mainCamera == null || canvas == null) return;
        
        // Make health bar always face camera
        transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
    }

    private void OnBossDeath()
    {
        StartCoroutine(FadeOutHealthBar());
    }

    private System.Collections.IEnumerator FadeOutHealthBar()
    {
        yield return new WaitForSeconds(2f);
        SetVisibility(false);
    }

    private void SetVisibility(bool visible)
    {
        isVisible = visible;
        
        if (canvas != null)
        {
            canvas.enabled = visible;
        }
    }

    public void Show()
    {
        SetVisibility(true);
        timeSinceLastDamage = 0f;
    }

    public void Hide()
    {
        SetVisibility(false);
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged.RemoveListener(UpdateHealthBar);
            bossHealth.OnDeath.RemoveListener(OnBossDeath);
        }
    }
}