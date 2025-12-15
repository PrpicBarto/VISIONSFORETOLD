using System;
using UnityEngine;

public class TornadoProjectile : MonoBehaviour
{
    private int damage;
    private float speed;
    private float lifetime = 8f;
    private bool hasHit;

    public void Initialize(int dmg, float spd)
    {
        damage = dmg;
        speed = spd;
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * speed;
        }
        
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        if (other.CompareTag("Player"))
        {
            Health playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);

                if (DamageNumberManager.Instance != null)
                {
                    DamageNumberManager.Instance.ShowDamage(other.transform.position + Vector3.up * 2f, damage);
                }
            }
            
            hasHit = true;
            Destroy(gameObject);
        }
        
        else if (other.gameObject.layer != LayerMask.NameToLayer("Enemy"))
        {
            hasHit = true;
            Destroy(gameObject);
        }
    }
}
