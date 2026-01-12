# ?? "To Be Continued" Screen Setup Guide

## Overview
Shows a dramatic "To Be Continued" screen when the Chaosmancer boss is defeated, then returns to the main menu.

---

## ?? Quick Setup (5 Minutes)

### **Step 1: Create UI Canvas**

1. **Create Canvas:**
   ```
   Hierarchy ? Right-click ? UI ? Canvas
   Name: "ToBeContinuedCanvas"
   
   Settings:
   - Render Mode: Screen Space - Overlay
   - Pixel Perfect: ? (optional)
   - Sort Order: 100 (render on top)
   ```

2. **Add Fade Panel:**
   ```
   ToBeContinuedCanvas ? Right-click ? UI ? Image
   Name: "FadePanel"
   
   Settings:
   - Anchor: Stretch (full screen)
   - Color: Black (0, 0, 0, 255)
   - Add Component: Canvas Group
     - Alpha: 0
     - Blocks Raycasts: ?
   ```

3. **Add Main Text:**
   ```
   ToBeContinuedCanvas ? Right-click ? UI ? Text
   Name: "ToBeContinuedText"
   
   Settings:
   - Text: "TO BE CONTINUED..."
   - Font: Choose dramatic font
   - Font Size: 72
   - Alignment: Center + Middle
   - Color: White
   - Anchor: Center
   - Best Fit: ? (optional)
   ```

4. **Add Subtitle (Optional):**
   ```
   ToBeContinuedCanvas ? Right-click ? UI ? Text
   Name: "SubtitleText"
   
   Settings:
   - Text: "The journey continues..."
   - Font Size: 32
   - Alignment: Center + Middle
   - Color: Light Gray
   - Anchor: Center (below main text)
   ```

5. **Add Skip Prompt:**
   ```
   ToBeContinuedCanvas ? Right-click ? UI ? Text
   Name: "SkipPromptText"
   
   Settings:
   - Text: "Press any key to continue"
   - Font Size: 24
   - Alignment: Center
   - Color: White with transparency (255, 255, 255, 200)
   - Anchor: Bottom-Center
   - Position: Y = 50 (from bottom)
   - Initially disabled ?
   ```

---

### **Step 2: Add Manager Script**

1. **Create Manager GameObject:**
   ```
   Hierarchy ? Right-click ? Create Empty
   Name: "ToBeContinuedManager"
   
   Add Component ? ToBeContinuedManager script
   ```

2. **Configure ToBeContinuedManager:**

   **UI References:**
   ```
   - To Be Continued Canvas: [ToBeContinuedCanvas]
   - To Be Continued Text: [ToBeContinuedText]
   - Subtitle Text: [SubtitleText]
   - Fade Panel: [FadePanel Canvas Group]
   ```

   **Scene Settings:**
   ```
   - Main Menu Scene Name: "MainMenuScene"
   - Use Scene Index: ?
   - Main Menu Scene Index: 0
   ```

   **Display Settings:**
   ```
   - Display Text: "TO BE CONTINUED..."
   - Subtitle: "The journey continues..."
   - Display Duration: 5.0 seconds
   - Fade In Duration: 2.0 seconds
   - Fade Out Duration: 1.0 seconds
   ```

   **Animation Settings:**
   ```
   - Animate Text: ?
   - Text Fade In Duration: 1.5 seconds
   - Text Delay: 0.5 seconds
   ```

   **Input Settings:**
   ```
   - Allow Skip: ?
   - Skip Prompt Text: [SkipPromptText]
   - Skip Delay: 2.0 seconds
   ```

---

### **Step 3: Update Chaosmancer (Already Done!)**

The Chaosmancer script has been updated to automatically show the "To Be Continued" screen when defeated:

```csharp
// In Chaosmancer.cs OnDeath() method:
StartCoroutine(ShowToBeContinuedAfterDelay(2f));
```

---

### **Step 4: Build Settings**

Ensure Main Menu scene is in Build Settings:

```
File ? Build Settings

Build Index 0: MainMenuScene ?
Build Index 1: IntroVideoScene (if you have it)
Build Index 2: PointNClickScene
Build Index 3: GameScene (with Chaosmancer)
```

---

## ?? How It Works

### **Flow Diagram:**

```
Player defeats Chaosmancer
    ?
Chaosmancer.OnDeath() called
    ?
Wait 2 seconds (death animation/effects)
    ?
ToBeContinuedManager.ShowToBeContinued()
    ?
???????????????????????????????????????
?  Stop Time (Time.timeScale = 0)    ?
?  Show Canvas                        ?
?  Fade in black background (2s)     ?
?  Animate text fade in (1.5s)       ?
?  Display "TO BE CONTINUED..."      ?
?  Wait 5 seconds OR player skips    ?
?  Fade out (1s)                      ?
?  Resume time                        ?
?  Load Main Menu Scene               ?
???????????????????????????????????????
```

