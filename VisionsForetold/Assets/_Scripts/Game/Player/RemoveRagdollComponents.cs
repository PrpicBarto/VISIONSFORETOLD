using UnityEngine;

/// <summary>
/// Utility script to remove ragdoll components from player.
/// Attach to player, right-click in Inspector, click "Remove All Ragdoll Components"
/// Then remove this script after cleanup.
/// </summary>
public class RemoveRagdollComponents : MonoBehaviour
{
    [ContextMenu("Remove All Ragdoll Components")]
    public void RemoveRagdoll()
    {
        int rigidbodyCount = 0;
        int colliderCount = 0;
        int jointCount = 0;
        
        // Get all rigidbodies in children
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>(true);
        
        Debug.Log($"[Ragdoll Cleanup] Found {rigidbodies.Length} Rigidbodies total");
        
        foreach (var rb in rigidbodies)
        {
            // Don't destroy the main rigidbody on player root
            if (rb.gameObject != gameObject)
            {
                Debug.Log($"[Ragdoll Cleanup] Removing Rigidbody from {GetPath(rb.gameObject)}");
                DestroyImmediate(rb);
                rigidbodyCount++;
            }
            else
            {
                Debug.Log($"[Ragdoll Cleanup] Keeping main Rigidbody on {rb.gameObject.name}");
            }
        }
        
        // Get all colliders in children (except main collider)
        Collider mainCollider = GetComponent<Collider>();
        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        
        Debug.Log($"[Ragdoll Cleanup] Found {colliders.Length} Colliders total");
        
        foreach (var col in colliders)
        {
            // Don't remove the main collider
            if (col != mainCollider && col.gameObject != gameObject)
            {
                Debug.Log($"[Ragdoll Cleanup] Removing Collider from {GetPath(col.gameObject)}");
                DestroyImmediate(col);
                colliderCount++;
            }
            else if (col == mainCollider)
            {
                Debug.Log($"[Ragdoll Cleanup] Keeping main Collider on {col.gameObject.name}");
            }
        }
        
        // Get all character joints
        CharacterJoint[] joints = GetComponentsInChildren<CharacterJoint>(true);
        
        Debug.Log($"[Ragdoll Cleanup] Found {joints.Length} Character Joints");
        
        foreach (var joint in joints)
        {
            Debug.Log($"[Ragdoll Cleanup] Removing Character Joint from {GetPath(joint.gameObject)}");
            DestroyImmediate(joint);
            jointCount++;
        }
        
        // Summary
        Debug.Log($"<color=green>[Ragdoll Cleanup] COMPLETE!</color>");
        Debug.Log($"<color=green>Removed: {rigidbodyCount} Rigidbodies, {colliderCount} Colliders, {jointCount} Joints</color>");
        Debug.Log($"<color=yellow>Remember to SAVE THE SCENE and remove this script!</color>");
    }
    
    /// <summary>
    /// Get the full path of a GameObject in the hierarchy
    /// </summary>
    private string GetPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        
        return path;
    }
}
