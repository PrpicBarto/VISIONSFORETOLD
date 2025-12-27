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

        // Make sure game isn't left paused
        Time.timeScale = 1f;

        // Ensure cursor is visible and unlocked for menus
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Ensure EventSystem exists
        if (EventSystem.current == null)
        {
            GameObject es = new GameObject("EventSystem");
            var eventSystem = es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
            DontDestroyOnLoad(es); // optional: keep it between loads
            Debug.Log("[SceneUIFixer] Created missing EventSystem");
        }

        // Re-enable PlayerInput components (in case they were disabled)
        var inputs = FindObjectsOfType<PlayerInput>(true);
        foreach (var pi in inputs)
        {
            if (!pi.enabled) pi.enabled = true;
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

        // Select the first interactable Button so gamepad/keyboard navigation works immediately
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

        Debug.Log("[SceneUIFixer] UI reinitialization complete");
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
