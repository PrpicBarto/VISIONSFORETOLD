using UnityEngine;

namespace VisionsForetold.Game.Player.Echo
{
    /// <summary>
    /// Keeps pulse-revealed areas visible temporarily after pulse passes
    /// Simpler alternative to EchoRevealSystem for area-based reveals
    /// </summary>
    public class EchoPulseMemory : MonoBehaviour
    {
        [Header("Memory Settings")]
        [Tooltip("How long revealed areas stay visible after pulse passes")]
        [SerializeField] private float memoryDuration = 3f;
        
        [Tooltip("Number of memory zones to track")]
        [SerializeField] private int maxMemoryZones = 10;
        
        [Header("Debug")]
        [SerializeField] private bool showDebug = true;

        private EcholocationController echoController;
        private Material fogMaterial;
        
        // Memory data
        private float[] memoryRadii;
        private float[] memoryStrengths;
        private float[] memoryEndTimes;
        private int activeMemoryCount = 0;
        
        // Shader property IDs
        private static readonly int MemoryCountID = Shader.PropertyToID("_MemoryCount");
        private static readonly int MemoryRadiiID = Shader.PropertyToID("_MemoryRadii");
        private static readonly int MemoryStrengthsID = Shader.PropertyToID("_MemoryStrengths");
        
        // Track last pulse
        private float lastPulseRadius = 0f;
        private bool wasJustPulsing = false;

        private void Awake()
        {
            echoController = GetComponent<EcholocationController>();
            if (echoController == null)
            {
                Debug.LogError("[EchoPulseMemory] EcholocationController not found!");
                enabled = false;
                return;
            }

            // Initialize arrays
            memoryRadii = new float[maxMemoryZones];
            memoryStrengths = new float[maxMemoryZones];
            memoryEndTimes = new float[maxMemoryZones];
            
            fogMaterial = echoController.GetFogMaterial();
        }

        private void Update()
        {
            bool currentlyPulsing = echoController.IsPulsing;
            float currentRadius = echoController.CurrentPulseRadius;

            // Detect pulse end
            if (wasJustPulsing && !currentlyPulsing)
            {
                // Pulse just ended - create memory zone
                CreateMemoryZone(lastPulseRadius);
            }

            // Track pulse state
            wasJustPulsing = currentlyPulsing;
            if (currentlyPulsing)
            {
                lastPulseRadius = currentRadius;
            }

            // Update memory zones
            UpdateMemoryZones();
            
            // Send to shader
            SendDataToShader();
        }

        private void CreateMemoryZone(float radius)
        {
            // Find empty slot or replace oldest
            int slot = FindFreeSlot();
            
            memoryRadii[slot] = radius;
            memoryStrengths[slot] = 1.0f;
            memoryEndTimes[slot] = Time.time + memoryDuration;
            
            activeMemoryCount = Mathf.Min(activeMemoryCount + 1, maxMemoryZones);

            if (showDebug)
            {
                Debug.Log($"[EchoPulseMemory] Created memory zone: radius {radius:F1}, duration {memoryDuration}s");
            }
        }

        private int FindFreeSlot()
        {
            // Find expired slot
            for (int i = 0; i < maxMemoryZones; i++)
            {
                if (Time.time >= memoryEndTimes[i])
                {
                    return i;
                }
            }
            
            // No free slot - replace oldest
            int oldestSlot = 0;
            float oldestTime = memoryEndTimes[0];
            for (int i = 1; i < maxMemoryZones; i++)
            {
                if (memoryEndTimes[i] < oldestTime)
                {
                    oldestTime = memoryEndTimes[i];
                    oldestSlot = i;
                }
            }
            return oldestSlot;
        }

        private void UpdateMemoryZones()
        {
            activeMemoryCount = 0;
            
            for (int i = 0; i < maxMemoryZones; i++)
            {
                if (Time.time < memoryEndTimes[i])
                {
                    // Calculate fade strength
                    float timeRemaining = memoryEndTimes[i] - Time.time;
                    float fadeOutDuration = 1f; // Last second fades out
                    
                    if (timeRemaining < fadeOutDuration)
                    {
                        memoryStrengths[i] = timeRemaining / fadeOutDuration;
                    }
                    else
                    {
                        memoryStrengths[i] = 1.0f;
                    }
                    
                    activeMemoryCount++;
                }
                else
                {
                    memoryStrengths[i] = 0f;
                }
            }
        }

        private void SendDataToShader()
        {
            if (fogMaterial == null) return;

            fogMaterial.SetInt(MemoryCountID, activeMemoryCount);
            fogMaterial.SetFloatArray(MemoryRadiiID, memoryRadii);
            fogMaterial.SetFloatArray(MemoryStrengthsID, memoryStrengths);
        }

        public void ClearAllMemory()
        {
            for (int i = 0; i < maxMemoryZones; i++)
            {
                memoryEndTimes[i] = 0f;
                memoryStrengths[i] = 0f;
            }
            activeMemoryCount = 0;
        }
    }
}
