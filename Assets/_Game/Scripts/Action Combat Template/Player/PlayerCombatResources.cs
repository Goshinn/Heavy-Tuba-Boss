using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using KinematicCharacterController;

/// <summary>
/// Responsible for managing health & stamina. Also handles death & respawn.
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerState))]
public class PlayerCombatResources : MonoBehaviour, IKillable
{
    [Header("References")]
    private Animator anim;
    private PlayerCombat playerCombat;
    private PlayerState playerState;
    private PlayerSoundManager playerSoundManager;

    [Header("Settings")]
    [SerializeField] private float respawnTime = 3f;
    private RespawnPoint[] respawnPoints;

    [Header("Combat Resources")]
    [SerializeField] private float maxHealth;
    [SerializeField] private float maxStamina;
    [SerializeField] private float maxBlockResource;

    [Header("Resource Regeneration")]
    [SerializeField] private float blockResourceRegenerationTickDuration = 4f;               // Time in seconds before a tick occurs for reiatsu regeneration
    [SerializeField, Range(0, 1f)] private float blockResourceRegenerationAmount = 0.33f;
    [SerializeField] private float staminaRegenerationRate = 20f;

    //[Header("FeedBack")]
    //[SerializeField] private CameraEffects_CameraShakeInstance rawDamageTakenFeedBack;
    //[SerializeField] private GameObject bladeAttributeHitVFXPrefab;
    //public static event Action<VFXInfo> NewVisualFeedBack;

    [Header("Members")]
    private float currentHealth;
    private float currentBlockResource;
    private float currentStamina;
    private float timeTillBlockResourceRegen;

    // Events
    public event Action<AttackInfo, bool> PlayerHit;
    public event Action HealthChanged;
    public event Action HealthLost;
    public event Action BlockResourceChanged;
    public event Action BlockResourceDepleted;
    public event Action StaminaChanged;
    public event Action<IKillable> EntityDeathEvent;
    public event Action<RespawnPoint> PlayerStartedRevive;      // Listened to by CameraBase to reposition to revived player

    #region Accessors
    public string KillableName { get { return "Player"; } }
    public Transform HealthbarPositionTrackerObject => throw new NotImplementedException();
    public GameObject AttachedGameObject { get => gameObject; }
    public float CurrentHealth
    {
        get { return currentHealth; }
        set
        {
            float prevVal = currentHealth;
            currentHealth = value;
            HealthChanged?.Invoke();
            if (currentHealth < prevVal) HealthLost?.Invoke();
        }
    }
    public float BlockResource
    {
        get { return currentBlockResource; }
        set
        {
            currentBlockResource = Mathf.Clamp(value, 0, MaxBlockResource);
            if (BlockResourceChanged != null) BlockResourceChanged();
        }
    }
    public float Stamina
    {
        get { return currentStamina; }
        set
        {
            currentStamina = value;
            StaminaChanged?.Invoke();
        }
    }
    public float MaxHealth { get { return maxHealth; } }
    public float MaxStamina { get { return maxStamina; } }
    public float MaxBlockResource { get { return maxBlockResource; } }
    #endregion

    private void Awake()
    {
        // Hook up vars
        anim = GetComponent<Animator>();
        playerCombat = GetComponent<PlayerCombat>();
        playerState = GetComponent<PlayerState>();
        playerSoundManager = GetComponent<PlayerSoundManager>();

        // Initialize vars
        timeTillBlockResourceRegen = blockResourceRegenerationTickDuration; // When player first consumes stamina, player will have to wait for a tick to occur
    }

