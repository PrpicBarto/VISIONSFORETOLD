# ?? AudioManager - Quick Integration Examples

## Combat Music Integration

### Option 1: Player Attack Script

**Add to PlayerAttack.cs:**

```csharp
// At the top of the class
private float lastCombatTime = 0f;
private float combatTimeout = 5f; // How long after attack to exit combat

// In your attack method
private void PerformAttack()
{
    // Your attack code...
    
    // Enter combat mode
    AudioManager.Instance.EnterCombat();
    lastCombatTime = Time.time;
}

// In Update()
private void Update()
{
    // Check if we should exit combat
    if (AudioManager.Instance.IsInCombat)
    {
        if (Time.time - lastCombatTime > combatTimeout)
        {
            AudioManager.Instance.ExitCombat();
        }
    }
    
    // Rest of your update code...
}
```

---

### Option 2: Enemy Detection (Recommended)

**Add to your BaseEnemy.cs:**

```csharp
protected virtual void OnPlayerDetected()
{
    // Enter combat when enemy detects player
    AudioManager.Instance.EnterCombat();
}

protected override void OnDeath()
{
    isDead = true;
    
    // Check if this was the last enemy
    BaseEnemy[] enemies = FindObjectsByType<BaseEnemy>(FindObjectsSortMode.None);
    int aliveEnemies = 0;
    
    foreach (var enemy in enemies)
    {
        if (enemy != this && !enemy.isDead)
        {
            aliveEnemies++;
        }
    }
    
    if (aliveEnemies == 0)
    {
        // No more enemies - exit combat
        AudioManager.Instance.ExitCombat();
    }
    
    // Rest of death logic...
}
```

---

### Option 3: Combat Zone Trigger

**Create a CombatZone script:**

```csharp
using UnityEngine;

public class CombatZone : MonoBehaviour
{
    [SerializeField] private bool enterCombatOnEnter = true;
    [SerializeField] private bool exitCombatOnExit = true;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && enterCombatOnEnter)
        {
            AudioManager.Instance.EnterCombat();
            Debug.Log("Player entered combat zone");
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && exitCombatOnExit)
        {
            AudioManager.Instance.ExitCombat();
            Debug.Log("Player left combat zone");
        }
    }
}
```

**Setup:**
```
1. Create empty GameObject
2. Name it "CombatZone"
3. Add CombatZone script
4. Add BoxCollider or SphereCollider
5. Set "Is Trigger" to true
6. Size collider to cover combat area
7. Place around enemy groups
```

---

### Option 4: Simple Distance Check

**Add to a GameManager or CombatManager:**

```csharp
public class SimpleCombatDetector : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float combatRange = 15f;
    [SerializeField] private float checkInterval = 0.5f;
    
    private float lastCheckTime;
    private bool wasInCombat = false;
    
    private void Update()
    {
        if (Time.time - lastCheckTime < checkInterval)
            return;
        
        lastCheckTime = Time.time;
        
        // Find all enemies
        BaseEnemy[] enemies = FindObjectsByType<BaseEnemy>(FindObjectsSortMode.None);
        
        bool isInCombat = false;
        
        // Check if any enemy is close to player
        foreach (var enemy in enemies)
        {
            if (enemy.isDead) continue;
            
            float distance = Vector3.Distance(player.position, enemy.transform.position);
            if (distance < combatRange)
            {
                isInCombat = true;
                break;
            }
        }
        
        // Update combat state
        if (isInCombat && !wasInCombat)
        {
            AudioManager.Instance.EnterCombat();
        }
        else if (!isInCombat && wasInCombat)
        {
            AudioManager.Instance.ExitCombat();
        }
        
        wasInCombat = isInCombat;
    }
}
```

---

## Sound Effects Integration

### Menu Buttons

**Add to MenuManager.cs:**

```csharp
[Header("Audio")]
[SerializeField] private AudioClip buttonClickSound;
[SerializeField] private AudioClip buttonHoverSound;

private void PlayButtonSound()
{
    if (buttonClickSound != null)
    {
        AudioManager.Instance.PlaySFX(buttonClickSound);
    }
}

public void OnPlayButton()
{
    PlayButtonSound(); // Add this
    LoadGameScene();
}

public void OnOptionsButton()
{
    PlayButtonSound(); // Add this
    ShowOptionsMenu();
}
```

---

### Player Actions

**Add to PlayerAttack.cs:**

