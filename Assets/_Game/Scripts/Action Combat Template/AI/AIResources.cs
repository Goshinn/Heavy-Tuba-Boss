using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Responsible for managing health & other resources.
/// Holds functions such as TakeDamage() and handles crowd control.
/// </summary>
public class AIResources : MonoBehaviour, IKillable
{
    [Header("References")]
    private Animator anim;
    private EnemyAIBase enemyAI;

    [Header("Killable Setup")]
    [SerializeField] private string enemyName;
    [SerializeField] private float maxHealth;
    [SerializeField] private Transform healthBarTrackerObj;
    private float health;

    [Header("Enemy Setup")]
    [SerializeField] protected Collider enemyCollider;
    [SerializeField] protected Collider aiHitbox;

    [Header("Hit FeedBack Types")]
    [SerializeField] protected GameObject bladeAttributeHitVFXPrefab;

    [Header("Despawn Settings")]
    [SerializeField] protected float despawnTime = 120f;
    [SerializeField] protected float deathScriptDeactivationDelay;

    // Events
    public event Action HealthLost;                             // Listened to by EntityInfoHUD to set healthbar fadeout timer 
    public event Action HealthChanged;                          // Listened to by EntityInfoHUD to update healthbar            
    public static Action<VFXInfo> NewVisualFeedBack;            // Listened to by VFXPooler to spawn hit feedback
    public event Action<AttackInfo> HitImpact;                  // Invoked when hit by an attack

    public event Action<IKillable> EntityDeathEvent;            // Listened to by EntityInfoController to despawn entityInfoHUD OnEntityDeath.
    public event Action CrowdControlled;

    #region Accessors
    public string KillableName { get => enemyName; }
    public float MaxHealth { get => maxHealth; }
    public float CurrentHealth
    {
        get { return health; }
        set
        {
            float prevHealth = health;
            health = Mathf.Clamp(value, 0, MaxHealth);
            HealthChanged?.Invoke();
            if (prevHealth > health)
            {
                HealthLost?.Invoke();
            }
        }
    }
    public Transform HealthbarPositionTrackerObject { get { return healthBarTrackerObj; } }
    public GameObject AttachedGameObject { get { return gameObject; } }
    public bool IsStaggering { get; private set; }
    #endregion

    private void Awake()
    {
        // Hook up references
        anim = GetComponent<Animator>();
        enemyAI = GetComponent<EnemyAIBase>();
    }

    private void Start()
    {
        // Init vars
        CurrentHealth = maxHealth;
    }

    #region IKillable
    public void HandleHit(AttackInfo attackInfo)
    {
        // Handle crowd control

        // Deal dmg 
        TakeDamage(attackInfo.Damage);

        // Invoke events to trigger sfx
        HitImpact?.Invoke(attackInfo);
    }

    // Possibly called by DoTs in the future
    protected void TakeDamage(float damage)
    {
        CurrentHealth -= damage;   // Clamping occurs via the setter
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        // Disable all colliders
        aiHitbox.enabled = false;
        enemyCollider.enabled = false;

        // Stop all movement coroutines?
        StopAllCoroutines();

        // Publish necessary evnts
        EntityDeathEvent?.Invoke(this);

        // Call necessary funcs  

        // Unsubscribe from evnts etc

        // Animate dead
        anim.SetTrigger("Die");

        // Begin despawn
        GetComponent<EnemyDespawner>().BeginDespawn(despawnTime);
    }
    #endregion
}