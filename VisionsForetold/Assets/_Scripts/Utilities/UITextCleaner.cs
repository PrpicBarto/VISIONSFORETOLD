using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Helper script to find and disable unwanted UI text elements
/// Attach to any GameObject and it will search for and disable text matching the filter
/// </summary>
public class UITextCleaner : MonoBehaviour
{
    [Header("Search Settings")]
    [Tooltip("Text to search for (case insensitive)")]
    [SerializeField] private string searchText = "press m";
    
    [Tooltip("Automatically run on Start")]
    [SerializeField] private bool runOnStart = true;
    
    [Tooltip("Destroy found objects instead of just disabling")]
    [SerializeField] private bool destroyObjects = false;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private void Start()
    {
        if (runOnStart)
        {
            FindAndDisableText();
        }
    }

    [ContextMenu("Find and Disable Text")]
    public void FindAndDisableText()
    {
        int foundCount = 0;
        
        // Search for TMP_Text components
        TMP_Text[] tmpTexts = FindObjectsOfType<TMP_Text>(true);
        foreach (TMP_Text textComponent in tmpTexts)
        {
            if (textComponent.text.ToLower().Contains(searchText.ToLower()))
            {
                if (showDebugLogs)
                {
                    Debug.Log($"[UITextCleaner] Found text: '{textComponent.text}' on GameObject: {textComponent.gameObject.name}");
                }
                
                if (destroyObjects)
                {
                    Destroy(textComponent.gameObject);
                    if (showDebugLogs)
                    {
                        Debug.Log($"[UITextCleaner] Destroyed GameObject: {textComponent.gameObject.name}");
                    }
                }
                else
                {
                    textComponent.gameObject.SetActive(false);
                    if (showDebugLogs)
                    {
                        Debug.Log($"[UITextCleaner] Disabled GameObject: {textComponent.gameObject.name}");
                    }
                }
                
                foundCount++;
            }
        }
        
        // Search for legacy Text components
        Text[] legacyTexts = FindObjectsOfType<Text>(true);
        foreach (Text textComponent in legacyTexts)
        {
            if (textComponent.text.ToLower().Contains(searchText.ToLower()))
            {
                if (showDebugLogs)
                {
                    Debug.Log($"[UITextCleaner] Found legacy text: '{textComponent.text}' on GameObject: {textComponent.gameObject.name}");
                }
                
                if (destroyObjects)
                {
                    Destroy(textComponent.gameObject);
                    if (showDebugLogs)
                    {
                        Debug.Log($"[UITextCleaner] Destroyed GameObject: {textComponent.gameObject.name}");
                    }
                }
                else
                {
                    textComponent.gameObject.SetActive(false);
                    if (showDebugLogs)
                    {
                        Debug.Log($"[UITextCleaner] Disabled GameObject: {textComponent.gameObject.name}");
                    }
                }
                
                foundCount++;
            }
        }
        
        if (showDebugLogs)
        {
            if (foundCount > 0)
            {
                Debug.Log($"[UITextCleaner] ? Found and processed {foundCount} text element(s)");
            }
            else
            {
                Debug.LogWarning($"[UITextCleaner] No text containing '{searchText}' was found!");
            }
        }
    }

    [ContextMenu("List All UI Text")]
    public void ListAllUIText()
    {
        Debug.Log("=== All TMP_Text Components ===");
        TMP_Text[] tmpTexts = FindObjectsOfType<TMP_Text>(true);
        foreach (TMP_Text text in tmpTexts)
        {
            Debug.Log($"GameObject: {text.gameObject.name} | Text: '{text.text}' | Active: {text.gameObject.activeInHierarchy}");
        }
        
        Debug.Log("=== All Legacy Text Components ===");
        Text[] legacyTexts = FindObjectsOfType<Text>(true);
        foreach (Text text in legacyTexts)
        {
            Debug.Log($"GameObject: {text.gameObject.name} | Text: '{text.text}' | Active: {text.gameObject.activeInHierarchy}");
        }
    }
}
