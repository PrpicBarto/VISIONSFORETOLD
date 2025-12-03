using UnityEngine;
using UnityEngine.Events;

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

    // events (initialized to avoid null refs)
    public UnityEvent<int, int> OnXPChanged = new UnityEvent<int, int>(); // currentXP, xpToNextLevel
    public UnityEvent<int> OnLevelUp = new UnityEvent<int>(); // new level (int)
    public UnityEvent<int> OnSkillPointGained = new UnityEvent<int>(); // amount of skill points gained

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

    /// <summary>
    /// Load XP data from save (called by SaveManager/PlayerSpawnManager)
    /// </summary>
    public void LoadXPData(int xp, int level, int xpToNext)
    {
        currentXP = xp;
        currentLevel = level;
        xpToNextLevel = xpToNext;

        // Recalculate XP requirement (in case formula changed)
        CalculateXPToNextLevel();

        // Trigger events to update UI
        OnXPChanged?.Invoke(currentXP, xpToNextLevel);
        OnLevelUp?.Invoke(currentLevel);

        Debug.Log($"[PlayerXP] Loaded XP data - Level: {currentLevel}, XP: {currentXP}/{xpToNextLevel}");
    }
}
