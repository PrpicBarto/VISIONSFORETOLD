using UnityEngine;

public static class HealthExtensions
{
    public static void TakeDamageWithNumber(this Health health, int damage)
    {
        health.TakeDamage(damage);
        
        // Show damage number
        if (DamageNumberManager.Instance != null)
        {
            Vector3 position = health.transform.position + Vector3.up * 2f;
            DamageNumberManager.Instance.ShowDamage(position, damage);
        }
    }

    public static void HealWithNumber(this Health health, int healing)
    {
        health.Heal(healing);
        
        // Show healing number
        if (DamageNumberManager.Instance != null)
        {
            Vector3 position = health.transform.position + Vector3.up * 2f;
            DamageNumberManager.Instance.ShowHealing(position, healing);
        }
    }
}