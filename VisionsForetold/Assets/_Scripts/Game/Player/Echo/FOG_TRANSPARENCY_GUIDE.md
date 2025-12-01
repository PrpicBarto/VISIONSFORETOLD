# Fog Transparency & Player Visibility Guide

## Visual Comparison

### Pure Black Fog (Old Default - Too Opaque)
```
╔═══════════════════════╗
║ ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓ ║  <- 100% opaque
║ ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓ ║     Can't see ANYTHING
║ ▓▓▓▓▓▓? P ?▓▓▓▓▓▓▓▓▓ ║     P = Player (hidden!)
║ ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓ ║     ? = Enemy (completely hidden)
║ ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓ ║
╚═══════════════════════╝

Settings:
  Fog Density: 1.0
  Fog Min Density: 0.95
  Fog Max Density: 1.0
  Permanent Reveal Radius: 0
```

**Problem:** Player can't see themselves! Too dark.

---

### Semi-Transparent Fog (New Default - Balanced)
```
╔═══════════════════════╗
║ ░░░░░░░░░░░░░░░░░░░░ ║  <- 85% opaque
║ ░░░░░░░░░░░░░░░░░░░░ ║     Can see vague shapes
║ ░░░░░   P   ░░░░░░░░ ║     P = Player (always visible!)
║ ░░░░? ░░░░░░░ ?░░░░░ ║     ? = Enemy (silhouette visible)
║ ░░░░░░░░░░░░░░░░░░░░ ║
╚═══════════════════════╝

Settings:
  Fog Density: 0.85
  Fog Min Density: 0.7
  Fog Max Density: 0.95
  Permanent Reveal Radius: 3
```

**Result:** Player always visible + can see enemy shapes!

---

### Very Transparent Fog (Optional - Easy Mode)
```
╔═══════════════════════╗
║ ░░░░░░░░░░░░░░░░░░░░ ║  <- 60% opaque
║ ░ ? ░░░░░░░░░░░ ? ░ ║     Can see enemies clearly
║ ░░░░░   P   ░░░░░░░░ ║     P = Player (fully visible)
║ ░░░ ? ░░░░░░░ ? ░░░ ║     ? = Enemy (clearly visible)
║ ░░░░░░░░░░░░░░░░░░░░ ║
╚═══════════════════════╝

Settings:
  Fog Density: 0.6
  Fog Min Density: 0.4
  Fog Max Density: 0.8
  Permanent Reveal Radius: 5
```

**Result:** Enemies visible but still somewhat hidden.

---

## Settings Breakdown

### 1. Fog Density (Master Opacity)
**Location:** Fog & Reveal Settings → Fog Density

Controls overall fog opacity:
- `1.0` = Completely opaque (pure black)
- `0.85` = **Recommended** (balanced visibility)
- `0.7` = Semi-transparent (can see through)
- `0.5` = Very transparent (minimal fog)

**Effect:**
```
Density 1.0:  ▓▓▓▓▓▓  (solid black)
Density 0.85: ▓▓▓▓▓░  (mostly opaque)
Density 0.7:  ▓▓▓░░░  (semi-transparent)
Density 0.5:  ▓▓░░░░  (very transparent)
```

---

### 2. Fog Min/Max Density (Distance Gradient)
**Location:** Distance-Based Fog Density

Controls how fog opacity changes with distance:

**Min Density (near player):**
- `0.95` = Very dense even near player
- `0.7` = **Recommended** - lighter near player
- `0.5` = Quite transparent near player
- `0.3` = Very light near player

**Max Density (far from player):**
- `1.0` = Pure black far away
- `0.95` = **Recommended** - very dark far away
- `0.8` = Semi-dark far away
- `0.6` = Light even far away

**Visual Effect:**
```
Distance Gradient (Min 0.7, Max 0.95):

Near Player     Mid Distance    Far Away
    ░░              ▓░             ▓▓
   (70%)           (85%)          (95%)
```

---

### 3. Permanent Reveal Radius (Player Visibility)
**Location:** Fog & Reveal Settings → Permanent Reveal Radius

Keeps area around player always visible:

**Radius = 0:**
```
  ▓▓▓▓▓▓▓
  ▓▓▓P▓▓▓  <- Player hidden!
  ▓▓▓▓▓▓▓
```

**Radius = 3 (Recommended):**
```
  ▓▓▓▓▓▓▓
  ▓░░░░░▓
  ▓░ P ░▓  <- Player visible!
  ▓░░░░░▓
  ▓▓▓▓▓▓▓
```

