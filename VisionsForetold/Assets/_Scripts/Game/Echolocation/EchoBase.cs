using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EchoBase : MonoBehaviour
{
    [Header("Echolocation Settings")]
    [SerializeField] private float echoRadius = 10f; // Radius of revealed area
    [SerializeField] private float echoInterval = 2f; // Time between echolocation pulses
    [SerializeField] private float echoFadeTime = 3f; // Time for echo to fade in/out (longer for visibility)
    [SerializeField] private bool autoEcho = true; // Automatically trigger echoes

    [Header("Manual Echo Settings")]
    [SerializeField] private KeyCode manualEchoKey = KeyCode.E; // Key for manual echo
    [SerializeField] private float manualEchoCooldown = 1f; // Cooldown for manual echoes

    [Header("Visual Settings")]
    [SerializeField] private Color echoColor = Color.cyan;
    [SerializeField] private AnimationCurve echoFadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool showEchoWaves = true; // Show visual echo waves

    [Header("Audio Settings")]
    [SerializeField] private AudioClip echoSound;
    [SerializeField] private float echoVolume = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    [SerializeField] private bool showDebugLogs = true;

    // Private variables
    private float lastEchoTime = -Mathf.Infinity;
    private float lastManualEchoTime = -Mathf.Infinity;
    private AudioSource audioSource;
    private List<EchoWave> activeEchoWaves = new List<EchoWave>();

    // Echo wave data structure
    private struct EchoWave
    {
        public Vector3 origin;
        public float startTime;
        public float maxRadius;
        public bool isActive;

        public EchoWave(Vector3 origin, float startTime, float maxRadius)
        {
            this.origin = origin;
            this.startTime = startTime;
            this.maxRadius = maxRadius;
            this.isActive = true;
        }
    }

    private void Awake()
    {
        SetupComponents();
    }

    private void SetupComponents()
    {
        // Get or create audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.volume = echoVolume;

        if (showDebugLogs)
        {
            Debug.Log($"EchoBase: Setup complete on {gameObject.name}");
        }
    }

    private void Start()
    {
        // Start automatic echoing if enabled
        if (autoEcho)
        {
            StartCoroutine(AutoEchoCoroutine());
        }

        // Trigger initial echo
        TriggerEcho();

        if (showDebugLogs)
        {
            Debug.Log($"EchoBase: Started with auto echo: {autoEcho}, initial echo triggered");
        }
    }

    private void Update()
    {
        HandleManualEcho();
        UpdateEchoWaves();

        // Debug info
        if (debugMode && Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log($"Active echo waves: {activeEchoWaves.Count}");
            for (int i = 0; i < activeEchoWaves.Count; i++)
            {
                var wave = activeEchoWaves[i];
                float age = Time.time - wave.startTime;
                float currentRadius = wave.maxRadius * (age / echoFadeTime);
                Debug.Log($"Wave {i}: Age {age:F2}s, Radius {currentRadius:F2}, Max {wave.maxRadius}");
            }
        }
    }

    private void HandleManualEcho()
    {
        if (Input.GetKeyDown(manualEchoKey))
        {
            if (Time.time >= lastManualEchoTime + manualEchoCooldown)
            {
                TriggerEcho();
                lastManualEchoTime = Time.time;
            }
        }
    }

    private void UpdateEchoWaves()
    {
        // Update and clean up echo waves
        for (int i = activeEchoWaves.Count - 1; i >= 0; i--)
        {
            EchoWave wave = activeEchoWaves[i];
            float waveAge = Time.time - wave.startTime;

            if (waveAge > echoFadeTime)
            {
                activeEchoWaves.RemoveAt(i);
                if (showDebugLogs)
                {
                    Debug.Log($"Echo wave expired after {waveAge:F2} seconds");
                }
            }
        }
    }

    private IEnumerator AutoEchoCoroutine()
    {
        while (autoEcho)
        {
            yield return new WaitForSeconds(echoInterval);
            if (autoEcho) // Check again in case it was disabled during wait
            {
                TriggerEcho();
            }
        }
    }

    public void TriggerEcho()
    {
        lastEchoTime = Time.time;

        // Create new echo wave
        EchoWave newWave = new EchoWave(transform.position, Time.time, echoRadius);
        activeEchoWaves.Add(newWave);

        // Play echo sound
        PlayEchoSound();

        if (showDebugLogs)
        {
            Debug.Log($"Echo triggered at {transform.position} with radius {echoRadius}. Total waves: {activeEchoWaves.Count}");
        }
    }

    private void PlayEchoSound()
    {
        if (echoSound != null && audioSource != null)
        {
            audioSource.clip = echoSound;
            audioSource.Play();
        }
    }

    // Public properties for EcholocationEffect
    public float EchoRadius => echoRadius;
    public Color EchoColor => echoColor;
    public Vector3 PlayerPosition => transform.position;
    public int ActiveWaveCount => activeEchoWaves.Count;

    // This is the key method that EcholocationEffect calls
    public void UpdateEcholocationMaterial(Material material)
    {
        if (material == null) return;

        // Set basic properties
        material.SetVector("_PlayerPosition", new Vector4(transform.position.x, transform.position.y, transform.position.z, 0));
        material.SetFloat("_EchoRadius", echoRadius);
        material.SetColor("_EchoColor", echoColor);
        material.SetColor("_BlackoutColor", Color.black);

        // Update echo waves
        if (activeEchoWaves.Count > 0)
        {
            UpdateEchoWavesInMaterial(material);
        }
        else
        {
            // No active waves - clear wave data
            material.SetInt("_WaveCount", 0);
        }

        if (debugMode && showDebugLogs && Time.frameCount % 60 == 0) // Log once per second
        {
            Debug.Log($"Material updated - Waves: {activeEchoWaves.Count}, Player pos: {transform.position}, Radius: {echoRadius}");
        }
    }

    private void UpdateEchoWavesInMaterial(Material material)
    {
        int waveCount = Mathf.Min(activeEchoWaves.Count, 8);
        Vector4[] wavePositions = new Vector4[8]; // Always allocate 8 to avoid array issues
        Vector4[] waveData = new Vector4[8];

        for (int i = 0; i < waveCount; i++)
        {
            EchoWave wave = activeEchoWaves[i];
            float waveAge = Time.time - wave.startTime;
            float normalizedAge = waveAge / echoFadeTime;
            float currentRadius = wave.maxRadius * normalizedAge;
            float intensity = echoFadeCurve.Evaluate(1f - normalizedAge);

            wavePositions[i] = new Vector4(wave.origin.x, wave.origin.y, wave.origin.z, currentRadius);
            waveData[i] = new Vector4(intensity, wave.maxRadius, 0, 0);
        }

        // Fill remaining slots with empty data
        for (int i = waveCount; i < 8; i++)
        {
            wavePositions[i] = Vector4.zero;
            waveData[i] = Vector4.zero;
        }

        material.SetVectorArray("_WavePositions", wavePositions);
        material.SetVectorArray("_WaveData", waveData);
        material.SetInt("_WaveCount", waveCount);

        if (debugMode && showDebugLogs && Time.frameCount % 120 == 0)
        {
            Debug.Log($"Wave data updated - Count: {waveCount}, First wave radius: {(waveCount > 0 ? wavePositions[0].w : 0):F2}");
        }
    }

    // Public utility methods
    public bool IsPositionRevealed(Vector3 worldPosition)
    {
        foreach (EchoWave wave in activeEchoWaves)
        {
            float distance = Vector3.Distance(worldPosition, wave.origin);
            float waveAge = Time.time - wave.startTime;
            float currentRadius = wave.maxRadius * (waveAge / echoFadeTime);

            if (distance <= currentRadius && waveAge <= echoFadeTime)
            {
                return true;
            }
        }
        return false;
    }

    public float GetRevealIntensityAtPosition(Vector3 worldPosition)
    {
        float maxIntensity = 0f;

        foreach (EchoWave wave in activeEchoWaves)
        {
            float distance = Vector3.Distance(worldPosition, wave.origin);
            float waveAge = Time.time - wave.startTime;
            float normalizedAge = waveAge / echoFadeTime;
            float currentRadius = wave.maxRadius * normalizedAge;

            if (distance <= currentRadius && waveAge <= echoFadeTime)
            {
                float distanceIntensity = 1f - (distance / currentRadius);
                float timeIntensity = echoFadeCurve.Evaluate(1f - normalizedAge);
                float intensity = distanceIntensity * timeIntensity;
                maxIntensity = Mathf.Max(maxIntensity, intensity);
            }
        }

        return maxIntensity;
    }

    // Configuration methods
    public void SetEchoRadius(float newRadius)
    {
        echoRadius = Mathf.Max(0.1f, newRadius);
    }

    public void SetEchoInterval(float newInterval)
    {
        echoInterval = Mathf.Max(0.1f, newInterval);
    }

    public void EnableAutoEcho(bool enable)
    {
        bool wasEnabled = autoEcho;
        autoEcho = enable;

        if (enable && !wasEnabled)
        {
            StartCoroutine(AutoEchoCoroutine());
        }

        Debug.Log($"Auto echo: {autoEcho}");
    }

    // Debug methods
    [ContextMenu("Trigger Manual Echo")]
    public void TriggerManualEcho()
    {
        TriggerEcho();
    }

    [ContextMenu("Clear All Echoes")]
    public void ClearAllEchoes()
    {
        activeEchoWaves.Clear();
        Debug.Log("All echo waves cleared");
    }

    [ContextMenu("Debug Echo State")]
    public void DebugEchoState()
    {
        Debug.Log($"=== ECHO DEBUG ===");
        Debug.Log($"Position: {transform.position}");
        Debug.Log($"Echo radius: {echoRadius}");
        Debug.Log($"Auto echo: {autoEcho}");
        Debug.Log($"Active waves: {activeEchoWaves.Count}");
        Debug.Log($"Echo interval: {echoInterval}");
        Debug.Log($"Fade time: {echoFadeTime}");
        Debug.Log($"Last echo: {Time.time - lastEchoTime:F2}s ago");
    }

    private void OnDrawGizmosSelected()
    {
        // Draw echo radius
        Gizmos.color = echoColor;
        Gizmos.DrawWireSphere(transform.position, echoRadius);

        // Draw active echo waves
        Gizmos.color = Color.yellow;
        foreach (EchoWave wave in activeEchoWaves)
        {
            float waveAge = Time.time - wave.startTime;
            if (waveAge <= echoFadeTime)
            {
                float currentRadius = wave.maxRadius * (waveAge / echoFadeTime);
                Gizmos.DrawWireSphere(wave.origin, currentRadius);
            }
        }

        // Draw a small sphere at player position
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}