using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Helper script to automatically create health bars for characters
/// Attach this to your player/enemy prefabs to auto-generate health bars
/// </summary>
public class HealthBarSetup : MonoBehaviour
{
    [Header("Auto-Setup Settings")]
    [SerializeField] private bool createHealthBarOnStart = true;
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0, 2.5f, 0);
    [SerializeField] private Vector2 healthBarSize = new Vector2(1f, 0.15f);
    
    [Header("For Player")]
    [SerializeField] private bool isPlayer = false;
    [SerializeField] private bool playerHealthBarHideWhenFull = false;
    
    [Header("For Enemies")]
    [SerializeField] private bool hideEnemyHealthWhenFull = true;
    [SerializeField] private bool showOnlyWhenDamaged = true;

    private void Start()
    {
        if (createHealthBarOnStart)
        {
            CreateHealthBar();
        }
    }

    public void CreateHealthBar()
    {
        // Check if health bar already exists
        WorldHealthBar existingBar = GetComponentInChildren<WorldHealthBar>();
        if (existingBar != null)
        {
            Debug.Log("Health bar already exists!");
            return;
        }

        // Create canvas
        GameObject canvasObj = new GameObject("HealthBarCanvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = healthBarOffset;
        canvasObj.transform.localRotation = Quaternion.identity;
        canvasObj.transform.localScale = Vector3.one;

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        // Set canvas size
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = healthBarSize;

        // Create background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform);
        bgObj.transform.localPosition = Vector3.zero;
        bgObj.transform.localRotation = Quaternion.identity;
        bgObj.transform.localScale = Vector3.one;

        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Create fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(canvasObj.transform);
        fillObj.transform.localPosition = Vector3.zero;
        fillObj.transform.localRotation = Quaternion.identity;
        fillObj.transform.localScale = Vector3.one;

        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = Color.green;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        // Add WorldHealthBar component
        WorldHealthBar healthBar = canvasObj.AddComponent<WorldHealthBar>();
        
        // Use reflection to set private fields (since they're SerializeField)
        var healthBarType = typeof(WorldHealthBar);
        
        healthBarType.GetField("targetHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(healthBar, GetComponent<Health>());
        healthBarType.GetField("healthBarFill", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(healthBar, fillImage);
        healthBarType.GetField("healthBarBackground", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(healthBar, bgImage);
        healthBarType.GetField("canvas", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(healthBar, canvas);
        healthBarType.GetField("hideWhenFull", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(healthBar, isPlayer ? playerHealthBarHideWhenFull : hideEnemyHealthWhenFull);
        healthBarType.GetField("showOnlyWhenDamaged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(healthBar, !isPlayer && showOnlyWhenDamaged);

        Debug.Log($"Health bar created for {gameObject.name}");
    }
}
