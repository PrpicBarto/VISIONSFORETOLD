using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Visual indicator showing where the player is aiming
/// Displays a crosshair/reticle at the aim point
/// Works with both mouse and gamepad
/// </summary>
public class AimIndicator : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The player's aim target from PlayerMovement")]
    [SerializeField] private Transform aimTarget;
    
    [Tooltip("PlayerMovement script reference")]
    [SerializeField] private PlayerMovement playerMovement;
    
    [Tooltip("Player transform")]
    [SerializeField] private Transform playerTransform;

    [Header("Visual Settings")]
    [Tooltip("Sprite/model to show at aim position")]
    [SerializeField] private GameObject indicatorPrefab;
    
    [Tooltip("Indicator color")]
    [SerializeField] private Color indicatorColor = new Color(1f, 1f, 1f, 0.7f);
    
    [Tooltip("Indicator size")]
    [SerializeField] private float indicatorSize = 0.5f;
    
    [Tooltip("Hover height above ground")]
    [SerializeField] private float hoverHeight = 0.1f;

    [Header("Behavior")]
    [Tooltip("Show indicator")]
    [SerializeField] private bool showIndicator = true;
    
    [Tooltip("Only show when aiming (not when idle)")]
    [SerializeField] private bool hideWhenIdle = true;
    
    [Tooltip("Minimum aim distance to show indicator")]
    [SerializeField] private float minAimDistance = 1f;
    
    [Tooltip("Fade distance for indicator")]
    [SerializeField] private float fadeDistance = 0.5f;

    [Header("Animation")]
    [Tooltip("Rotate indicator")]
    [SerializeField] private bool rotateIndicator = true;
    
    [Tooltip("Rotation speed")]
    [SerializeField] private float rotationSpeed = 90f;
    
    [Tooltip("Pulse indicator")]
    [SerializeField] private bool pulseIndicator = true;
    
    [Tooltip("Pulse speed")]
    [SerializeField] private float pulseSpeed = 2f;
    
    [Tooltip("Pulse amount")]
    [SerializeField, Range(0f, 1f)] private float pulseAmount = 0.2f;

    [Header("Line Renderer")]
    [Tooltip("Show line from player to aim point")]
    [SerializeField] private bool showAimLine = false;
    
    [Tooltip("Line color")]
    [SerializeField] private Color lineColor = new Color(1f, 0f, 0f, 0.3f);
    
    [Tooltip("Line width")]
    [SerializeField] private float lineWidth = 0.05f;

    [Header("Attack Mode Colors")]
    [Tooltip("Change color based on attack mode")]
    [SerializeField] private bool useAttackModeColors = true;
    
    [SerializeField] private Color meleeColor = Color.red;
    [SerializeField] private Color rangedColor = Color.green;
    [SerializeField] private Color spellColor = Color.cyan;

    // Runtime objects
    private GameObject indicatorObject;
    private SpriteRenderer spriteRenderer;
    private MeshRenderer meshRenderer;
    private LineRenderer lineRenderer;
    private PlayerAttack playerAttack;

    // Animation state
    private float currentRotation;
    private float pulseTimer;
    private float currentAlpha = 1f;
    private Vector3 baseScale;

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        CreateIndicator();
        CreateLineRenderer();
    }

    private void Update()
    {
        // Try to get aim target if we don't have it yet
        if (aimTarget == null && playerMovement != null)
        {
            aimTarget = playerMovement.AimTarget;
            if (aimTarget != null)
            {
                Debug.Log($"[AimIndicator] Late-found aim target: {aimTarget.name}");
            }
        }

        UpdateIndicatorPosition();
        UpdateIndicatorVisibility();
        UpdateIndicatorAnimation();
        UpdateIndicatorColor();
        UpdateAimLine();
    }

    private void OnDestroy()
    {
        if (indicatorObject != null)
        {
            Destroy(indicatorObject);
        }
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        // If this component is on the player itself, use this transform
        if (playerTransform == null)
        {
            // First try to use this GameObject's transform
            playerTransform = transform;
            
            // If this doesn't have PlayerMovement, try to find player by tag
            if (GetComponent<PlayerMovement>() == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    playerTransform = playerObj.transform;
                }
            }
        }

        if (playerMovement == null && playerTransform != null)
        {
            playerMovement = playerTransform.GetComponent<PlayerMovement>();
        }

        if (playerAttack == null && playerTransform != null)
        {
            playerAttack = playerTransform.GetComponent<PlayerAttack>();
        }

        // Get aim target from PlayerMovement
        if (aimTarget == null && playerMovement != null)
        {
            aimTarget = playerMovement.AimTarget;
            
            if (aimTarget != null)
            {
                Debug.Log($"[AimIndicator] Found aim target: {aimTarget.name}");
            }
            else
            {
                Debug.LogWarning("[AimIndicator] Could not find aim target from PlayerMovement!");
            }
        }
    }

    private void CreateIndicator()
    {
        if (indicatorPrefab != null)
        {
            // Use provided prefab
            indicatorObject = Instantiate(indicatorPrefab);
        }
        else
        {
            // Create default indicator (quad with texture)
            indicatorObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            
            // Remove collider
            Destroy(indicatorObject.GetComponent<Collider>());
            
            // Rotate to face up
            indicatorObject.transform.rotation = Quaternion.Euler(90, 0, 0);
        }

        indicatorObject.name = "AimIndicator";
        indicatorObject.transform.localScale = Vector3.one * indicatorSize;
        baseScale = indicatorObject.transform.localScale;

        // Get renderer
        spriteRenderer = indicatorObject.GetComponent<SpriteRenderer>();
        meshRenderer = indicatorObject.GetComponent<MeshRenderer>();

        // Set initial color
        SetIndicatorColor(indicatorColor);

        // Initially hide
        indicatorObject.SetActive(showIndicator);
    }

    private void CreateLineRenderer()
    {
        if (!showAimLine)
            return;

        // Create line renderer on player
        GameObject lineObj = new GameObject("AimLine");
        lineObj.transform.SetParent(playerTransform);
        lineObj.transform.localPosition = Vector3.zero;

        lineRenderer = lineObj.AddComponent<LineRenderer>();
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = new Color(lineColor.r, lineColor.g, lineColor.b, 0f);
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = showAimLine;
    }

    #endregion

    #region Update Methods

    private void UpdateIndicatorPosition()
    {
        if (indicatorObject == null)
        {
            Debug.LogWarning("[AimIndicator] Indicator object is null!");
            return;
        }
        
        if (aimTarget == null)
        {
            // Don't spam console, but show warning once per second
            if (Time.frameCount % 60 == 0)
            {
                Debug.LogWarning("[AimIndicator] Aim target is null! Check PlayerMovement AimTarget assignment.");
            }
            return;
        }

        // Position at aim target with hover height
        Vector3 targetPos = aimTarget.position;
        targetPos.y += hoverHeight;

        // Raycast down to find ground
        if (Physics.Raycast(aimTarget.position + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
        {
            targetPos = hit.point + Vector3.up * hoverHeight;
        }

        indicatorObject.transform.position = targetPos;
    }

    private void UpdateIndicatorVisibility()
    {
        if (indicatorObject == null || aimTarget == null || playerTransform == null)
            return;

        bool shouldShow = showIndicator;

        // Check if player is aiming
        if (hideWhenIdle)
        {
            float aimDistance = Vector3.Distance(playerTransform.position, aimTarget.position);
            shouldShow = shouldShow && aimDistance > minAimDistance;

            // Calculate fade alpha based on distance
            if (aimDistance < minAimDistance + fadeDistance)
            {
                currentAlpha = Mathf.Clamp01((aimDistance - minAimDistance) / fadeDistance);
            }
            else
            {
                currentAlpha = 1f;
            }
        }

        indicatorObject.SetActive(shouldShow);
    }

    private void UpdateIndicatorAnimation()
    {
        if (indicatorObject == null)
            return;

        // Rotation
        if (rotateIndicator)
        {
            currentRotation += rotationSpeed * Time.deltaTime;
            indicatorObject.transform.rotation = Quaternion.Euler(90, currentRotation, 0);
        }

        // Pulse
        if (pulseIndicator)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float pulse = 1f + Mathf.Sin(pulseTimer) * pulseAmount;
            indicatorObject.transform.localScale = baseScale * pulse;
        }
        else
        {
            indicatorObject.transform.localScale = baseScale;
        }
    }

    private void UpdateIndicatorColor()
    {
        if (!useAttackModeColors || playerAttack == null)
            return;

        Color targetColor = indicatorColor;

        // Get current attack mode via reflection (since it's private)
        var modeField = typeof(PlayerAttack).GetField("currentAttackMode", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (modeField != null)
        {
            var attackMode = modeField.GetValue(playerAttack);
            
            targetColor = attackMode.ToString() switch
            {
                "Melee" => meleeColor,
                "Ranged" => rangedColor,
                "SpellWielding" => spellColor,
                _ => indicatorColor
            };
        }

        // Apply alpha
        targetColor.a = indicatorColor.a * currentAlpha;
        SetIndicatorColor(targetColor);
    }

    private void UpdateAimLine()
    {
        if (!showAimLine || lineRenderer == null || aimTarget == null || playerTransform == null)
            return;

        // Update line positions
        Vector3 startPos = playerTransform.position + Vector3.up * 1f;
        Vector3 endPos = aimTarget.position + Vector3.up * hoverHeight;

        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);

        // Update line visibility
        float distance = Vector3.Distance(playerTransform.position, aimTarget.position);
        lineRenderer.enabled = showAimLine && distance > minAimDistance;
    }

    #endregion

    #region Helper Methods

    private void SetIndicatorColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
        else if (meshRenderer != null)
        {
            if (meshRenderer.material != null)
            {
                meshRenderer.material.color = color;
            }
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// Show or hide the indicator
    /// </summary>
    public void SetVisible(bool visible)
    {
        showIndicator = visible;
        if (indicatorObject != null)
        {
            indicatorObject.SetActive(visible);
        }
    }

    /// <summary>
    /// Set indicator color
    /// </summary>
    public void SetColor(Color color)
    {
        indicatorColor = color;
    }

    /// <summary>
    /// Set indicator size
    /// </summary>
    public void SetSize(float size)
    {
        indicatorSize = size;
        baseScale = Vector3.one * size;
    }

    /// <summary>
    /// Enable or disable aim line
    /// </summary>
    public void SetAimLineEnabled(bool enabled)
    {
        showAimLine = enabled;
        if (lineRenderer != null)
        {
            lineRenderer.enabled = enabled;
        }
    }

    #endregion

    #region Debug

    private void OnDrawGizmosSelected()
    {
        if (aimTarget == null || playerTransform == null)
            return;

        // Draw aim direction
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(playerTransform.position, aimTarget.position);

        // Draw min aim distance
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(playerTransform.position, minAimDistance);

        // Draw fade distance
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(playerTransform.position, minAimDistance + fadeDistance);
    }

    private void OnValidate()
    {
        indicatorSize = Mathf.Max(0.1f, indicatorSize);
        minAimDistance = Mathf.Max(0f, minAimDistance);
        fadeDistance = Mathf.Max(0f, fadeDistance);
        rotationSpeed = Mathf.Max(0f, rotationSpeed);
        pulseSpeed = Mathf.Max(0f, pulseSpeed);
        lineWidth = Mathf.Max(0.01f, lineWidth);
    }

    #endregion
}
