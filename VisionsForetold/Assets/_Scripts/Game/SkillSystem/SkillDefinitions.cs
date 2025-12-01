using UnityEngine;
using System.Collections.Generic;

namespace VisionsForetold.Game.SkillSystem
{
    /// <summary>
    /// Combat skills that enhance attack damage and capabilities
    /// </summary>
    public class CombatSkills
    {
        /// <summary>
        /// Increases melee attack damage
        /// </summary>
        public class PowerStrike : Skill
        {
            public PowerStrike() : base(
                "power_strike",
                "Power Strike",
                "Increases melee attack damage",
                SkillCategory.Combat,
                SkillTier.Basic)
            {
                maxLevel = 5;
                requirements = new SkillRequirement(1, 1);

                effects.Add(new SkillEffect
                {
                    effectType = SkillEffectType.DamageBoost,
                    baseValue = 5f,
                    valuePerLevel = 3f,
                    isPercentage = false
                });
            }

            public override void ApplyEffects(GameObject player)
            {
                // Effect applied through SkillManager.GetTotalBonus()
            }

            public override void RemoveEffects(GameObject player) { }
        }

        /// <summary>
        /// Extends melee combo count
        /// </summary>
        public class ComboMaster : Skill
        {
            public ComboMaster() : base(
                "combo_master",
                "Combo Master",
                "Extends combo count by 1 and increases final hit multiplier",
                SkillCategory.Combat,
                SkillTier.Advanced)
            {
                maxLevel = 3;
                requirements = new SkillRequirement(5, 2);
                requirements.prerequisiteSkillIds.Add("power_strike");

                effects.Add(new SkillEffect
                {
                    effectType = SkillEffectType.ComboExtension,
                    baseValue = 1f,
                    valuePerLevel = 1f,
                    isPercentage = false
                });

                effects.Add(new SkillEffect
                {
                    effectType = SkillEffectType.DamageBoost,
                    baseValue = 10f,
                    valuePerLevel = 5f,
                    isPercentage = true // Percentage bonus to final hit
                });
            }

            public override void ApplyEffects(GameObject player)
            {
                // Effect applied through SkillManager
            }

            public override void RemoveEffects(GameObject player) { }
        }

        /// <summary>
        /// Increases critical hit chance
        /// </summary>
        public class CriticalStrike : Skill
        {
            public CriticalStrike() : base(
                "critical_strike",
                "Critical Strike",
                "Chance to deal double damage on attacks",
                SkillCategory.Combat,
                SkillTier.Advanced)
            {
                maxLevel = 5;
                requirements = new SkillRequirement(3, 2);

                effects.Add(new SkillEffect
                {
                    effectType = SkillEffectType.CriticalChance,
                    baseValue = 5f,
                    valuePerLevel = 5f,
                    isPercentage = true
                });
            }

            public override void ApplyEffects(GameObject player)
            {
                // Effect applied through SkillManager
            }

            public override void RemoveEffects(GameObject player) { }
        }

        /// <summary>
        /// Increases attack speed
        /// </summary>
        public class SwiftStrikes : Skill
        {
            public SwiftStrikes() : base(
                "swift_strikes",
                "Swift Strikes",
                "Reduces attack cooldown, allowing faster attacks",
                SkillCategory.Combat,
                SkillTier.Basic)
            {
                maxLevel = 5;
                requirements = new SkillRequirement(2, 1);

                effects.Add(new SkillEffect
                {
                    effectType = SkillEffectType.AttackSpeedBoost,
                    baseValue = 5f,
                    valuePerLevel = 5f,
                    isPercentage = true
                });
            }

            public override void ApplyEffects(GameObject player)
            {
                // Effect applied through SkillManager
            }

            public override void RemoveEffects(GameObject player) { }
        }
    }

