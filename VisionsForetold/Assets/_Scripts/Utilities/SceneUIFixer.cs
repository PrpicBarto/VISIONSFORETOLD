using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SceneUIFixer : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureExists()
    {
        if (FindObjectOfType<SceneUIFixer>() == null)
        {
            var go = new GameObject("SceneUIFixer");
            DontDestroyOnLoad(go);
            go.AddComponent<SceneUIFixer>();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Delay one frame so objects from the scene finish Awake/Start
        StartCoroutine(ReinitializeNextFrame());
    }

    private IEnumerator ReinitializeNextFrame()
    {
        yield return null;

        // Check if we're in gameplay scene (not main menu)
        string currentScene = SceneManager.GetActiveScene().name.ToLower();
        bool isMainMenu = currentScene.Contains("menu") || currentScene.Contains("title");
        
        // Only reset time/cursor in menu scenes, not during gameplay
        if (isMainMenu)
        {
            // Make sure game isn't left paused
            Time.timeScale = 1f;

            // Ensure cursor is visible and unlocked for menus
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            
            // DON'T re-enable PlayerInput in menu scenes - they shouldn't have player input!
            // Remove any leftover player objects from gameplay
            GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
            foreach (GameObject obj in allObjects)
            {
                // Check if it's a player object from gameplay that shouldn't be in menu
                if (obj.scene.name != currentScene && 
                    (obj.CompareTag("Player") || obj.name.Contains("Player")))
                {
                    Destroy(obj);
                    Debug.Log($"[SceneUIFixer] Destroyed leftover player object: {obj.name}");
                }
            }
        }
        else
        {
            // In gameplay scenes, don't mess with time scale or cursor
            // (Let the game control these)
        }

        // Ensure EventSystem exists
        if (EventSystem.current == null)
        {
            GameObject es = new GameObject("EventSystem");
            var eventSystem = es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
            Debug.Log("[SceneUIFixer] Created missing EventSystem");
        }

        // Restore CanvasGroup blocksRaycasts for all canvases (so buttons receive clicks)
        var canvasGroups = FindObjectsOfType<CanvasGroup>(true);
        foreach (var cg in canvasGroups)
        {
            // If a canvas group is intended to block raycasts, ensure it's allowed
            if (cg.gameObject.activeInHierarchy)
            {
                cg.blocksRaycasts = cg.blocksRaycasts || true; // ensure raycasts are allowed
            }
        }

        // Ensure GraphicRaycaster present on all canvases
        var canvases = FindObjectsOfType<Canvas>(true);
        foreach (var c in canvases)
        {
            if (c.gameObject.activeInHierarchy && c.GetComponent<GraphicRaycaster>() == null)
            {
                c.gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        // Reactivate / enable map objects (common names/tags)
        // Try to find map by tag - but check if tag exists first
        GameObject mapByTag = null;
        try
        {
            mapByTag = GameObject.FindGameObjectWithTag("Map");
        }
        catch (UnityException)
        {
            // Tag doesn't exist - that's okay, we'll try by name instead
            Debug.Log("[SceneUIFixer] 'Map' tag not defined, searching by name instead");
        }
        
        if (mapByTag != null)
        {
            mapByTag.SetActive(true);
            EnableMonoBehavioursOn(mapByTag);
        }
        else
        {
            // Also try by name contains "Map"
            var all = FindObjectsOfType<GameObject>(true);
            foreach (var go in all)
            {
                if (go.name.ToLower().Contains("map"))
                {
                    go.SetActive(true);
                    EnableMonoBehavioursOn(go);
                }
            }
        }

        // Select the first interactable Button only in menu scenes
        if (isMainMenu)
        {
            var buttons = FindObjectsOfType<Button>(true);
            Button first = null;
            foreach (var b in buttons)
            {
                if (b.gameObject.activeInHierarchy && b.interactable)
                {
                    first = b;
                    break;
                }
            }

            if (first != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(first.gameObject);
            }
        }

        Debug.Log("[SceneUIFixer] UI reinitialization complete for scene: " + currentScene);
    }

    private void EnableMonoBehavioursOn(GameObject root)
    {
        var monos = root.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var m in monos)
        {
            if (m != null && !m.enabled)
            {
                m.enabled = true;
            }
        }
    }
}
