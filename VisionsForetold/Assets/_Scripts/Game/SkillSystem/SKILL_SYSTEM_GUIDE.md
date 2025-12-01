# Skill System - Complete Setup Guide

## ? Overview

The skill system adds RPG-like progression to your game with:
- **13 Unique Skills** across 4 categories (Combat, Magic, Defense, Utility)
- **Experience & Leveling** system
- **Skill Points** earned through leveling
- **Skill Tree UI** accessible from save stations
- **Combat Integration** - skills directly affect damage, speed, cooldowns
- **Save System Integration** - skills persist across saves

---

## ? Features

### Skill Categories

#### **Combat Skills** (Melee/Ranged)
- **Power Strike** - Increases attack damage (+5 damage per level)
- **Swift Strikes** - Increases attack speed (+5% per level)
- **Critical Strike** - Chance to deal double damage (+5% chance per level)
- **Combo Master** - Extends combo chain and boosts final hit damage

#### **Magic Skills** (Spells)
- **Arcane Power** - Increases spell damage (+10% per level)
- **Spell Mastery** - Reduces spell cooldowns (-10% per level)
- **Widening Blast** - Increases spell AOE radius (+15% per level)
- **Elemental Focus** - Adds elemental damage (+20% per level)

#### **Defense Skills** (Survivability)
- **Vitality** - Increases max health (+20 HP per level)
- **Iron Skin** - Reduces damage taken (-5% per level)
- **Life Steal** - Heal for damage dealt (+5% per level)

#### **Utility Skills** (Passive Bonuses)
- **Fleet Footed** - Increases movement speed (+5% per level)
- **Quick Learner** - Bonus experience gain (+10% per level)

### Progression System
- Gain XP by defeating enemies
- Level up to earn skill points
- XP required scales with level (100 ? 120 ? 144...)
- 1 skill point per level

---

## ?? Quick Setup (5 Minutes)

### Step 1: Add SkillManager to Scene

1. Create empty GameObject: "SkillManager"
2. Add component: `SkillManager`
3. Check "Don't Destroy On Load" (optional)
4. Configure in Inspector:
   ```
   Base Experience Per Level: 100
   Experience Scaling: 1.2
   Skill Points Per Level: 1
   Show Debug Info: ? (optional)
   ```

### Step 2: Add Skill Extension to Player

1. Select Player GameObject
2. Add component: `PlayerAttackSkillExtension`
3. (Automatically finds PlayerAttack and SkillManager)

### Step 3: Setup Skill Panel UI

1. Open SaveStation Canvas
2. Find existing "SkillsPanel" 
3. Add component: `SkillTreeUI`
4. Assign UI references:
   ```
   - Skill Panel: SkillsPanel GameObject
   - Skill List Container: ScrollView/Content
   - Player Info: Level/XP/Points texts
   - Skill Details Panel: Detail panel GameObject
   - Category Filters: Combat/Magic/Defense buttons
   ```

### Step 4: Create Skill Button Prefab

1. Create UI GameObject: "SkillButton"
2. Add components:
   - Image (background)
   - Button component
   - TMP_Text "SkillName"
   - TMP_Text "SkillLevel"
   - Image "Icon" (child)
3. Save as prefab
4. Assign to SkillTreeUI ? Skill Button Prefab

### Step 5: Test!

1. Press Play
2. Press F12 to see skill debug overlay (optional)
3. Walk to Save Station
4. Click "Skills" button
5. Unlock and level up skills!

---

## ?? Detailed Integration

### Giving Experience to Player

```csharp
// In enemy death code:
SkillManager skillManager = SkillManager.Instance;
if (skillManager != null)
{
    skillManager.AddExperience(50); // Give 50 XP
}
```

### Using Skill Bonuses in Combat

The skill system automatically enhances PlayerAttack through `PlayerAttackSkillExtension`:

**Automatic Enhancements:**
- Damage bonuses (Power Strike, Arcane Power)
- Critical hits (Critical Strike skill)
- Attack speed (Swift Strikes)
- Spell cooldown reduction (Spell Mastery)
- Life steal on damage dealt
- Damage reduction when taking damage

**Manual Integration (Optional):**

```csharp
// In your attack code:
PlayerAttackSkillExtension skillExt = GetComponent<PlayerAttackSkillExtension>();

// Enhance damage
int enhancedDamage = skillExt.GetEnhancedDamage(baseDamage, isSpell: false);

// Reduce cooldown
float enhancedCooldown = skillExt.GetEnhancedCooldown(baseCooldown, isSpell: true);

// Get combo bonus
int totalComboHits = baseComboCount + skillExt.GetComboCountBonus();

// Apply lifesteal after dealing damage
skillExt.ApplyLifeSteal(damageDealt);

// Increase AOE radius
float enhancedRadius = baseRadius * skillExt.GetAOEMultiplier();
```

