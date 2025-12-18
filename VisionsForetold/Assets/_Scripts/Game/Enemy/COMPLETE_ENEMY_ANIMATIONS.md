# ?? Complete Enemy Animation Guide - All Types

## Overview

All 6 enemy types now have **complete animation systems** with idle, run, attack, death, and special animations!

---

## ? **Enemy Types & Animations**

### Standard Animations (ALL Enemies):
- ?? **Idle** - Standing/waiting
- ?? **Run** - Moving/chasing
- ?? **Attack** - Attacking player
- ?? **Death** - Dying

### Special Animations:
- ?? **Spawn** - Lich only (summoning units)
- ?? **Heal** - Revenant only (healing allies)

---

## ?? **Animation Parameters by Enemy**

### All Enemies Need (Base):
```
IsMoving ? Bool (movement state)
Speed ? Float (0-1, normalized speed)
Attack ? Trigger (attack action)
Death ? Trigger (death action)
```

### Lich Additional:
```
Spawn ? Trigger (summoning action)
```

### Revenant Additional:
```
Heal ? Trigger (healing action)
```

---

## ?? **Enemy-by-Enemy Setup**

### 1. Ghoul (Melee Attacker)

**Type:** Common melee enemy
**Variants:** Basic, Strong, Fast

**Animations:**
```
? Idle - Menacing stance
? Run - Aggressive sprint
? Attack - Bite attack
? Death - Collapse
```

**Parameters:**
```
IsMoving (Bool)
Speed (Float)
Attack (Trigger)
Death (Trigger)
```

**Attack Behavior:**
- Melee bite attack
- Damage: 15 (Basic), 25 (Strong), 12 (Fast)
- Cooldown: 1.5 seconds
- Animation triggers on attack

**Speed Variants:**
```
Basic: Run animation speed 1.0
Strong: Run animation speed 0.9 (heavier)
Fast: Run animation speed 1.3 (aggressive)
```

---

### 2. Ghost (Ranged Attacker)

**Type:** Common ranged enemy
**Variants:** Basic, Elite, Phantom

**Animations:**
```
? Idle - Floating/hovering
? Run - Float movement
? Attack - Soul blast cast
? Death - Dissipate
```

**Parameters:**
```
IsMoving (Bool)
Speed (Float)
Attack (Trigger)
Death (Trigger)
```

**Attack Behavior:**
- Ranged soul blast projectile
- Damage: 12 (Basic), 18 (Elite), 15 (Phantom)
- Cooldown: 2s (Basic), 1.5s (Elite), 1s (Phantom)
- Maintains distance from player
- Animation triggers before projectile spawn

**AI Pattern:**
```
Too far ? Chase
Too close ? Retreat
Optimal range ? Stop & Attack
```

---

### 3. Wraith (Aggressive Fighter)

**Type:** Uncommon aggressive enemy
**Variants:** Aggressive, Ranged

**Animations:**
```
? Idle - Menacing stance
? Run - Quick movement
? Attack - Claw swipe or magic explosion
? Death - Fade/dissipate
```

**Parameters:**
```
IsMoving (Bool)
Speed (Float)
Attack (Trigger)
Death (Trigger)
```

**Attack Behavior:**

**Aggressive Type:**
- Rapid claw attacks
- 3-hit combo
- Damage: 20 per hit
- Attack speed: 0.3 seconds
- Cooldown after combo: 1 second

**Ranged Type:**
- Magic explosion
- Damage: 30
- Attack range: 6 units
- Cooldown: 0.3 seconds

---

### 4. Spectre (Tank)

**Type:** Rare tank enemy
**Special:** Last stand mechanic

**Animations:**
```
? Idle - Heavy stance
? Run - Slow but determined
? Attack - Shockwave slam
? Death - Dramatic collapse
```

**Parameters:**
```
IsMoving (Bool)
Speed (Float)
Attack (Trigger)
Death (Trigger)
```

