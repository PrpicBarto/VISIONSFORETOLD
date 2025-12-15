using System;
using System.Collections;
using UnityEngine;

public class Chaosmancer : MonoBehaviour
{
    [Header("Boss stats")] [SerializeField]
    private Health health;

    [SerializeField] private int maxHealth = 500;

    [Header("Movement")] [SerializeField] private Transform player;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float minDistance = 8f;
    [SerializeField] private float maxDistance = 15f;

    [Header("Attack 1 - Tornado Projectile")] [SerializeField]
    private GameObject tornadoProjectilePrefab;

    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private int tornadoDamage = 25;
    [SerializeField] private float tornadoSpeed = 10f;
    [SerializeField] private float tornadoCooldown = 4f;

    [Header("Attack 2 - Tornado Transformation")] [SerializeField]
    private GameObject tornadoFormPrefab;

    [SerializeField] private float pullForce = 5f;
    [SerializeField] private float pullRadius = 10f;
    [SerializeField] private float transformDuration = 5f;
    [SerializeField] private int transformDamagePerTick = 10;
    [SerializeField] private float transformCooldown = 12f;

    [Header("Attack 3 - Ground Slam")] [SerializeField]
    private float slamRange = 6f;

    [SerializeField] private int slamDamage = 35;
    [SerializeField] private float knockupForce = 10f;
    [SerializeField] private GameObject slamEffectPrefab;
    [SerializeField] private float slamCooldown = 8f;

    [Header("Boss Behavior")] [SerializeField]
    private float phaseTransitionHealth = 0.5f;

    private bool isEnraged = false;

    [Header("Audio")] [SerializeField] private AudioClip tornadoSound;
    [SerializeField] private AudioClip transformSound;
    [SerializeField] private AudioClip slamSound;
    [SerializeField] private AudioClip roarSound;

    private float lastTornadoTime = -999f;
    private float lastTransformTime = -999f;
    private float lastSlamTime = -999f;
    private bool isTransformed;
    private bool isDead;
    private Rigidbody rb;
    private AudioSource audioSource;
    private GameObject tornadoFormDistance;

    private int attackCounter;
    private bool inPhase2;

    private void Awake()
    {
        if (health == null) health = GetComponent<Health>();
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        FindPlayer();
    }

    private void Start()
    {
        if (health == null)
        {
            health.SetMaxHealth(maxHealth, false);
            health.OnHealthChanged.AddListener(OnHealthChanged);
            health.OnDeath.AddListener(OnDeath);
        }

        if (projectileSpawnPoint == null)
        {
            projectileSpawnPoint = transform;
        }

        PlaySound(roarSound);
    }

    private void FindPlayer()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    private void Update()
    {
        if (isDead || player == null || health.isDead) return;

        if (!isTransformed)
        {
            HandleMovement();
            DecideNextAttack();
        }
    }

