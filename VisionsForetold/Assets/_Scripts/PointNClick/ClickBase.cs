using UnityEngine;
using UnityEngine.EventSystems;

namespace VisionsForetold.PointNClick
{
    /// <summary>
    /// Base class for clickable objects in the point-and-click map system
    /// Handles hover, click detection, and visual state changes
    /// </summary>
    public abstract class ClickBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Visual Feedback")]
        [Tooltip("Sprite renderer to update (usually UI Image or SpriteRenderer)")]
        [SerializeField] protected SpriteRenderer spriteRenderer;
        
        [Tooltip("Cursor texture when hovering (optional)")]
        [SerializeField] protected Texture2D hoverCursor;
        
        [Tooltip("Enable debug logging")]
        [SerializeField] protected bool showDebug = false;

        protected bool isHovered = false;
        protected bool isClickable = true;

        protected virtual void Awake()
        {
            // Auto-find sprite renderer if not assigned
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            // Validate setup
            ValidateSetup();
        }

        protected virtual void ValidateSetup()
        {
            if (spriteRenderer == null)
            {
                Debug.LogWarning($"[{name}] No SpriteRenderer found! Assign one in Inspector or add component.", this);
            }

            // Ensure collider exists for mouse detection
            Collider2D col = GetComponent<Collider2D>();
            if (col == null)
            {
                Debug.LogWarning($"[{name}] No Collider2D found! Adding BoxCollider2D for mouse detection.", this);
                gameObject.AddComponent<BoxCollider2D>();
            }
        }

        #region Mouse Events (EventSystem)

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (!isClickable) return;

            isHovered = true;
            OnHoverEnter();

            if (hoverCursor != null)
            {
                Cursor.SetCursor(hoverCursor, Vector2.zero, CursorMode.Auto);
            }

            if (showDebug)
            {
                Debug.Log($"[{name}] Mouse entered");
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (!isHovered) return;

            isHovered = false;
            OnHoverExit();

            // Reset cursor
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

            if (showDebug)
            {
                Debug.Log($"[{name}] Mouse exited");
            }
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (!isClickable) return;

            // Left click
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                OnLeftClick();
            }
            // Right click
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

        #region Abstract/Virtual Methods (Override in derived classes)

        /// <summary>
        /// Called when mouse enters the clickable area
        /// </summary>
        protected virtual void OnHoverEnter()
        {
            // Override to add hover effects (sprite change, highlight, etc.)
        }

        /// <summary>
        /// Called when mouse exits the clickable area
        /// </summary>
        protected virtual void OnHoverExit()
        {
            // Override to remove hover effects
        }

        /// <summary>
        /// Called when left-clicked
        /// </summary>
        protected virtual void OnLeftClick()
        {
            // Override to handle primary action
        }

        /// <summary>
        /// Called when right-clicked
        /// </summary>
        protected virtual void OnRightClick()
        {
            // Override to handle secondary action
        }

        #endregion

        #region Public API

        /// <summary>
        /// Enable or disable click interaction
        /// </summary>
        public virtual void SetClickable(bool clickable)
        {
            isClickable = clickable;

            if (!clickable && isHovered)
            {
                OnHoverExit();
                isHovered = false;
            }
        }

        /// <summary>
        /// Change the sprite (useful for state changes)
        /// </summary>
        public virtual void SetSprite(Sprite newSprite)
        {
            if (spriteRenderer != null && newSprite != null)
            {
                spriteRenderer.sprite = newSprite;
            }
        }

        /// <summary>
        /// Get current hover state
        /// </summary>
        public bool IsHovered => isHovered;

        /// <summary>
        /// Get current clickable state
        /// </summary>
        public bool IsClickable => isClickable;

        #endregion
    }
}
