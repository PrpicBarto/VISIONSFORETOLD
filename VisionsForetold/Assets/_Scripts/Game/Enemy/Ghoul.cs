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
            if (player == null)
            {
                Debug.LogError($"Ghoul: player is null");
            }

            if (health.isDead)
            {
                Debug.Log($"Ghoul is dead, not moving");
                return;
            }
            
            float distanceToPlayer=Vector3.Distance(transform.position, player.position);
            Debug.Log($"Distance to player: {distanceToPlayer} || Detection range: {detectionRange}");
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
            Debug.Log($"UpdateBehavior called. Distance: {distanceToPlayer} || Attack range: {attackRange}");
            if (distanceToPlayer > attackRange)
            {
                if (agent != null && agent.isOnNavMesh)
                {
                    agent.SetDestination(player.position);
                    Debug.Log(
                        $"Agent status: hasPath={agent.hasPath}, pathPending={agent.pathPending}, enabled={agent.enabled}, isOnNavMesh={agent.isOnNavMesh}");
                }
                else
                {
                    Debug.LogError($"Agent problem: agent={agent}, isOnNavMesh={agent?.isOnNavMesh}");
                }
                Debug.Log($"Chasing player. Setting destination to {player.position}");
                //agent.SetDestination(player.position);
                Debug.Log($"Agent velocity: {agent.velocity}, Speed: {agent.speed}");
            }
            else
            {
                Debug.Log($"In attack range, stopping");
                agent.SetDestination(transform.position);
                TryAttack();
            }
            
        }

        private void TryAttack()
        {
            LookAtPlayer();
            
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(biteDamage);
                Debug.Log($"Ghoul ({ghoulType}) bit player for {biteDamage} damage!");
            }
        }
    }
}