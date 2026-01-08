# ? Quick Fix Card - Unity Inspector Errors

## ?? Getting These Errors?

```
NullReferenceException: GameObjectInspector.OnDisable
MissingReferenceException: m_Targets doesn't exist anymore
```

---

## ? **INSTANT FIX (Do This Now!)**

**Press: `Ctrl + Shift + A`**

That's it! Errors gone! ??

---

## ??? **Why It Happens**

You selected the Chaosmancer boss ? Boss died ? Inspector lost reference ? Error!

---

## ?? **5-Second Fixes (Pick One)**

| Fix | How | Why |
|-----|-----|-----|
| **Deselect** | `Ctrl+Shift+A` | Clears Inspector |
| **Lock/Unlock** | Inspector ? Lock icon 2x | Refreshes Inspector |
| **Clear Console** | `Ctrl+Shift+C` | Removes error spam |
| **Exit Play** | `Ctrl+P` | Resets everything |
| **Click Elsewhere** | Click empty Hierarchy space | Selects nothing |

---

## ?? **Don't Do This**

? Don't select enemies during Play Mode
? Don't select projectiles during Play Mode  
? Don't select anything that gets destroyed

---

## ? **Do This Instead**

? Select Player (persistent)
? Select AudioManager (persistent)
? Select Terrain (static)
? Use `Debug.Log` for boss info
? Use Scene View Gizmos (already working!)

---

## ?? **Best Practice**

**When testing boss:**
1. Enter Play Mode
2. **DON'T** select Chaosmancer in Hierarchy
3. Watch Console for logs
4. Use Scene View for visualization
5. Defeat boss
6. No errors! ?

---

## ?? **Already Fixed in Code**

Your Chaosmancer now:
- ? Prevents multiple death calls
- ? Disables component after death
- ? Cleans up all references properly
- ? Stops all coroutines safely
- ? Doesn't destroy GameObject (prevents errors)

---

## ?? **Pro Tip**

**Want to see boss info?**

Use the Scene View (not Inspector):
- Boss has colored range circles (Gizmos)
- Red = Slam range
- Cyan = Pull radius
- Yellow = Min distance
- Green = Max distance

**No selection needed!** ??

---

## ?? **Keyboard Shortcuts**

| Key | Action |
|-----|--------|
| `Ctrl+Shift+A` | Deselect All (FIX ERRORS!) |
| `Ctrl+Shift+P` | Pause Play Mode |
| `Ctrl+Shift+C` | Clear Console |
| `Ctrl+P` | Start/Stop Play Mode |
| `F` | Focus on selected |
| `Escape` | Deselect |

---

## ?? **Testing Workflow**

```
? CORRECT:
Start Play Mode
? Watch Console logs
? Use Scene View
? Fight boss
? No errors!

? WRONG:
Start Play Mode
? Select Chaosmancer in Hierarchy
? Boss dies
? ERRORS!
```

---

## ?? **If Errors Persist**

1. **Deselect:** `Ctrl+Shift+A`
2. **Lock Inspector:** Click lock icon 2x
3. **Clear Console:** `Ctrl+Shift+C`
4. **Exit Play Mode:** `Ctrl+P`
5. **Restart Unity** (last resort)

---

## ? **Summary**

**Error Cause:** Selecting boss that gets destroyed
**Quick Fix:** `Ctrl+Shift+A`
**Prevention:** Don't select enemies during play
**Alternative:** Use Debug.Log and Gizmos

---

**You're all set! Fight that boss!** ?????

---

Print this card and keep it handy! ??
