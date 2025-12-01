using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using VisionsForetold.Game.SaveSystem;

namespace VisionsForetold.Game.SkillSystem
{
    /// <summary>
    /// Manages player skills, skill tree, and applies skill effects
    /// Singleton pattern for global access
    /// </summary>
    public class SkillManager : MonoBehaviour
    {
        public static SkillManager Instance { get; private set; }

        [Header("Progression Settings")]
        [SerializeField] private int baseExperiencePerLevel = 100;
        [SerializeField] private float experienceScaling = 1.2f;
        [SerializeField] private int skillPointsPerLevel = 1;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;

        // Skill data
        private SkillSaveData currentSkillData;
        private Dictionary<string, Skill> availableSkills;
        private GameObject player;

        // Events
        public event System.Action<int> OnLevelUp; // New level
        public event System.Action<int> OnExperienceGained; // XP amount
        public event System.Action<Skill> OnSkillUnlocked;
        public event System.Action<Skill> OnSkillLeveledUp;
        public event System.Action<int> OnSkillPointsChanged; // Current points

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSkillSystem();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        #endregion

        #region Initialization

        private void InitializeSkillSystem()
        {
            currentSkillData = new SkillSaveData();
            availableSkills = new Dictionary<string, Skill>();

            RegisterAllSkills();
        }

        /// <summary>
        /// Register all available skills
        /// </summary>
        private void RegisterAllSkills()
        {
            // Combat skills
            RegisterSkill(new CombatSkills.PowerStrike());
            RegisterSkill(new CombatSkills.SwiftStrikes());
            RegisterSkill(new CombatSkills.CriticalStrike());
            RegisterSkill(new CombatSkills.ComboMaster());

            // Magic skills
            RegisterSkill(new MagicSkills.ArcanePower());
            RegisterSkill(new MagicSkills.SpellMastery());
            RegisterSkill(new MagicSkills.WideningBlast());
            RegisterSkill(new MagicSkills.ElementalFocus());

            // Defense skills
            RegisterSkill(new DefenseSkills.Vitality());
            RegisterSkill(new DefenseSkills.IronSkin());
            RegisterSkill(new DefenseSkills.LifeSteal());

            // Utility skills
            RegisterSkill(new UtilitySkills.FleetFooted());
            RegisterSkill(new UtilitySkills.QuickLearner());
        }

        private void RegisterSkill(Skill skill)
        {
            if (!availableSkills.ContainsKey(skill.skillId))
            {
                availableSkills.Add(skill.skillId, skill);
            }
        }

        #endregion

        #region Skill Operations

        /// <summary>
        /// Unlock a skill
        /// </summary>
        public bool UnlockSkill(string skillId)
        {
            if (!availableSkills.ContainsKey(skillId))
                return false;

            Skill skill = availableSkills[skillId];

            if (skill.IsUnlocked)
                return false;

            if (!skill.CanUnlock(currentSkillData))
                return false;

            // Deduct skill points
            currentSkillData.skillPoints -= skill.requirements.requiredSkillPoints;

            // Unlock skill
            skill.Unlock();
            currentSkillData.unlockedSkillIds.Add(skillId);
            currentSkillData.skillLevels[skillId] = 1;

            // Apply effects
            if (player != null)
            {
                skill.ApplyEffects(player);
            }

            // Invoke event
            OnSkillUnlocked?.Invoke(skill);
            OnSkillPointsChanged?.Invoke(currentSkillData.skillPoints);

            return true;
        }

        /// <summary>
        /// Level up a skill
        /// </summary>
        public bool LevelUpSkill(string skillId)
        {
            if (!availableSkills.ContainsKey(skillId))
                return false;

            Skill skill = availableSkills[skillId];

            if (!skill.CanLevelUp(currentSkillData))
                return false;

            // Remove old effects
            if (player != null)
            {
                skill.RemoveEffects(player);
            }

            // Deduct skill points
            currentSkillData.skillPoints -= skill.GetLevelUpCost();

            // Level up
            skill.LevelUp();
            currentSkillData.skillLevels[skillId] = skill.currentLevel;

            // Apply new effects
            if (player != null)
            {
                skill.ApplyEffects(player);
            }

            // Invoke event
            OnSkillLeveledUp?.Invoke(skill);
            OnSkillPointsChanged?.Invoke(currentSkillData.skillPoints);

            return true;
        }

