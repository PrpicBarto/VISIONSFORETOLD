using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health bossHealth;
    [SerializeField] private Image HealthBarFill;
    [SerializeField] private TMP_Text bossNameText;
    [SerializeField] private TMP_Text bossHealthText;
    [SerializeField] private GameObject healthBarPanel;
    
    [Header("Settings")]
    [SerializeField] private string bossName = "Chaosmancer";
    [SerializeField] private bool showOnlyInCombat = false;

    private bool isVisible;

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

        if (showOnlyInCombat)
        {
            SetVisibility(false);
        }
        else
        {
            SetVisibility(true);
        }
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        // Safety check for division by zero
        if (maxHealth <= 0)
        {
            Debug.LogWarning("[BossHealthBar] MaxHealth is 0 or negative!");
            return;
        }

        // Check if bossHealth reference is still valid
        if (bossHealth == null)
        {
            Debug.LogWarning("[BossHealthBar] Boss Health reference lost!");
            return;
        }

        if (!isVisible && showOnlyInCombat)
        {
            SetVisibility(true);
        }

        float healthPercent = (float)currentHealth / (float)maxHealth;

        // Validate the calculated percentage
        if (float.IsNaN(healthPercent) || float.IsInfinity(healthPercent))
        {
            Debug.LogError($"[BossHealthBar] Invalid health percent: {healthPercent}");
            return;
        }

        if (HealthBarFill != null)
        {
            HealthBarFill.fillAmount = healthPercent;
        }

        if (bossHealthText != null)
        {
            bossHealthText.text = $"{currentHealth}/{maxHealth}";
        }
    }

    private void OnBossDeath()
    {
        // Check if still valid before starting coroutine
        if (this != null && gameObject != null && gameObject.activeInHierarchy)
        {
            StartCoroutine(FadeOutHealthBar());
        }
        else
        {
            SetVisibility(false);
        }
    }

    private System.Collections.IEnumerator FadeOutHealthBar()
    {
        yield return new WaitForSeconds(2f);
        SetVisibility(false);
    }

    private void SetVisibility(bool visible)
    {
        isVisible = visible;
        if (healthBarPanel != null)
        {
            healthBarPanel.SetActive(visible);
        }
    }

    public void Show()
    {
        SetVisibility(true);
    }

    private void OnDestroy()
    {
        // Clean up event listeners
        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged.RemoveListener(UpdateHealthBar);
            bossHealth.OnDeath.RemoveListener(OnBossDeath);
        }
    }
}