---

## ?? Customization

### **Change Display Text:**

**In Unity Inspector:**
```
ToBeContinuedManager ? Display Settings:
- Display Text: "Your custom text here"
- Subtitle: "Optional subtitle"
```

**Or in code:**
```csharp
ToBeContinuedManager.Instance.ShowToBeContinued(
    "CHAPTER 1 COMPLETE", 
    "Chapter 2 coming soon..."
);
```

---

### **Change Timing:**

```
ToBeContinuedManager ? Display Settings:
- Display Duration: How long to show (seconds)
- Fade In Duration: Fade in speed (seconds)
- Fade Out Duration: Fade out speed (seconds)
- Text Fade In Duration: Text animation speed
- Text Delay: Delay before text appears
```

---

### **Disable Skipping:**

```
ToBeContinuedManager ? Input Settings:
- Allow Skip: ? (unchecked)
```

---

### **Add Sound Effects:**

1. **Import Audio:**
   ```
   Project ? Assets/Audio/ToBeContinued/
   - dramatic_sound.wav
   - ending_music.mp3
   ```

2. **Assign in Inspector:**
   ```
   ToBeContinuedManager ? Audio Settings:
   - To Be Continued Sound: [dramatic_sound]
   - To Be Continued Music: [ending_music]
   - Sound Volume: 1.0
   - Music Volume: 0.5
   ```

---

### **Different Target Scene:**

**To load a different scene instead of main menu:**

```
ToBeContinuedManager ? Scene Settings:
- Main Menu Scene Name: "CreditsScene" (or any scene)
```

**Or in code:**
```csharp
// In Chaosmancer.cs
private IEnumerator ShowToBeContinuedAfterDelay(float delay)
{
    yield return new WaitForSeconds(delay);
    
    // Override scene name before showing
    if (ToBeContinuedManager.Instance != null)
    {
        // Access via reflection or make fields public
        ToBeContinuedManager.Instance.ShowToBeContinued();
    }
}
```

---

## ?? Styling Tips

### **Dramatic Font:**

Good font choices:
- **Cinzel** (medieval/fantasy)
- **Bebas Neue** (bold impact)
- **Playfair Display** (elegant)
- **Avenir** (clean modern)

**Import custom font:**
```
1. Import .ttf or .otf file to Assets/Fonts/
2. Select font in Unity
3. Assign to ToBeContinuedText ? Font
```

---

### **Text Effects:**

**Shadow:**
```
ToBeContinuedText ? Add Component ? Shadow
- Effect Color: Black
- Effect Distance: (3, -3)
```

**Outline:**
```
ToBeContinuedText ? Add Component ? Outline
- Effect Color: Black
- Effect Distance: (2, 2)
```

**Glow Effect:**
- Use TextMeshPro instead of standard Text
- Material: Distance Field with glow

---

### **Background Styles:**

**Solid Color (Current):**
```
FadePanel:
- Color: Black (0, 0, 0, 255)
```

**Gradient:**
```
FadePanel:
- Remove Image component
- Add: Raw Image
- Create gradient texture in image editor
- Assign texture
```

**Vignette:**
```
FadePanel:
- Color: Black
- Sprite: Vignette texture (dark edges, transparent center)
```

---

## ?? Animation Ideas

### **Text Animation Styles:**

**Current: Fade In**
```csharp
// Already implemented in ToBeContinuedManager
Alpha: 0 ? 1 over 1.5 seconds
```

**Scale In:**
```csharp
// Modify AnimateTextIn() in ToBeContinuedManager
transform.localScale: Vector3.zero ? Vector3.one
```

**Slide In:**
```csharp
// Modify AnimateTextIn()
RectTransform position: Off-screen ? Center
```

**Typewriter Effect:**
```csharp
// Display one character at a time
for (int i = 0; i <= displayText.Length; i++)
{
    toBeContinuedText.text = displayText.Substring(0, i);
    yield return new WaitForSecondsRealtime(0.05f);
}
```

---

### **Background Effects:**

**Particles:**
```
1. FadePanel ? Add Child: Particle System
2. Particles: Slow floating embers/sparkles
3. Color: White/Gold
4. Emission: Low rate (5-10/sec)
```

**Animated Vignette:**
```csharp
// Pulse the fade panel alpha
while (showing)
{
    float pulse = Mathf.Sin(Time.unscaledTime * 2f) * 0.1f + 0.9f;
    fadePanel.alpha = pulse;
    yield return null;
}
```

