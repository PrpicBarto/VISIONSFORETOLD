using UnityEngine;
using VisionsForetold.Game.SkillSystem;

/// <summary>
/// Extension component for PlayerAttack that applies skill bonuses
/// Attach this to the same GameObject as PlayerAttack
/// </summary>
[RequireComponent(typeof(PlayerAttack))]
public class PlayerAttackSkillExtension : MonoBehaviour
{
    private PlayerAttack playerAttack;
    private SkillManager skillManager;

    // Cached values for performance
    private int baseDamage;
    private float baseCooldown;
    private int baseComboCount;

    private void Awake()
    {
        playerAttack = GetComponent<PlayerAttack>();
    }

    private void Start()
    {
        skillManager = SkillManager.Instance;

        if (skillManager == null)
        {
            enabled = false;
            return;
        }

        // Subscribe to skill events
        skillManager.OnSkillUnlocked += OnSkillChanged;
        skillManager.OnSkillLeveledUp += OnSkillChanged;

        // Apply initial bonuses
        ApplyAllSkillBonuses();
    }

    private void OnDestroy()
    {
        if (skillManager != null)
        {
            skillManager.OnSkillUnlocked -= OnSkillChanged;
            skillManager.OnSkillLeveledUp -= OnSkillChanged;
        }
    }

    private void OnSkillChanged(Skill skill)
    {
        ApplyAllSkillBonuses();
    }

    /// <summary>
    /// Apply all skill bonuses to player attack
    /// </summary>
    private void ApplyAllSkillBonuses()
    {
        if (skillManager == null || playerAttack == null) return;

        // This method would modify PlayerAttack fields
        // Since PlayerAttack fields are private/serialized, we can't directly modify them
        // Instead, we create wrapper methods in PlayerAttack that skills can enhance
    }

    /// <summary>
    /// Calculate final damage with skill bonuses (call from PlayerAttack)
    /// </summary>
    public int GetEnhancedDamage(int baseDamage, bool isSpell = false)
    {
        if (skillManager == null) return baseDamage;

        return skillManager.CalculateDamageWithBonuses(baseDamage, isSpell);
    }

    /// <summary>
    /// Calculate cooldown with skill reductions (call from PlayerAttack)
    /// </summary>
    public float GetEnhancedCooldown(float baseCooldown, bool isSpell = false)
    {
        if (skillManager == null) return baseCooldown;

        if (isSpell)
        {
            return skillManager.CalculateCooldownWithReduction(baseCooldown);
        }
        else
        {
            // Attack speed affects attack cooldown
            return baseCooldown * skillManager.GetAttackSpeedModifier();
        }
    }

    /// <summary>
    /// Get combo count bonus from skills
    /// </summary>
    public int GetComboCountBonus()
    {
        if (skillManager == null) return 0;

        return Mathf.RoundToInt(skillManager.GetTotalBonus(SkillEffectType.ComboExtension, false));
    }

    /// <summary>
    /// Apply life steal on damage dealt
    /// </summary>
    public void ApplyLifeSteal(int damageDealt)
    {
        if (skillManager == null) return;

        float lifestealPercent = skillManager.GetLifeStealPercentage();
        if (lifestealPercent > 0)
        {
            int healAmount = Mathf.RoundToInt(damageDealt * lifestealPercent);
            if (healAmount > 0)
            {
                Health playerHealth = GetComponent<Health>();
                if (playerHealth != null)
                {
                    playerHealth.Heal(healAmount);
                }
            }
        }
    }

    /// <summary>
    /// Get AOE radius multiplier from skills
    /// </summary>
    public float GetAOEMultiplier()
    {
        if (skillManager == null) return 1f;
        return skillManager.GetAOEMultiplier();
    }

    /// <summary>
    /// Check if damage should be reduced by defense skills
    /// </summary>
    public int ApplyDamageReduction(int incomingDamage)
    {
        if (skillManager == null) return incomingDamage;

        float reduction = skillManager.GetDamageReduction();
        int reducedDamage = Mathf.RoundToInt(incomingDamage * (1f - reduction));

        return reducedDamage;
    }
}