**Attack Behavior:**
- AOE shockwave attack
- Damage: 25
- Range: 8 units
- Cooldown: 6 seconds
- Stuns player for 2 seconds

**Last Stand (20% HP):**
- Charges at player
- Speed increases dramatically
- Charge damage: 40
- Explodes on contact

---

### 5. Revenant (Support/Healer) ? Special

**Type:** Uncommon support enemy
**Variants:** Healer, Buffer
**Special Animation:** Heal

**Animations:**
```
? Idle - Supportive stance
? Run - Cautious movement
? Attack - Support buff cast (uses Attack trigger)
? Heal - Healing cast ? UNIQUE
? Death - Fade away
```

**Parameters:**
```
IsMoving (Bool)
Speed (Float)
Attack (Trigger) ? Used for buff
Heal (Trigger) ? UNIQUE
Death (Trigger)
```

**Heal Animation Behavior:**

**When triggered:**
```
1. Revenant detects injured ally nearby
2. TriggerHealAnimation() called
3. Heal animation plays
4. Allied enemy heals for 15 HP
5. Returns to idle
```

**AI Pattern:**
```
Player too close (< 6 units) ? Retreat
Safe distance ? Stop and scan for allies
Ally found + cooldown ready ? Heal animation
Cooldown: 5 seconds
Support range: 10 units
```

**Healer Type:**
- Heals allies for 15 HP
- Cooldown: 5 seconds
- Heal animation triggers
- Multiple allies can be healed

**Buffer Type:**
- Applies attack buffs
- Cooldown: 8 seconds
- Uses attack animation
- Buff duration: 10 seconds

---

### 6. Lich (Summoner/Boss) ? Special

**Type:** Rare summoner enemy
**Special Animation:** Spawn
**Special:** Last stand mechanic

**Animations:**
```
? Idle - Commanding presence
? Run - Slow, dignified movement
? Attack - Hellfire cast
? Spawn - Summoning ritual ? UNIQUE
? Death - Dramatic disintegration
```

**Parameters:**
```
IsMoving (Bool)
Speed (Float)
Attack (Trigger) ? Hellfire
Spawn (Trigger) ? UNIQUE
Death (Trigger)
```

**Spawn Animation Behavior:**

**When triggered:**
```
1. Lich summon cooldown ready (10 seconds)
2. TriggerSpawnAnimation() called
3. Spawn animation plays
4. Over 3-6 seconds:
   - 2 Ghouls spawn
   - 2 Ghosts spawn
   - 1 Uncommon enemy spawn
5. Returns to idle
```

**Attack Behavior:**
- Hellfire spell
- Damage: 35
- Range: 12 units
- Cooldown: 3 seconds
- Attack animation triggers

**AI Pattern:**
```
Player too close (< 8 units) ? Retreat
Safe distance ? Summon minions (10s cooldown)
Player in range (? 12 units) ? Cast hellfire (3s cooldown)
```

**Last Stand (25% HP):**
- Ghost sword rain
- 10 swords over 3 seconds
- AOE pattern
- High damage

---

## ?? **Animator Setup Guide**

### Basic Setup (All Enemies):

**1. Create Animator Controller:**
```
Right-click in Project
Create ? Animator Controller
Name: "[EnemyName]Animator"
```

**2. Add Parameters:**
```
+ IsMoving (Bool)
+ Speed (Float)
+ Attack (Trigger)
+ Death (Trigger)

FOR LICH ONLY:
+ Spawn (Trigger)

FOR REVENANT ONLY:
+ Heal (Trigger)
```

**3. Create States:**
```
- Idle (default, orange)
- Run
- Attack
- Death

FOR LICH ONLY:
- Spawn

FOR REVENANT ONLY:
- Heal
```

**4. Assign Animation Clips:**
```
Click each state ? Inspector ? Motion ? Drag animation
```

**5. Setup Transitions:**

**Movement:**
```
Idle ? Run:
- Condition: IsMoving == true
- Has Exit Time: ?
- Duration: 0.15

Run ? Idle:
- Condition: IsMoving == false
- Has Exit Time: ?
- Duration: 0.15
```