    private void HandleMovement()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer < minDistance)
        {
            Vector3 retreatDir = (transform.position - player.position).normalized;
            Vector3 newPosition = transform.position + retreatDir * (moveSpeed * Time.deltaTime);
            transform.position = newPosition;
        }
        else if (distanceToPlayer > maxDistance)
        {
            Vector3 approachDir = (player.position - transform.position).normalized;
            Vector3 newPosition = transform.position + approachDir * (moveSpeed * Time.deltaTime);
            transform.position = newPosition;
        }

        LookAtPlayer();
    }

    private void LookAtPlayer()
    {
        Vector3 direction = (player.position - transform.position);
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void DecideNextAttack()
    {
        float currentTime = Time.time;

        bool canTornado = currentTime - lastTransformTime > tornadoCooldown;
        bool canTransform = currentTime - lastTransformTime > transformCooldown;
        bool canSlam = currentTime - lastSlamTime > slamCooldown;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (canTransform && attackCounter >= 3)
        {
            StartCoroutine(TransformAttack());
            attackCounter = 0;
        }
        
        else if (canSlam && distanceToPlayer <= slamDamage)
        {
            GroundSlamAttack();
            attackCounter = 0;
        }
        
        else if (canTornado)
        {
            TornadoProjectileAttack();
            attackCounter++;
        }
    }

    private void TornadoProjectileAttack()
    {
        if (tornadoProjectilePrefab == null)
        {
            lastTornadoTime = Time.time;
            LookAtPlayer();

            Vector3 direction = (player.position - projectileSpawnPoint.position).normalized;
            GameObject tornado = Instantiate(tornadoFormPrefab, projectileSpawnPoint.position,
                Quaternion.LookRotation(direction));

            TornadoProjectile tornadoScript = tornado.GetComponent<TornadoProjectile>();
            if (tornadoScript != null)
            {
                tornadoScript.Initialize(tornadoDamage, tornadoSpeed);
            }
            else
            {
                ProjectileDamage projDamage = tornado.GetComponent<ProjectileDamage>();
                if (projDamage != null)
                {
                    projDamage.Initialize(tornadoDamage, gameObject, "Player");
                }
                
                Rigidbody tornadoRb = tornado.GetComponent<Rigidbody>();
                if (tornadoRb != null)
                {
                    tornadoRb.linearVelocity = direction * tornadoSpeed;
                }
            }
            PlaySound(tornadoSound);
            Debug.Log("Chaosmancer fired tornado!");
        }
    }

    private IEnumerator TransformAttack()
    {
        isTransformed = true;
        lastTransformTime = Time.time;

        if (tornadoFormPrefab != null)
        {
            tornadoFormDistance = Instantiate(tornadoFormPrefab, transform.position, Quaternion.identity);
            tornadoFormDistance.transform.SetParent(transform);
        }

        MeshRenderer renderer = GetComponentInChildren<MeshRenderer>();
        if (renderer != null) renderer.enabled = false;
        
        PlaySound(transformSound);
        Debug.Log($"Chaosmancer transformed into tornado!");
        
        float elapsedTime = 0f;
        float damageTickRate = 0.5f;
        float lastDamageTick = 0f;

        while (elapsedTime < transformDuration)
        {
            PullPlayerTowardBoss();

            if (elapsedTime - lastDamageTick >= damageTickRate)
            {
                DealTransformDamage();
                lastDamageTick = elapsedTime;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (tornadoFormDistance != null)
        {
            Destroy(tornadoFormDistance);
        }
        
        if(renderer != null) renderer.enabled = true;
        
        isTransformed = false;
        Debug.Log($"Chaosmancer transformation ended!");
    }

    private void PullPlayerTowardBoss()
    {
        if(player==null) return;
        
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= pullRadius)
        {
            Vector3 pullDir = (transform.position - player.position).normalized;
            
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.AddForce(pullDir * pullForce, ForceMode.Force);
            }
            else
            {
                player.position += pullDir * (pullForce * Time.deltaTime * 0.1f);
            }
        }
    }

    private void DealTransformDamage()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= pullRadius)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(transformDamagePerTick);

                if (DamageNumberManager.Instance != null)
                {
                    DamageNumberManager.Instance.ShowDamage(player.position + Vector3.up * 2f, transformDamagePerTick);
                }
            }
        }
    }

    private void GroundSlamAttack()
    {
        lastSlamTime = Time.time;
        
        StartCoroutine(PerformGroundSlam());
    }

    private IEnumerator PerformGroundSlam()
    {
        yield return new WaitForSeconds(0.5f);

        PlaySound(slamSound);

        if (slamEffectPrefab != null)
        {
            GameObject slamEffect = Instantiate(slamEffectPrefab, transform.position, Quaternion.identity);
            Destroy(slamEffect, 2f);
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= slamRange)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(slamDamage);

                if (DamageNumberManager.Instance != null)
                {
                    DamageNumberManager.Instance.ShowDamage(player.position + Vector3.up * 2f, slamDamage);
                }
            }
        }
        
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            playerRb.AddForce(Vector3.up * knockupForce, ForceMode.Impulse);
        }

        Debug.Log($"Chaosmancer slammed the ground! Player knocked up!");
    }

    private void OnHealthChanged(int currentHealth, int maxHealth)
    {
        float healthPercent = (float)currentHealth / maxHealth;

        if (!inPhase2 && healthPercent < phaseTransitionHealth)
        {
            EnterPhase2();
        }
    }

    private void EnterPhase2()
    {
        inPhase2 = true;
        isEnraged = true;

        tornadoCooldown *= 0.7f;
        transformCooldown *= 0.7f;
        slamCooldown *= 0.7f;

        moveSpeed *= 1.3f;

        PlaySound(roarSound);
        Debug.Log($"Chaosmancer entered Phase 2! ENRAGED!!!");
    }

    private void OnDeath()
    {
        isDead = true;
        
        StopAllCoroutines();

        if (tornadoFormDistance != null)
        {
            Destroy(tornadoFormDistance);
        }

        Debug.Log($"Chaosmancer defeated!");
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, slamRange);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pullRadius);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minDistance);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, maxDistance);
    }
}