using UnityEngine;

/// <summary>
/// Debug helper to visualize attack range and detection
/// Add to Player GameObject for debugging
/// </summary>
public class PlayerAttackDebugger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerAttack playerAttack;
    
    [Header("Visualization")]
    [SerializeField] private bool showAttackRange = true;
    [SerializeField] private bool showAttackCone = true;
    [SerializeField] private bool showDetectedEnemies = true;
    [SerializeField] private Color rangeColor = Color.red;
    [SerializeField] private Color coneColor = Color.yellow;
    [SerializeField] private Color enemyColor = Color.green;
    
    private void OnDrawGizmos()
    {
        if (playerAttack == null)
        {
            playerAttack = GetComponent<PlayerAttack>();
            if (playerAttack == null) return;
        }
        
        Vector3 attackOrigin = transform.position + Vector3.up * 0.5f;
        Vector3 attackDirection = transform.forward;
        
        // Get attack range via reflection (since it's private)
        var field = typeof(PlayerAttack).GetField("attackRange", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        float attackRange = field != null ? (float)field.GetValue(playerAttack) : 2.5f;
        
        // Draw attack range sphere
        if (showAttackRange)
        {
            Gizmos.color = rangeColor;
            Gizmos.DrawWireSphere(attackOrigin, attackRange);
        }
        
        // Draw attack cone
        if (showAttackCone)
        {
            Gizmos.color = coneColor;
            Vector3 forward = attackDirection * attackRange;
            Gizmos.DrawLine(attackOrigin, attackOrigin + forward);
            
            // Draw cone edges (simplified)
            Vector3 right = Quaternion.Euler(0, 45, 0) * forward;
            Vector3 left = Quaternion.Euler(0, -45, 0) * forward;
            Gizmos.DrawLine(attackOrigin, attackOrigin + right);
            Gizmos.DrawLine(attackOrigin, attackOrigin + left);
        }
        
        // Find and highlight enemies in range
        if (showDetectedEnemies && Application.isPlaying)
        {
            var field2 = typeof(PlayerAttack).GetField("aimingLayerMask",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            LayerMask layerMask = field2 != null ? (LayerMask)field2.GetValue(playerAttack) : -1;
            
            Collider[] hitColliders = Physics.OverlapSphere(attackOrigin, attackRange, layerMask);
            
            foreach (var col in hitColliders)
            {
                if (col.gameObject != gameObject) // Don't highlight self
                {
                    Gizmos.color = enemyColor;
                    Gizmos.DrawWireSphere(col.bounds.center, 0.5f);
                    Gizmos.DrawLine(attackOrigin, col.bounds.center);
                }
            }
            
            // Draw text in scene view
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(attackOrigin + Vector3.up * 2, 
                $"Enemies in range: {hitColliders.Length}\nLayer Mask: {layerMask.value}");
            #endif
        }
    }
    
    private void OnGUI()
    {
        if (!Application.isPlaying) return;
        
        // Get layer mask value
        var field = typeof(PlayerAttack).GetField("aimingLayerMask",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        LayerMask layerMask = field != null ? (LayerMask)field.GetValue(playerAttack) : -1;
        
        // Draw debug info on screen
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label($"<b>Player Attack Debug</b>", new GUIStyle(GUI.skin.label) { richText = true, fontSize = 16 });
        GUILayout.Label($"Layer Mask Value: {layerMask.value}");
        GUILayout.Label($"Layer Mask Includes Enemy: {IsLayerInMask(LayerMask.NameToLayer("Enemy"), layerMask)}");
        
        // Find enemies in range
        Vector3 attackOrigin = transform.position + Vector3.up * 0.5f;
        var rangeField = typeof(PlayerAttack).GetField("attackRange",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        float attackRange = rangeField != null ? (float)rangeField.GetValue(playerAttack) : 2.5f;
        
        Collider[] hitColliders = Physics.OverlapSphere(attackOrigin, attackRange, layerMask);
        GUILayout.Label($"Enemies in Range: {hitColliders.Length}");
        
        // List enemies
        foreach (var col in hitColliders)
        {
            if (col.gameObject != gameObject)
            {
                Health health = col.GetComponent<Health>();
                string healthInfo = health != null ? $" (HP: {health.CurrentHealth}/{health.MaxHealth})" : " (No Health)";
                GUILayout.Label($"  - {col.gameObject.name}{healthInfo}");
            }
        }
        
        GUILayout.EndArea();
    }
    
    private bool IsLayerInMask(int layer, LayerMask layerMask)
    {
        return ((1 << layer) & layerMask.value) != 0;
    }
}
