# ?? INSPECTOR ERRORS - EMERGENCY CARD

## ? Getting ANY of These?

```
NullReferenceException: GameObjectInspector.OnDisable
MissingReferenceException: m_Targets doesn't exist
SerializedObjectNotCreatableException: Object at index 0 is null
```

---

## ? **INSTANT FIX**

### **Press This Key Combination:**

# `Ctrl + Shift + A`

**That's it! Error gone!** ?

---

## ?? **What Just Happened?**

You selected the boss ? Boss died ? Inspector confused ? ERROR!

**The fix:** Deselect the dead boss (Ctrl+Shift+A)

---

## ?? **Alternative Fixes**

| Method | Action |
|--------|--------|
| **Deselect** | `Ctrl+Shift+A` ? |
| **Click Away** | Click empty space in Hierarchy |
| **Lock Inspector** | Lock icon 2x (top right) |
| **Clear Console** | `Ctrl+Shift+C` |
| **Exit Play Mode** | `Ctrl+P` |

---

## ?? **Stop Getting These Errors**

### **DON'T:**
? Select enemies during Play Mode
? Select projectiles during Play Mode
? Select anything that gets destroyed

### **DO:**
? Select Player (persistent)
? Select Terrain (static)
? Select AudioManager (DontDestroyOnLoad)
? Use Debug.Log for info
? Use Scene View Gizmos

---

## ?? **Pro Tip**

**Want to check boss health?**

DON'T select boss in Hierarchy!

INSTEAD:
```csharp
Debug.Log($"Boss HP: {health.CurrentHealth}");
```

Or look at Scene View (colored circles show ranges)

---

## ?? **Why 3 Different Errors?**

Same root cause, different Inspector components:

1. **NullReferenceException**
   - GameObjectInspector tries to disable
   - Object is null ? Error!

2. **MissingReferenceException**
   - GameObjectInspector tries to enable
   - m_Targets missing ? Error!

3. **SerializedObjectNotCreatableException**
   - TransformInspector tries to serialize
   - Can't serialize null ? Error!

**All mean:** Stop selecting destroyed objects!

---

## ?? **Safe Testing Workflow**

```
? CORRECT:
1. Enter Play Mode
2. DON'T select boss
3. Watch Console logs
4. Fight boss
5. Boss dies
6. No errors!

? WRONG:
1. Enter Play Mode
2. Select boss in Hierarchy
3. Fight boss
4. Boss dies
5. ERRORS EVERYWHERE! ??
```

---

## ?? **Already Fixed in Code**

Your Chaosmancer now:
- ? Prevents multiple death calls
- ? Disables component (not destroys GameObject)
- ? Cleans up references properly
- ? Stops all coroutines safely

**These errors only happen if you SELECT the boss!**

---

## ?? **Quick Reference**

| Problem | Solution |
|---------|----------|
| Any Inspector error | `Ctrl+Shift+A` |
| Boss selected | Deselect it |
| Want boss info | Use Debug.Log |
| Testing boss | Don't select it |
| Error spam | Clear Console |

---

## ?? **Still Getting Errors?**

**Do this in order:**

1. **Deselect:** `Ctrl+Shift+A`
2. **Clear Console:** `Ctrl+Shift+C`
3. **Lock/Unlock Inspector:** Click lock icon 2x
4. **Exit Play Mode:** `Ctrl+P`
5. **Save & Restart Unity** (last resort)

---

## ?? **Remember**

**The Rule:**
> If it can die, don't select it during Play Mode!

**The Fix:**
> `Ctrl+Shift+A` (Deselect All)

**The Prevention:**
> Use Debug.Log, not Inspector selection

---

## ? **Checklist**

During Play Mode:
```
? Don't select Chaosmancer
? Don't select enemies
? Don't select projectiles
? Use Debug.Log for info
? Use Scene View Gizmos
? Select persistent objects only
```

If you get error:
```
? Press Ctrl+Shift+A
? Error fixed!
? Keep playing!
```

---

## ?? **One-Liner Summary**

**Error:** Selected destroyed object
**Fix:** `Ctrl+Shift+A`
**Prevention:** Don't select enemies

---

**Print this card. Keep it visible. Save time.** ???

---

## ?? **Full Documentation**

See `INSPECTOR_ERROR_FIX.md` for:
- Detailed explanations
- Advanced debugging
- Code examples
- Prevention strategies
- Pro tips

---

**You got this!** ????