### Defense Integration (Taking Damage)

```csharp
// In Health.TakeDamage():
PlayerAttackSkillExtension skillExt = GetComponent<PlayerAttackSkillExtension>();
if (skillExt != null)
{
    damage = skillExt.ApplyDamageReduction(damage);
}

currentHealth -= damage;
```

---

## ?? UI Setup Details

### Skill Panel Layout

```
SkillPanel (GameObject)
?? Header
?  ?? Title: "Skills"
?  ?? LevelText: "Level: 5"
?  ?? ExperienceText: "XP: 250/500"
?  ?? SkillPointsText: "Skill Points: 3"
?  ?? ExperienceBar (Slider)
?? CategoryFilters (Horizontal Layout)
?  ?? AllSkillsButton
?  ?? CombatSkillsButton
?  ?? MagicSkillsButton
?  ?? DefenseSkillsButton
?  ?? UtilitySkillsButton
?? SkillList (Scroll View)
?  ?? Content (Vertical Layout Group)
?      ?? [SkillButton Prefabs spawned here]
?? SkillDetailPanel
   ?? SkillNameText
   ?? SkillIcon
   ?? SkillDescriptionText
   ?? SkillLevelText
   ?? SkillCostText
   ?? UnlockButton
   ?? LevelUpButton
```

### Skill Button Prefab Structure

```
SkillButton (Button)
?? Background (Image) - Color changes based on unlock status
?? Icon (Image) - Skill icon sprite
?? SkillName (TMP_Text)
?? SkillLevel (TMP_Text) - "Lv. 3/5" or "Locked"
```

### Color Coding

Configure in SkillTreeUI:
- **Unlocked**: Green - Skill is unlocked and active
- **Can Unlock**: Yellow - Requirements met, can unlock now
- **Locked**: Gray - Requirements not met

---

## ? Save System Integration

### Automatic Saving

Skills are automatically saved when you save the game:

```csharp
// SaveManager.CollectPlayerData() already includes:
currentSaveData.skills = skillManager.GetSkillSaveData();
```

### Automatic Loading

Skills are automatically restored when loading:

```csharp
// SaveManager.ApplyPlayerData() already includes:
skillManager.LoadSkillData(currentSaveData.skills);
```

### What Gets Saved

```json
{
  "skillPoints": 5,
  "level": 12,
  "experience": 150,
  "experienceToNextLevel": 300,
  "unlockedSkillIds": [
    "power_strike",
    "swift_strikes",
    "arcane_power",
    "vitality"
  ],
  "skillLevels": {
    "power_strike": 3,
    "swift_strikes": 2,
    "arcane_power": 4,
    "vitality": 1
  }
}
```

---

## ?? Skill Progression Tree

### Combat Path
```
Power Strike (Lv1, 1SP)
    ?
Swift Strikes (Lv2, 1SP)
    ?
Critical Strike (Lv3, 2SP)
    ?
Combo Master (Lv5, 2SP) [Requires Power Strike]
```

### Magic Path
```
Arcane Power (Lv1, 1SP)
    ?
Spell Mastery (Lv4, 2SP) [Requires Arcane Power]
    ?
Widening Blast (Lv7, 3SP) [Requires Spell Mastery]
    ?
Elemental Focus (Lv10, 4SP) [Requires Widening Blast]
```

### Defense Path
```
Vitality (Lv1, 1SP)
    ?
Iron Skin (Lv3, 2SP)
    ?
Life Steal (Lv6, 3SP) [Requires Iron Skin]
```

### Utility Path
```
Fleet Footed (Lv2, 1SP)

Quick Learner (Lv1, 1SP)
```

---

## ?? Testing Checklist

### Basic Functionality
- [ ] SkillManager exists in scene
- [ ] Player has PlayerAttackSkillExtension
- [ ] Save station Skills button opens skill panel
- [ ] Skill list displays correctly
- [ ] Can filter skills by category
- [ ] Clicking skill shows details

### Progression
- [ ] Gaining XP updates UI
- [ ] Level up grants skill point
- [ ] Unlocking skill costs points
- [ ] Leveling up skill costs points
- [ ] Prerequisites prevent early unlock
- [ ] Level requirements work

### Combat Integration
- [ ] Power Strike increases damage
- [ ] Critical Strike shows crit messages
- [ ] Spell Mastery reduces cooldowns
- [ ] Vitality increases max health
- [ ] Life Steal heals on hit

### Save System
- [ ] Skills saved with game
- [ ] Skills loaded correctly
- [ ] Skill effects persist after load

