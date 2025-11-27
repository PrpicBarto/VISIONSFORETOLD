# Melee Combo System - Three-Hit Combo with Final Hit Multiplier

## ? **Update Complete!**

The PlayerAttack system now features a **three-piece melee combo** where the final slash deals **multiplied damage** for a satisfying combat flow!

## ?? **How It Works**

### Combo Progression

```
Hit 1: Normal Damage (10)
   ?
Hit 2: Normal Damage (10)
   ?
Hit 3: FINAL HIT! Multiplied Damage (10 × 2.5 = 25) ?
   ?
Combo Resets ? Back to Hit 1
```

### Visual Feedback

**UI Display:**
```
Combo: ? ? ? (1/3)     ? First hit
Combo: ? ? ? (2/3)     ? Second hit
Combo: ? ? ? (3/3) [FINAL HIT READY!] ? Final hit charged!
```

**Console Logs:**
```
Combo 1/3 - Damage: 10
Combo 2/3 - Damage: 10
FINAL HIT! Combo 3/3 - Damage: 25 (x2.5)
CRITICAL HIT on Enemy_Ghoul!
```

## ?? **Configuration Settings**

### Inspector Settings

```
Melee Combo Settings:
?? Combo Count: 3              ? Number of hits in combo
?? Combo Window: 1.5           ? Time to continue combo (seconds)
?? Combo Reset Delay: 0.5      ? Delay before combo restarts
?? Final Hit Damage Multiplier: 2.5 ? Damage multiplier for last hit
?? Combo Text: [Assign UI Text] ? Shows combo progress
```

### Customization Examples

#### **Fast Combo (Aggressive)**
```csharp
comboCount = 3
comboWindow = 1.0f           // Faster window
comboResetDelay = 0.3f       // Quick reset
finalHitDamageMultiplier = 3.0f  // Higher damage!
```

#### **Slow Power Combo (Strategic)**
```csharp
comboCount = 3
comboWindow = 2.5f           // Generous window
comboResetDelay = 1.0f       // Longer reset
finalHitDamageMultiplier = 4.0f  // Massive damage!
```

#### **Five-Hit Combo (Advanced)**
```csharp
comboCount = 5               // Extended combo
comboWindow = 1.5f
comboResetDelay = 0.5f
finalHitDamageMultiplier = 3.5f  // Rewarding finish
```

## ?? **Gameplay Mechanics**

### Combo Window System

**Timing Requirements:**
- Attack within **1.5 seconds** of previous hit ? Combo continues
- Wait longer than 1.5 seconds ? **Combo resets** to Hit 1
- After final hit ? **0.5 second delay** before new combo can start

**Example Timeline:**
```
0.0s: Hit 1 (10 damage)
0.8s: Hit 2 (10 damage)
1.5s: Hit 3 FINAL (25 damage) ? Within window!
2.0s: Combo resets (0.5s delay)
2.5s: Hit 1 (10 damage) ? New combo begins
```

### Damage Calculation

```csharp
// Hit 1 & 2: Normal damage
damage = attackDamage; // 10

// Hit 3: Multiplied damage
damage = attackDamage × finalHitDamageMultiplier; // 10 × 2.5 = 25
```

**With Different Base Damage:**
```
attackDamage = 15:
  Hit 1: 15
  Hit 2: 15  
  Hit 3: 37 (15 × 2.5)

attackDamage = 20:
  Hit 1: 20
  Hit 2: 20
  Hit 3: 50 (20 × 2.5)
```

### Combo Reset Conditions

Combo resets when:
1. ? **Combo window expires** (>1.5s between hits)
2. ? **Final hit lands** (after 0.5s delay)
3. ? **Player switches modes** (Melee ? Ranged/Spell)

## ?? **UI Implementation**

### Combo Text Display

**Visual Progression:**
```
? ? ?         ? No combo active
? ? ? (1/3)   ? First hit
? ? ? (2/3)   ? Second hit
? ? ? (3/3) [FINAL HIT READY!] ? Ready for finisher!
```

**Symbols:**
- `?` = Empty/Uncharged hit
- `?` = Charged hit
- `?` = Final hit (special)

### Setup UI Text

1. **Create UI Text:**
   ```
   Canvas ? Right-click ? UI ? Text - TextMeshPro
   Name: "ComboText"
   ```

2. **Position:**
   ```
   Bottom center of screen
   Or near player health bar
   ```

3. **Assign:**
   ```
   PlayerAttack Component:
     Combo Text: [Drag ComboText here]
   ```

### Example UI Layout
```
???????????????????????????
?  Health: ?????????? 80  ?
?  Combo: ? ? ? (2/3)     ? ? Combo display
?  Mode: Melee            ?
???????????????????????????
```

## ?? **Strategic Gameplay**

### When to Use Final Hit

**Optimal Targets:**
- ? **High-health enemies** (Lich, Spectre bosses)
- ? **Clustered groups** (save for strongest enemy)
- ? **Last enemy** in a fight (guaranteed kill)

**Avoid Wasting:**
- ? Low-health enemies (1-2 hits to kill)
- ? When surrounded (might miss)
- ? When combo is about to expire

### Combo Strategies

#### **Aggro Rush**
```
Fast attacks ? Build to final hit ? Execute enemy
Requires: Good timing, enemy tracking
```

#### **Patient Power**
```
Wait for opening ? Charge combo ? Unleash final hit
Requires: Defensive play, positioning
```

#### **Group Control**
```
Hit weak enemies (1-2) ? Save final for elite/boss
Requires: Target prioritization
```

## ?? **Technical Details**

### Code Flow

