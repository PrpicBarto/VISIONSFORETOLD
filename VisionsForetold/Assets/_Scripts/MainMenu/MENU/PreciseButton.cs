using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class PreciseButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Image image;
    private Button button;
    
    [SerializeField] private float alphaThreshold = 0.1f;

    private void Awake()
    {
        image = GetComponent<Image>();
        button = GetComponent<Button>();
        
        // Disable default raycasting
        image.raycastTarget = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (IsPixelVisible(eventData))
        {
            button?.OnPointerDown(eventData);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (IsPixelVisible(eventData))
        {
            button?.OnPointerUp(eventData);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsPixelVisible(eventData))
        {
            button?.OnPointerEnter(eventData);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        button?.OnPointerExit(eventData);
    }

    private bool IsPixelVisible(PointerEventData eventData)
    {
        if (image == null || image.sprite == null) return true;

        // Convert screen point to local point in image
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            image.rectTransform, 
            eventData.position, 
            eventData.pressEventCamera, 
            out Vector2 localPoint);

        // Get texture coordinates
        Rect rect = image.rectTransform.rect;
        Vector2 pivot = image.rectTransform.pivot;
        
        Vector2 normalized = new Vector2(
            (localPoint.x + pivot.x * rect.width) / rect.width,
            (localPoint.y + pivot.y * rect.height) / rect.height
        );

        // Check if within bounds
        if (normalized.x < 0 || normalized.x > 1 || normalized.y < 0 || normalized.y > 1)
            return false;

        // Sample texture
        Texture2D texture = image.sprite.texture;
        if (!texture.isReadable) return true; // If not readable, allow click

        Rect spriteRect = image.sprite.rect;
        int x = Mathf.FloorToInt(spriteRect.x + normalized.x * spriteRect.width);
        int y = Mathf.FloorToInt(spriteRect.y + normalized.y * spriteRect.height);

        Color pixel = texture.GetPixel(x, y);
        return pixel.a >= alphaThreshold;
    }
}
