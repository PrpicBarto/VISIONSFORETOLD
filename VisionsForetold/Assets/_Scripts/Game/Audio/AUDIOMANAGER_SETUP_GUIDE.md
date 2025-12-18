# ?? AudioManager - Complete Setup Guide

## Overview

Your AudioManager provides:
- **Scene Persistence** - Survives scene transitions (DontDestroyOnLoad)
- **Music Crossfading** - Smooth transitions between tracks
- **Combat Music System** - Automatic switching between passive/aggressive
- **Volume Control** - Master, Music, and SFX volumes
- **Sound Effects** - One-shot SFX support

---

## ? **Quick Setup**

### Step 1: Create AudioManager GameObject

**In any scene (usually Main Menu):**
```
1. Right-click in Hierarchy
2. Create Empty GameObject
3. Name it: "AudioManager"
4. Add Component ? AudioManager script
```

**The AudioManager will persist across all scenes automatically!**

---

### Step 2: Assign Music Tracks

**Select AudioManager in Hierarchy:**

```
Inspector ? AudioManager component:

Music Tracks:
?? Main Menu Music: [Drag your main menu music clip]
?? Point Click Music: [Drag your point & click scene music]
?? Combat Passive Music: [Drag exploration/passive combat music]
?? Combat Aggressive Music: [Drag active combat music]

Note: Audio Sources will be created automatically
```

---

### Step 3: Configure Settings (Optional)

**Volume Settings:**
```
Master Volume: 1.0 (100%)
Music Volume: 0.7 (70%)
SFX Volume: 1.0 (100%)
```

**Transition Settings:**
```
Music Fade Duration: 2.0 seconds
Combat Transition Duration: 1.5 seconds
Combat Exit Delay: 3.0 seconds
```

**Debug:**
```
Show Debug Logs: ? (check for testing)
```

---

## ?? **How It Works**

### Automatic Scene Detection

**Music plays automatically based on scene name:**

```
Scene name contains "MainMenu" or "Menu"
? Plays Main Menu Music

Scene name contains "PointNClick" or "PointAndClick"
? Plays Point & Click Music

Any other scene
? Plays Combat Passive Music (exploration)
```

---

### Combat Music System

**Passive (Exploration):**
```
Default state when not in combat
Calm, atmospheric music
Plays when exploring
```

**Aggressive (Combat):**
```
Plays when EnterCombat() is called
Intense, action music
Automatically fades back to passive after combat ends
3 second delay before switching back
```

---

## ?? **Usage in Code**

### Access AudioManager

```csharp
// Get instance from anywhere
AudioManager audioManager = AudioManager.Instance;
```

---

### Combat Music Control

**Enter Combat:**
```csharp
// Call when player enters combat
AudioManager.Instance.EnterCombat();
```

**Exit Combat:**
```csharp
// Call when combat ends
AudioManager.Instance.ExitCombat();
```

**Example in Enemy AI:**
```csharp
public class Enemy : MonoBehaviour
{
    private void OnPlayerDetected()
    {
        // Player spotted - enter combat
        AudioManager.Instance.EnterCombat();
    }
    
    private void OnDeath()
    {
        // Enemy died - check if last enemy
        if (IsLastEnemy())
        {
            AudioManager.Instance.ExitCombat();
        }
    }
}
```

---

### Manual Music Control

**Play Specific Track:**
```csharp
AudioManager.Instance.PlayMainMenuMusic();
AudioManager.Instance.PlayPointClickMusic();
AudioManager.Instance.PlayCombatPassiveMusic();
AudioManager.Instance.PlayCombatAggressiveMusic();
```

**Stop/Pause/Resume:**
```csharp
AudioManager.Instance.StopMusic();
AudioManager.Instance.PauseMusic();
AudioManager.Instance.ResumeMusic();
```

---

### Sound Effects

**Play One-Shot SFX:**
```csharp
public AudioClip buttonClick;

// Play at default volume
AudioManager.Instance.PlaySFX(buttonClick);

// Play with custom volume (0-1)
AudioManager.Instance.PlaySFX(buttonClick, 0.5f);

// Play at specific 3D position
Vector3 position = transform.position;
AudioManager.Instance.PlaySFXAtPosition(explosionSound, position);
```

---

### Volume Control

**From Code:**
```csharp
// Set master volume (0-1)
AudioManager.Instance.SetMasterVolume(0.8f);

// Set music volume (0-1)
AudioManager.Instance.SetMusicVolume(0.6f);

// Set SFX volume (0-1)
AudioManager.Instance.SetSFXVolume(1.0f);
```

