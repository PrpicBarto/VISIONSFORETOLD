using UnityEngine;

namespace VisionsForetold.Game.Player.Echo
{
    /// <summary>
    /// Controls the echolocation pulse system that reveals areas in fog of war
    /// </summary>
    public class EcholocationController : MonoBehaviour
    {
        [Header("Pulse Settings")]
        [SerializeField] private float pulseSpeed = 15f;
        [SerializeField] private float maxPulseDistance = 40f;
        [SerializeField] private float pulseInterval = 3f;
        [SerializeField] private bool autoPulse = true;

        [Header("Visual Settings")]
        [SerializeField] private float pulseWidth = 5f;
        [SerializeField] private Color revealColor = Color.white;
        [SerializeField] private Color darkColor = new Color(0.5f, 0.5f, 0.55f, 1f); // BRIGHTER - was 0.05!
        [SerializeField] private float edgeGlow = 2f;

        [Header("References")]
        [SerializeField] private Transform pulseOrigin;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;

        // Runtime variables
        private float currentPulseDistance;
        private float timeSinceLastPulse;
        private bool isPulsing;
        private Vector3 pulseOriginPosition;

        // Shader property IDs
        private static readonly int PulseOriginID = Shader.PropertyToID("_PulseOrigin");
        private static readonly int PulseDistanceID = Shader.PropertyToID("_PulseDistance");
        private static readonly int PulseWidthID = Shader.PropertyToID("_PulseWidth");
        private static readonly int MaxDistanceID = Shader.PropertyToID("_MaxDistance");
        private static readonly int RevealColorID = Shader.PropertyToID("_RevealColor");
        private static readonly int DarkColorID = Shader.PropertyToID("_DarkColor");
        private static readonly int EdgeGlowID = Shader.PropertyToID("_EdgeGlow");

        private void Start()
        {
            if (pulseOrigin == null)
            {
                pulseOrigin = transform;
            }

            if (showDebugLogs)
            {
                Debug.Log($"[Echo] Starting with Dark Color: {darkColor} (should be >= 0.3 to be visible)");
            }

            // Initialize shader properties
            UpdateShaderProperties();

            if (autoPulse)
            {
                TriggerPulse();
            }
        }

        private void Update()
        {
            if (autoPulse)
            {
                timeSinceLastPulse += Time.deltaTime;

                if (timeSinceLastPulse >= pulseInterval && !isPulsing)
                {
                    TriggerPulse();
                }
            }

            if (isPulsing)
            {
                UpdatePulse();
            }

            UpdateShaderProperties();
        }

        /// <summary>
        /// Manually trigger a pulse
        /// </summary>
        public void TriggerPulse()
        {
            isPulsing = true;
            currentPulseDistance = 0f;
            timeSinceLastPulse = 0f;
            pulseOriginPosition = pulseOrigin.position;

            if (showDebugLogs)
            {
                Debug.Log($"[Echo] Pulse triggered at {pulseOriginPosition}");
            }
        }

        private void UpdatePulse()
        {
            currentPulseDistance += pulseSpeed * Time.deltaTime;

            if (currentPulseDistance >= maxPulseDistance)
            {
                isPulsing = false;
                currentPulseDistance = 0f;

                if (showDebugLogs)
                {
                    Debug.Log("[Echo] Pulse completed");
                }
            }
        }

        private void UpdateShaderProperties()
        {
            Shader.SetGlobalVector(PulseOriginID, pulseOriginPosition);
            Shader.SetGlobalFloat(PulseDistanceID, currentPulseDistance);
            Shader.SetGlobalFloat(PulseWidthID, pulseWidth);
            Shader.SetGlobalFloat(MaxDistanceID, maxPulseDistance);
            Shader.SetGlobalColor(RevealColorID, revealColor);
            Shader.SetGlobalColor(DarkColorID, darkColor);
            Shader.SetGlobalFloat(EdgeGlowID, edgeGlow);
        }

        private void OnValidate()
        {
            pulseSpeed = Mathf.Max(0.1f, pulseSpeed);
            maxPulseDistance = Mathf.Max(1f, maxPulseDistance);
            pulseInterval = Mathf.Max(0.1f, pulseInterval);
            pulseWidth = Mathf.Max(0.1f, pulseWidth);
            edgeGlow = Mathf.Max(0f, edgeGlow);

            // Warn if fog color is too dark
            if (darkColor.r < 0.3f || darkColor.g < 0.3f || darkColor.b < 0.3f)
            {
                Debug.LogWarning($"[Echo] Dark Color {darkColor} is very dark! World may appear black. Try values >= 0.3");
            }
        }
    }
}