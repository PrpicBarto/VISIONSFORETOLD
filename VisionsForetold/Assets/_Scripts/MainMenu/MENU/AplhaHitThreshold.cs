using System;
using UnityEngine;
using UnityEngine.UI;

public class AplhaHitThreshold : MonoBehaviour
{
    [RequireComponent(typeof(Image))]
    public class AlphaHitTest : MonoBehaviour
    {
        [SerializeField] private float threshold = 0.1f;
    
        private void Awake()
        {
            Image image = GetComponent<Image>();
        
            if (image != null && image.sprite != null)
            {
                // Check if texture is readable
                Texture2D texture = image.sprite.texture;
            
                try
                {
                    image.alphaHitTestMinimumThreshold = threshold;
                }
                catch (System.InvalidOperationException)
                {
                    Debug.LogError($"Texture '{texture.name}' needs Read/Write enabled! " +
                                   $"Select the image in Project window → Inspector → Advanced → Check 'Read/Write Enabled'");
                }
            }
        }
    }
}