    private void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        currentBlockResource = maxBlockResource;
    }

    private void Update()
    {
        // Resource regeneration
        if (playerState.IsAlive)
        {
            // Regenerate Reiatsu
            if (BlockResource < maxBlockResource)
            {
                RegenerateBlockResource();
            }

            // Regenerate Stamina
            if (Stamina < maxStamina && !playerState.IsDodging)
            {
                RegenerateStamina();
            }

            //if (Health < playerInfo.maxHealth)
            //{
            //    //Debug.Log("Current Health" + Health + "," + "max Health" + HealthStat.Value);
            //    RegenerateHealth();
            //}
        }
    }

    #region Resource Regeneration
    private void RegenerateBlockResource()
    {
        if (timeTillBlockResourceRegen <= 0)
        {
            // Add stamina then start timer to countdown till next tick
            BlockResource = Mathf.Clamp(currentBlockResource + maxBlockResource * blockResourceRegenerationAmount, 0, maxBlockResource);
            timeTillBlockResourceRegen = blockResourceRegenerationTickDuration;
        }
        else
        {
            timeTillBlockResourceRegen -= Time.deltaTime;
        }
    }

    private void RegenerateStamina()
    {
        Stamina = Mathf.Clamp(Stamina + staminaRegenerationRate * Time.deltaTime, 0, maxStamina);
    }

    private void RegenerateHealth()
    {
        //Health = Mathf.Clamp(Health + HealthRegenerationStat.Value * Time.deltaTime, 0, HealthStat.Value);
    }
    #endregion

    #region IKillable
    /// <summary>
    /// Deals direct damage to the player w/o visual feedback. Does not take into account whether player is blocking/dodging. 
    /// Use this for straightforward stuff like Damage over Time.
    /// </summary>
    /// <param name="damage"></param>
    public void TakeDamage(float damage)
    {
        if (CurrentHealth <= 0)
        {
            return;
        }

        CurrentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);

        if (currentHealth <= 0) Die();
    }

    /// <summary>
    /// Use this if damage was dealt to player and u need some form of visual feedback.
    /// Handles how much damage to deal to player depending on player blocking/dodging etc.
    /// Information needed to reflect the visual feedback is contained within AttackInfo.
    /// </summary>
    /// <param name="attackInfo"></param>
    public void HandleHit(AttackInfo attackInfo)
    {
        // Null damage if dodging
        if (playerState.IFrame || CurrentHealth <= 0)
        {
            return;
        }

        // Process damage when blocking
        if (playerState.IsBlocking && attackInfo.IsBlockable && playerCombat.WasAttackWithinEffectiveBlockCoverage(attackInfo.AttackerObj))
        {
            // Consume BlockResource
            BlockResource -= attackInfo.BlockResourceDrain;

            if (BlockResource > 0)
            {
                // Mitigate dmg and consume block resource
                //Debug.Log("atk blocked");
                float damageModifier = 1 - playerCombat.blockDamageMitigation;
                float damage = attackInfo.Damage * damageModifier;
                TakeDamage(damage);
                playerCombat.SuccessfulBlock.Invoke(attackInfo); // This will trigger block feedback anim, vfx etc
            }
            else
            {
                //Debug.Log("Failed to block due to insufficient block resource, receiving attack");

                // Invoke events, reset timeTillBlockResourceRegenTick, receive attack
                BlockResourceDepleted?.Invoke();
                timeTillBlockResourceRegen = blockResourceRegenerationTickDuration;
                ReceiveAttack(attackInfo);
                playerCombat.FailedBlock.Invoke(attackInfo);
            }
        }
        // Just take the dmg
        else
        {
            //Debug.Log("not blocking or dodging, receiving attack");
            ReceiveAttack(attackInfo);

            // Handle hurt camera shake feedback from attackInfo

            /*
            // Publish camera shake upon taking raw damage (if any)
            if (rawDamageTakenFeedBack != null)
            {
                GetComponent<CameraEffectPublisher>()?.PublishCameraShake(rawDamageTakenFeedBack);
            }

            // Handle VFX
            switch (attackInfo.attackAttribute)
            {
                case AttackAttribute.Blade:
                    VFXInfo vfxInfo = new VFXInfo(gameObject, bladeAttributeHitVFXPrefab, GetComponent<CharacterController>().bounds.center);
                    NewVisualFeedBack?.Invoke(vfxInfo);
                    break;
            }
            */
        }
    }

    private void ReceiveAttack(AttackInfo attackInfo)
    {
        TakeDamage(attackInfo.Damage);

        // Handle CC
        if (CurrentHealth > 0)
        {
            for (int i = 0; i < attackInfo.SkillEffects.Count; i++)
            {
                attackInfo.SkillEffects[i].UseSkillEffect(attackInfo);
            }
        }

        // Publish events
        PlayerHit?.Invoke(attackInfo, playerState.IsAlive);
    }

    public void Die()
    {
        // Animate death
        anim.SetTrigger("Die");

        // Publish evnts
        EntityDeathEvent?.Invoke(this);

        // Begin respawn
        StartCoroutine(BeginRespawn());
    }

    private IEnumerator BeginRespawn()
    {
        yield return new WaitForSeconds(respawnTime);
        RespawnPlayer();
    }

    private void RespawnPlayer()
    {
        // Animate revive
        anim.SetTrigger("Revive");

        // Get nearest spawnpoint
        if (respawnPoints == null)
        {
            Debug.Log("getting respawn points...");
            respawnPoints = FindObjectsOfType<RespawnPoint>();
        }
        respawnPoints.OrderBy(respawnPoint => (transform.position - respawnPoint.transform.position).sqrMagnitude);

        // Move to spawn point
        GetComponent<KinematicCharacterMotor>().SetPositionAndRotation(respawnPoints[0].transform.position, respawnPoints[0].transform.rotation);

        // Replenish resources
        CurrentHealth = maxHealth;
        BlockResource = maxBlockResource;
        Stamina = maxStamina;

        // Publish evnts
        PlayerStartedRevive?.Invoke(respawnPoints[0]);
        //PlayerRespawned?.Invoke();
    }

    public GameObject GetAttachedGameObject()
    {
        return gameObject;
    }
    #endregion
}

/// <summary>
/// For spawning hitfeedback vfx on successful hit; this struct contains information on how to orient the spawned vfx, as well as what hitfeedback to spawn
/// </summary>
public struct VFXInfo
{
    public GameObject target;
    public GameObject hitFeedBackVFXPrefab;
    public Vector3 position;

    public VFXInfo(GameObject target, GameObject hitFeedBackPrefab, Vector3 position)
    {
        this.target = target;
        this.hitFeedBackVFXPrefab = hitFeedBackPrefab;
        this.position = position;
    }
}