---

## ? Debug Features

### In-Game Overlay (Press F12)

Shows real-time skill stats:
- Current level & XP
- Skill points available
- Unlocked skill count
- Total damage bonus

### Context Menu Debug Commands

Right-click SkillManager in Inspector:
- **Add 100 XP** - Quick level testing
- **Add 5 Skill Points** - Quick unlock testing
- **Print Skill Stats** - Detailed console log

### Console Logging

Enable "Show Debug Info" in SkillManager for:
- Skill unlock messages
- Level up notifications
- Bonus calculations
- Effect applications

---

## ?? Example: Adding Enemy XP Drops

### Method 1: In Enemy Death Event

```csharp
// In EnemyHealth.cs or similar:
private void Die()
{
    Debug.Log($"{gameObject.name} has died.");
    
    // Give XP to player
    SkillManager skillManager = SkillManager.Instance;
    if (skillManager != null)
    {
        int xpReward = 25; // Base XP
        skillManager.AddExperience(xpReward);
    }
    
    // Rest of death logic...
    Destroy(gameObject);
}
```

### Method 2: XP Component

```csharp
// Create new component: XPReward.cs
public class XPReward : MonoBehaviour
{
    [SerializeField] private int experienceAmount = 25;

    private void Start()
    {
        Health health = GetComponent<Health>();
        if (health != null)
        {
            health.OnDeath.AddListener(GiveExperience);
        }
    }

    private void GiveExperience()
    {
        SkillManager skillManager = SkillManager.Instance;
        if (skillManager != null)
        {
            skillManager.AddExperience(experienceAmount);
            Debug.Log($"Gained {experienceAmount} XP!");
        }
    }
}
```

Then attach `XPReward` component to all enemies!

---

## ? Advanced Customization

### Adding New Skills

1. **Create Skill Class** in `SkillDefinitions.cs`:

```csharp
public class MyNewSkill : Skill
{
    public MyNewSkill() : base(
        "my_new_skill",
        "My New Skill",
        "Description of what it does",
        SkillCategory.Combat,
        SkillTier.Basic)
    {
        maxLevel = 5;
        requirements = new SkillRequirement(3, 2);
        
        effects.Add(new SkillEffect
        {
            effectType = SkillEffectType.DamageBoost,
            baseValue = 10f,
            valuePerLevel = 5f,
            isPercentage = false
        });
    }

    public override void ApplyEffects(GameObject player)
    {
        // Custom logic here
    }

    public override void RemoveEffects(GameObject player)
    {
        // Cleanup here
    }
}
```

2. **Register in SkillManager**:

```csharp
// In SkillManager.RegisterAllSkills():
RegisterSkill(new MyNewSkill());
```

### Adjusting XP Scaling

```csharp
// In SkillManager Inspector:
Base Experience Per Level: 100    // Starting XP requirement
Experience Scaling: 1.2           // Multiplier per level

// Level progression:
// Lv1 ? Lv2: 100 XP
// Lv2 ? Lv3: 120 XP  (100 * 1.2)
// Lv3 ? Lv4: 144 XP  (120 * 1.2)
// etc...
```

### Custom Skill Points

```csharp
// Give bonus points on specific levels:
if (newLevel % 5 == 0) // Every 5 levels
{
    currentSkillData.skillPoints += 2; // Bonus point
}
```

---

## ?? Troubleshooting

### Skills Panel Doesn't Open
- Check SaveStationMenu has SkillsPanel assigned
- Verify SkillTreeUI component attached to panel
- Check SkillManager exists in scene

### Skills Don't Affect Combat
- Verify PlayerAttackSkillExtension on player
- Check SkillManager.Instance is not null
- Enable "Show Debug Info" and watch console

### XP Not Gained
- Check SkillManager exists before calling AddExperience
- Verify enemy death calls AddExperience
- Watch debug overlay (F12) for XP changes

### Skills Not Saving
- SaveManager integration is automatic
- Check SaveData.skills is not null
- Verify save/load cycle with debug

---

## ? Summary

? **13 Skills** across 4 categories
? **Full progression** with XP and leveling
? **Combat integration** - skills directly enhance gameplay
? **Save system** - progress persists
? **UI panel** in save station
? **Easy to extend** - add new skills easily

**Required Components:**
1. SkillManager (scene GameObject)
2. PlayerAttackSkillExtension (on Player)
3. SkillTreeUI (on Skills Panel)
4. Skill Button Prefab

**Next Steps:**
1. Add XP rewards to enemies
2. Design skill icons
3. Balance skill costs and effects
4. Add visual feedback for skill activation

The skill system is **production-ready** and fully integrated with your save and combat systems!