**From Inspector:**
```
AudioManager component ? Volume Settings
Adjust sliders while game is running
Changes apply in real-time
```

---

## ?? **Integration Examples**

### Example 1: Combat Manager

```csharp
public class CombatManager : MonoBehaviour
{
    private int enemiesInCombat = 0;
    
    public void OnEnemyEnterCombat()
    {
        enemiesInCombat++;
        
        if (enemiesInCombat == 1)
        {
            // First enemy - enter combat
            AudioManager.Instance.EnterCombat();
        }
    }
    
    public void OnEnemyExitCombat()
    {
        enemiesInCombat--;
        
        if (enemiesInCombat <= 0)
        {
            // No more enemies - exit combat
            enemiesInCombat = 0;
            AudioManager.Instance.ExitCombat();
        }
    }
}
```

---

### Example 2: Player Health

```csharp
public class PlayerHealth : MonoBehaviour
{
    public AudioClip hurtSound;
    public AudioClip healSound;
    
    public void TakeDamage(int damage)
    {
        health -= damage;
        
        // Play hurt sound
        AudioManager.Instance.PlaySFX(hurtSound);
        
        // Enter combat (taking damage = in combat)
        AudioManager.Instance.EnterCombat();
    }
    
    public void Heal(int amount)
    {
        health += amount;
        
        // Play heal sound
        AudioManager.Instance.PlaySFX(healSound);
    }
}
```

---

### Example 3: Menu Buttons

```csharp
public class MenuButton : MonoBehaviour
{
    public AudioClip clickSound;
    
    public void OnButtonClick()
    {
        // Play click sound
        AudioManager.Instance.PlaySFX(clickSound);
        
        // Your button logic...
    }
}
```

---

### Example 4: Enemy Detection

```csharp
public class Enemy : MonoBehaviour
{
    private void OnPlayerDetected()
    {
        // Player detected - enter combat
        AudioManager.Instance.EnterCombat();
        
        ChasePlayer();
    }
    
    private void OnPlayerLost()
    {
        // Player escaped - exit combat after delay
        AudioManager.Instance.ExitCombat();
        
        ReturnToPatrol();
    }
}
```

---

## ?? **Combat Music Behavior**

### Transition Flow:

**1. Exploration (Passive Music):**
```
Player exploring
Passive combat music playing
Calm, atmospheric
```

**2. Enemy Detected (Enter Combat):**
```
Enemy spots player
EnterCombat() called
Crossfade to aggressive music (1.5 seconds)
Intense combat music plays
```

**3. Combat Ongoing:**
```
Multiple enemies can call EnterCombat()
Music stays on aggressive track
Won't restart if already playing
```

**4. Combat Ends (Exit Combat):**
```
Last enemy dies or loses player
ExitCombat() called
Waits 3 seconds
Crossfades back to passive music (1.5 seconds)
Returns to exploration music
```

**5. Re-Enter Combat:**
```
If enemy detected during 3-second delay
Delay is cancelled
Stays on aggressive music
```

---

## ?? **Settings Explained**

### Music Fade Duration (2.0s)
```
How long scene music transitions take
Longer = smoother but slower
Shorter = faster but more abrupt
Recommended: 1.5 - 3.0 seconds
```

### Combat Transition Duration (1.5s)
```
How long combat music switches take
Should be faster than scene transitions
Responsive combat feel
Recommended: 1.0 - 2.0 seconds
```

### Combat Exit Delay (3.0s)
```
Time before switching from aggressive to passive
Prevents rapid switching if enemies re-engage
Gives player breathing room
Recommended: 2.0 - 5.0 seconds
```

---

## ?? **Scene Name Detection**

### Automatic Music by Scene Name:

**Main Menu:**
```
Scene names that work:
- "MainMenu"
- "Menu"
- "Main Menu"
- "StartMenu"

Plays: Main Menu Music
```

**Point & Click:**
```
Scene names that work:
- "PointNClick"
- "PointAndClick"
- "PointClick"
- "PNC"

Plays: Point & Click Music
```

**Game Scenes:**
```
Any other scene name:
- "GameScene"
- "Level1"
- "Combat"
- etc.

Plays: Combat Passive Music (default)
```

**Custom Scene Music:**
```csharp
// In your scene's initialization script
void Start()
{
    // Force specific music for this scene
    AudioManager.Instance.PlayPointClickMusic();
}
```

---

## ?? **Advanced Features**

