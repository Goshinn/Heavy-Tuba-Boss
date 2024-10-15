using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// To add new melee attacks, 
/// 1. Create a reference to the hitbox
/// 2. Add a new entry to the HeavyTubaGunnerMeleeAttacks enum. 
/// 3. Update InitializeAttackDictionary() to include the new hitbox with the appropriate enum.
/// For ranged attacks, its a case by case basis. 
/// In this example, instead of calling ActivateHitbox() via anim evnt, we call HornBlast() instead which is a custom method for the ranged "HornBlast" attack.
/// </summary>
public class HeavyTubaGunnerWeapon : MonoBehaviour
{
    [Header("References")]
    private AIResources aiResources;
    private HeavyTubaGunnerAI htgAI;
    private HeavyTubaGunnerAIState htgAIState;

    [Header("Setup")]
    [SerializeField] private Transform tubaGunMuzzlePosition;
    [SerializeField] private List<ProjectileHitbox> hornBlastProjectiles;
    [SerializeField] private MeleeAttackHitbox hitboxTubaCharge;
    [SerializeField] private MeleeAttackHitbox hitboxSwing;

    [Header("Musical Missile Barrage")]
    [SerializeField] private List<MusicalMissileBarrageProjectile> musicalMissileProjectiles;
    [SerializeField] private float summonMissileIntervalDuration = 1f;
    [SerializeField] private int musicalMissilesToSummon = 3;
    [SerializeField] private float musicalMissileFireInterval = 0.5f;

    [Header("Members")]
    private Dictionary<HeavyTubaGunnerMeleeAttacks, MeleeAttackHitbox> attackDictionary = new Dictionary<HeavyTubaGunnerMeleeAttacks, MeleeAttackHitbox>();
    private Queue<MusicalMissileBarrageProjectile> musicalMissileQueue;

    // Events
    public event Action FiredHornBlast;

    [Header("Lazy Developer")]
    public bool autoPopulateAttackHitboxes;

    public enum HeavyTubaGunnerMeleeAttacks
    {
        TubaCharge = 1 << 1,
        Swing = 1 << 2
    }

    private void OnValidate()
    {
        if (autoPopulateAttackHitboxes)
        {
            hitboxSwing = GetComponentsInChildren<MeleeAttackHitbox>(true)[0];
        }
    }

    protected void Awake()
    {
        // Hook up references
        aiResources = GetComponent<AIResources>();
        htgAI = GetComponent<HeavyTubaGunnerAI>();
        htgAIState = GetComponent<HeavyTubaGunnerAIState>();

        // Init attack dict
        InitializeAttackDictionary();
    }

    // Update this as new attacks are created
    private void InitializeAttackDictionary()
    {
        attackDictionary.Add(HeavyTubaGunnerMeleeAttacks.Swing, hitboxSwing);
        attackDictionary.Add(HeavyTubaGunnerMeleeAttacks.TubaCharge, hitboxTubaCharge);

        // Create musicalMissileQueue
        musicalMissileQueue = new Queue<MusicalMissileBarrageProjectile>(musicalMissileProjectiles);
    }

    public void ActivateHitbox(HeavyTubaGunnerMeleeAttacks hitbox)
    {
        // Do not activate hitboxes if boss is already dead / in CC
        if (aiResources.CurrentHealth > 0 && !htgAIState.IsCrowdControlled)
        {
            attackDictionary[hitbox].ActivateHitbox();
        }
    }

    public void HornBlast()
    {
        // Dequeue a projectile from the queue of horn blast projectiles
        Vector3 shootDirection = (htgAI.AggroedPlayer.GetComponent<Collider>().bounds.center - tubaGunMuzzlePosition.position).normalized;
        hornBlastProjectiles[0].ShootFromTowards(tubaGunMuzzlePosition.position, shootDirection);

        // Trigger shoot sound
        FiredHornBlast?.Invoke();
    }

    public void BeginMusicalMissileBarrage()
    {
        StartCoroutine(SummonMusicalMissiles());
    }

    private IEnumerator SummonMusicalMissiles()
    {
        int musicalMissilesSummoned = 0;
        while (musicalMissilesSummoned < musicalMissilesToSummon && !htgAIState.IsCrowdControlled && aiResources.CurrentHealth > 0)
        {
            // Summon projectiles which float out of the Tuba and hover above HTG, orbiting him until the next command is given.
            MusicalMissileBarrageProjectile musicalMissile = musicalMissileQueue.Dequeue();
            musicalMissile.SummonProjectile(tubaGunMuzzlePosition.position);
            musicalMissileQueue.Enqueue(musicalMissile);
            musicalMissilesSummoned++;
            yield return new WaitForSeconds(summonMissileIntervalDuration);
        }     
    }

    public IEnumerator CommenceMusicalMissileBarrage()
    {
        // The musicalMissileProjectiles turn off automatically upon HTG crowdControlled
        // Hence, we do not want to attempt starting a coroutine on objects that would be disabled. We do this by checking if HTG is currently crowdControlled.
        if (!htgAIState.IsCrowdControlled)
        {
            GameObject target = htgAI.AggroedPlayer;    // Cache the target so that projectiles know who to home towards in the event that the player dies.
            int musicalMissilesFired = 0;
            while (musicalMissilesFired < musicalMissilesToSummon)
            {
                // Tell the projectiles to home towards the aggroedTarget consecutively.
                MusicalMissileBarrageProjectile musicalMissile = musicalMissileQueue.Dequeue();
                musicalMissile.StartCoroutine(musicalMissile.BeginHomeTowardsTarget(target));
                musicalMissileQueue.Enqueue(musicalMissile);
                musicalMissilesFired++;
                yield return new WaitForSeconds(musicalMissileFireInterval);
            }
        }
    }
}
