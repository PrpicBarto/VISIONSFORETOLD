using UnityEngine;

namespace _Scripts.Game.Enemy
{
    //uncommon aggressive enemy (2 verzije)
    public class Wraith : BaseEnemy
    {
        public enum WraithType
        {
            Aggressive,
            Ranged
        }

        [Header("Wraith Settings")] [SerializeField]
        private WraithType wraithType = WraithType.Aggressive;

        [SerializeField] private GameObject magicExplosionPrefab;
        [SerializeField] private int clawDamage = 20;
        [SerializeField] private int explosionDamage = 30;
        [SerializeField] private float attackRange = 3f;
        [SerializeField] private float rapidAttackSpeed = 0.3f;
        [SerializeField] private int rapidAttackCount = 3;

        private float lastAttackTime;
        private int currentAttackCount;

        protected override void Awake()
        {
            base.Awake();
            ConfigureWraithType();
        }
        
        protected override void Update()
        {
            base.Update();
            // Animations are updated in base.Update() automatically
        }

        private void ConfigureWraithType()
        {
            switch (wraithType)
            {
                case WraithType.Aggressive:
                    moveSpeed = 4f;
                    attackRange = 2.5f;
                    if (health != null) health.SetMaxHealth(50, false);
                    break;
                case WraithType.Ranged:
                    moveSpeed = 3f;
                    attackRange = 6f;
                    if (health != null) health.SetMaxHealth(40, false);
                    break;
            }

            if (agent != null) agent.speed = moveSpeed;
        }

        protected override void UpdateBehavior(float distanceToPlayer)
        {
            if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            {
                Debug.LogWarning($"{name}: Agent not ready - enabled={agent?.enabled}, isOnNavMesh={agent?.isOnNavMesh}");
                return;
            }

            if (distanceToPlayer > attackRange)
            {
                agent.SetDestination(player.position);
            }
            else
            {
                agent.SetDestination(transform.position);
                TryAttack();
            }
        }

        private void TryAttack()
        {
            if (Time.time - lastAttackTime > rapidAttackSpeed)
            {
                if (wraithType == WraithType.Aggressive)
                {
                    RapidClawAttack();
                }
                else
                {
                    MagicExplosion();
                }

                lastAttackTime = Time.time;
            }
        }

        private void RapidClawAttack()
        {
            LookAtPlayer();

            // Trigger attack animation
            TriggerAttackAnimation();

            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(clawDamage);
                currentAttackCount++;

                if (currentAttackCount >= rapidAttackCount)
                {
                    currentAttackCount = 0;
                    lastAttackTime = Time.time + 1f; //duži cooldown nakon combo-a
                }
            }
            
            if (VFXManager.Instance != null)
            {
                Vector3 attackPos = transform.position + transform.forward * 0.5f;
                VFXManager.Instance.PlayWraithClaw(attackPos, transform.forward);
            }
        }

        private void MagicExplosion()
        {
            LookAtPlayer();

            // Trigger attack animation
            TriggerAttackAnimation();

            if (magicExplosionPrefab != null)
            {
                Vector3 spawnPos = player.position;
                Instantiate(magicExplosionPrefab, spawnPos, Quaternion.identity);
            }

            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(explosionDamage);
            }
        }
    }
}