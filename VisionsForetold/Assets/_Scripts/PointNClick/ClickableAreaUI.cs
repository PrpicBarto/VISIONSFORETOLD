using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace VisionsForetold.PointNClick
{
    /// <summary>
    /// UI-based clickable area for fullscreen map systems
    /// Uses UI Image instead of SpriteRenderer for proper canvas rendering
    /// </summary>
    public class ClickableAreaUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Area Configuration")]
        [Tooltip("Data asset containing area information and sprites")]
        [SerializeField] private AreaData areaData;

        [Header("UI Image Reference")]
        [Tooltip("UI Image component to display sprites (auto-detected if not assigned)")]
        [SerializeField] private Image uiImage;

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

        [Header("Debug")]
        [SerializeField] private bool showDebug = false;

        // Runtime state
        private AreaState currentState = AreaState.Idle;
        private Color originalColor;
        private Color targetColor;
        private bool isHovered = false;
        private bool isClickable = true;

        private void Awake()
        {
            // Auto-find UI Image if not assigned
            if (uiImage == null)
            {
                uiImage = GetComponent<Image>();
            }

            // Validate setup
            if (uiImage == null)
            {
                Debug.LogError($"[{name}] No UI Image found! Add Image component or assign in Inspector.", this);
            }

            // Get or create audio source
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
            if (uiImage != null)
            {
                originalColor = uiImage.color;
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
            if (uiImage != null && uiImage.color != targetColor)
            {
                uiImage.color = Color.Lerp(
                    uiImage.color,
                    targetColor,
                    Time.deltaTime * transitionSpeed
                );
            }
        }

        #region UI Event Handlers

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isClickable || areaData == null || !areaData.isUnlocked) return;

            isHovered = true;
            UpdateVisualState(AreaState.Hover);
            PlaySound(areaData.hoverSound);

            // Notify map controller
            MapController.Instance?.OnAreaHoverEnter(areaData);

            if (showDebug)
            {
                Debug.Log($"[{name}] Hover enter: {areaData.areaName}");
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isHovered || areaData == null) return;

            isHovered = false;
            UpdateVisualState(currentState == AreaState.Selected ? AreaState.Selected : AreaState.Idle);
            
            // Notify map controller
            MapController.Instance?.OnAreaHoverExit(areaData);

            if (showDebug)
            {
                Debug.Log($"[{name}] Hover exit");
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isClickable || areaData == null || !areaData.isUnlocked) return;

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                OnLeftClick();
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                OnRightClick();
            }

            if (showDebug)
            {
                Debug.Log($"[{name}] Clicked with {eventData.button} button");
            }
        }

        #endregion

        #region Click Handlers

        private void OnLeftClick()
        {
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

        private void OnRightClick()
        {
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
            if (areaData != null && uiImage != null)
            {
                Sprite newSprite = areaData.GetCurrentStateSprite(newState);
                if (newSprite != null)
                {
                    uiImage.sprite = newSprite;
                    
                    // Ensure image fills the rect
                    uiImage.preserveAspect = false; // Fill container
                }
            }

            // Update color tint
            Color newColor = newState switch
            {
                AreaState.Hover => hoverTint,
                AreaState.Selected => selectedTint,
                _ => originalColor
            };

            if (immediate && uiImage != null)
            {
                uiImage.color = newColor;
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
        /// Set area data at runtime
        /// </summary>
        public void SetAreaData(AreaData data)
        {
            areaData = data;
            
            if (areaData != null && uiImage != null)
            {
                UpdateVisualState(areaData.isUnlocked ? AreaState.Idle : AreaState.Locked, true);
                SetClickable(areaData.isUnlocked);
            }
        }

        /// <summary>
        /// Enable or disable click interaction
        /// </summary>
        public void SetClickable(bool clickable)
        {
            isClickable = clickable;

            if (!clickable && isHovered)
            {
                UpdateVisualState(AreaState.Idle);
                isHovered = false;
            }

            // Also disable raycast target on Image
            if (uiImage != null)
            {
                uiImage.raycastTarget = clickable;
            }
        }

        /// <summary>
        /// Unlock this area
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
        /// Lock this area
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

            // Auto-find Image component
            if (uiImage == null)
            {
                uiImage = GetComponent<Image>();
            }
        }

        #endregion
    }
}