**Radius = 5 (Generous):**
```
  ▓▓▓▓▓▓▓▓▓
  ▓░░░░░░░▓
  ▓░░░░░░░▓
  ▓░░ P ░░▓  <- Player + area visible!
  ▓░░░░░░░▓
  ▓░░░░░░░▓
  ▓▓▓▓▓▓▓▓▓
```

**Values:**
- `0` = No reveal (player hidden in fog)
- `2` = Tight (just player visible)
- `3` = **Recommended** (player + immediate area)
- `5` = Comfortable (player + surroundings)
- `10` = Very generous (large clear area)

---

### 4. Fog Color Alpha
**Location:** Fog & Reveal Settings → Fog Color

The alpha channel of fog color affects overall transparency:

```
Fog Color (R, G, B, A):
  (0, 0, 0, 1.0)   = Solid black
  (0, 0, 0, 0.85)  = Recommended - semi-transparent
  (0, 0, 0, 0.7)   = Transparent
  (0, 0, 0, 0.5)   = Very transparent
```

---

## Preset Configurations

### Preset 1: Horror Mode (Very Dark)
```csharp
fogDensity = 0.95f;
fogMinDensity = 0.9f;
fogMaxDensity = 1.0f;
fogColor = new Color(0, 0, 0, 0.95f);
permanentRevealRadius = 2f;
```

**Feel:** Tense, scary, limited visibility
**Use:** Horror games, stealth games

---

### Preset 2: Balanced Mode (Recommended)
```csharp
fogDensity = 0.85f;
fogMinDensity = 0.7f;
fogMaxDensity = 0.95f;
fogColor = new Color(0, 0, 0, 0.85f);
permanentRevealRadius = 3f;
```

**Feel:** Challenging but fair, can see player clearly
**Use:** Most action/adventure games

---

### Preset 3: Easy Mode (Light Fog)
```csharp
fogDensity = 0.6f;
fogMinDensity = 0.4f;
fogMaxDensity = 0.8f;
fogColor = new Color(0, 0, 0, 0.6f);
permanentRevealRadius = 5f;
```

**Feel:** Forgiving, enemies visible as silhouettes
**Use:** Casual games, accessibility mode

---

### Preset 4: Minimal Fog (Atmospheric Only)
```csharp
fogDensity = 0.4f;
fogMinDensity = 0.2f;
fogMaxDensity = 0.6f;
fogColor = new Color(0, 0, 0, 0.4f);
permanentRevealRadius = 10f;
```

**Feel:** Light atmosphere, mostly for aesthetics
**Use:** Games where fog is just visual flair

---

## Testing & Tuning

### Step-by-Step Tuning Process:

1. **Start with Balanced Preset:**
   ```
   Fog Density: 0.85
   Permanent Reveal Radius: 3
   ```

2. **Test Player Visibility:**
   - Can't see player? Increase Reveal Radius (3 → 5)
   - Player too visible? Decrease Reveal Radius (3 → 2)

3. **Test Enemy Visibility:**
   - Enemies too hidden? Decrease Fog Density (0.85 → 0.7)
   - Enemies too visible? Increase Fog Density (0.85 → 0.95)

4. **Test Distance Gradient:**
   - Fog too uniform? Increase difference between Min/Max
   - Too much variation? Decrease difference between Min/Max

5. **Fine-Tune:**
   - Adjust in increments of 0.05
   - Test with actual gameplay
   - Get feedback from players

---

## Common Issues & Fixes

### "Player is invisible in fog!"
**Fix:**
```
Permanent Reveal Radius: 3-5
Fog Min Density: 0.7 (reduce from 0.95)
```

### "Fog is too light, no atmosphere"
**Fix:**
```
Fog Density: 0.9-0.95
Fog Max Density: 1.0
```

### "Can't see enemy shapes at all"
**Fix:**
```
Fog Density: 0.7-0.8
Fog Color Alpha: 0.7-0.8
```

### "Fog looks patchy/uneven"
**Fix:**
```
Enable Multiple Planes: ☑
Vertical Plane Count: 5-10
```

---

## Summary: Quick Reference

| Setting | Horror | Balanced | Easy | Minimal |
|---------|--------|----------|------|---------|
| **Fog Density** | 0.95 | 0.85 | 0.6 | 0.4 |
| **Min Density** | 0.9 | 0.7 | 0.4 | 0.2 |
| **Max Density** | 1.0 | 0.95 | 0.8 | 0.6 |
| **Fog Alpha** | 0.95 | 0.85 | 0.6 | 0.4 |
| **Reveal Radius** | 2m | 3m | 5m | 10m |

**Most Popular:** Balanced Mode (85% opacity, 3m reveal)
