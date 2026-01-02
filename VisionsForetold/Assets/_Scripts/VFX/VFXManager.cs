using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// manages visual effects
/// </summary>
public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [Header("Combat VFX")]
    [SerializeField] private GameObject swordSlashEffect;
    [SerializeField] private GameObject arrowShotEffect;
    [SerializeField] private GameObject[] spellCastEffects; // different spells
    
    [Header("Hit Effects")]
    [SerializeField] private GameObject hitSparksEffect;
    [SerializeField] private GameObject bloodEffect;
    [SerializeField] private GameObject magicHitEffect;
    
    [Header("Movement VFX")]
    [SerializeField] private GameObject walkDustEffect;
    [SerializeField] private GameObject dashEffect;
    
    [Header("Enemy VFX")]
    [SerializeField] private GameObject ghoulAttackEffect;
    [SerializeField] private GameObject ghostShotEffect;
    [SerializeField] private GameObject wraithClawEffect;
    
    [Header("Boss VFX")]
    [SerializeField] private GameObject tornadoEffect;
    [SerializeField] private GameObject groundSlamEffect;
    [SerializeField] private GameObject bossChargeEffect;
    
    [Header("Settings")]
    [SerializeField] private float defaultLifetime = 2f;
    
    // pool for performance
    private Dictionary<string, Queue<GameObject>> effectPools = new Dictionary<string, Queue<GameObject>>();

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

    #region Spawn Methods

    /// <summary>
    /// Spawn effect at position and rotation
    /// </summary>
    public GameObject SpawnEffect(GameObject effectPrefab, Vector3 position, Quaternion rotation, float lifetime = 0f)
    {
        if (effectPrefab == null) return null;

        GameObject effect = Instantiate(effectPrefab, position, rotation);
        
        if (lifetime <= 0)
        {
            lifetime = defaultLifetime;
        }
        
        Destroy(effect, lifetime);
        return effect;
    }

    /// <summary>
    /// spawn effect at position facing direction
    /// </summary>
    public GameObject SpawnEffect(GameObject effectPrefab, Vector3 position, Vector3 direction, float lifetime = 0f)
    {
        Quaternion rotation = direction != Vector3.zero 
            ? Quaternion.LookRotation(direction) 
            : Quaternion.identity;
        
        return SpawnEffect(effectPrefab, position, rotation, lifetime);
    }

    /// <summary>
    /// spawn effect and parent to transform
    /// </summary>
    public GameObject SpawnEffectAttached(GameObject effectPrefab, Transform parent, Vector3 localPosition, float lifetime = 0f)
    {
        if (effectPrefab == null) return null;

        GameObject effect = Instantiate(effectPrefab, parent);
        effect.transform.localPosition = localPosition;
        
        if (lifetime > 0)
        {
            Destroy(effect, lifetime);
        }
        
        return effect;
    }

    #endregion

    #region Combat VFX

    public void PlaySwordSlash(Vector3 position, Vector3 direction)
    {
        SpawnEffect(swordSlashEffect, position, direction, 0.5f);
    }

    public void PlayArrowShot(Vector3 position, Vector3 direction)
    {
        SpawnEffect(arrowShotEffect, position, direction, 0.3f);
    }

    public void PlaySpellCast(Vector3 position, int spellIndex = 0)
    {
        if (spellCastEffects.Length > spellIndex)
        {
            SpawnEffect(spellCastEffects[spellIndex], position, Quaternion.identity, 1f);
        }
    }

    public void PlayHitEffect(Vector3 position, HitType hitType = HitType.Physical)
    {
        GameObject effect = hitType switch
        {
            HitType.Physical => hitSparksEffect,
            HitType.Blood => bloodEffect,
            HitType.Magic => magicHitEffect,
            _ => hitSparksEffect
        };
        
        SpawnEffect(effect, position, Quaternion.identity, 1f);
    }

    #endregion

    #region Movement VFX

    public void PlayWalkDust(Vector3 position)
    {
        SpawnEffect(walkDustEffect, position, Quaternion.identity, 0.5f);
    }

    public void PlayDash(Vector3 position, Vector3 direction)
    {
        SpawnEffect(dashEffect, position, direction, 0.8f);
    }

    #endregion

    #region Enemy VFX

    public void PlayGhoulAttack(Vector3 position)
    {
        SpawnEffect(ghoulAttackEffect, position, Quaternion.identity, 0.5f);
    }

    public void PlayGhostShot(Vector3 position, Vector3 direction)
    {
        SpawnEffect(ghostShotEffect, position, direction, 0.3f);
    }

    public void PlayWraithClaw(Vector3 position, Vector3 direction)
    {
        SpawnEffect(wraithClawEffect, position, direction, 0.4f);
    }

    #endregion

    #region Boss VFX

    public void PlayTornadoEffect(Vector3 position)
    {
        SpawnEffect(tornadoEffect, position, Quaternion.identity, 3f);
    }

    public void PlayGroundSlam(Vector3 position)
    {
        SpawnEffect(groundSlamEffect, position, Quaternion.identity, 1.5f);
    }

    public void PlayBossCharge(Transform bossTransform)
    {
        SpawnEffectAttached(bossChargeEffect, bossTransform, Vector3.zero, 2f);
    }

    #endregion

    public enum HitType //vjv useless za sada
    {
        Physical,
        Blood,
        Magic
    }
}
