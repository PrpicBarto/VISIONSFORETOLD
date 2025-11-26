using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerHealthBarUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private GameObject player;

    private Health playerHealth;
    private UnityAction<int, int> onHealthChanged;

    private void Awake()
    {
        onHealthChanged = (current, max) =>
        {
            if (healthSlider == null) return;
            healthSlider.maxValue = Mathf.Max(1, max);
            healthSlider.value = Mathf.Clamp(current, 0, max);
        };
    }

    private void OnEnable()
    {
        // Try to auto-assign the slider if it's not set in inspector
        if (healthSlider == null)
        {
            // Prefer a slider on this object or children
            healthSlider = GetComponentInChildren<Slider>();
            if (healthSlider == null)
            {
                // Fallback: find any Slider in the scene (last resort)
                healthSlider = FindAnyObjectByType<Slider>();
            }

            if (healthSlider != null)
            {
                Debug.Log($"{name}: Auto-assigned Slider -> {healthSlider.name}");
            }
            else
            {
                Debug.LogWarning($"{name}: No Slider assigned or found in scene. Player health UI will not be visible.");
                return;
            }
        }

        // Ensure slider and its canvas are active and in front
        if (!healthSlider.gameObject.activeInHierarchy)
            healthSlider.gameObject.SetActive(true);

        var parentCanvas = healthSlider.GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            if (!parentCanvas.gameObject.activeInHierarchy)
                parentCanvas.gameObject.SetActive(true);

            // Bring canvas to front if it's screen-space overlay or adjust sorting order for visibility
            try
            {
                Canvas.ForceUpdateCanvases();
                parentCanvas.transform.SetAsLastSibling();
                parentCanvas.sortingOrder = Mathf.Max(parentCanvas.sortingOrder, 100);
            }
            catch { }
        }
        else
        {
            Debug.LogWarning($"{name}: Slider has no parent Canvas. Assign a Canvas to display UI.");
        }

        // Find player if not assigned
        if (player == null)
            player = GameObject.FindWithTag("Player");

        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
                playerHealth.OnHealthChanged.AddListener(onHealthChanged);
            else
                Debug.LogWarning($"{name}: Player found but no Health component on it.");
        }
        else
        {
            Debug.LogWarning($"{name}: Player not found by tag 'Player'.");
        }

        // Immediate update
        if (playerHealth != null)
            onHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged.RemoveListener(onHealthChanged);
    }
}
