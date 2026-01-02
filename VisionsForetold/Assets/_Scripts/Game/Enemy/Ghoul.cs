using UnityEngine;

namespace _Scripts.Game.Enemy
{
    //GHOUL - common melee enemy (3 verzije)
    public class Ghoul : BaseEnemy
    {
        public enum GhoulType
        {
            Basic,
            Strong,
            Fast
        }

        [Header("Ghoul Settings")] 
        [SerializeField] private GhoulType ghoulType = GhoulType.Basic;
        [SerializeField] private int biteDamage = 15;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackCooldown = 1.5f;

        private float lastAttackTime;

        protected override void Awake()
        {
            base.Awake();
            ConfigureGhoulType();
        }
        
        protected override void Update()
        {
            base.Update();
            // Animations are updated in base.Update() automatically
        }
        
        private void ConfigureGhoulType()
        {
            switch (ghoulType)
            {
                case GhoulType.Basic:
                    biteDamage = 15;
                    moveSpeed = 3f;
                    if (health != null) health.SetMaxHealth(40, false);
                    break;
                case GhoulType.Strong:
                    biteDamage = 25;
                    moveSpeed = 2.5f;
                    if (health != null) health.SetMaxHealth(60, false);
                    break;
                case GhoulType.Fast:
                    biteDamage = 12;
                    moveSpeed = 4.5f;
                    if (health != null) health.SetMaxHealth(30, false);
                    break;
            }

            if (agent != null) agent.speed = moveSpeed;
        }

        protected override void UpdateBehavior(float distanceToPlayer)
        {
            if (distanceToPlayer > attackRange)
            {
                // Chase the player
                if (agent != null && agent.enabled && agent.isOnNavMesh)
                {
                    agent.SetDestination(player.position);
                }
                else
                {
                    Debug.LogWarning($"{name}: Agent not ready - enabled={agent?.enabled}, isOnNavMesh={agent?.isOnNavMesh}");
                }
            }
            else
            {
                // In attack range - stop and attack
                if (agent != null && agent.enabled && agent.isOnNavMesh)
                {
                    agent.SetDestination(transform.position); // Stop moving
                }
                TryAttack();
            }
        }

        private void TryAttack()
        {
            // Check attack cooldown
            if (Time.time < lastAttackTime + attackCooldown)
            {
                return; // Still on cooldown
            }
            
            LookAtPlayer();
            
            if (VFXManager.Instance != null)
            {
                Vector3 attackPos = transform.position + transform.forward * 1f;
                VFXManager.Instance.PlayGhoulAttack(attackPos);
            }
            
            // Trigger attack animation using base class method
            TriggerAttackAnimation();
            
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(biteDamage);
                lastAttackTime = Time.time; // Reset cooldown
                Debug.Log($"Ghoul ({ghoulType}) bit player for {biteDamage} damage!");
            }
        }
    }
}