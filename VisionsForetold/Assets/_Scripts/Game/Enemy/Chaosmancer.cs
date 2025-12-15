using System;
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

    }
}