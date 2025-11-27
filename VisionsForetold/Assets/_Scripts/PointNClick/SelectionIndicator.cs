using UnityEngine;
using UnityEngine.UI;

namespace VisionsForetold.PointNClick
{
    /// <summary>
    /// Visual selection indicator for clickable areas
    /// Shows which area is currently selected (for gamepad/keyboard navigation)
    /// </summary>
    public class SelectionIndicator : MonoBehaviour
    {
        [Header("Indicator Settings")]
        [Tooltip("Visual indicator (border, outline, highlight)")]
        [SerializeField] private Image indicatorImage;
        
        [Tooltip("Color when area is selected")]
        [SerializeField] private Color selectedColor = new Color(1f, 1f, 0f, 0.8f); // Yellow
        
        [Tooltip("Color when area is hovered")]
        [SerializeField] private Color hoverColor = new Color(0.5f, 0.8f, 1f, 0.6f); // Light blue
        
        [Tooltip("Pulse animation speed")]
        [SerializeField] private float pulseSpeed = 2f;
        
        [Tooltip("Pulse intensity (0-1)")]
        [SerializeField] private float pulseIntensity = 0.3f;
        
        [Tooltip("Enable pulsing animation")]
        [SerializeField] private bool enablePulse = true;

        [Header("Border Settings")]
        [Tooltip("Border thickness (if using border mode)")]
        [SerializeField] private float borderThickness = 4f;
        
        [Tooltip("Border offset from target")]
        [SerializeField] private float borderOffset = 10f;

        // Runtime state
        private RectTransform indicatorRect;
        private RectTransform targetRect;
        private CanvasGroup canvasGroup;
        private bool isActive = false;
        private bool isHoverMode = false;
        private float pulseTimer = 0f;

        private void Awake()
        {
            // Get components
            if (indicatorImage == null)
            {
                indicatorImage = GetComponent<Image>();
            }

            indicatorRect = GetComponent<RectTransform>();
            
            // Add canvas group for fading
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // Start hidden
            Hide(true);
        }

        private void Update()
        {
            if (!isActive) return;

            // Follow target if it exists
            if (targetRect != null)
            {
                UpdatePosition();
            }

            // Pulse animation
            if (enablePulse)
            {
                UpdatePulse();
            }
        }

        #region Public API

        /// <summary>
        /// Show indicator on target area (selected state)
        /// </summary>
        public void ShowSelected(RectTransform target)
        {
            targetRect = target;
            isHoverMode = false;
            isActive = true;

            if (indicatorImage != null)
            {
                indicatorImage.color = selectedColor;
            }

            UpdatePosition();
            UpdateSize();
            
            gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
        }

        /// <summary>
        /// Show indicator on target area (hover state)
        /// </summary>
        public void ShowHover(RectTransform target)
        {
            targetRect = target;
            isHoverMode = true;
            isActive = true;

            if (indicatorImage != null)
            {
                indicatorImage.color = hoverColor;
            }

            UpdatePosition();
            UpdateSize();
            
            gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
        }

        /// <summary>
        /// Hide the indicator
        /// </summary>
        public void Hide(bool immediate = false)
        {
            isActive = false;

            if (immediate)
            {
                gameObject.SetActive(false);
                canvasGroup.alpha = 0f;
            }
            else
            {
                StartCoroutine(FadeOut());
            }
        }

        /// <summary>
        /// Update indicator to follow target
        /// </summary>
        public void UpdateIndicator()
        {
            if (isActive && targetRect != null)
            {
                UpdatePosition();
                UpdateSize();
            }
        }

        #endregion

        #region Position & Size

        private void UpdatePosition()
        {
            if (targetRect == null || indicatorRect == null) return;

            // Match target position
            indicatorRect.position = targetRect.position;
            indicatorRect.rotation = targetRect.rotation;
        }

        private void UpdateSize()
        {
            if (targetRect == null || indicatorRect == null) return;

            // Match target size with offset for border
            Vector2 targetSize = targetRect.sizeDelta;
            Vector2 indicatorSize = targetSize + new Vector2(borderOffset * 2, borderOffset * 2);
            indicatorRect.sizeDelta = indicatorSize;
        }

        #endregion

        #region Animation

        private void UpdatePulse()
        {
            if (canvasGroup == null) return;

            pulseTimer += Time.deltaTime * pulseSpeed;
            
            // Sine wave for smooth pulsing
            float pulse = Mathf.Sin(pulseTimer) * pulseIntensity;
            float alpha = isHoverMode ? 0.6f : 0.8f;
            canvasGroup.alpha = alpha + pulse;
        }

        private System.Collections.IEnumerator FadeOut()
        {
            float duration = 0.2f;
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
                yield return null;
            }

            gameObject.SetActive(false);
        }

        #endregion
    }
}
