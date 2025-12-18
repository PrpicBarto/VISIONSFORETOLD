using System;
using UnityEngine;
using UnityEngine.UI;

public class WorldHealthBar : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private Health targetHealth;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Image healthBarBackground;
    [SerializeField] private Canvas canvas;
    
    [Header("Display Settings")]
    [SerializeField] private bool hideWhenFull = true;
    [SerializeField] private bool showOnlyWhenDamaged = true;
    [SerializeField] private float hideDelay = 3f;
    [SerializeField] private Vector3 offset = new Vector3(0, 2.5f, 0);
    
    [Header("Colors")]
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private Color midHealthColor = Color.yellow;
    [SerializeField] private float lowHealthThreshold = 0.3f;
    [SerializeField] private float midHealthThreshold = 0.6f;
    
    [Header("Animation")]
    [SerializeField] private bool smoothTransition = true;
    [SerializeField] private float smoothSpeed = 5f;
    
    private Camera mainCamera;
    private float currentFillAmount;
    private float timeSinceLastDamage;
    private bool isVisible = true;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (targetHealth == null)
        {
            targetHealth.GetComponentInParent<Health>();
        }

        if (canvas == null)
        {
            canvas = GetComponent<Canvas>();
        }

        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
        }
    }

    private void Start()
    {
        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged.AddListener(OnHealthChanged);
            targetHealth.OnDeath.AddListener(OnDeath);

            UpdateHealthBar(targetHealth.CurrentHealth, targetHealth.MaxHealth);

            if (hideWhenFull && targetHealth.IsAtFullHealth)
            {
                SetVisibility(false);
            }
        }
        else
        {
            Debug.LogError($"WorldHealthBar: No health component found");
        }
    }

    private void Update()
    {
        // Check if target health or GameObject is destroyed
        if (targetHealth == null)
        {
            // Target destroyed, destroy health bar too
            Destroy(gameObject);
            return;
        }

        FaceCamera();

        if (smoothTransition && healthBarFill != null)
        {
            healthBarFill.fillAmount =
                Mathf.Lerp(healthBarFill.fillAmount, 
                    currentFillAmount, Time.deltaTime * smoothSpeed);
        }
        
        if(showOnlyWhenDamaged && isVisible)
        {
            timeSinceLastDamage += Time.deltaTime;
            if (timeSinceLastDamage > hideDelay && !targetHealth.IsAtFullHealth)
            {
                SetVisibility(false);
            }
        }
    }
    
    private void OnHealthChanged(int currentHealth, int maxHealth)
    {
        UpdateHealthBar(currentHealth, maxHealth);

        if (showOnlyWhenDamaged || hideWhenFull)
        {
            SetVisibility(true);
            timeSinceLastDamage = 0f;
        }
    }

    private void UpdateHealthBar(int currentHealth, int MaxHealth)
    {
        if (healthBarFill == null) return;

        float healthPercent = (float)currentHealth / MaxHealth;
        currentFillAmount = healthPercent;

        if (!smoothTransition)
        {
            healthBarFill.fillAmount = healthPercent;
        }

        UpdateHealthBarColor(healthPercent);

        if (hideWhenFull && healthPercent >= 1f)
        {
            SetVisibility(false);
        }
    }
    
    private void UpdateHealthBarColor(float healthPercent)
    {
        if (healthBarFill == null) return;

        Color newColor;
        if (healthPercent <= lowHealthThreshold)
        {
            float t = healthPercent / lowHealthThreshold;
            newColor = Color.Lerp(lowHealthColor, midHealthColor, t);
        }
        else if (healthPercent <= midHealthThreshold)
        {
            float t = (healthPercent - lowHealthThreshold) / (midHealthThreshold - lowHealthThreshold);
            newColor = Color.Lerp(midHealthColor, fullHealthColor, t);
        }
        else
        {
            newColor = fullHealthColor;
        }

        healthBarFill.color = newColor;
    }
    
    private void FaceCamera()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        if (canvas == null || transform == null) return;

        transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
    }
    
    private void SetVisibility(bool visible)
    {
        isVisible = visible;
        
        if (canvas == null)
        {
            canvas.enabled = visible;
        }
    }
    
    private void OnDeath()
    {
        SetVisibility(false);
        
        // Destroy health bar shortly after death
        // Small delay to allow death animation/sound to play
        Destroy(gameObject, 0.1f);
    }
    
    private void OnDestroy()
    {
        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged.RemoveListener(OnHealthChanged);
            targetHealth.OnDeath.RemoveListener(OnDeath);
        }
    }
    
    /// <summary>
    /// manualni prikaz health bara
    /// </summary>
    public void Show()
    {
        SetVisibility(true);
        timeSinceLastDamage = 0f;
    }
    
    /// <summary>
    /// manualno sakrivanje health bara
    /// </summary>
    public void Hide()
    {
        SetVisibility(false);
    }
}