---

## ?? Troubleshooting

### **Screen doesn't appear:**

**Check:**
```
? ToBeContinuedManager exists in scene
? ToBeContinuedCanvas is assigned
? Chaosmancer has updated OnDeath() code
? Console shows "[Chaosmancer] Showing 'To Be Continued' screen!"
```

**Fix:**
```
- Verify ToBeContinuedManager.Instance is not null
- Check Canvas is enabled
- Ensure no errors in Console
```

---

### **Screen appears but is blank:**

**Check:**
```
? ToBeContinuedText is assigned
? Text has content in Display Text field
? Text color is not transparent (alpha = 255)
? Font is assigned
```

---

### **Player can't skip:**

**Check:**
```
? Allow Skip is enabled
? Skip Delay has passed (default 2 seconds)
? Time is not frozen forever
```

---

### **Doesn't return to main menu:**

**Check:**
```
? Main Menu Scene Name is correct
? Scene is in Build Settings
? Console shows scene load message
```

**Fix:**
```
- Verify scene name exactly matches
- Check File ? Build Settings
- Try using Scene Index instead
```

---

### **Game stays paused:**

If something goes wrong and game stays frozen:

```csharp
// Emergency fix in console:
Time.timeScale = 1f;

// Or add safety timeout:
void Update()
{
    if (Time.timeScale == 0 && Input.GetKeyDown(KeyCode.Escape))
    {
        Time.timeScale = 1f;
        Debug.Log("Emergency unpause!");
    }
}
```

---

## ?? Advanced: Multiple Endings

**To show different screens based on outcome:**

```csharp
// In Chaosmancer.cs
public enum BossDefeatType
{
    Normal,
    PerfectVictory,
    LowHealth
}

private IEnumerator ShowToBeContinuedAfterDelay(float delay)
{
    yield return new WaitForSeconds(delay);
    
    if (ToBeContinuedManager.Instance != null)
    {
        // Determine victory type
        BossDefeatType defeatType = DetermineDefeatType();
        
        switch (defeatType)
        {
            case BossDefeatType.PerfectVictory:
                ToBeContinuedManager.Instance.ShowToBeContinued(
                    "FLAWLESS VICTORY!", 
                    "The hero stands unscathed..."
                );
                break;
                
            case BossDefeatType.LowHealth:
                ToBeContinuedManager.Instance.ShowToBeContinued(
                    "BARELY VICTORIOUS...", 
                    "The hero survives by a thread..."
                );
                break;
                
            default:
                ToBeContinuedManager.Instance.ShowToBeContinued();
                break;
        }
    }
}

private BossDefeatType DetermineDefeatType()
{
    // Check player health, time, etc.
    Health playerHealth = player.GetComponent<Health>();
    if (playerHealth != null && playerHealth.HealthPercentage >= 0.8f)
    {
        return BossDefeatType.PerfectVictory;
    }
    else if (playerHealth != null && playerHealth.HealthPercentage <= 0.2f)
    {
        return BossDefeatType.LowHealth;
    }
    return BossDefeatType.Normal;
}
```

---

## ?? Testing Checklist

**Before releasing:**
```
? Defeat Chaosmancer ? Screen appears
? Text is readable and centered
? Fade in/out animations work
? Skip works after delay
? Returns to main menu after duration
? Time resumes correctly
? No errors in Console
? Works in Build (not just Editor)
? Audio plays correctly (if assigned)
? All text is spelled correctly
```

---

## ?? Quick Reference

### **Call from any script:**

```csharp
// Basic
ToBeContinuedManager.Instance.ShowToBeContinued();

// With custom text
ToBeContinuedManager.Instance.ShowToBeContinued(
    "CHAPTER COMPLETE", 
    "Your adventure continues..."
);
```

### **Check if available:**

```csharp
if (ToBeContinuedManager.Instance != null)
{
    ToBeContinuedManager.Instance.ShowToBeContinued();
}
else
{
    Debug.LogError("ToBeContinuedManager not found!");
}
```

---

## ? Summary

**What was implemented:**
- ? ToBeContinuedManager script
- ? Fade in/out system
- ? Text animation
- ? Skip functionality
- ? Auto return to main menu
- ? Time freeze during display
- ? Chaosmancer integration
- ? Audio support

**Flow:**
```
Boss defeated ? Wait 2s ? Show screen ? Wait 5s ? Main menu
```

**Build Status:** ? Successful

---

**Your "To Be Continued" screen is ready!** ??

Defeat the Chaosmancer boss and watch your dramatic ending sequence! ?
