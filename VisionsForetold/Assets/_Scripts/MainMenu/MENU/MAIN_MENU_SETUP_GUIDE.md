# ?? Main Menu System - Complete Setup Guide

## ? What You Have

A fully functional main menu system with:
- **Play Button** - Loads your PointNClickScene
- **Options Button** - Opens settings menu
- **Exit Button** - Quits the game
- **Smooth Fade Transitions** - Professional scene loading
- **Settings Management** - Graphics, Audio, Controls
- **ESC Key Support** - Navigate back through menus

## ?? Quick Setup (3 Steps)

### Step 1: Add Scene to Build Settings

```
1. Go to: File ? Build Settings
2. Click "Add Open Scenes" (if MainMenu scene is open)
   OR drag your scenes into the list:
   - Scene 0: MainMenu
   - Scene 1: PointNClickScene (your game scene)
3. Close Build Settings
```

### Step 2: Create UI Layout

**In your MainMenu scene:**

```
1. Create Canvas (if doesn't exist)
   - Right-click Hierarchy ? UI ? Canvas
   - Canvas Scaler ? UI Scale Mode: Scale with Screen Size
   - Reference Resolution: 1920x1080

2. Create Main Menu Panel:
   - Right-click Canvas ? UI ? Panel
   - Name: "MainMenuPanel"
   
3. Add Buttons to MainMenuPanel:
   - Right-click MainMenuPanel ? UI ? Button (TextMeshPro)
   - Create 3 buttons: "PlayButton", "OptionsButton", "ExitButton"
   - Arrange vertically in center

4. Create Options Menu Panel:
   - Right-click Canvas ? UI ? Panel
   - Name: "OptionsMenuPanel"
   - Deactivate it (uncheck checkbox in Inspector)
   - Add your settings UI inside (see detailed layout below)
```

### Step 3: Connect MenuManager

```
1. Create Empty GameObject in scene
   - Name: "MenuManager"

2. Add MenuManager script:
   - Add Component ? MenuManager

3. Assign references in Inspector:
   Main Menu Panel: MainMenuPanel
   Options Menu Panel: OptionsMenuPanel
   Game Scene Name: "PointNClickScene"
   Use Fade Transition: ?
   Fade Duration: 1

4. Connect buttons:
   Play Button ? OnClick() ? MenuManager ? OnPlayButton
   Options Button ? OnClick() ? MenuManager ? OnOptionsButton
   Exit Button ? OnClick() ? MenuManager ? OnExitButton
```

**Done! Your menu works now!**

## ?? Detailed UI Layout

### Main Menu Panel

```
MainMenuPanel (Panel)
?? Background (Image - dark/transparent)
?? Title (TextMeshPro - "YOUR GAME NAME")
?? PlayButton (Button)
?  ?? Text: "PLAY"
?? OptionsButton (Button)
?  ?? Text: "OPTIONS"
?? CreditsButton (Button - optional)
?  ?? Text: "CREDITS"
?? ExitButton (Button)
   ?? Text: "EXIT"
```

### Options Menu Panel

```
OptionsMenuPanel (Panel)
?? Background (Image)
?? Title (TextMeshPro - "OPTIONS")
?
?? Graphics Section
?  ?? Quality Dropdown (TMP_Dropdown)
?  ?? Resolution Dropdown (TMP_Dropdown)
?  ?? Fullscreen Toggle
?  ?? VSync Toggle
?
?? Audio Section
?  ?? Master Volume Slider + Text
?  ?? Music Volume Slider + Text
?  ?? SFX Volume Slider + Text
?
?? Controls Section
?  ?? Mouse Sensitivity Slider + Text
?  ?? Invert Y Toggle
?
?? Back Button (Button)
   ?? OnClick ? MenuManager ? OnBackButton
```

## ?? MenuManager Settings

### Scene Settings

| Setting | Value | Description |
|---------|-------|-------------|
| **Game Scene Name** | "PointNClickScene" | Your game scene name |
| **Use Scene Index** | ? | Use name instead of index |
| **Game Scene Index** | 1 | If using index instead |

### Transition Settings

| Setting | Value | Description |
|---------|-------|-------------|
| **Use Fade Transition** | ? | Enable smooth fade |
| **Fade Duration** | 1.0 | Fade time in seconds |
| **Fade Panel** | (Optional) | Auto-creates if empty |

### Audio Settings

| Setting | Value | Description |
|---------|-------|-------------|
| **Play Button Sounds** | ? | Enable button clicks |
| **Button Click Sound** | (Audio Clip) | Button sound effect |
| **Menu Audio Source** | (Auto-created) | For menu sounds |

## ?? Button Setup

### Play Button

```
1. Select PlayButton
2. Inspector ? Button Component
3. OnClick() ? Add event
4. Drag MenuManager GameObject
5. Function: MenuManager ? OnPlayButton
```

### Options Button

```
OnClick() ? MenuManager ? OnOptionsButton
```

### Exit Button

```
OnClick() ? MenuManager ? OnExitButton
```

### Back Button (in Options)

```
OnClick() ? MenuManager ? OnBackButton
```

## ?? Options Menu Setup

### Add OptionsMenu Script

```
1. Select MenuManager GameObject (or create new)
2. Add Component ? OptionsMenu
3. Assign UI elements in Inspector:
   - Quality Dropdown
   - Resolution Dropdown
   - Fullscreen Toggle
   - VSync Toggle
   - Master Volume Slider + Text
   - Music Volume Slider + Text
   - SFX Volume Slider + Text
   - Mouse Sensitivity Slider + Text
   - Invert Y Toggle
```