### Check Current State

```csharp
// Is music playing?
bool isPlaying = AudioManager.Instance.IsMusicPlaying();

// Is player in combat?
bool inCombat = AudioManager.Instance.IsInCombat;

// What's currently playing?
string trackName = AudioManager.Instance.GetCurrentMusicName();
Debug.Log($"Now playing: {trackName}");
```

---

### Crossfade System

**How it works:**
```
1. New music starts at 0% volume
2. Both tracks play simultaneously
3. Old music fades out (100% ? 0%)
4. New music fades in (0% ? 100%)
5. Old music stops
6. New music continues

Result: Smooth, seamless transition
No silence between tracks
Professional audio experience
```

---

### Dual Audio Source System

**Why two music sources?**
```
Source 1: Plays current track
Source 2: Plays next track during crossfade

They swap roles after each transition
Enables seamless crossfading
No interruptions or gaps
```

---

## ?? **Testing**

### Test Checklist:

```
? AudioManager persists between scenes
? Main menu music plays in main menu
? Point & click music plays in PNC scene
? Combat passive plays in game scene
? EnterCombat() switches to aggressive music
? ExitCombat() switches back to passive after delay
? Multiple EnterCombat() calls don't restart music
? Volume sliders work in Inspector
? SFX plays correctly
? Music crossfades smoothly
```

---

### Debug Testing:

**Enable Debug Logs:**
```
AudioManager ? Show Debug Logs: ?
```

**Console Output:**
```
[AudioManager] Initialized and persisted across scenes
[AudioManager] Scene loaded: GameScene
[AudioManager] Playing passive combat music (exploration)
[AudioManager] Entered combat
[AudioManager] Playing aggressive combat music
[AudioManager] Exiting combat (delay: 3s)
[AudioManager] Exited combat - playing passive music
```

---

## ?? **Setup Checklist**

```
AudioManager Setup:
? Create AudioManager GameObject
? Add AudioManager script
? Assign Main Menu Music clip
? Assign Point & Click Music clip
? Assign Combat Passive Music clip
? Assign Combat Aggressive Music clip
? Configure volume settings
? Configure transition settings
? Enable debug logs for testing

Integration:
? Call EnterCombat() when combat starts
? Call ExitCombat() when combat ends
? Use PlaySFX() for sound effects
? Test scene transitions
? Test combat music switching
? Test volume controls

Audio Clips:
? Import all music tracks
? Set to Streaming for large files
? Verify Loop enabled for music
? Verify Loop disabled for SFX
```

---

## ?? **Common Issues**

### Music doesn't play:
```
? Audio clips assigned in Inspector?
? Volume settings too low?
? AudioSource components created?
? Check Console for warnings
```

### Music doesn't change between scenes:
```
? AudioManager has DontDestroyOnLoad?
? Scene names match detection patterns?
? Check Debug logs for scene detection
```

### Combat music doesn't switch:
```
? EnterCombat() being called?
? ExitCombat() being called?
? Aggressive music clip assigned?
? Check Debug logs for state changes
```

### Multiple AudioManagers:
```
? Only create AudioManager in one scene
? It will persist automatically
? Duplicates are destroyed automatically
```

---

## ?? **Quick Reference**

**Common Methods:**
```csharp
// Combat control
AudioManager.Instance.EnterCombat();
AudioManager.Instance.ExitCombat();

// Music control
AudioManager.Instance.PlayMainMenuMusic();
AudioManager.Instance.PlayCombatPassiveMusic();

// Sound effects
AudioManager.Instance.PlaySFX(clip);
AudioManager.Instance.PlaySFXAtPosition(clip, position);

// Volume
AudioManager.Instance.SetMasterVolume(0.8f);
AudioManager.Instance.SetMusicVolume(0.7f);

// Playback
AudioManager.Instance.PauseMusic();
AudioManager.Instance.ResumeMusic();
AudioManager.Instance.StopMusic();
```

---

## Summary

**Your AudioManager provides:**
```
? Persistent across all scenes (DontDestroyOnLoad)
? Automatic scene music detection
? Smooth crossfading between tracks
? Combat music system (passive ? aggressive)
? Combat state management with delay
? Sound effects support
? Volume control (Master, Music, SFX)
? Pause/Resume functionality
? Debug logging
? Easy integration with any code
```

**Setup time:** ~5 minutes
**Integration:** Simple method calls
**Result:** Professional audio system! ???

**Your game now has a complete, professional audio system with combat music transitions!**