        /// <summary>
        /// Get skill by ID
        /// </summary>
        public Skill GetSkill(string skillId)
        {
            // Ensure availableSkills is initialized
            if (availableSkills == null)
                InitializeSkillSystem();
            
            return availableSkills.ContainsKey(skillId) ? availableSkills[skillId] : null;
        }

        /// <summary>
        /// Get all skills
        /// </summary>
        public List<Skill> GetAllSkills()
        {
            // Ensure availableSkills is initialized
            if (availableSkills == null)
                InitializeSkillSystem();
            
            return availableSkills.Values.ToList();
        }

        /// <summary>
        /// Get skills by category
        /// </summary>
        public List<Skill> GetSkillsByCategory(SkillCategory category)
        {
            // Ensure availableSkills is initialized
            if (availableSkills == null)
                InitializeSkillSystem();
            
            return availableSkills.Values.Where(s => s.category == category).ToList();
        }

        /// <summary>
        /// Get unlocked skills
        /// </summary>
        public List<Skill> GetUnlockedSkills()
        {
            // Ensure availableSkills is initialized
            if (availableSkills == null)
                InitializeSkillSystem();
            
            return availableSkills.Values.Where(s => s.IsUnlocked).ToList();
        }

        #endregion

        #region Experience & Leveling

        /// <summary>
        /// Add experience to player
        /// </summary>
        public void AddExperience(int amount)
        {
            // Apply Quick Learner bonus
            Skill quickLearner = GetSkill("quick_learner");
            if (quickLearner != null && quickLearner.IsUnlocked)
            {
                float bonus = quickLearner.GetEffectValue(SkillEffectType.DamageBoost) / 100f;
                amount = Mathf.RoundToInt(amount * (1f + bonus));
            }

            currentSkillData.experience += amount;
            OnExperienceGained?.Invoke(amount);

            // Check for level up
            while (currentSkillData.experience >= currentSkillData.experienceToNextLevel)
            {
                LevelUp();
            }
        }

        private void LevelUp()
        {
            currentSkillData.experience -= currentSkillData.experienceToNextLevel;
            currentSkillData.level++;
            currentSkillData.skillPoints += skillPointsPerLevel;

            // Calculate next level XP requirement
            currentSkillData.experienceToNextLevel = Mathf.RoundToInt(
                baseExperiencePerLevel * Mathf.Pow(experienceScaling, currentSkillData.level - 1)
            );

            OnLevelUp?.Invoke(currentSkillData.level);
            OnSkillPointsChanged?.Invoke(currentSkillData.skillPoints);
        }

        #endregion

        #region Skill Effect Queries

        /// <summary>
        /// Get total bonus from all skills for a specific effect type
        /// </summary>
        public float GetTotalBonus(SkillEffectType effectType, bool isPercentage)
        {
            float total = 0f;

            foreach (Skill skill in GetUnlockedSkills())
            {
                SkillEffect effect = skill.effects.FirstOrDefault(e => e.effectType == effectType && e.isPercentage == isPercentage);
                if (effect != null)
                {
                    total += effect.GetValue(skill.currentLevel);
                }
            }

            return total;
        }

        /// <summary>
        /// Calculate damage with skill bonuses
        /// </summary>
        public int CalculateDamageWithBonuses(int baseDamage, bool isSpell = false)
        {
            float damage = baseDamage;

            // Add flat damage bonus
            damage += GetTotalBonus(SkillEffectType.DamageBoost, false);

            // Apply percentage damage bonus
            float percentBonus = GetTotalBonus(SkillEffectType.DamageBoost, true);
            damage *= (1f + percentBonus / 100f);

            // Add spell power if it's a spell
            if (isSpell)
            {
                float spellBonus = GetTotalBonus(SkillEffectType.SpellPowerBoost, true);
                damage *= (1f + spellBonus / 100f);

                // Add elemental damage
                float elementalBonus = GetTotalBonus(SkillEffectType.ElementalDamage, true);
                damage *= (1f + elementalBonus / 100f);
            }

            // Check for critical hit
            float critChance = GetTotalBonus(SkillEffectType.CriticalChance, true);
            if (Random.Range(0f, 100f) < critChance)
            {
                damage *= 2f;
            }

            return Mathf.RoundToInt(damage);
        }

        /// <summary>
        /// Calculate cooldown with reductions
        /// </summary>
        public float CalculateCooldownWithReduction(float baseCooldown)
        {
            float reduction = GetTotalBonus(SkillEffectType.CooldownReduction, true);
            return baseCooldown * (1f - reduction / 100f);
        }