**Attack:**
```
Any State ? Attack:
- Condition: Attack (Trigger)
- Has Exit Time: ?
- Can Transition To Self: ?
- Duration: 0.05

Attack ? Idle:
- Has Exit Time: ?
- Exit Time: 0.9
- Duration: 0.15
```

**Spawn (Lich Only):**
```
Any State ? Spawn:
- Condition: Spawn (Trigger)
- Has Exit Time: ?
- Can Transition To Self: ?
- Duration: 0.05

Spawn ? Idle:
- Has Exit Time: ?
- Exit Time: 0.9
- Duration: 0.2
```

**Heal (Revenant Only):**
```
Any State ? Heal:
- Condition: Heal (Trigger)
- Has Exit Time: ?
- Can Transition To Self: ?
- Duration: 0.05

Heal ? Idle:
- Has Exit Time: ?
- Exit Time: 0.9
- Duration: 0.2
```

**Death:**
```
Any State ? Death:
- Condition: Death (Trigger)
- Has Exit Time: ?
- Priority: Highest
- Duration: 0.05

Death: No return transition (final state)
```

---

## ?? **Animation Timing Recommendations**

### All Enemies:

**Idle:**
```
Duration: 2-4 seconds
Loop: ?
Type: Breathing, idle motion
Feel: Character personality
```

**Run:**
```
Duration: 0.5-1.0 second cycle
Loop: ?
Type: Movement style per enemy
Speed variants per type
```

**Attack:**
```
Duration: 0.5-1.0 seconds
Loop: ?
Type: Attack action
Damage: Mid-animation
```

**Death:**
```
Duration: 1.0-2.0 seconds
Loop: ?
Type: Collapse/fade
No return
```

---

### Special Animations:

**Spawn (Lich):**
```
Duration: 1.5-2.5 seconds
Loop: ?
Type: Summoning ritual
Effect: Dramatic casting
Timing: Units spawn during animation
```

**Heal (Revenant):**
```
Duration: 1.0-1.5 seconds
Loop: ?
Type: Healing cast/buff gesture
Effect: Healing particles
Timing: Heal applies mid-animation
```

---

## ?? **Testing Each Enemy**

### Ghoul Test:
```
? Spawns ? Idle plays
? Detects player ? Run plays
? Reaches player ? Attack plays
? Attack damages player
? Dies ? Death plays
```

### Ghost Test:
```
? Spawns ? Idle (floating)
? Chases ? Run (float movement)
? In range ? Attack (soul blast cast)
? Projectile spawns after animation
? Too close ? Retreats
? Dies ? Death (dissipate)
```

### Wraith Test:
```
? Idle plays
? Chases ? Run
? Attack ? Claw or explosion animation
? Combo timing correct
? Dies ? Death
```

### Spectre Test:
```
? Idle ? Heavy stance
? Run ? Slow movement
? Attack ? Shockwave animation
? Shockwave hits player
? Last stand at 20% HP
? Death ? Dramatic
```

### Revenant Test ?:
```
? Spawns ? Idle
? Maintains distance from player
? Detects injured ally
? Heal animation plays ?
? Ally heals
? Returns to idle
? Dies ? Death
```

### Lich Test ?:
```
? Spawns ? Idle (commanding)
? Maintains distance
? Spawn animation plays ?
? Minions appear during animation
? Attack ? Hellfire cast
? Last stand at 25% HP
? Death ? Dramatic disintegration
```

---

## ?? **Animation Import Settings**

### For ALL Enemy Animations:

```
Rig Tab:
- Animation Type: Humanoid (or Generic)
- Avatar: Create or Copy
- Apply

Animation Tab:
Loop Settings:
  ? Idle (loop)
  ? Run (loop)
  ? Attack (no loop)
  ? Spawn (no loop) ? Lich
  ? Heal (no loop) ? Revenant
  ? Death (no loop)

- Loop Pose: ? (for looping only)
- Root Transform Rotation: Bake Into Pose ?
- Root Transform Position (Y): Bake Into Pose ?
- Root Transform Position (XZ): Bake Into Pose ?
- Apply
```