```csharp
[Header("Audio")]
[SerializeField] private AudioClip swordSwingSound;
[SerializeField] private AudioClip bowReleaseSound;
[SerializeField] private AudioClip spellCastSound;

private void PerformMeleeAttack()
{
    // Play sword swing sound
    AudioManager.Instance.PlaySFX(swordSwingSound);
    
    // Your attack code...
}

private void PerformRangedAttack()
{
    // Play bow release sound
    AudioManager.Instance.PlaySFX(bowReleaseSound);
    
    // Your attack code...
}

private void CastSpell()
{
    // Play spell cast sound
    AudioManager.Instance.PlaySFX(spellCastSound);
    
    // Your spell code...
}
```

---

### Enemy Actions

**Add to BaseEnemy.cs or child classes:**

```csharp
[Header("Audio")]
[SerializeField] protected AudioClip attackSound;
[SerializeField] protected AudioClip hurtSound;
[SerializeField] protected AudioClip deathSound;

protected void TryAttack()
{
    // Play attack sound
    if (attackSound != null)
    {
        AudioManager.Instance.PlaySFX(attackSound);
    }
    
    // Your attack code...
}

protected virtual void OnDeath()
{
    // Play death sound
    if (deathSound != null)
    {
        AudioManager.Instance.PlaySFX(deathSound);
    }
    
    // Your death code...
}
```

---

## Volume Settings UI

**Create a volume settings UI:**

```csharp
using UnityEngine;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    
    private void Start()
    {
        // Load saved values or use defaults
        masterVolumeSlider.value = AudioManager.Instance.MasterVolume;
        musicVolumeSlider.value = AudioManager.Instance.MusicVolume;
        sfxVolumeSlider.value = AudioManager.Instance.SfxVolume;
        
        // Add listeners
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }
    
    private void OnMasterVolumeChanged(float value)
    {
        AudioManager.Instance.SetMasterVolume(value);
        // Save to PlayerPrefs
        PlayerPrefs.SetFloat("MasterVolume", value);
    }
    
    private void OnMusicVolumeChanged(float value)
    {
        AudioManager.Instance.SetMusicVolume(value);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }
    
    private void OnSFXVolumeChanged(float value)
    {
        AudioManager.Instance.SetSFXVolume(value);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }
}
```

---

## Recommended: CombatManager

**Create a dedicated CombatManager:**

```csharp
using UnityEngine;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }
    
    private HashSet<BaseEnemy> activeEnemies = new HashSet<BaseEnemy>();
    private bool isInCombat = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Register an enemy entering combat
    /// </summary>
    public void RegisterEnemyInCombat(BaseEnemy enemy)
    {
        if (enemy == null) return;
        
        activeEnemies.Add(enemy);
        
        if (!isInCombat)
        {
            isInCombat = true;
            AudioManager.Instance.EnterCombat();
            Debug.Log("Combat started!");
        }
    }
    
    /// <summary>
    /// Unregister an enemy leaving combat (death or lost player)
    /// </summary>
    public void UnregisterEnemyFromCombat(BaseEnemy enemy)
    {
        if (enemy == null) return;
        
        activeEnemies.Remove(enemy);
        
        if (activeEnemies.Count == 0 && isInCombat)
        {
            isInCombat = false;
            AudioManager.Instance.ExitCombat();
            Debug.Log("Combat ended!");
        }
    }
    
    /// <summary>
    /// Check if player is in combat
    /// </summary>
    public bool IsInCombat()
    {
        return isInCombat;
    }
    
    /// <summary>
    /// Get number of active enemies
    /// </summary>
    public int GetActiveEnemyCount()
    {
        return activeEnemies.Count;
    }
}
```

**Then in BaseEnemy.cs:**

```csharp
protected virtual void OnPlayerDetected()
{
    CombatManager.Instance?.RegisterEnemyInCombat(this);
}

protected virtual void OnPlayerLost()
{
    CombatManager.Instance?.UnregisterEnemyFromCombat(this);
}

protected override void OnDeath()
{
    isDead = true;
    CombatManager.Instance?.UnregisterEnemyFromCombat(this);
    // Rest of death code...
}
```

---

## Quick Start Checklist

```
AudioManager Setup:
? Create AudioManager GameObject
? Assign all 4 music tracks
? Test music plays in each scene

Combat Music:
? Choose integration method (Enemy detection recommended)
? Add EnterCombat() calls
? Add ExitCombat() calls
? Test transitions

Sound Effects:
? Import SFX audio clips
? Add to Inspector fields
? Call PlaySFX() where needed
? Test volume levels

Testing:
? Music persists between scenes
? Combat music switches correctly
? SFX plays at correct volume
? Volume sliders work
? No audio errors in Console
```

---

## Summary

**Simplest Integration:**
```csharp
// When enemy detects player
AudioManager.Instance.EnterCombat();

// When last enemy dies
AudioManager.Instance.ExitCombat();

// Play sound effect
AudioManager.Instance.PlaySFX(soundClip);
```

**That's it! Your audio system is ready to use!** ???
