# ?? Joystick Support - Quick Reference

## Controller Button Map

```
Xbox:
A (South)    ? Confirm
B (East)     ? Back
Start        ? Pause
D-Pad/Stick  ? Navigate

PlayStation:
Cross (X)    ? Confirm
Circle (O)   ? Back
Options      ? Pause
D-Pad/Stick  ? Navigate

Switch:
B            ? Confirm
A            ? Back
+            ? Pause
D-Pad/Stick  ? Navigate
```

## Quick Setup (3 Steps)

### 1. Install Input System
```
Window ? Package Manager ? Input System ? Install
Restart Unity
```

### 2. Set Button Navigation
```
Each Button:
Inspector ? Navigation ? Automatic
```

### 3. Assign First Buttons
```
MenuManager Inspector:
- Main Menu First Button: PlayButton
- Options Menu First Button: BackButton
```

## Features

- ? D-Pad/Stick navigation
- ? A/Cross to confirm
- ? B/Circle to back
- ? Start to pause (in-game)
- ? Auto cursor hide/show
- ? Seamless input switching
- ? Button highlighting
- ? All controllers supported

## Testing

```
1. Connect controller
2. Press Play
3. Use D-Pad to navigate
4. Press A to select
5. Press B to go back
? It works!
```

## Troubleshooting

| Issue | Fix |
|-------|-----|
| No navigation | Navigation: Automatic |
| No highlight | Assign First Button |
| Cursor won't hide | Auto Detect: ? |
| Wrong button first | Reassign First Button |

## Inspector Settings

```
MenuManager:
Input Settings:
?? Allow Escape Key: ?
?? Allow Gamepad Back: ?
?? Auto Detect Input Method: ?
?? Input Switch Delay: 0.2s

First Selected Buttons:
?? Main Menu: PlayButton
?? Options Menu: BackButton
?? Credits: BackButton
```

**Full joystick support ready!** ???