---

## ?? **Common Issues**

### Spawn animation doesn't play (Lich):
```
? Spawn parameter exists as Trigger
? Spawn state created
? Spawn animation assigned
? Any State ? Spawn transition exists
? TriggerSpawnAnimation() called in SummonMinions()
? Cooldown working (10 seconds)
```

### Heal animation doesn't play (Revenant):
```
? Heal parameter exists as Trigger
? Heal state created
? Heal animation assigned
? Any State ? Heal transition exists
? TriggerHealAnimation() called in ProvideSupport()
? Injured allies nearby
? Cooldown working (5 seconds)
```

### Attack animation doesn't play:
```
? Attack parameter is Trigger (not Bool)
? TriggerAttackAnimation() called
? Any State ? Attack transition configured
? Has Exit Time unchecked on Any ? Attack
```

---

## ?? **Complete Setup Checklist**

### For Each Enemy:

```
Base Setup:
? Create [EnemyName]Animator
? Add IsMoving, Speed, Attack, Death parameters
? Create Idle, Run, Attack, Death states
? Assign animation clips
? Setup transitions

Special (If Applicable):
? Lich: Add Spawn parameter + state
? Revenant: Add Heal parameter + state

Code Verification:
? base.Update() called in child class
? TriggerAttackAnimation() called on attack
? TriggerSpawnAnimation() called (Lich only)
? TriggerHealAnimation() called (Revenant only)

Testing:
? All animations play correctly
? Transitions smooth
? Timing matches behavior
? Special animations work
```

---

## ?? **Enemy Animation Matrix**

| Enemy | Idle | Run | Attack | Death | Special | Unique Behavior |
|-------|------|-----|--------|-------|---------|-----------------|
| **Ghoul** | ? | ? | Bite | ? | - | 3 speed types |
| **Ghost** | Float | Float | Cast | Fade | - | Ranged, kiting |
| **Wraith** | ? | ? | Claw/Magic | ? | - | Rapid combo |
| **Spectre** | ? | ? | Shockwave | ? | - | Last stand |
| **Revenant** | ? | ? | Buff | ? | **Heal** ? | Support/Healer |
| **Lich** | ? | ? | Hellfire | ? | **Spawn** ? | Summoner/Boss |

---

## ?? **Pro Tips**

**Spawn Animation (Lich):**
```
Make it dramatic and long (2+ seconds)
Units can spawn gradually during animation
Add particle effects at summon points
Sound effect for each unit spawn
Lich glows/channels during cast
```

**Heal Animation (Revenant):**
```
Casting gesture toward ally
Healing particles from Revenant to ally
Green/holy glow effects
Sound effect for healing
Ally reacts with heal particle effect
```

**Visual Polish:**
```
Add VFX for each attack type
Different sounds per enemy type
Hit reactions on player
Death effects (fade, particles, etc.)
Special animation camera shake
```

**AI Coordination:**
```
Revenant + Lich combo is powerful
Revenant heals, Lich summons
Keep them apart for balance
Focus targets: Kill Revenant first!
```

---

## Summary

**Your Complete Enemy System:**
```
? 6 enemy types fully animated
? Base animations: Idle, Run, Attack, Death
? Special animations: Spawn (Lich), Heal (Revenant)
? Unified parameter system
? Automatic state management
? Easy to extend
? Professional, polished behavior
```

**Animation Parameters:**
```
All Enemies: IsMoving, Speed, Attack, Death
Lich: + Spawn
Revenant: + Heal
```

**Setup Time Per Enemy:**
```
Base enemies: ~15 minutes
Lich: +5 minutes (Spawn setup)
Revenant: +5 minutes (Heal setup)
```

**Result:**
Complete, professional enemy AI with unique animations and behaviors! ????????

**Your enemy animation system is production-ready!**
