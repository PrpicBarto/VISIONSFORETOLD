using UnityEngine;

public class DamageNumberManager : MonoBehaviour
{
    public static DamageNumberManager Instance { get; private set; }
    
    [Header("Prefab")]
    [SerializeField] private GameObject damageNumberPrefab;
    
    [Header("Settings")]
    [SerializeField] private float numberLifetime = 1f;
    [SerializeField] private float numberRiseSpeed = 2f;
    [SerializeField] private float numberFadeSpeed = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowDamage(Vector3 position, int damage)
    {
        if (damageNumberPrefab == null) return;
        
        GameObject numObj = Instantiate(damageNumberPrefab, position, Quaternion.identity);
        DamageNumber damageNum = numObj.GetComponent<DamageNumber>();
        
        if (damageNum != null)
        {
            damageNum.Initialize($"-{damage}", Color.red, numberRiseSpeed, numberLifetime);
        }
    }

    public void ShowHealing(Vector3 position, int healing)
    {
        if (damageNumberPrefab == null) return;
        
        GameObject numObj = Instantiate(damageNumberPrefab, position, Quaternion.identity);
        DamageNumber damageNum = numObj.GetComponent<DamageNumber>();
        
        if (damageNum != null)
        {
            damageNum.Initialize($"+{healing}", Color.green, numberRiseSpeed, numberLifetime);
        }
    }

    public void ShowXPGain(Vector3 position, int xp)
    {
        if (damageNumberPrefab == null) return;
        
        GameObject numObj = Instantiate(damageNumberPrefab, position, Quaternion.identity);
        DamageNumber damageNum = numObj.GetComponent<DamageNumber>();
        
        if (damageNum != null)
        {
            damageNum.Initialize($"+{xp} XP", new Color(0.3f, 0.5f, 1f), numberRiseSpeed, numberLifetime);
        }
    }
}

