# 🎨 Main Menu UI Layout - Visual Guide

## Main Menu Panel Layout

```
┌──────────────────────────────────────────────────┐
│                                                  │
│                  YOUR GAME TITLE                 │
│                                                  │
│                                                  │
│              ┌──────────────────┐               │
│              │      PLAY        │               │
│              └──────────────────┘               │
│                                                  │
│              ┌──────────────────┐               │
│              │    OPTIONS       │               │
│              └──────────────────┘               │
│                                                  │
│              ┌──────────────────┐               │
│              │    CREDITS       │               │
│              └──────────────────┘               │
│                                                  │
│              ┌──────────────────┐               │
│              │      EXIT        │               │
│              └──────────────────┘               │
│                                                  │
│                                                  │
│                     v1.0.0                       │
└──────────────────────────────────────────────────┘
```

## Options Menu Panel Layout

```
┌──────────────────────────────────────────────────┐
│                                                  │
│  ← BACK              OPTIONS                    │
│                                                  │
│  ┌─ GRAPHICS ────────────────────────────────┐ │
│  │                                            │ │
│  │  Quality:      [Ultra ▼]                  │ │
│  │  Resolution:   [1920x1080 ▼]              │ │
│  │  Fullscreen:   [✓]                        │ │
│  │  VSync:        [✓]                        │ │
│  │                                            │ │
│  └────────────────────────────────────────────┘ │
│                                                  │
│  ┌─ AUDIO ───────────────────────────────────┐ │
│  │                                            │ │
│  │  Master:   [████████░░] 80%               │ │
│  │  Music:    [███████░░░] 70%               │ │
│  │  SFX:      [█████████░] 90%               │ │
│  │                                            │ │
│  └────────────────────────────────────────────┘ │
│                                                  │
│  ┌─ CONTROLS ────────────────────────────────┐ │
│  │                                            │ │
│  │  Mouse Sens:  [███████░░░] 1.0            │ │
│  │  Invert Y:    [✗]                         │ │
│  │                                            │ │
│  └────────────────────────────────────────────┘ │
│                                                  │
│              ┌──────────────────┐               │
│              │  RESET DEFAULTS  │               │
│              └──────────────────┘               │
│                                                  │
└──────────────────────────────────────────────────┘
```

## Pause Menu Layout (In-Game)

```
┌──────────────────────────────────────────────────┐
│                                                  │
│                    PAUSED                        │
│                                                  │
│                                                  │
│              ┌──────────────────┐               │
│              │     RESUME       │               │
│              └──────────────────┘               │
│                                                  │
│              ┌──────────────────┐               │
│              │     RESTART      │               │
│              └──────────────────┘               │
│                                                  │
│              ┌──────────────────┐               │
│              │    OPTIONS       │               │
│              └──────────────────┘               │
│                                                  │
│              ┌──────────────────┐               │
│              │   MAIN MENU      │               │
│              └──────────────────┘               │
│                                                  │
│              ┌──────────────────┐               │
│              │      QUIT        │               │
│              └──────────────────┘               │
│                                                  │
│                                                  │
│               Press ESC to Resume                │
└──────────────────────────────────────────────────┘
```

## Hierarchy Structure