```csharp
PerformMeleeAttack()
  ?? Check combo window expired? ? Reset if yes
  ?? Increment combo step (1 ? 2 ? 3)
  ?? Calculate damage:
  ?    if (currentComboStep == 3)
  ?        damage = attackDamage × multiplier
  ?    else
  ?        damage = attackDamage
  ?? Perform raycast attack
  ?? Deal damage to target
  ?? If final hit ? Schedule reset after delay
```

### Combo State Tracking

```csharp
currentComboStep = 0;        // Current position in combo
lastComboHitTime = -Infinity; // Last successful hit time
comboResetInProgress = false; // Prevents double-reset
```

### Reset Logic

```csharp
// Automatic reset on window expire
if (Time.time > lastComboHitTime + comboWindow)
{
    ResetCombo();
}

// Manual reset after final hit
if (isFinalHit)
{
    Invoke(nameof(ResetCombo), comboResetDelay);
}
```

## ?? **Damage Comparison**

### Standard Attacks (No Combo)
```
3 separate attacks: 10 + 10 + 10 = 30 total damage
```

### With Combo System
```
1 full combo: 10 + 10 + 25 = 45 total damage
Difference: +15 damage (50% increase!)
```

### Boss Fight Example
```
Boss Health: 120

Without Combo:
  120 ÷ 10 = 12 hits required

With Combo:
  4 combos (45 × 4 = 180 damage)
  = 9 hits required (3 fewer hits!)
```

## ?? **Player Experience**

### Visual Feedback

**Per Hit:**
- ? UI updates combo progress
- ? Console logs hit number
- ? Damage dealt to enemy

**Final Hit:**
- ? **"FINAL HIT!"** console message
- ? **"CRITICAL HIT!"** on enemy
- ? **? symbol** in UI
- ? **Yellow highlight** when ready
- ? **Increased damage** visual (enemy reaction)

### Audio Suggestions (Optional)

Add these for enhanced feedback:
```csharp
// In PerformMeleeAttack():
if (currentComboStep == 1)
    AudioManager.PlaySound("MeleeHit1");
else if (currentComboStep == 2)
    AudioManager.PlaySound("MeleeHit2");
else if (isFinalHit)
    AudioManager.PlaySound("MeleeHitFinal"); // Heavier sound
```

## ?? **Testing Guide**

### Basic Combo Test
1. Enter Melee mode
2. Attack enemy 3 times quickly
3. **Expected:**
   - Hit 1: 10 damage, ? ? ?
   - Hit 2: 10 damage, ? ? ?
   - Hit 3: 25 damage, ? ? ? + "FINAL HIT!"

### Window Expiry Test
1. Hit enemy once
2. **Wait 2 seconds** (longer than window)
3. Hit again
4. **Expected:** Combo resets, back to Hit 1 (? ? ?)

### Final Hit Reset Test
1. Complete full combo (3 hits)
2. Immediately try to attack
3. **Expected:** 0.5s delay, then new combo starts

### Multi-Enemy Test
1. Attack Enemy A twice (? ? ?)
2. Switch to Enemy B
3. Attack Enemy B
4. **Expected:** Final hit triggers (? ? ?) on Enemy B

## ? **Performance Notes**

**Impact:** Minimal
- ? Simple integer tracking (combo step)
- ? Single float comparison (time check)
- ? No additional raycasts
- ? Lightweight UI updates

**Optimizations Applied:**
- Combo text only updates in Melee mode
- Reset check only when combo active
- No coroutines (uses Invoke for delay)

## ?? **Common Issues & Solutions**

### Issue: Combo doesn't progress

**Check:**
- [ ] Attacking fast enough (within 1.5s window)
- [ ] In Melee mode (not Ranged/Spell)
- [ ] Actually hitting enemies (check raycast)

### Issue: Combo resets too fast

**Solution:**
```csharp
comboWindow = 1.5f ? 2.5f  // Increase window
```

### Issue: Final hit not dealing extra damage

**Check:**
- [ ] `finalHitDamageMultiplier` > 1.0
- [ ] Enemy has Health component
- [ ] Console shows "FINAL HIT!" message

### Issue: Combo UI not showing

**Check:**
- [ ] Combo Text assigned in Inspector
- [ ] UI Canvas active
- [ ] In Melee mode
- [ ] At least 1 combo hit registered

## ?? **Future Enhancements**

### Potential Additions

**Different Final Hits:**
```csharp
// Spinning slash (AOE)
if (isFinalHit)
{
    DealAreaDamage(transform.position, 3f, damage);
}
```

**Combo Variation:**
```csharp
// 3-3-5 combo chain
comboChain = [3, 3, 5];  // 3 hits, then 3, then 5 for ultimate
```

**Elemental Finishers:**
```csharp
// Fire/Ice/Lightning effects on final hit
if (isFinalHit && hasFireBuff)
{
    ApplyBurnEffect(target);
}
```

**Animation Integration:**
```csharp
if (animator != null)
{
    animator.SetInteger("ComboStep", currentComboStep);
    animator.SetTrigger(isFinalHit ? "FinalSlash" : "Attack");
}
```

## ?? **Quick Reference**

### Default Values
```
Combo Count: 3
Combo Window: 1.5 seconds
Reset Delay: 0.5 seconds
Final Multiplier: 2.5x
Base Damage: 10

Full Combo Damage: 10 + 10 + 25 = 45
```

### Key Variables
```csharp
currentComboStep      // 0-3 (current hit in combo)
lastComboHitTime      // When last hit landed
comboResetInProgress  // Prevents double-reset
```

### Inspector References
```
Attack Settings ? Attack Damage: 10
Melee Combo Settings ? Final Hit Damage Multiplier: 2.5
Melee Combo Settings ? Combo Text: [UI Text]
```

---

**Status:** Melee combo system fully implemented!
**Build:** ? Success
**Ready for Testing:** ? YES
**Recommended Settings:** Default (3-hit, 2.5x multiplier)
