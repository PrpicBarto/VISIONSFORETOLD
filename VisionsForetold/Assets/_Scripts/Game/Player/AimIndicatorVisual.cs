using UnityEngine;

/// <summary>
/// Visual indicator that shows where the player is aiming
/// Follows the aim target position with customizable appearance
/// </summary>
public class AimIndicatorVisual : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The aim target to follow (from PlayerMovement or PlayerAimDistanceController)")]
    [SerializeField] private Transform aimTarget;
    
    [Tooltip("Player transform for positioning")]
    [SerializeField] private Transform player;

    [Header("Visual Settings")]
    [Tooltip("Type of indicator to display")]
    [SerializeField] private IndicatorType indicatorType = IndicatorType.Circle;
    
    [Tooltip("Size of the aim indicator")]
    [SerializeField] private float indicatorSize = 0.8f; // Smaller for isometric view
    
    [Tooltip("Color of the aim indicator")]
    [SerializeField] private Color indicatorColor = new Color(0f, 1f, 0f, 0.6f); // Green for friendly target
    
    [Tooltip("Color when hovering over enemy")]
    [SerializeField] private Color enemyHoverColor = new Color(1f, 0f, 0f, 0.8f); // Red for enemy
    
    [Tooltip("Height offset above ground")]
    [SerializeField] private float heightOffset = 0.05f; // Very small offset for isometric

    [Header("Line Settings")]
    [Tooltip("Show line from player to aim point")]
    [SerializeField] private bool showAimLine = false; // Usually off for isometric
    
    [Tooltip("Color of the aim line")]
    [SerializeField] private Color aimLineColor = new Color(1f, 1f, 0f, 0.2f);
    
    [Tooltip("Width of the aim line")]
    [SerializeField] private float aimLineWidth = 0.05f;

    [Header("Animation")]
    [Tooltip("Enable pulsing animation")]
    [SerializeField] private bool enablePulse = true;
    
    [Tooltip("Speed of pulse animation")]
    [SerializeField] private float pulseSpeed = 3f; // Faster for snappier feel
    
    [Tooltip("Amount of size change during pulse")]
    [SerializeField] private float pulseAmount = 0.15f;

    [Header("Enemy Detection")]
    [Tooltip("Enable enemy hover detection")]
    [SerializeField] private bool detectEnemies = true;
    
    [Tooltip("Radius for enemy detection")]
    [SerializeField] private float detectionRadius = 0.8f; // Smaller for isometric
    
    [Tooltip("Layer mask for enemies")]
    [SerializeField] private LayerMask enemyLayer = -1;

    [Header("Visibility")]
    [Tooltip("Show indicator only during aiming/combat")]
    [SerializeField] private bool showOnlyWhenAiming = false;
    
    [Tooltip("Fade in/out duration")]
    [SerializeField] private float fadeDuration = 0.15f; // Faster fading

    public enum IndicatorType
    {
        Circle,
        Cross,
        Dot,
        Ring,
        Square
    }

    // Visual components
    private GameObject indicatorObject;
    private MeshRenderer indicatorRenderer;
    private LineRenderer aimLineRenderer;
    private Material indicatorMaterial;
    
    // State
    private bool isOverEnemy;
    private float currentAlpha;
    private float targetAlpha;
    private float pulseTimer;

    #region Unity Lifecycle

    private void Awake()
    {
        // Auto-find player if not assigned
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null)
            {
                Debug.LogWarning("[AimIndicatorVisual] Player not found! Searching by name...");
                GameObject playerObj = GameObject.Find("Player");
                if (playerObj != null)
                {
                    player = playerObj.transform;
                }
            }
        }

        if (player == null)
        {
            Debug.LogError("[AimIndicatorVisual] Could not find Player! Please assign manually in Inspector.");
            enabled = false;
            return;
        }

        // Try to find aim target from multiple sources
        if (aimTarget == null)
        {
            Debug.Log("[AimIndicatorVisual] Searching for aim target...");
            
            // Try PlayerAimDistanceController first
            var aimController = player.GetComponent<PlayerAimDistanceController>();
            if (aimController != null)
            {
                // Wait for controller to initialize, then get aim target
                StartCoroutine(GetAimTargetDelayed(aimController));
                return;
            }
            
            // Try PlayerMovement
            var playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement != null && playerMovement.AimTarget != null)
            {
                aimTarget = playerMovement.AimTarget;
                Debug.Log($"[AimIndicatorVisual] Found aim target from PlayerMovement: {aimTarget.name}");
            }

            // Search for AimTarget by name as fallback
            if (aimTarget == null)
            {
                GameObject aimObj = GameObject.Find("AimTarget");
                if (aimObj != null)
                {
                    aimTarget = aimObj.transform;
                    Debug.Log($"[AimIndicatorVisual] Found aim target by name: {aimTarget.name}");
                }
            }

            if (aimTarget == null)
            {
                Debug.LogError("[AimIndicatorVisual] Could not find aim target! Make sure PlayerAimDistanceController or PlayerMovement is on the Player GameObject, or assign Aim Target manually in Inspector.");
                enabled = false;
                return;
            }
        }

        InitializeIndicator();
    }

    /// <summary>
    /// Delayed aim target retrieval to wait for PlayerAimDistanceController initialization
    /// </summary>
    private System.Collections.IEnumerator GetAimTargetDelayed(PlayerAimDistanceController controller)
    {
        // Wait one frame for controller to initialize
        yield return null;
        
        aimTarget = controller.GetAimTarget();
        
        if (aimTarget != null)
        {
            Debug.Log($"[AimIndicatorVisual] Found aim target from PlayerAimDistanceController: {aimTarget.name}");
            InitializeIndicator();
        }
        else
        {
            Debug.LogError("[AimIndicatorVisual] PlayerAimDistanceController found but aim target is null! Check controller setup.");
            enabled = false;
        }
    }

    private void InitializeIndicator()
    {
        CreateIndicator();
        
        if (showAimLine)
        {
            CreateAimLine();
        }

        currentAlpha = showOnlyWhenAiming ? 0f : 1f;
        targetAlpha = currentAlpha;
        
        Debug.Log("[AimIndicatorVisual] Successfully initialized!");
    }

    private void Update()
    {
        if (aimTarget == null || indicatorObject == null) return;

        UpdatePosition();
        UpdateVisuals();
        UpdateEnemyDetection();
        UpdateVisibility();
    }

    #endregion

    #region Indicator Creation

    private void CreateIndicator()
    {
        indicatorObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        indicatorObject.name = "AimIndicator";
        indicatorObject.transform.SetParent(transform);
        
        // Remove collider
        Destroy(indicatorObject.GetComponent<Collider>());

        // Setup renderer
        indicatorRenderer = indicatorObject.GetComponent<MeshRenderer>();
        indicatorMaterial = new Material(Shader.Find("Sprites/Default"));
        indicatorMaterial.color = indicatorColor;
        indicatorRenderer.material = indicatorMaterial;

        // Rotate to face up
        indicatorObject.transform.rotation = Quaternion.Euler(90, 0, 0);
        indicatorObject.transform.localScale = Vector3.one * indicatorSize;

        // Create texture based on type
        CreateIndicatorTexture();

        Debug.Log("[AimIndicatorVisual] Indicator created successfully");
    }

    private void CreateIndicatorTexture()
    {
        int resolution = 128;
        Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        
        Color transparent = new Color(1, 1, 1, 0);
        Color opaque = Color.white;

        // Fill based on indicator type
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float centerX = x - resolution / 2f;
                float centerY = y - resolution / 2f;
                float distance = Mathf.Sqrt(centerX * centerX + centerY * centerY);
                float normalizedDistance = distance / (resolution / 2f);

                Color pixelColor = transparent;

                switch (indicatorType)
                {
                    case IndicatorType.Circle:
                        pixelColor = normalizedDistance < 0.9f ? opaque : transparent;
                        if (normalizedDistance < 0.7f) pixelColor = transparent;
                        break;

                    case IndicatorType.Ring:
                        pixelColor = (normalizedDistance > 0.7f && normalizedDistance < 0.9f) ? opaque : transparent;
                        break;

                    case IndicatorType.Cross:
                        if (Mathf.Abs(centerX) < resolution * 0.1f || Mathf.Abs(centerY) < resolution * 0.1f)
                        {
                            if (normalizedDistance < 0.8f) pixelColor = opaque;
                        }
                        break;

                    case IndicatorType.Dot:
                        pixelColor = normalizedDistance < 0.3f ? opaque : transparent;
                        break;

                    case IndicatorType.Square:
                        if (Mathf.Abs(centerX) < resolution * 0.4f && Mathf.Abs(centerY) < resolution * 0.4f)
                        {
                            if (Mathf.Abs(centerX) > resolution * 0.3f || Mathf.Abs(centerY) > resolution * 0.3f)
                            {
                                pixelColor = opaque;
                            }
                        }
                        break;
                }

                texture.SetPixel(x, y, pixelColor);
            }
        }

        texture.Apply();
        indicatorMaterial.mainTexture = texture;
    }

    private void CreateAimLine()
    {
        GameObject lineObject = new GameObject("AimLine");
        lineObject.transform.SetParent(transform);
        
        aimLineRenderer = lineObject.AddComponent<LineRenderer>();
        aimLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        aimLineRenderer.startColor = aimLineColor;
        aimLineRenderer.endColor = aimLineColor;
        aimLineRenderer.startWidth = aimLineWidth;
        aimLineRenderer.endWidth = aimLineWidth;
        aimLineRenderer.positionCount = 2;
        aimLineRenderer.useWorldSpace = true;
    }

    #endregion

    #region Update Logic

    private void UpdatePosition()
    {
        if (aimTarget == null) return;

        // Position at aim target with height offset
        Vector3 targetPosition = aimTarget.position;
        targetPosition.y += heightOffset;
        indicatorObject.transform.position = targetPosition;

        // Update aim line if enabled
        if (showAimLine && aimLineRenderer != null && player != null)
        {
            Vector3 startPos = player.position + Vector3.up;
            Vector3 endPos = aimTarget.position + Vector3.up * heightOffset;
            
            aimLineRenderer.SetPosition(0, startPos);
            aimLineRenderer.SetPosition(1, endPos);
        }
    }

    private void UpdateVisuals()
    {
        if (indicatorMaterial == null) return;

        // Pulse animation
        float scale = indicatorSize;
        if (enablePulse)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float pulse = Mathf.Sin(pulseTimer) * pulseAmount;
            scale = indicatorSize + pulse;
        }

        indicatorObject.transform.localScale = Vector3.one * scale;

        // Color based on enemy hover
        Color targetColor = isOverEnemy ? enemyHoverColor : indicatorColor;
        targetColor.a *= currentAlpha;
        indicatorMaterial.color = Color.Lerp(indicatorMaterial.color, targetColor, Time.deltaTime * 10f);

        // Update line color
        if (aimLineRenderer != null)
        {
            Color lineColor = aimLineColor;
            lineColor.a *= currentAlpha;
            aimLineRenderer.startColor = lineColor;
            aimLineRenderer.endColor = lineColor;
        }
    }

    private void UpdateEnemyDetection()
    {
        if (!detectEnemies || aimTarget == null) return;

        Collider[] hits = Physics.OverlapSphere(aimTarget.position, detectionRadius, enemyLayer);
        isOverEnemy = hits.Length > 0;
    }

    private void UpdateVisibility()
    {
        if (!showOnlyWhenAiming)
        {
            targetAlpha = 1f;
        }
        else
        {
            // Check if player is aiming/attacking
            // You can add custom logic here based on your game
            targetAlpha = 1f; // For now, always visible
        }

        // Smooth fade
        if (fadeDuration > 0)
        {
            currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime / fadeDuration);
        }
        else
        {
            currentAlpha = targetAlpha;
        }

        // Hide if fully transparent
        if (indicatorRenderer != null)
        {
            indicatorRenderer.enabled = currentAlpha > 0.01f;
        }
        
        if (aimLineRenderer != null)
        {
            aimLineRenderer.enabled = currentAlpha > 0.01f && showAimLine;
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// Set the aim target to follow
    /// </summary>
    public void SetAimTarget(Transform target)
    {
        aimTarget = target;
    }

    /// <summary>
    /// Set indicator visibility
    /// </summary>
    public void SetVisible(bool visible)
    {
        targetAlpha = visible ? 1f : 0f;
    }

    /// <summary>
    /// Change indicator size
    /// </summary>
    public void SetSize(float size)
    {
        indicatorSize = size;
    }

    /// <summary>
    /// Change indicator color
    /// </summary>
    public void SetColor(Color color)
    {
        indicatorColor = color;
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        if (!detectEnemies || aimTarget == null) return;

        // Draw detection radius
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(aimTarget.position, detectionRadius);
    }

    #endregion
}
