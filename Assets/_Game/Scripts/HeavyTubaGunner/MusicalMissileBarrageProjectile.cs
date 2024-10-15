using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicalMissileBarrageProjectile : AttackHitbox
{
    [Header("Additional References")]
    private Transform projectilePoolerGO;
    private Collider col;
    private HeavyTubaGunnerAI htgAI;
    private IKillable user;

    [Header("Projectile Settings")]
    [SerializeField] private LayerMask collisionTriggersEOL;
    [SerializeField] private float summoningProjectileMoveSpeed = 3f;   // Speed at which this projectile will move towards its targetPos before entering orbit
    [SerializeField] private float orbitSpeed = 90f;                    // Metric: Degrees per second around the orbit object
    [SerializeField] private float projectileLifetime = 3f;
    [SerializeField] private float projectileHomingSpeed = 100f;
    [SerializeField] private float stopHomingDistance = 0.5f;           // Distance to home target at which this projectile will lose its homing ability

    [Header("Members")]
    private bool hasBegunHomeTowardsTarget;     // If false, projectile will orbit the user

    // todo: 
    // 1. trigger EOL on damageBreakEnter
    // 2. trigger EOL on death

    protected override void Awake()
    {
        base.Awake();

        // Hook up additional references
        projectilePoolerGO = transform.parent;
        col = GetComponent<Collider>();
        htgAI = ownerGO.GetComponent<HeavyTubaGunnerAI>();
        user = ownerGO.GetComponent<IKillable>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        transform.parent = null;
        hasBegunHomeTowardsTarget = false;

        // Setup callbacks
        user.EntityDeathEvent += killable => OnEndOfLifeReached();
        htgAI.DamageBreakEntered += OnEndOfLifeReached;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        // Get the killable component; return if killable not found
        IKillable killable = other.transform.GetComponentInParent<IKillable>();
        GameObject target = killable?.AttachedGameObject;
        if (killable != null && killable.CurrentHealth > 0 && target != null && target != ownerGO && !HasAlreadyHitEntity(target))
        {
            // Has not hit this entity since activation yet, add to entitiesHitSinceActivation & DealAttackEffectsToTarget
            entitiesHitSinceActivation.Add(target);
            DealAttackEffectsToTarget(target, killable);
        }

        // If a collision occurs between this trigger collider and an object that belongs in collisionTriggersEOL layermask, triggerOnEndOfLifeReached()
        if ((other.gameObject.layer | 1 << collisionTriggersEOL) == collisionTriggersEOL)
        {
            Debug.Log($"{name} collided w {other.name}, EOL");
            OnEndOfLifeReached();
        }
    }

    public void SummonProjectile(Vector3 tubaGunMuzzlePosition)
    {
        gameObject.SetActive(false);            // Trigger OnDisable logic
        gameObject.SetActive(true);             // Trigger OnEnable logic
        col.enabled = false;                    // Prevent projectiles from hitting players before they are fired

        // Reset position of this projectile
        transform.position = tubaGunMuzzlePosition;
        StartCoroutine(AwaitFurtherOrders());
    }

    public IEnumerator AwaitFurtherOrders()
    {    
        // Float to targetpos
        Vector3 targetPos = ownerGO.transform.rotation * new Vector3(0, 3, -1) + ownerGO.transform.position;
        while (transform.position != targetPos)
        {
            yield return null;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, summoningProjectileMoveSpeed * Time.deltaTime);
        }

        // Then orbit ownerGO
        while (!hasBegunHomeTowardsTarget)
        {
            yield return null;
            transform.RotateAround(ownerGO.transform.position, Vector3.up, orbitSpeed * Time.deltaTime);
        }
    }

    public IEnumerator BeginHomeTowardsTarget(GameObject target)
    {
        StartCoroutine(BeginLifeTime());
        entitiesHitSinceActivation.Clear();
        col.enabled = true;

        //Debug.Log($"begin home towards target...");
        hasBegunHomeTowardsTarget = true;
        Collider targetCol = target.GetComponent<Collider>();

        // Home towards the target until a collision is detected, afterwhich it loses its homing ability. 
        Vector3 lastHomeDirection = Vector3.zero;
        float distanceToTarget = Vector3.Distance(transform.position, targetCol.bounds.center);
        while (distanceToTarget > stopHomingDistance)
        {
            // Home towards target
            Vector3 prevPos = transform.position;
            yield return null;
            transform.position = Vector3.MoveTowards(transform.position, targetCol.bounds.center, projectileHomingSpeed * Time.deltaTime);

            // Set vars
            distanceToTarget = Vector3.Distance(transform.position, targetCol.bounds.center);
            lastHomeDirection = (transform.position - prevPos).normalized;
        }

        //Debug.Log("came close enough to target. Breaking out of homing loop");

        // Keep travelling until this projectile is destroyed (deactivate and reset self on collision with ground)
        while (gameObject.activeSelf)
        {
            yield return null;
            transform.position += lastHomeDirection * projectileHomingSpeed * Time.deltaTime;
        }
    }

    protected override void DealAttackEffectsToTarget(GameObject target, IKillable killable)
    {
        // Calculate damage & call HandleHit() from the target killable
        Vector3 attackDirection = Vector3.ProjectOnPlane(transform.position - target.transform.position, Vector3.up).normalized;
        AttackInfo attackInfo = new AttackInfo();
        attackInfo.SetAttackHitInfo(attackHitInfo.BaseDamage, attackHitInfo.IsBlockable, attackHitInfo.BlockResourceDrainAmount, attackHitInfo.SkillEffects);
        attackInfo.SetHitFeedBackInfo(ownerGO, target, attackHitInfo.AttackAttribute, attackDirection);
        killable.HandleHit(attackInfo);
    }

    private IEnumerator BeginLifeTime()
    {
        float elapsed = 0;
        while (elapsed < projectileLifetime)
        {
            yield return null;
            elapsed += Time.deltaTime;
        }

        // End of Life reached, deactivate this projectile and return it to owner (object pool GO)
        OnEndOfLifeReached();
    }

    private void OnEndOfLifeReached()
    {
        //Debug.Log("EOL reached.");

        // Repositions the projectile to its original parent
        transform.parent = projectilePoolerGO;
        transform.localPosition = Vector3.zero;

        // Spawn destroy self PE

        // Deactivate gameObject
        gameObject.SetActive(false);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        // Unregister callbacks
        htgAI.DamageBreakEntered -= OnEndOfLifeReached;
        user.EntityDeathEvent -= killable => OnEndOfLifeReached();
    }
}