    /// <summary>
    /// Magic skills that enhance spell casting
    /// </summary>
    public class MagicSkills
    {
        /// <summary>
        /// Increases spell damage
        /// </summary>
        public class ArcanePower : Skill
        {
            public ArcanePower() : base(
                "arcane_power",
                "Arcane Power",
                "Increases all spell damage",
                SkillCategory.Magic,
                SkillTier.Basic)
            {
                maxLevel = 5;
                requirements = new SkillRequirement(1, 1);

                effects.Add(new SkillEffect
                {
                    effectType = SkillEffectType.SpellPowerBoost,
                    baseValue = 10f,
                    valuePerLevel = 10f,
                    isPercentage = true
                });
            }

            public override void ApplyEffects(GameObject player)
            {
                // Effect applied through SkillManager
            }

            public override void RemoveEffects(GameObject player) { }
        }

        /// <summary>
        /// Reduces spell cooldowns
        /// </summary>
        public class SpellMastery : Skill
        {
            public SpellMastery() : base(
                "spell_mastery",
                "Spell Mastery",
                "Reduces all spell cooldowns",
                SkillCategory.Magic,
                SkillTier.Advanced)
            {
                maxLevel = 5;
                requirements = new SkillRequirement(4, 2);
                requirements.prerequisiteSkillIds.Add("arcane_power");

                effects.Add(new SkillEffect
                {
                    effectType = SkillEffectType.CooldownReduction,
                    baseValue = 10f,
                    valuePerLevel = 5f,
                    isPercentage = true
                });
            }

            public override void ApplyEffects(GameObject player)
            {
                // Effect applied through SkillManager
            }

            public override void RemoveEffects(GameObject player) { }
        }

        /// <summary>
        /// Increases spell area of effect
        /// </summary>
        public class WideningBlast : Skill
        {
            public WideningBlast() : base(
                "widening_blast",
                "Widening Blast",
                "Increases area of effect for all spells",
                SkillCategory.Magic,
                SkillTier.Expert)
            {
                maxLevel = 3;
                requirements = new SkillRequirement(7, 3);
                requirements.prerequisiteSkillIds.Add("spell_mastery");

                effects.Add(new SkillEffect
                {
                    effectType = SkillEffectType.AreaOfEffect,
                    baseValue = 15f,
                    valuePerLevel = 10f,
                    isPercentage = true
                });
            }

            public override void ApplyEffects(GameObject player)
            {
                // Effect applied through SkillManager
            }

            public override void RemoveEffects(GameObject player) { }
        }

        /// <summary>
        /// Adds elemental damage to spells
        /// </summary>
        public class ElementalFocus : Skill
        {
            public ElementalFocus() : base(
                "elemental_focus",
                "Elemental Focus",
                "Adds additional elemental damage to all spells",
                SkillCategory.Magic,
                SkillTier.Master)
            {
                maxLevel = 5;
                requirements = new SkillRequirement(10, 4);
                requirements.prerequisiteSkillIds.Add("widening_blast");

                effects.Add(new SkillEffect
                {
                    effectType = SkillEffectType.ElementalDamage,
                    baseValue = 20f,
                    valuePerLevel = 15f,
                    isPercentage = true
                });
            }

            public override void ApplyEffects(GameObject player)
            {
                // Effect applied through SkillManager
            }

            public override void RemoveEffects(GameObject player) { }
        }
    }

    /// <summary>
    /// Defense skills that increase survivability
    /// </summary>
    public class DefenseSkills
    {
        /// <summary>
        /// Increases maximum health
        /// </summary>
        public class Vitality : Skill
        {
            public Vitality() : base(
                "vitality",
                "Vitality",
                "Increases maximum health",
                SkillCategory.Defense,
                SkillTier.Basic)
            {
                maxLevel = 5;
                requirements = new SkillRequirement(1, 1);

                effects.Add(new SkillEffect
                {
                    effectType = SkillEffectType.HealthBoost,
                    baseValue = 20f,
                    valuePerLevel = 15f,
                    isPercentage = false
                });
            }

