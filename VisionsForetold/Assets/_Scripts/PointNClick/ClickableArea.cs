using UnityEngine;
using UnityEngine.Events;

namespace VisionsForetold.PointNClick
{
    /// <summary>
    /// Clickable area on the map that changes sprites based on interaction
    /// Handles hover effects, click events, and area state management
    /// </summary>
    public class ClickableArea : ClickBase
    {
        [Header("Area Configuration")]
        [Tooltip("Data asset containing area information and sprites")]
        [SerializeField] private AreaData areaData;

        [Header("Visual Settings")]
        [Tooltip("Color tint when hovering (multiplied with sprite)")]
        [SerializeField] private Color hoverTint = new Color(1.2f, 1.2f, 1.2f, 1f);
        
        [Tooltip("Color tint when selected")]
        [SerializeField] private Color selectedTint = new Color(1f, 1f, 0.8f, 1f);
        
        [Tooltip("Animation speed for state transitions")]
        [SerializeField] private float transitionSpeed = 10f;

        [Header("Audio")]
        [Tooltip("Audio source for playing sounds (optional)")]
        [SerializeField] private AudioSource audioSource;

        [Header("Events")]
        [Tooltip("Called when area is clicked (before showing info)")]
        public UnityEvent<AreaData> OnAreaClicked;
        
        [Tooltip("Called when enter button is pressed")]
        public UnityEvent<AreaData> OnAreaEntered;

        // Runtime state
        private AreaState currentState = AreaState.Idle;
        private Color originalColor;
        private Color targetColor;

        protected override void Awake()
        {
            base.Awake();

            // Get audio source if not assigned
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                }
            }

            // Store original color
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
                targetColor = originalColor;
            }
        }

        private void Start()
        {
            // Initialize with area data
            if (areaData != null)
            {
                UpdateVisualState(AreaState.Idle, true);
                
                // Lock if not unlocked
                if (!areaData.isUnlocked)
                {
                    SetClickable(false);
                    UpdateVisualState(AreaState.Locked, true);
                }
            }
            else
            {
                Debug.LogError($"[{name}] No AreaData assigned! Assign one in Inspector.", this);
            }
        }

        private void Update()
        {
            // Smooth color transition
            if (spriteRenderer != null && spriteRenderer.color != targetColor)
            {
                spriteRenderer.color = Color.Lerp(
                    spriteRenderer.color,
                    targetColor,
                    Time.deltaTime * transitionSpeed
                );
            }
        }

        #region Hover & Click Overrides

        protected override void OnHoverEnter()
        {
            if (areaData == null || !areaData.isUnlocked) return;

            UpdateVisualState(AreaState.Hover);
            PlaySound(areaData.hoverSound);

            // Notify map controller
            MapController.Instance?.OnAreaHoverEnter(areaData);
        }

        protected override void OnHoverExit()
        {
            if (areaData == null) return;

            UpdateVisualState(currentState == AreaState.Selected ? AreaState.Selected : AreaState.Idle);
            
            // Notify map controller
            MapController.Instance?.OnAreaHoverExit(areaData);
        }

        protected override void OnLeftClick()
        {
            if (areaData == null || !areaData.isUnlocked) return;

            UpdateVisualState(AreaState.Selected);
            PlaySound(areaData.clickSound);

            // Invoke event
            OnAreaClicked?.Invoke(areaData);

            // Notify map controller to show info panel
            MapController.Instance?.OnAreaClicked(areaData, this);

            if (showDebug)
            {
                Debug.Log($"[{name}] Area clicked: {areaData.areaName}");
            }
        }

        protected override void OnRightClick()
        {
            if (areaData == null || !areaData.isUnlocked) return;

            // Right-click could show quick info or context menu
            if (showDebug)
            {
                Debug.Log($"[{name}] Right-clicked: {areaData.areaName}");
            }
        }

        #endregion

        #region Visual State Management

        /// <summary>
        /// Update the visual appearance based on state
        /// </summary>
        private void UpdateVisualState(AreaState newState, bool immediate = false)
        {
            currentState = newState;

            // Update sprite
            if (areaData != null && spriteRenderer != null)
            {
                Sprite newSprite = areaData.GetCurrentStateSprite(newState);
                if (newSprite != null)
                {
                    spriteRenderer.sprite = newSprite;
                }
            }

            // Update color tint
            Color newColor = newState switch
            {
                AreaState.Hover => hoverTint,
                AreaState.Selected => selectedTint,
                _ => originalColor
            };

            if (immediate && spriteRenderer != null)
            {
                spriteRenderer.color = newColor;
            }
            
            targetColor = newColor;
        }

        /// <summary>
        /// Reset to idle state
        /// </summary>
        public void ResetToIdle()
        {
            UpdateVisualState(AreaState.Idle);
        }

        #endregion

        #region Audio

        private void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Trigger the "Enter Area" action (load scene)
        /// </summary>
        public void EnterArea()
        {
            if (areaData == null || !areaData.isUnlocked) return;

            OnAreaEntered?.Invoke(areaData);
            
            // Notify map controller to load scene
            MapController.Instance?.EnterArea(areaData);

            if (showDebug)
            {
                Debug.Log($"[{name}] Entering area: {areaData.areaName} ? Scene: {areaData.sceneName}");
            }
        }

        /// <summary>
        /// Get the area data
        /// </summary>
        public AreaData GetAreaData() => areaData;

        /// <summary>
        /// Set area data at runtime (useful for dynamic map generation)
        /// </summary>
        public void SetAreaData(AreaData data)
        {
            areaData = data;
            
            if (areaData != null)
            {
                UpdateVisualState(areaData.isUnlocked ? AreaState.Idle : AreaState.Locked, true);
                SetClickable(areaData.isUnlocked);
            }
        }

        /// <summary>
        /// Unlock this area (makes it clickable)
        /// </summary>
        public void UnlockArea()
        {
            if (areaData != null)
            {
                areaData.isUnlocked = true;
                SetClickable(true);
                UpdateVisualState(AreaState.Idle, true);
            }
        }

        /// <summary>
        /// Lock this area (makes it unclickable)
        /// </summary>
        public void LockArea()
        {
            if (areaData != null)
            {
                areaData.isUnlocked = false;
                SetClickable(false);
                UpdateVisualState(AreaState.Locked, true);
            }
        }

        #endregion

        #region Editor Helpers

        private void OnValidate()
        {
            // Validate in editor
            if (areaData != null)
            {
                areaData.IsValid();
            }
        }

        #endregion
    }
}
