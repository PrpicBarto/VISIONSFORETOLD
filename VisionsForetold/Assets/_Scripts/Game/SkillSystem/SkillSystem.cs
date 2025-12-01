using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VisionsForetold.Game.SkillSystem
{
    #region Enums

    /// <summary>
    /// Categories of skills
    /// </summary>
    public enum SkillCategory
    {
        Combat,
        Defense,
        Magic,
        Utility,
        Passive
    }

    /// <summary>
    /// Skill tier/rarity
    /// </summary>
    public enum SkillTier
    {
        Basic,
        Advanced,
        Expert,
        Master,
        Ultimate
    }

    /// <summary>
    /// Type of skill effect
    /// </summary>
    public enum SkillEffectType
    {
        DamageBoost,
        AttackSpeedBoost,
        HealthBoost,
        DefenseBoost,
        SpellPowerBoost,
        CooldownReduction,
        CriticalChance,
        LifeSteal,
        MovementSpeed,
        ManaRegeneration,
        ComboExtension,
        AreaOfEffect,
        PierceChance,
        ElementalDamage,
        StatusResistance
    }

    #endregion

    #region Data Structures

    /// <summary>
    /// Serializable skill data for saving/loading
    /// </summary>
    [Serializable]
    public class SkillSaveData
    {
        public int skillPoints;
        public int level;
        public int experience;
        public int experienceToNextLevel;
        public List<string> unlockedSkillIds;
        public Dictionary<string, int> skillLevels; // Skill ID -> Level

        public SkillSaveData()
        {
            skillPoints = 0;
            level = 1;
            experience = 0;
            experienceToNextLevel = 100;
            unlockedSkillIds = new List<string>();
            skillLevels = new Dictionary<string, int>();
        }
    }

    /// <summary>
    /// Requirement for unlocking a skill
    /// </summary>
    [Serializable]
    public class SkillRequirement
    {
        public int minimumLevel;
        public List<string> prerequisiteSkillIds;
        public int requiredSkillPoints;

        public SkillRequirement(int level = 1, int skillPoints = 1)
        {
            minimumLevel = level;
            requiredSkillPoints = skillPoints;
            prerequisiteSkillIds = new List<string>();
        }
    }

    /// <summary>
    /// Effect applied by a skill
    /// </summary>
    [Serializable]
    public class SkillEffect
    {
        public SkillEffectType effectType;
        public float baseValue;
        public float valuePerLevel;
        public bool isPercentage;

        public float GetValue(int skillLevel)
        {
            return baseValue + (valuePerLevel * (skillLevel - 1));
        }
    }

    #endregion

    /// <summary>
    /// Base class for all skills
    /// </summary>
    [Serializable]
    public abstract class Skill
    {
        [Header("Base Info")]
        public string skillId;
        public string skillName;
        public string description;
        public Sprite icon;

        [Header("Classification")]
        public SkillCategory category;
        public SkillTier tier;

        [Header("Progression")]
        public int maxLevel = 5;
        public int currentLevel = 0;

        [Header("Requirements")]
        public SkillRequirement requirements;

        [Header("Effects")]
        public List<SkillEffect> effects;

        // Constructor
        protected Skill(string id, string name, string desc, SkillCategory cat, SkillTier tier)
        {
            skillId = id;
            skillName = name;
            description = desc;
            category = cat;
            this.tier = tier;
            requirements = new SkillRequirement();
            effects = new List<SkillEffect>();
        }

        public bool IsUnlocked => currentLevel > 0;
        public bool IsMaxLevel => currentLevel >= maxLevel;

        /// <summary>
        /// Check if skill can be unlocked
        /// </summary>
        public virtual bool CanUnlock(SkillSaveData saveData)
        {
            // Check level requirement
            if (saveData.level < requirements.minimumLevel)
                return false;

            // Check skill points
            if (saveData.skillPoints < requirements.requiredSkillPoints)
                return false;

            // Check prerequisites
            foreach (string prereqId in requirements.prerequisiteSkillIds)
            {
                if (!saveData.unlockedSkillIds.Contains(prereqId))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Check if skill can be leveled up
        /// </summary>
        public virtual bool CanLevelUp(SkillSaveData saveData)
        {
            if (!IsUnlocked) return false;
            if (IsMaxLevel) return false;
            if (saveData.skillPoints < GetLevelUpCost()) return false;
            return true;
        }

        /// <summary>
        /// Get cost to level up this skill
        /// </summary>
        public virtual int GetLevelUpCost()
        {
            return 1 + (currentLevel / 2); // Cost increases every 2 levels
        }

        /// <summary>
        /// Unlock this skill
        /// </summary>
        public virtual void Unlock()
        {
            if (!IsUnlocked)
            {
                currentLevel = 1;
                OnSkillUnlocked();
            }
        }

        /// <summary>
        /// Level up this skill
        /// </summary>
        public virtual void LevelUp()
        {
            if (!IsMaxLevel)
            {
                currentLevel++;
                OnSkillLevelUp();
            }
        }

        /// <summary>
        /// Get total effect value for specific type
        /// </summary>
        public float GetEffectValue(SkillEffectType type)
        {
            SkillEffect effect = effects.FirstOrDefault(e => e.effectType == type);
            return effect?.GetValue(currentLevel) ?? 0f;
        }

        /// <summary>
        /// Apply skill effects to player
        /// </summary>
        public abstract void ApplyEffects(GameObject player);

        /// <summary>
        /// Remove skill effects from player
        /// </summary>
        public abstract void RemoveEffects(GameObject player);

        /// <summary>
        /// Called when skill is first unlocked
        /// </summary>
        protected virtual void OnSkillUnlocked()
        {
            Debug.Log($"Skill unlocked: {skillName}");
        }

        /// <summary>
        /// Called when skill levels up
        /// </summary>
        protected virtual void OnSkillLevelUp()
        {
            Debug.Log($"{skillName} leveled up to {currentLevel}");
        }
    }
}