            public override void ApplyEffects(GameObject player)
            {
                var health = player.GetComponent<Health>();
                if (health != null)
                {
                    int bonus = Mathf.RoundToInt(GetEffectValue(SkillEffectType.HealthBoost));
                    health.SetMaxHealth(health.MaxHealth + bonus, true);
                }
            }

            public override void RemoveEffects(GameObject player)
            {
                var health = player.GetComponent<Health>();
                if (health != null)
                {
                    int bonus = Mathf.RoundToInt(GetEffectValue(SkillEffectType.HealthBoost));
                    health.SetMaxHealth(health.MaxHealth - bonus, true);
                }
            }
        }

        /// <summary>
        /// Reduces damage taken
        /// </summary>
        public class IronSkin : Skill
        {
            public IronSkin() : base(
                "iron_skin",
                "Iron Skin",
                "Reduces all incoming damage",
                SkillCategory.Defense,
                SkillTier.Advanced)
            {
                maxLevel = 5;
                requirements = new SkillRequirement(3, 2);

                effects.Add(new SkillEffect
                {
                    effectType = SkillEffectType.DefenseBoost,
                    baseValue = 5f,
                    valuePerLevel = 5f,
                    isPercentage = true
                });
            }

            public override void ApplyEffects(GameObject player)
            {
                // Effect applied through SkillManager
            }

            public override void RemoveEffects(GameObject player) { }
        }

        /// <summary>
        /// Heal on hit
        /// </summary>
        public class LifeSteal : Skill
        {
            public LifeSteal() : base(
                "life_steal",
                "Life Steal",
                "Heal for a percentage of damage dealt",
                SkillCategory.Defense,
                SkillTier.Expert)
            {
                maxLevel = 3;
                requirements = new SkillRequirement(6, 3);
                requirements.prerequisiteSkillIds.Add("iron_skin");

                effects.Add(new SkillEffect
                {
                    effectType = SkillEffectType.LifeSteal,
                    baseValue = 5f,
                    valuePerLevel = 5f,
                    isPercentage = true
                });
            }

            public override void ApplyEffects(GameObject player)
            {
                // Effect applied through SkillManager
            }

            public override void RemoveEffects(GameObject player) { }
        }
    }

    /// <summary>
    /// Utility skills that provide passive bonuses
    /// </summary>
    public class UtilitySkills
    {
        /// <summary>
        /// Increases movement speed
        /// </summary>
        public class FleetFooted : Skill
        {
            public FleetFooted() : base(
                "fleet_footed",
                "Fleet Footed",
                "Increases movement speed",
                SkillCategory.Utility,
                SkillTier.Basic)
            {
                maxLevel = 5;
                requirements = new SkillRequirement(2, 1);

                effects.Add(new SkillEffect
                {
                    effectType = SkillEffectType.MovementSpeed,
                    baseValue = 5f,
                    valuePerLevel = 5f,
                    isPercentage = true
                });
            }

            public override void ApplyEffects(GameObject player)
            {
                // Effect applied through SkillManager
            }

            public override void RemoveEffects(GameObject player) { }
        }

        /// <summary>
        /// Gain more experience
        /// </summary>
        public class QuickLearner : Skill
        {
            public QuickLearner() : base(
                "quick_learner",
                "Quick Learner",
                "Gain bonus experience from all sources",
                SkillCategory.Utility,
                SkillTier.Basic)
            {
                maxLevel = 3;
                requirements = new SkillRequirement(1, 1);

                effects.Add(new SkillEffect
                {
                    effectType = SkillEffectType.DamageBoost, // Reusing for XP bonus
                    baseValue = 10f,
                    valuePerLevel = 10f,
                    isPercentage = true
                });
            }

            public override void ApplyEffects(GameObject player)
            {
                // Effect applied through SkillManager
            }

            public override void RemoveEffects(GameObject player) { }
        }
    }
}