```
MainMenu Scene
│
├─ Canvas
│  │
│  ├─ MainMenuPanel (Active)
│  │  ├─ Background (Image - dark)
│  │  ├─ Title (TextMeshPro)
│  │  │  └─ Text: "YOUR GAME NAME"
│  │  ├─ ButtonsContainer (Vertical Layout Group)
│  │  │  ├─ PlayButton
│  │  │  │  ├─ Text: "PLAY"
│  │  │  │  └─ OnClick → MenuManager.OnPlayButton
│  │  │  ├─ OptionsButton
│  │  │  │  ├─ Text: "OPTIONS"
│  │  │  │  └─ OnClick → MenuManager.OnOptionsButton
│  │  │  ├─ CreditsButton
│  │  │  │  ├─ Text: "CREDITS"
│  │  │  │  └─ OnClick → MenuManager.OnCreditsButton
│  │  │  └─ ExitButton
│  │  │     ├─ Text: "EXIT"
│  │  │     └─ OnClick → MenuManager.OnExitButton
│  │  └─ Version (TextMeshPro)
│  │     └─ Text: "v1.0.0"
│  │
│  ├─ OptionsMenuPanel (Inactive)
│  │  ├─ Background (Image - dark)
│  │  ├─ Header
│  │  │  ├─ BackButton
│  │  │  │  ├─ Text: "← BACK"
│  │  │  │  └─ OnClick → MenuManager.OnBackButton
│  │  │  └─ Title (TextMeshPro)
│  │  │     └─ Text: "OPTIONS"
│  │  ├─ ScrollView (Scroll Rect)
│  │  │  └─ Content
│  │  │     ├─ GraphicsSection (Panel)
│  │  │     │  ├─ Header: "GRAPHICS"
│  │  │     │  ├─ QualityRow
│  │  │     │  │  ├─ Label: "Quality:"
│  │  │     │  │  └─ QualityDropdown (TMP_Dropdown)
│  │  │     │  ├─ ResolutionRow
│  │  │     │  │  ├─ Label: "Resolution:"
│  │  │     │  │  └─ ResolutionDropdown (TMP_Dropdown)
│  │  │     │  ├─ FullscreenRow
│  │  │     │  │  ├─ Label: "Fullscreen:"
│  │  │     │  │  └─ FullscreenToggle
│  │  │     │  └─ VSyncRow
│  │  │     │     ├─ Label: "VSync:"
│  │  │     │     └─ VSyncToggle
│  │  │     │
│  │  │     ├─ AudioSection (Panel)
│  │  │     │  ├─ Header: "AUDIO"
│  │  │     │  ├─ MasterVolumeRow
│  │  │     │  │  ├─ Label: "Master:"
│  │  │     │  │  ├─ MasterVolumeSlider
│  │  │     │  │  └─ MasterVolumeText: "80%"
│  │  │     │  ├─ MusicVolumeRow
│  │  │     │  │  ├─ Label: "Music:"
│  │  │     │  │  ├─ MusicVolumeSlider
│  │  │     │  │  └─ MusicVolumeText: "70%"
│  │  │     │  └─ SFXVolumeRow
│  │  │     │     ├─ Label: "SFX:"
│  │  │     │     ├─ SFXVolumeSlider
│  │  │     │     └─ SFXVolumeText: "90%"
│  │  │     │
│  │  │     └─ ControlsSection (Panel)
│  │  │        ├─ Header: "CONTROLS"
│  │  │        ├─ MouseSensitivityRow
│  │  │        │  ├─ Label: "Mouse Sensitivity:"
│  │  │        │  ├─ MouseSensitivitySlider
│  │  │        │  └─ MouseSensitivityText: "1.0"
│  │  │        └─ InvertYRow
│  │  │           ├─ Label: "Invert Y:"
│  │  │           └─ InvertYToggle
│  │  │
│  │  └─ Footer
│  │     └─ ResetButton
│  │        ├─ Text: "RESET DEFAULTS"
│  │        └─ OnClick → OptionsMenu.ResetToDefaults
│  │
│  ├─ CreditsPanel (Inactive)
│  │  ├─ Background (Image)
│  │  ├─ ScrollView
│  │  │  └─ CreditsText (TextMeshPro)
│  │  └─ BackButton
│  │     └─ OnClick → MenuManager.OnBackButton
│  │
│  └─ FadePanel (Auto-created or manual)
│     ├─ Image (Black, fullscreen)
│     └─ CanvasGroup
│
├─ MenuManager (GameObject)
│  ├─ MenuManager (Script)
│  └─ OptionsMenu (Script)
│
└─ EventSystem (Auto-created)
```

## Component Settings

### Button Settings

```
Button Component:
- Transition: Color Tint
- Normal Color: White
- Highlighted Color: Light Gray (1.2x)
- Pressed Color: Dark Gray (0.8x)
- Disabled Color: Gray (0.5x)
- Fade Duration: 0.1s

TextMeshPro:
- Font Size: 24-36pt
- Color: White
- Alignment: Center
- Auto Size: Optional
```

### Slider Settings

```
Slider Component:
- Direction: Left to Right
- Min Value: 0
- Max Value: 1
- Whole Numbers: False
- Value: 1 (default)

Fill Area:
- Sprite: Solid color bar
- Color: Accent color (blue/green)

Background:
- Sprite: Darker bar
- Color: Dark gray
```

### Dropdown Settings

```
TMP_Dropdown:
- Template: Default dropdown template
- Caption Text: Current selection
- Item Text: Option text
- Arrow: Down arrow sprite
- Options: Populated at runtime
```

### Toggle Settings

```
Toggle Component:
- Is On: False (default)
- Toggle Transition: Fade
- Graphic: Checkmark sprite
- Background: Box sprite

Colors:
- Normal: White
- Highlighted: Light Gray
- Pressed: Dark Gray
- Checked: Accent color
```

## Size Recommendations

```
Canvas:
- Reference Resolution: 1920x1080
- Scale Mode: Scale with Screen Size
- Match: 0.5 (Width & Height)

Buttons:
- Width: 300-400px
- Height: 60-80px
- Spacing: 20-30px

Text:
- Title: 60-80pt
- Buttons: 24-36pt
- Labels: 18-24pt
- Small text: 14-16pt

Sliders:
- Width: 300-400px
- Height: 20-30px

Dropdowns:
- Width: 300-400px
- Height: 40-50px
```

## Color Scheme Example

```
Dark Theme:
- Background: #1a1a1a (very dark gray)
- Panels: #2a2a2a (dark gray)
- Buttons: #3a3a3a (medium gray)
- Text: #ffffff (white)
- Accent: #4a9eff (blue)

Light Theme:
- Background: #f0f0f0 (light gray)
- Panels: #ffffff (white)
- Buttons: #e0e0e0 (light gray)
- Text: #1a1a1a (dark)
- Accent: #2196f3 (blue)
```

## Quick Create Steps

1. **Create Canvas** (if needed)
2. **Create MainMenuPanel**: Panel → Name it
3. **Add Title**: TextMeshPro - Text → Anchor top-center
4. **Add Buttons**: Button → Duplicate 4 times
5. **Name Buttons**: Play, Options, Credits, Exit
6. **Position Buttons**: Stack vertically, center
7. **Create OptionsMenuPanel**: Panel → Deactivate
8. **Add Options UI**: Use ScrollView for content
9. **Create Sections**: Graphics, Audio, Controls
10. **Add Settings**: Dropdowns, Sliders, Toggles

**Time:** ~15-20 minutes for basic setup

**Your menu is ready to customize!** 🎨✨
