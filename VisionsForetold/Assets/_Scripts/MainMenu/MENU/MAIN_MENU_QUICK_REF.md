# ?? Main Menu System - Quick Reference

## ?? Quick Setup (3 Steps)

### 1. Add Scene to Build Settings
```
File ? Build Settings
Add scenes:
0: MainMenu
1: PointNClickScene
```

### 2. Create UI
```
Canvas
?? MainMenuPanel
?  ?? PlayButton
?  ?? OptionsButton
?  ?? ExitButton
?? OptionsMenuPanel (Inactive)
   ?? Settings UI
```

### 3. Connect MenuManager
```
Create GameObject "MenuManager"
Add MenuManager script
Assign panels in Inspector
Connect buttons:
- Play ? OnPlayButton
- Options ? OnOptionsButton
- Exit ? OnExitButton
```

## ?? Button Connections

```
PlayButton:
OnClick() ? MenuManager ? OnPlayButton

OptionsButton:
OnClick() ? MenuManager ? OnOptionsButton

ExitButton:
OnClick() ? MenuManager ? OnExitButton

BackButton (in Options):
OnClick() ? MenuManager ? OnBackButton
```

## ?? MenuManager Settings

```
Scene Settings:
- Game Scene Name: "PointNClickScene"
- Use Scene Index: ?

Transition Settings:
- Use Fade Transition: ?
- Fade Duration: 1.0

Audio Settings:
- Play Button Sounds: ?
```

## ?? Options Menu Setup

```
Add OptionsMenu script to MenuManager

Assign UI:
Graphics:
- Quality Dropdown
- Resolution Dropdown
- Fullscreen Toggle
- VSync Toggle

Audio:
- Master Volume Slider + Text
- Music Volume Slider + Text
- SFX Volume Slider + Text

Controls:
- Mouse Sensitivity Slider + Text
- Invert Y Toggle
```

## ?? Pause Menu (In-Game)

```
Create in game scene:
1. GameObject "PauseMenu"
2. Add PauseMenu script
3. Create pause UI panel
4. Assign panel in Inspector
5. Press ESC to pause

Buttons:
- Resume ? OnResumeButton
- Restart ? OnRestartButton
- Main Menu ? OnMainMenuButton
- Quit ? OnQuitButton
```

## ?? Troubleshooting

| Problem | Solution |
|---------|----------|
| Scene won't load | Add to Build Settings |
| Fade not working | Check Fade Panel or leave empty |
| Buttons not working | Check OnClick connections |
| Settings not saving | Check OptionsMenu assigned |

## ?? Testing Checklist

- [ ] Play loads game scene ?
- [ ] Options opens/closes ?
- [ ] Exit quits game ?
- [ ] ESC key works ?
- [ ] Fade transition smooth ?
- [ ] Settings save/load ?

## ?? Files Created

```
? MenuManager.cs - Main menu controller
? OptionsMenu.cs - Settings management
? PauseMenu.cs - In-game pause
? MAIN_MENU_SETUP_GUIDE.md - Full guide
? MAIN_MENU_QUICK_REF.md - This file
```

**Your menu system is ready!** ???

**Next:** Ask for AudioManager integration!
