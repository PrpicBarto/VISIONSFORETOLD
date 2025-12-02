using System;
using UnityEngine;

public class PlayerXP : MonoBehaviour
{
    [Header("XP Settings")]
    [SerializeField] private int currentXP = 0;
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int baseXPToLevel = 100;
    [SerializeField] private float xpScalingFactor = 1.5f;
    
    [Header("Rewards")]
    [SerializeField] private int skillPointsPerLevel = 3;
    [SerializeField] private int healthIncreasePerLevel = 10;
    
    //eventovi
    public System.Action<int, int> OnXPChanged; //currentXP, xpToNextLevel
    public System.Action<int> OnLevelUp; //new level
    public System.Action<int> OnSkillPointGained; //amount of skill points gained

    private int xpToNextLevel;
    private Health health;

    private void Start()
    {
        health = GetComponent<Health>();
        CalculateXPToNextLevel();
        OnXPChanged?.Invoke(currentXP, xpToNextLevel);
        OnLevelUp?.Invoke(currentLevel);
    }

    public void AddXP(int amount)
    {
        currentXP += amount;
        
        DamageNumberManager.Instance.ShowXPGain(transform.position + Vector3.up * 2f, amount);
        
        while (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
        
        OnXPChanged?.Invoke(currentXP, xpToNextLevel);
    }

    private void LevelUp()
    {
        currentXP -= xpToNextLevel;
        currentLevel++;
        
        CalculateXPToNextLevel();

        GiveSkillPoints(skillPointsPerLevel);
        IncreaseMaxHealth(healthIncreasePerLevel);

        if (health != null)
        {
            health.RestoreToFullHealth();
        }
        
        OnLevelUp?.Invoke(currentLevel);

        Debug.Log($"Player leveled up to {currentLevel}!");
    }
    
    private void CalculateXPToNextLevel()
    {
        xpToNextLevel = Mathf.RoundToInt(baseXPToLevel * Mathf.Pow(xpScalingFactor, currentLevel - 1));
    }

    private void GiveSkillPoints(int amount)
    {
        OnSkillPointGained?.Invoke(amount);
        Debug.Log($"Gained {amount} skill points!");
    }

    private void IncreaseMaxHealth(int amount)
    {
        if (health != null)
        {
            health.SetMaxHealth(health.MaxHealth + amount, true);
            Debug.Log($"Max health increased by {amount}!");
        }
    }

    public int CurrentXP => currentXP;
    public int Level => currentLevel;
    public int XPToNextLevel => xpToNextLevel;
}
