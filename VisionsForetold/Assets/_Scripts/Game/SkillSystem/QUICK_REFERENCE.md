# Skill System - Quick Reference

## ? At a Glance

**13 Skills** | **4 Categories** | **5-Level Max** | **XP & Leveling** | **Save Integration**

---

## ?? Scene Setup (30 seconds)

```
1. Create GameObject: "SkillManager"
   ?? Add: SkillManager.cs

2. Select Player
   ?? Add: PlayerAttackSkillExtension.cs

3. Done! Skills now work.
```

---

## ? All Skills List

### **Combat** (Melee/Ranged)
| Skill | Effect | Unlock |
|-------|--------|--------|
| Power Strike | +5 damage/level | Lv1, 1SP |
| Swift Strikes | +5% attack speed/level | Lv2, 1SP |
| Critical Strike | +5% crit chance/level | Lv3, 2SP |
| Combo Master | +1 combo hit, +10% final hit | Lv5, 2SP |

### **Magic** (Spells)
| Skill | Effect | Unlock |
|-------|--------|--------|
| Arcane Power | +10% spell damage/level | Lv1, 1SP |
| Spell Mastery | -10% cooldowns/level | Lv4, 2SP |
| Widening Blast | +15% AoE radius/level | Lv7, 3SP |
| Elemental Focus | +20% elemental dmg/level | Lv10, 4SP |

### **Defense** (Survivability)
| Skill | Effect | Unlock |
|-------|--------|--------|
| Vitality | +20 max HP/level | Lv1, 1SP |
| Iron Skin | -5% damage taken/level | Lv3, 2SP |
| Life Steal | +5% lifesteal/level | Lv6, 3SP |

### **Utility** (Passive)
| Skill | Effect | Unlock |
|-------|--------|--------|
| Fleet Footed | +5% move speed/level | Lv2, 1SP |
| Quick Learner | +10% XP gain/level | Lv1, 1SP |

---

## ?? Code Snippets

### Give XP to Player
```csharp
SkillManager.Instance?.AddExperience(50);
```

### Get Enhanced Damage
```csharp
PlayerAttackSkillExtension ext = GetComponent<PlayerAttackSkillExtension>();
int damage = ext.GetEnhancedDamage(baseDamage, isSpell: false);
```

### Apply Lifesteal
```csharp
ext.ApplyLifeSteal(damageDealt);
```

### Check Skill Unlocked
```csharp
Skill skill = SkillManager.Instance.GetSkill("power_strike");
bool unlocked = skill != null && skill.IsUnlocked;
```

---

## ?? Key Functions

| Class | Method | Purpose |
|-------|--------|---------|
| `SkillManager` | `AddExperience(int)` | Give player XP |
| `SkillManager` | `UnlockSkill(string)` | Unlock a skill |
| `SkillManager` | `LevelUpSkill(string)` | Level up skill |
| `SkillManager` | `GetTotalBonus(type, %)` | Get bonus value |
| `SkillManager` | `CalculateDamageWithBonuses(int, bool)` | Enhanced damage |
| `PlayerAttackSkillExtension` | `GetEnhancedDamage(int, bool)` | Damage with skills |
| `PlayerAttackSkillExtension` | `GetEnhancedCooldown(float, bool)` | Cooldown reduction |
| `PlayerAttackSkillExtension` | `ApplyLifeSteal(int)` | Heal on damage |

---

## ? Debug Commands

**In Inspector (SkillManager):**
- Right-click ? Add 100 XP
- Right-click ? Add 5 Skill Points
- Right-click ? Print Skill Stats

**In-Game:**
- Press **F12** to toggle stats overlay

---

## ? File Structure

```
Assets/_Scripts/Game/SkillSystem/
?? SkillSystem.cs          (Base classes, enums, data)
?? SkillDefinitions.cs      (All 13 skills defined)
?? SkillManager.cs          (Core progression system)
?? SkillTreeUI.cs           (UI controller)
?? SKILL_SYSTEM_GUIDE.md    (Full documentation)

Assets/_Scripts/Game/Player/
?? PlayerAttackSkillExtension.cs  (Combat integration)

Assets/_Scripts/Game/SaveSystem/
?? SaveBase.cs              (Updated with SkillSaveData)
```

---

## ? XP Progression

| Level | XP Required | Total XP | Skill Points |
|-------|-------------|----------|--------------|
| 1?2 | 100 | 100 | 1 |
| 2?3 | 120 | 220 | 2 |
| 3?4 | 144 | 364 | 3 |
| 4?5 | 173 | 537 | 4 |
| 5?6 | 207 | 744 | 5 |
| 10?11 | 517 | 3,094 | 10 |
| 20?21 | 3,833 | 48,359 | 20 |

---

## ?? Example: Enemy XP Drop

```csharp
// In enemy death:
private void Die()
{
    // Give XP
    SkillManager.Instance?.AddExperience(25);
    
    // Rest of death logic
    Destroy(gameObject);
}
```

---

## ? Typical Build Path

**Early Game (Levels 1-5):**
1. Power Strike (Lv1) - Damage boost
2. Quick Learner (Lv1) - Level faster
3. Swift Strikes (Lv2) - Attack speed
4. Vitality (Lv1) - More health
5. Arcane Power (Lv1) - If using spells

**Mid Game (Levels 6-10):**
1. Level up Power Strike to 3
2. Critical Strike (Lv3) - Crit chance
3. Spell Mastery (Lv4) - Cooldown reduction
4. Iron Skin (Lv3) - Damage reduction

**Late Game (Levels 10+):**
1. Combo Master (Lv5) - Bigger combos
2. Widening Blast (Lv7) - AOE spells
3. Life Steal (Lv6) - Sustain
4. Elemental Focus (Lv10) - Ultimate power

---

## ? Checklist

**Scene Setup:**
- [ ] SkillManager in scene
- [ ] PlayerAttackSkillExtension on player
- [ ] SkillTreeUI on Skills Panel
- [ ] Skill Button Prefab created

**Combat Integration:**
- [ ] Enemies give XP on death
- [ ] Skills affect damage (test with Power Strike)
- [ ] Skills affect cooldowns (test with Spell Mastery)
- [ ] Lifesteal works (test with Life Steal skill)

**UI Integration:**
- [ ] Save station Skills button works
- [ ] Can see skill list
- [ ] Can unlock skills
- [ ] Can level up skills
- [ ] UI shows current level/XP/points

**Save Integration:**
- [ ] Skills saved with game
- [ ] Skills loaded correctly
- [ ] Skill effects persist after load

---

## ? Need Help?

1. **Read Full Guide:** `SKILL_SYSTEM_GUIDE.md`
2. **Check Console:** Enable debug logging
3. **Press F12:** View real-time stats
4. **Build Successful:** All scripts compile ?

**System Status:** ? **Production Ready**