        /// <summary>
        /// Calculate attack speed modifier
        /// </summary>
        public float GetAttackSpeedModifier()
        {
            float speedBonus = GetTotalBonus(SkillEffectType.AttackSpeedBoost, true);
            return 1f - (speedBonus / 100f); // Returns multiplier for cooldown (lower = faster)
        }

        /// <summary>
        /// Get movement speed bonus
        /// </summary>
        public float GetMovementSpeedBonus()
        {
            return GetTotalBonus(SkillEffectType.MovementSpeed, true) / 100f;
        }

        /// <summary>
        /// Calculate damage reduction
        /// </summary>
        public float GetDamageReduction()
        {
            return GetTotalBonus(SkillEffectType.DefenseBoost, true) / 100f;
        }

        /// <summary>
        /// Get life steal percentage
        /// </summary>
        public float GetLifeStealPercentage()
        {
            return GetTotalBonus(SkillEffectType.LifeSteal, true) / 100f;
        }

        /// <summary>
        /// Get AOE radius multiplier
        /// </summary>
        public float GetAOEMultiplier()
        {
            float bonus = GetTotalBonus(SkillEffectType.AreaOfEffect, true);
            return 1f + (bonus / 100f);
        }

        #endregion

        #region Save/Load Integration

        /// <summary>
        /// Get current skill save data
        /// </summary>
        public SkillSaveData GetSkillSaveData()
        {
            // Ensure currentSkillData is initialized
            if (currentSkillData == null)
                currentSkillData = new SkillSaveData();
            
            return currentSkillData;
        }

        /// <summary>
        /// Load skill data from save
        /// </summary>
        public void LoadSkillData(SkillSaveData saveData)
        {
            if (saveData == null)
                return;

            // Remove all current skill effects
            foreach (Skill skill in GetUnlockedSkills())
            {
                if (player != null)
                {
                    skill.RemoveEffects(player);
                }
            }

            // Load save data
            currentSkillData = saveData;

            // Restore skill levels and apply effects
            foreach (var kvp in currentSkillData.skillLevels)
            {
                string skillId = kvp.Key;
                int level = kvp.Value;

                if (availableSkills.ContainsKey(skillId))
                {
                    Skill skill = availableSkills[skillId];
                    skill.currentLevel = level;

                    if (player != null)
                    {
                        skill.ApplyEffects(player);
                    }
                }
            }
        }

        /// <summary>
        /// Create new skill save data
        /// </summary>
        public void CreateNewSkillData()
        {
            currentSkillData = new SkillSaveData();
        }

        #endregion

        #region Debug

        [ContextMenu("Add 100 XP")]
        private void DebugAddXP()
        {
            AddExperience(100);
        }

        [ContextMenu("Add 5 Skill Points")]
        private void DebugAddSkillPoints()
        {
            currentSkillData.skillPoints += 5;
            OnSkillPointsChanged?.Invoke(currentSkillData.skillPoints);
        }

        [ContextMenu("Print Skill Stats")]
        private void PrintSkillStats()
        {
            // Print to console for debugging
        }

        private void OnGUI()
        {
            if (!showDebugInfo) return;

            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 12;
            style.padding = new RectOffset(10, 10, 10, 10);

            GUI.Box(new Rect(10, 280, 300, 150), "");

            int yPos = 285;
            GUI.Label(new Rect(20, yPos, 280, 20), $"=== SKILL SYSTEM ===", style);
            yPos += 20;
            GUI.Label(new Rect(20, yPos, 280, 20), $"Level: {currentSkillData.level}", style);
            yPos += 20;
            GUI.Label(new Rect(20, yPos, 280, 20), $"XP: {currentSkillData.experience}/{currentSkillData.experienceToNextLevel}", style);
            yPos += 20;
            GUI.Label(new Rect(20, yPos, 280, 20), $"Skill Points: {currentSkillData.skillPoints}", style);
            yPos += 20;
            GUI.Label(new Rect(20, yPos, 280, 20), $"Unlocked Skills: {GetUnlockedSkills().Count}/{availableSkills.Count}", style);
            yPos += 20;
            GUI.Label(new Rect(20, yPos, 280, 20), $"Total Damage Bonus: +{GetTotalBonus(SkillEffectType.DamageBoost, false):F0}", style);
        }

        #endregion
    }
}
