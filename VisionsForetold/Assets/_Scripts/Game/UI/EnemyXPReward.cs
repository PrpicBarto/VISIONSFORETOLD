using UnityEngine;

public class EnemyXPReward : MonoBehaviour
{
    [Header("XP Reward")]
    [SerializeField] private int xpReward = 25;
    [SerializeField] private int xpRewardMin = 20;
    [SerializeField] private int xpRewardMax = 30;
    [SerializeField] private bool randomizeReward = false;
    
    private Health health;
    private bool hasGivenXP = false;

    private void Start()
    {
        health = GetComponent<Health>();
        
        if (health != null)
        {
            health.OnDeath.AddListener(GiveXPToPlayer);
        }
    }

    private void GiveXPToPlayer()
    {
        if (hasGivenXP) return;
        hasGivenXP = true;
        
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerXP playerXP = player.GetComponent<PlayerXP>();
            if (playerXP != null)
            {
                int reward = randomizeReward ? Random.Range(xpRewardMin, xpRewardMax + 1) : xpReward;
                playerXP.AddXP(reward);
            }
        }
    }
}
