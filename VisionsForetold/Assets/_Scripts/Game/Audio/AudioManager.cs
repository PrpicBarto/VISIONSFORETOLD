using UnityEngine;
using System.Collections;

/// <summary>
/// AudioManager - Persistent audio system across scenes
/// Handles background music, combat music transitions, and sound effects
/// Singleton pattern ensures one instance across all scenes
/// </summary>
public class AudioManager : MonoBehaviour
{
    #region Singleton

    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<AudioManager>();
                if (instance == null)
                {
                    GameObject audioManagerObj = new GameObject("AudioManager");
                    instance = audioManagerObj.AddComponent<AudioManager>();
                }
            }
            return instance;
        }
    }

    #endregion

    #region Music Tracks

    [Header("Music Tracks")]
    [Tooltip("Music for the main menu")]
    [SerializeField] private AudioClip mainMenuMusic;
    
    [Tooltip("Music for the point and click scene")]
    [SerializeField] private AudioClip pointClickMusic;
    
    [Tooltip("Passive combat music (exploration/out of combat) - plays simultaneously with aggressive")]
    [SerializeField] private AudioClip combatPassiveMusic;
    
    [Tooltip("Aggressive combat music (during combat) - plays simultaneously with passive")]
    [SerializeField] private AudioClip combatAggressiveMusic;

    #endregion

    #region Audio Sources

    [Header("Audio Sources")]
    [Tooltip("Primary music source")]
    [SerializeField] private AudioSource musicSource1;
    
    [Tooltip("Secondary music source (for crossfading)")]
    [SerializeField] private AudioSource musicSource2;
    
    [Tooltip("Combat passive layer source (always playing in combat scenes)")]
    [SerializeField] private AudioSource combatPassiveSource;
    
    [Tooltip("Combat aggressive layer source (always playing in combat scenes)")]
    [SerializeField] private AudioSource combatAggressiveSource;
    
    [Tooltip("Sound effects source")]
    [SerializeField] private AudioSource sfxSource;

    #endregion

    #region Settings

    [Header("Volume Settings")]
    [Tooltip("Master volume (0-1)")]
    [SerializeField] [Range(0f, 1f)] private float masterVolume = 1f;
    
    [Tooltip("Music volume (0-1)")]
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.7f;
    
    [Tooltip("SFX volume (0-1)")]
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 1f;

    [Header("Transition Settings")]
    [Tooltip("How long music takes to fade in/out (seconds)")]
    [SerializeField] private float musicFadeDuration = 2f;
    
    [Tooltip("How long combat music takes to transition between layers (seconds)")]
    [SerializeField] private float combatTransitionDuration = 1.5f;
    
    [Tooltip("Delay before switching from aggressive to passive combat music")]
    [SerializeField] private float combatExitDelay = 3f;

    [Header("Combat Music Settings")]
    [Tooltip("Use layered combat music (both tracks play simultaneously, crossfade volumes)")]
    [SerializeField] private bool useLayeredCombatMusic = true;
    
    [Tooltip("Start both combat tracks at the same time (synced)")]
    [SerializeField] private bool syncCombatTracks = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    #endregion

    #region State Variables

    private AudioSource currentMusicSource;
    private AudioSource fadingMusicSource;
    
    private bool isTransitioning = false;
    private bool isInCombat = false;
    private bool isInCombatScene = false;
    private Coroutine fadeCoroutine;
    private Coroutine combatExitCoroutine;
    private Coroutine combatLayerFadeCoroutine;
    
    private AudioClip currentlyPlaying;
    private string currentScene = "";

    #endregion

    #region Properties

    public float MasterVolume
    {
        get => masterVolume;
        set
        {
            masterVolume = Mathf.Clamp01(value);
            UpdateVolumes();
        }
    }

    public float MusicVolume
    {
        get => musicVolume;
        set
        {
            musicVolume = Mathf.Clamp01(value);
            UpdateVolumes();
        }
    }

    public float SfxVolume
    {
        get => sfxVolume;
        set
        {
            sfxVolume = Mathf.Clamp01(value);
            UpdateVolumes();
        }
    }

    public bool IsInCombat => isInCombat;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        // Singleton pattern
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeAudioSources();
        
        if (showDebugLogs)
        {
            Debug.Log("[AudioManager] Initialized and persisted across scenes");
        }
    }

    private void Start()
    {
        UpdateVolumes();
        
        // Play music based on current scene
        DetectAndPlaySceneMusic();
    }

    private void OnEnable()
    {
        // Subscribe to scene loaded event
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Unsubscribe from scene loaded event
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    #endregion

    #region Initialization

    private void InitializeAudioSources()
    {
        // Create music sources if they don't exist
        if (musicSource1 == null)
        {
            GameObject musicObj1 = new GameObject("MusicSource1");
            musicObj1.transform.SetParent(transform);
            musicSource1 = musicObj1.AddComponent<AudioSource>();
            ConfigureMusicSource(musicSource1);
        }

        if (musicSource2 == null)
        {
            GameObject musicObj2 = new GameObject("MusicSource2");
            musicObj2.transform.SetParent(transform);
            musicSource2 = musicObj2.AddComponent<AudioSource>();
            ConfigureMusicSource(musicSource2);
        }

        // Create combat layer sources
        if (combatPassiveSource == null)
        {
            GameObject passiveObj = new GameObject("CombatPassiveSource");
            passiveObj.transform.SetParent(transform);
            combatPassiveSource = passiveObj.AddComponent<AudioSource>();
            ConfigureMusicSource(combatPassiveSource);
        }

        if (combatAggressiveSource == null)
        {
            GameObject aggressiveObj = new GameObject("CombatAggressiveSource");
            aggressiveObj.transform.SetParent(transform);
            combatAggressiveSource = aggressiveObj.AddComponent<AudioSource>();
            ConfigureMusicSource(combatAggressiveSource);
        }

        // Create SFX source if it doesn't exist
        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
        }

        // Set initial current source
        currentMusicSource = musicSource1;
        fadingMusicSource = musicSource2;
    }

    private void ConfigureMusicSource(AudioSource source)
    {
        source.playOnAwake = false;
        source.loop = true;
        source.priority = 0;
    }

    #endregion

    #region Scene Management

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        currentScene = scene.name;
        
        if (showDebugLogs)
        {
            Debug.Log($"[AudioManager] Scene loaded: {scene.name}");
        }

        DetectAndPlaySceneMusic();
    }

    private void DetectAndPlaySceneMusic()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        currentScene = sceneName;

        // Determine which music to play based on scene name
        if (sceneName.Contains("MainMenu") || sceneName.Contains("Menu"))
        {
            PlayMainMenuMusic();
            isInCombatScene = false;
        }
        else if (sceneName.Contains("PointNClick") || sceneName.Contains("PointAndClick"))
        {
            PlayPointClickMusic();
            isInCombatScene = false;
        }
        else
        {
            // Assume game scene - start layered combat music
            PlayLayeredCombatMusic();
            isInCombatScene = true;
        }
    }

    #endregion

    #region Music Playback

    /// <summary>
    /// Play main menu music
    /// </summary>
    public void PlayMainMenuMusic()
    {
        if (mainMenuMusic == null)
        {
            Debug.LogWarning("[AudioManager] Main menu music not assigned!");
            return;
        }

        StopLayeredCombatMusic();
        
        if (currentlyPlaying == mainMenuMusic && currentMusicSource.isPlaying)
        {
            return; // Already playing
        }

        PlayMusic(mainMenuMusic, musicFadeDuration);
        isInCombat = false;
        isInCombatScene = false;
        
        if (showDebugLogs)
        {
            Debug.Log("[AudioManager] Playing main menu music");
        }
    }

    /// <summary>
    /// Play point and click scene music
    /// </summary>
    public void PlayPointClickMusic()
    {
        if (pointClickMusic == null)
        {
            Debug.LogWarning("[AudioManager] Point & Click music not assigned!");
            return;
        }

        StopLayeredCombatMusic();
        
        if (currentlyPlaying == pointClickMusic && currentMusicSource.isPlaying)
        {
            return; // Already playing
        }

        PlayMusic(pointClickMusic, musicFadeDuration);
        isInCombat = false;
        isInCombatScene = false;
        
        if (showDebugLogs)
        {
            Debug.Log("[AudioManager] Playing Point & Click music");
        }
    }

    /// <summary>
    /// Start playing layered combat music (both tracks simultaneously)
    /// </summary>
    private void PlayLayeredCombatMusic()
    {
        if (combatPassiveMusic == null || combatAggressiveMusic == null)
        {
            Debug.LogWarning("[AudioManager] Combat music tracks not assigned!");
            return;
        }

        if (!useLayeredCombatMusic)
        {
            // Fall back to old system
            PlayCombatPassiveMusic();
            return;
        }

        // Stop regular music sources
        StopRegularMusic();

        // Setup both combat tracks
        combatPassiveSource.clip = combatPassiveMusic;
        combatAggressiveSource.clip = combatAggressiveMusic;

        // Start both tracks
        if (syncCombatTracks)
        {
            // Sync both tracks to play from same position
            combatPassiveSource.time = 0;
            combatAggressiveSource.time = 0;
            combatPassiveSource.Play();
            combatAggressiveSource.Play();
        }
        else
        {
            combatPassiveSource.Play();
            combatAggressiveSource.Play();
        }

        // Set initial volumes (passive full, aggressive silent)
        combatPassiveSource.volume = musicVolume * masterVolume;
        combatAggressiveSource.volume = 0f;

        isInCombatScene = true;
        isInCombat = false; // Start in exploration mode
        
        if (showDebugLogs)
        {
            Debug.Log("[AudioManager] Started layered combat music (passive layer active)");
        }
    }

    /// <summary>
    /// Stop layered combat music
    /// </summary>
    private void StopLayeredCombatMusic()
    {
        if (combatLayerFadeCoroutine != null)
        {
            StopCoroutine(combatLayerFadeCoroutine);
            combatLayerFadeCoroutine = null;
        }

        combatPassiveSource.Stop();
        combatAggressiveSource.Stop();
        isInCombatScene = false;
    }

    /// <summary>
    /// Stop regular music sources (for switching to layered combat)
    /// </summary>
    private void StopRegularMusic()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        musicSource1.Stop();
        musicSource2.Stop();
        currentlyPlaying = null;
    }

    /// <summary>
    /// Play passive combat music (legacy - for non-layered mode)
    /// </summary>
    public void PlayCombatPassiveMusic()
    {
        if (combatPassiveMusic == null)
        {
            Debug.LogWarning("[AudioManager] Passive combat music not assigned!");
            return;
        }

        if (currentlyPlaying == combatPassiveMusic && currentMusicSource.isPlaying)
        {
            return; // Already playing
        }

        PlayMusic(combatPassiveMusic, musicFadeDuration);
        isInCombat = false;
        
        if (showDebugLogs)
        {
            Debug.Log("[AudioManager] Playing passive combat music (exploration)");
        }
    }

    /// <summary>
    /// Play aggressive combat music (legacy - for non-layered mode)
    /// </summary>
    public void PlayCombatAggressiveMusic()
    {
        if (combatAggressiveMusic == null)
        {
            Debug.LogWarning("[AudioManager] Aggressive combat music not assigned!");
            return;
        }

        if (currentlyPlaying == combatAggressiveMusic && currentMusicSource.isPlaying)
        {
            return; // Already playing
        }

        PlayMusic(combatAggressiveMusic, combatTransitionDuration);
        isInCombat = true;
        
        if (showDebugLogs)
        {
            Debug.Log("[AudioManager] Playing aggressive combat music");
        }
    }

    /// <summary>
    /// Generic music playback with crossfade
    /// </summary>
    private void PlayMusic(AudioClip newClip, float fadeDuration)
    {
        if (newClip == null) return;

        // Stop any existing fade
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(CrossfadeMusic(newClip, fadeDuration));
    }

    #endregion

    #region Combat Music System

    /// <summary>
    /// Enter combat - switch to aggressive music layer
    /// Call this when player enters combat
    /// </summary>
    public void EnterCombat()
    {
        if (isInCombat) return; // Already in combat

        // Cancel any pending combat exit
        if (combatExitCoroutine != null)
        {
            StopCoroutine(combatExitCoroutine);
            combatExitCoroutine = null;
        }

        isInCombat = true;

        if (useLayeredCombatMusic && isInCombatScene)
        {
            // Crossfade to aggressive layer
            if (combatLayerFadeCoroutine != null)
            {
                StopCoroutine(combatLayerFadeCoroutine);
            }
            combatLayerFadeCoroutine = StartCoroutine(CrossfadeCombatLayers(true));
        }
        else
        {
            // Use old system
            PlayCombatAggressiveMusic();
        }
        
        if (showDebugLogs)
        {
            Debug.Log("[AudioManager] Entered combat - fading to aggressive layer");
        }
    }

    /// <summary>
    /// Exit combat - switch to passive music layer after delay
    /// Call this when player exits combat
    /// </summary>
    public void ExitCombat()
    {
        if (!isInCombat) return; // Not in combat

        // Start delayed transition to passive music
        if (combatExitCoroutine != null)
        {
            StopCoroutine(combatExitCoroutine);
        }

        combatExitCoroutine = StartCoroutine(ExitCombatDelayed());
        
        if (showDebugLogs)
        {
            Debug.Log($"[AudioManager] Exiting combat (delay: {combatExitDelay}s)");
        }
    }

    private IEnumerator ExitCombatDelayed()
    {
        yield return new WaitForSeconds(combatExitDelay);

        isInCombat = false;

        if (useLayeredCombatMusic && isInCombatScene)
        {
            // Crossfade to passive layer
            if (combatLayerFadeCoroutine != null)
            {
                StopCoroutine(combatLayerFadeCoroutine);
            }
            combatLayerFadeCoroutine = StartCoroutine(CrossfadeCombatLayers(false));
        }
        else
        {
            // Use old system
            PlayCombatPassiveMusic();
        }

        combatExitCoroutine = null;
        
        if (showDebugLogs)
        {
            Debug.Log("[AudioManager] Exited combat - fading to passive layer");
        }
    }

    /// <summary>
    /// Crossfade between combat music layers
    /// </summary>
    /// <param name="toAggressive">True = fade to aggressive, False = fade to passive</param>
    private IEnumerator CrossfadeCombatLayers(bool toAggressive)
    {
        float elapsed = 0f;
        float startPassiveVolume = combatPassiveSource.volume;
        float startAggressiveVolume = combatAggressiveSource.volume;
        
        float targetPassiveVolume = toAggressive ? 0f : musicVolume * masterVolume;
        float targetAggressiveVolume = toAggressive ? musicVolume * masterVolume : 0f;

        while (elapsed < combatTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / combatTransitionDuration;

            // Smooth curve for more natural transition
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            combatPassiveSource.volume = Mathf.Lerp(startPassiveVolume, targetPassiveVolume, smoothT);
            combatAggressiveSource.volume = Mathf.Lerp(startAggressiveVolume, targetAggressiveVolume, smoothT);

            yield return null;
        }

        // Finalize volumes
        combatPassiveSource.volume = targetPassiveVolume;
        combatAggressiveSource.volume = targetAggressiveVolume;

        combatLayerFadeCoroutine = null;
        
        if (showDebugLogs)
        {
            Debug.Log($"[AudioManager] Combat layer crossfade complete - {(toAggressive ? "Aggressive" : "Passive")} layer active");
        }
    }

    #endregion

    #region Crossfade System

    private IEnumerator CrossfadeMusic(AudioClip newClip, float duration)
    {
        isTransitioning = true;

        // Setup new music source
        AudioSource newSource = GetInactiveSource();
        newSource.clip = newClip;
        newSource.volume = 0f;
        newSource.Play();

        float elapsed = 0f;

        // Crossfade
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Fade out old, fade in new
            if (currentMusicSource.isPlaying)
            {
                currentMusicSource.volume = Mathf.Lerp(musicVolume * masterVolume, 0f, t);
            }
            newSource.volume = Mathf.Lerp(0f, musicVolume * masterVolume, t);

            yield return null;
        }

        // Finalize transition
        if (currentMusicSource.isPlaying)
        {
            currentMusicSource.Stop();
            currentMusicSource.volume = 0f;
        }

        newSource.volume = musicVolume * masterVolume;

        // Swap sources
        fadingMusicSource = currentMusicSource;
        currentMusicSource = newSource;
        currentlyPlaying = newClip;

        isTransitioning = false;
        fadeCoroutine = null;
    }

    private AudioSource GetInactiveSource()
    {
        return currentMusicSource == musicSource1 ? musicSource2 : musicSource1;
    }

    #endregion

    #region Sound Effects

    /// <summary>
    /// Play a one-shot sound effect
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        
        sfxSource.PlayOneShot(clip, sfxVolume * masterVolume);
    }

    /// <summary>
    /// Play a one-shot sound effect at a specific position (3D)
    /// </summary>
    public void PlaySFXAtPosition(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;
        
        AudioSource.PlayClipAtPoint(clip, position, sfxVolume * masterVolume);
    }

    /// <summary>
    /// Play a one-shot sound effect with custom volume
    /// </summary>
    public void PlaySFX(AudioClip clip, float volumeScale)
    {
        if (clip == null) return;
        
        sfxSource.PlayOneShot(clip, volumeScale * sfxVolume * masterVolume);
    }

    #endregion

    #region Volume Control

    private void UpdateVolumes()
    {
        if (currentMusicSource != null && currentMusicSource.isPlaying)
        {
            currentMusicSource.volume = musicVolume * masterVolume;
        }

        // Update combat layer volumes based on current state
        if (isInCombatScene && useLayeredCombatMusic)
        {
            if (isInCombat)
            {
                combatPassiveSource.volume = 0f;
                combatAggressiveSource.volume = musicVolume * masterVolume;
            }
            else
            {
                combatPassiveSource.volume = musicVolume * masterVolume;
                combatAggressiveSource.volume = 0f;
            }
        }

        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume * masterVolume;
        }
    }

    /// <summary>
    /// Set master volume (affects all audio)
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        MasterVolume = volume;
    }

    /// <summary>
    /// Set music volume
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        MusicVolume = volume;
    }

    /// <summary>
    /// Set SFX volume
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        SfxVolume = volume;
    }

    #endregion

    #region Playback Control

    /// <summary>
    /// Stop all music
    /// </summary>
    public void StopMusic()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        musicSource1.Stop();
        musicSource2.Stop();
        
        StopLayeredCombatMusic();
        
        currentlyPlaying = null;
        
        if (showDebugLogs)
        {
            Debug.Log("[AudioManager] Music stopped");
        }
    }

    /// <summary>
    /// Pause current music
    /// </summary>
    public void PauseMusic()
    {
        if (currentMusicSource != null && currentMusicSource.isPlaying)
        {
            currentMusicSource.Pause();
        }

        if (isInCombatScene)
        {
            combatPassiveSource.Pause();
            combatAggressiveSource.Pause();
        }
    }

    /// <summary>
    /// Resume paused music
    /// </summary>
    public void ResumeMusic()
    {
        if (currentMusicSource != null && currentMusicSource.clip != null)
        {
            currentMusicSource.UnPause();
        }

        if (isInCombatScene)
        {
            combatPassiveSource.UnPause();
            combatAggressiveSource.UnPause();
        }
    }

    /// <summary>
    /// Check if music is currently playing
    /// </summary>
    public bool IsMusicPlaying()
    {
        if (isInCombatScene)
        {
            return combatPassiveSource.isPlaying || combatAggressiveSource.isPlaying;
        }
        return currentMusicSource != null && currentMusicSource.isPlaying;
    }

    #endregion

    #region Debug Helpers

    /// <summary>
    /// Get current music track name
    /// </summary>
    public string GetCurrentMusicName()
    {
        if (isInCombatScene)
        {
            return isInCombat ? "Combat Aggressive Layer" : "Combat Passive Layer";
        }
        if (currentlyPlaying == null) return "None";
        return currentlyPlaying.name;
    }

    /// <summary>
    /// Force play a specific music track (for testing)
    /// </summary>
    public void DebugPlayMusic(AudioClip clip)
    {
        if (clip != null)
        {
            PlayMusic(clip, musicFadeDuration);
        }
    }

    #endregion
}
