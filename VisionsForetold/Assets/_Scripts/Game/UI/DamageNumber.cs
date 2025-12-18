using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DamageNumber : MonoBehaviour
{
    [SerializeField] private TMP_Text textComponent;
    [SerializeField] private Canvas canvas;
    
    private float lifetime;
    private float riseSpeed;
    private float fadeSpeed;
    private Color startColor;
    private Camera mainCamera;

    private void Awake()
    {
        if (textComponent == null)
        {
            textComponent = GetComponentInChildren<TMP_Text>();
        }
        
        if (canvas == null)
        {
            canvas = GetComponentInChildren<Canvas>();
        }
        
        mainCamera = Camera.main;
        
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
        }
    }

    public void Initialize(string text, Color color, float speed, float life)
    {
        if (textComponent != null)
        {
            textComponent.text = text;
            textComponent.color = color;
            startColor = color;
        }
        
        riseSpeed = speed;
        lifetime = life;
        fadeSpeed = 1f / life;
        
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Check if components are valid
        if (textComponent == null || transform == null)
        {
            // Component destroyed, clean up
            Destroy(gameObject);
            return;
        }

        // Rise up
        transform.position += Vector3.up * riseSpeed * Time.deltaTime;
        
        // Face camera
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null && transform != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        }
        
        // Fade out
        if (textComponent != null)
        {
            Color newColor = textComponent.color;
            newColor.a -= fadeSpeed * Time.deltaTime;
            textComponent.color = newColor;
        }
    }
}