### Sliders Setup

```
For each volume slider:
1. Min Value: 0
2. Max Value: 1
3. Whole Numbers: ?
4. OnValueChanged() ? OptionsMenu ? SetMasterVolume (or Music/SFX)
```

### Dropdowns Setup

```
Quality Dropdown:
- OnValueChanged() ? OptionsMenu ? SetQuality

Resolution Dropdown:
- OnValueChanged() ? OptionsMenu ? SetResolution
```

### Toggles Setup

```
Fullscreen Toggle:
- OnValueChanged() ? OptionsMenu ? SetFullscreen

VSync Toggle:
- OnValueChanged() ? OptionsMenu ? SetVSync

Invert Y Toggle:
- OnValueChanged() ? OptionsMenu ? SetInvertY
```

## ?? Scene Structure Example

```
MainMenu Scene
?? Canvas
?  ?? MainMenuPanel (Active)
?  ?  ?? Title
?  ?  ?? PlayButton ? OnPlayButton
?  ?  ?? OptionsButton ? OnOptionsButton
?  ?  ?? ExitButton ? OnExitButton
?  ?
?  ?? OptionsMenuPanel (Inactive)
?  ?  ?? Graphics Section
?  ?  ?? Audio Section
?  ?  ?? Controls Section
?  ?  ?? BackButton ? OnBackButton
?  ?
?  ?? FadePanel (Auto-created)
?     ?? Black image covering screen
?
?? MenuManager (GameObject)
?  ?? MenuManager (Script)
?  ?? OptionsMenu (Script)
?
?? EventSystem (Auto-created with Canvas)
```

## ?? How It Works

### Play Flow

```
1. Player clicks Play Button
2. MenuManager.OnPlayButton() called
3. Fade panel fades to black (1 second)
4. Scene loads asynchronously
5. PointNClickScene appears
```

### Options Flow

```
1. Player clicks Options Button
2. MainMenuPanel hides
3. OptionsMenuPanel shows
4. Player adjusts settings
5. Settings auto-save to PlayerPrefs
6. Click Back to return to main menu
```

### Exit Flow

```
1. Player clicks Exit Button
2. MenuManager.OnExitButton() called
3. In Editor: Stops play mode
4. In Build: Application.Quit()
```

## ?? Troubleshooting

### Play Button Doesn't Load Scene

**Check:**
1. ? Scene added to Build Settings
2. ? Scene name matches exactly (case-sensitive)
3. ? Scene index correct (if using index)
4. ? Button OnClick connected to OnPlayButton

**Console Error:**
```
"Scene 'PointNClickScene' couldn't be loaded"
Solution: Add scene to Build Settings!
```

### Fade Not Working

**Check:**
1. Use Fade Transition: ?
2. Fade Panel assigned OR auto-create enabled
3. Canvas exists in scene

**Fix:**
```
Leave Fade Panel empty - auto-creates on start
```

### Options Not Saving

**Check:**
1. OptionsMenu script added
2. UI elements assigned in Inspector
3. OnValueChanged events connected

**Test:**
```
Change setting ? Close game ? Reopen
Settings should persist
```

### Buttons Not Clickable

**Check:**
1. EventSystem exists in scene
2. Canvas has GraphicRaycaster
3. Buttons not blocked by other UI
4. Canvas Render Mode: Screen Space - Overlay

## ?? Styling Tips

### Professional Look

```
Buttons:
- Background: Dark gradient
- Hover: Lighter tint
- Pressed: Darker tint
- Text: White/Yellow/Gold

Panels:
- Background: Semi-transparent black (alpha 0.8)
- Border: Subtle glow or outline
- Shadow: Soft drop shadow

Fonts:
- Title: Large, bold (60-80pt)
- Buttons: Medium (24-36pt)
- Settings: Regular (18-24pt)
```

### Animations (Optional)

```csharp
// Add to button hover
button.GetComponent<Animator>().SetTrigger("Highlight");

// Fade in menu panels
panel.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
```

## ?? Testing Checklist

- [ ] Play button loads PointNClickScene
- [ ] Options button opens options menu
- [ ] Exit button quits game (in build)
- [ ] Back button returns to main menu
- [ ] ESC key works in options menu
- [ ] Fade transition smooth
- [ ] Settings save and load
- [ ] Volume sliders work
- [ ] Graphics settings apply
- [ ] Mouse sensitivity saves
- [ ] No console errors

## ?? Next Steps

After testing your menu:

1. **Add AudioManager** (you mentioned you'll ask for this)
   - Menu music
   - Button click sounds
   - Volume control integration

2. **Polish**
   - Add button hover effects
   - Add transition animations
   - Add sound effects

3. **More Features**
   - Loading screen with progress bar
   - Pause menu (already coded below)
   - Key rebinding
   - Language selection

## ?? Save Data Location

Settings are saved in **PlayerPrefs**:

```
Windows: Registry
  HKEY_CURRENT_USER\Software\[CompanyName]\[ProductName]

Mac: ~/Library/Preferences/unity.[CompanyName].[ProductName].plist

Linux: ~/.config/unity3d/[CompanyName]/[ProductName]/prefs
```

**Clear PlayerPrefs:**
```csharp
PlayerPrefs.DeleteAll();
```

## ?? Summary

**What You Have:**
- ? Fully functional main menu
- ? Play/Options/Exit buttons
- ? Smooth scene transitions
- ? Complete settings menu
- ? Save/load settings
- ? Professional structure

**Setup Time:** ~10 minutes
**Customization:** Easy via Inspector
**Ready For:** AudioManager integration

**Your main menu is production-ready!** ???
