# ?? Quick Start - Respawn at Save Location

## ? 2-Minute Setup

### **1. Add to Scene (30 seconds)**

```
In Unity Hierarchy:
Right-click ? Create Empty
Name: "PlayerSpawnManager"
Add Component ? PlayerSpawnManager
```

### **2. Configure (10 seconds)**

```
Inspector:
Spawn Delay: 0.1 ?
Debug Mode: ? (during testing)
```

### **3. Test (1 minute)**

```
1. Play game
2. Walk to save station
3. Press E to save
4. Press M for map
5. Re-enter area
6. ? You spawn at save station!
```

---

## ?? That's It!

**No other setup needed.** The system automatically:
- Detects save station position when you save
- Remembers it in save file
- Spawns you there when you return from map

---

## ?? How Players Use It

```
SAVE ? MAP ? RETURN ? RESPAWN!
```

Simple as that.

---

## ?? Scene Checklist

For **each gameplay scene**, ensure:
- [ ] PlayerSpawnManager exists
- [ ] At least one SaveStation
- [ ] Player tagged as "Player"
- [ ] SaveManager in scene (or DontDestroyOnLoad)

---

## ?? Quick Debug

**Enable Debug Mode** in PlayerSpawnManager.

**Look for in Console:**
```
[PlayerSpawnManager] ? Spawned player at save station: (x,y,z)
```

**If you see it:** ? Working!
**If you don't:** Check "Player" tag and SaveManager exists

---

## ?? Key Features

? Spawns at last save station (not default spawn)
? Restores health and skills
? Works with multiple save stations
? Zero performance impact
? Automatic - no code needed

---

## ?? Full Documentation

See `RESPAWN_AT_SAVE_GUIDE.md` for:
- Detailed explanations
- Troubleshooting
- Advanced customization
- Code examples

---

## ? You're Done!

Add `PlayerSpawnManager` to your scenes and enjoy automatic respawning at save locations!

**Build Status:** ? Compiled Successfully
**Ready to Use:** ? Production-Ready

**Happy developing!** ????
