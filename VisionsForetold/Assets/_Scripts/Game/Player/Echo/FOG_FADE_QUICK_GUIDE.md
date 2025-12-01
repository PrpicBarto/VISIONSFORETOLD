# Quick Setup Guide - Fog Fade-Back Effect

## ? 30-Second Setup

1. **Open your Echolocation Material**
2. **Find these new properties:**
   - Pulse Fade Delay
   - Pulse Fade Speed
3. **Adjust values:**
   - Delay: 0.5 (default)
   - Speed: 2.0 (default)
4. **Test in Play Mode**

---

## ??? Quick Presets

### Instant Return (Like Before)
```
Pulse Fade Delay: 0.0
Pulse Fade Speed: 10.0
```

### Fast Action Game
```
Pulse Fade Delay: 0.2
Pulse Fade Speed: 3.0
```

### Balanced (Recommended)
```
Pulse Fade Delay: 0.5
Pulse Fade Speed: 2.0
```

### Exploration Game
```
Pulse Fade Delay: 1.0
Pulse Fade Speed: 1.0
```

### Easy Mode / Long Memory
```
Pulse Fade Delay: 2.0
Pulse Fade Speed: 0.5
```

---

## ?? What Each Setting Does

### **Pulse Fade Delay**
- Time before fog starts fading back
- Higher = longer reveal window
- Lower = fog returns sooner
- Range: 0.0 - 2.0 seconds

### **Pulse Fade Speed**
- How fast fog fades in
- Higher = faster fade
- Lower = slower fade
- Range: 0.5 - 5.0

---

## ?? Visual Guide

```
Delay = 0.5, Speed = 2.0 (Default):
Pulse passes ? 0.5s wait ? Fade over 0.5s ? Fog restored

Delay = 0.0, Speed = 5.0 (Fast):
Pulse passes ? 0s wait ? Fade over 0.2s ? Fog restored

Delay = 1.0, Speed = 1.0 (Slow):
Pulse passes ? 1s wait ? Fade over 1s ? Fog restored
```

---

## ? Testing Checklist

1. [ ] Play mode active
2. [ ] Trigger pulse (auto-pulse or manual)
3. [ ] Watch fog behind pulse
4. [ ] Check if fade timing feels right
5. [ ] Adjust delay/speed as needed
6. [ ] Repeat until satisfied

---

## ?? Recommended Settings by Game Type

| Game Type | Delay | Speed | Why |
|-----------|-------|-------|-----|
| Action | 0.2 | 3.0 | Quick reveal/hide |
| Horror | 0.5 | 1.5 | Tension building |
| Puzzle | 1.0 | 1.0 | Time to think |
| Exploration | 1.5 | 0.8 | Generous viewing |
| Stealth | 0.3 | 2.5 | Tactical visibility |

---

**That's it! Your fog now fades back naturally.** ???
