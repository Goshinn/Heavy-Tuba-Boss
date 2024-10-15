using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the player state - whether the player is crowd controlled, blocking, attacking etc. Other scripts may reference this for their behaviour logic.
/// </summary>
[RequireComponent(typeof(Animator), typeof(PlayerCombat), typeof(PlayerCombatResources))]
public class PlayerState : MonoBehaviour, ICrowdControl, IFlinch, IKnockback, IKnockdown, IRevive
{
    [Header("References")]
    private Animator anim;
    private PlayerCombat playerCombat;
    private PlayerCombatResources combatResources;
    private int lightAttackTagHash = Animator.StringToHash("LightAttack");
    private int skillTagHash = Animator.StringToHash("Skill");

    [Header("Combat")]
    [SerializeField] private bool isAttacking;                      // Set to true OnStateEnter of Attacking state via statemachinebehaviour and false on OnStateMachineExit
    [SerializeField] private bool allowActionQueuing;               // Allow desired actions to be queued and played whenever ready
    [SerializeField] private bool allowAttacking;                   // Used for allowing continuation of attack chain/ attacking
    [SerializeField] private bool canBlock;                         // Determines whether player is able to block. Attacking sets this false. Set this to true via anim evnts.
    [SerializeField] private bool isBlocking;                       // Set to true via anim evnt(?) and false on transition begin to other state other than blocking state
    [SerializeField] private bool blockCooledDown;                  // When true, the player is allowed to block.
    [SerializeField] private bool canAnimCancel;                    // Used for cancelling anims with DODGE
    [SerializeField] private bool isDodging;                        // Acts as a cooldown for dodge. Set to false when dodge state has been fully exited.
    [SerializeField] private bool iFrame;                           // Boolean to check against when determining whether an attack was dodged or not.

    [Header("CrowdControl")]
    [SerializeField] private int currentCCDegree;
    [SerializeField] private bool isReviving;
    [SerializeField] private bool isFlinched;
    [SerializeField] private bool isKnockedback;
    [SerializeField] private bool isKnockeddown;
    [SerializeField] private bool isBlockRecoiling;

    // Properties
    public bool IsReviving { get => isReviving; }
    public bool IsFlinched { get => isFlinched; }
    public bool IsKnockedback { get => isKnockedback; }
    public bool IsKnockeddown { get => isKnockeddown; }
    public bool IsBlockRecoiling { get => isBlockRecoiling; }

    // Misc
    public bool IsAlive { get { return combatResources.CurrentHealth > 0; } }

    #region Accessors
    public bool IsAttacking { get { return isAttacking; } }
    public bool AllowActionQueuing { get { return allowActionQueuing; } }
    public bool AllowAttacking { get { return allowAttacking; } }
    public bool CanBlock { get { return canBlock; } }
    public bool IsBlocking { get { return isBlocking; } }
    public bool BlockCooledDown { get { return blockCooledDown; } }
    private int CurrentCCDegree { get { return currentCCDegree; } }
    public bool IsCrowdControlled { get { return IsFlinched || IsKnockedback || IsKnockeddown; } }
    public bool CanAnimCancel { get { return canAnimCancel; } }
    public bool IsDodging { get { return isDodging; } }
    public bool IFrame { get { return iFrame; } }
    #endregion

    // Events
    public event Action CrowdControlled;

    private void Awake()
    {
        // Hook up references
        anim = GetComponent<Animator>();
        playerCombat = GetComponent<PlayerCombat>();
        combatResources = GetComponent<PlayerCombatResources>();

        // Init vars
        ResetPlayerState();
    }

    private void OnEnable()
    {
        // Subscribe to events
        playerCombat.StartedAttack += OnStartedAttack;
        playerCombat.StoppedAttacking += OnStoppedAttack;
        playerCombat.Dodged += OnDodged;
        playerCombat.EndedDodge += OnEndedDodge;
        playerCombat.SuccessfulBlock += OnBlockedAttack;

        combatResources.PlayerStartedRevive += OnPlayerStartedRevive;
    }

    #region Callbacks
    private void OnStartedAttack()
    {
        isAttacking = true;
        allowAttacking = false;
        allowActionQueuing = false;
        canBlock = false;
        canAnimCancel = false;  
    }

    private void OnStoppedAttack()
    {
        isAttacking = false;
        allowAttacking = true;
        allowActionQueuing = true;
        canBlock = true;
        canAnimCancel = true; 
    }

    private void OnDodged()
    {
        // Set vars
        isDodging = true;
        allowActionQueuing = false;
        allowAttacking = false;
        canAnimCancel = false;
    }

    private void OnEndedDodge()
    {
        iFrame = false; // set to false here in case designer forgot to turn off iframe
        isDodging = false;

        int currAnimatorStateTagHash = anim.GetCurrentAnimatorStateInfo(0).tagHash;
        if (currAnimatorStateTagHash != lightAttackTagHash && currAnimatorStateTagHash != skillTagHash)
        {
            ResetPlayerState();
        }
    }

    private void OnBlockedAttack(AttackInfo attackInfo)
    {
        isBlockRecoiling = true;
    }

    private void OnPlayerStartedRevive(RespawnPoint respawPoint)
    {
        isReviving = true;
    }
    #endregion

    public void SetIsBlocking(bool value)
    {
        isBlocking = value;
    }

    // Called in PlayerCombat upon blocking and OnStateExit of Player_Behaviour_Block statemachinebehaviour
    public void SetBlockCooledDown(bool value)
    {
        blockCooledDown = value;
    }

    // Called the moment player begins a transition out of block recoil state via SMB
    public void EndBlockRecoiling()
    {
        isBlockRecoiling = false;
    }

    #region IFlinch
    public void AttemptFlinch(AttackInfo attackInfo, int thisCCDegree)
    {
        // Check cc degree to see if new cc's ccDegree > existing ccDegree
        if (DetermineIfNewCCShouldBeApplied(thisCCDegree))
        {
            Flinch(attackInfo);
        }
    }

    public void Flinch(AttackInfo attackInfo)
    {
        // Set vars + animate
        isFlinched = true;
        anim.SetInteger("CrowdControl", (int)CrowdControlEnumeration.Flinch);

        // Snap rotation to lookAt attacker
        SnapRotationToAttacker(attackInfo.AttackerObj);

        // Publish evnts
        CrowdControlled?.Invoke();
    }

    public void EndFlinch()
    {
        isFlinched = false;
        ResetPlayerState();
    }
    #endregion

    #region IKnockback
    public void AttemptKnockback(AttackInfo attackInfo, int thisCCDegree)
    {
        // Check cc degree to see if new cc's ccDegree > existing ccDegree
        if (DetermineIfNewCCShouldBeApplied(thisCCDegree))
        {
            Knockback(attackInfo);
        }
    }

    public void Knockback(AttackInfo attackInfo)
    {
        // Set vars + animate
        isKnockedback = true;
        anim.SetInteger("CrowdControl", (int)CrowdControlEnumeration.Knockback);

        // Snap rotation to lookAt attacker
        SnapRotationToAttacker(attackInfo.AttackerObj);

        // Publish evnts
        CrowdControlled?.Invoke();
    }

    public void EndKnockback()
    {
        isKnockedback = false;
        ResetPlayerState();
    }
    #endregion

    #region IKnockdown
    public void AttemptKnockdown(AttackInfo attackInfo, int thisCCDegree)
    {
        if (DetermineIfNewCCShouldBeApplied(thisCCDegree))
        {
            Knockdown(attackInfo);
        }
    }

    public void Knockdown(AttackInfo attackInfo)
    {
        // Set vars + animate
        isKnockeddown = true;
        anim.SetInteger("CrowdControl", (int)CrowdControlEnumeration.Knockdown);

        // Snap rotation to lookAt attacker
        SnapRotationToAttacker(attackInfo.AttackerObj);

        // Publish evnts
        CrowdControlled?.Invoke();
    }

    public void EndKnockdown()
    {
        isKnockeddown = false;
        ResetPlayerState();
    }
    #endregion

    #region Utility
    private void SnapRotationToAttacker(GameObject attackerGO)
    {
        // Snap rotation to lookAt attacker
        Quaternion desiredRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(attackerGO.transform.position - transform.position, Vector3.up));
        transform.rotation = desiredRotation;
    }

    // Allows the player to do whatever he should be able to do under normal circumstances
    private void ResetPlayerState()
    {
        canAnimCancel = true;
        isDodging = false;
        canBlock = true;
        blockCooledDown = true;
        allowActionQueuing = true;
        allowAttacking = true;
    }

    private bool DetermineIfNewCCShouldBeApplied(int ccDegree)
    {
        return ccDegree > currentCCDegree;
    }
    #endregion

    #region Anim Events
    public void EnableActionQueuing()
    {
        allowActionQueuing = true;
    }

    // Called via animation events sometime after the attack has landed - to allow cancelling current anim to dodge/block
    public void AllowAnimCancellingOnlyIfNotInTransition()
    {
        if (!anim.IsInTransition(0))
        {
            canAnimCancel = true;
        }
    }

    // Called via animation events sometime after the attack has landed to allow continuation of attack chain
    public void AllowAttackingOnlyIfNotTransitioningToAttack()
    {
        int nextStateTagHash = anim.GetNextAnimatorStateInfo(0).tagHash;
        if (anim.IsInTransition(0) && (nextStateTagHash == lightAttackTagHash || nextStateTagHash == skillTagHash))
        {
            Debug.Break();
            return;
        }
        allowAttacking = true;
    }

    public void AllowBlockingOnlyIfNotTransitioningToAttack()
    {
        int nextStateTagHash = anim.GetNextAnimatorStateInfo(0).tagHash;
        if (anim.IsInTransition(0) && (nextStateTagHash == lightAttackTagHash || nextStateTagHash == skillTagHash))
        {
            return;
        }
        canBlock = true; // Set to false OnStartedAttack
    }

    public void ActivateIFrame()
    {
        iFrame = true;
    }

    public void DeactivateIFrame()
    {
        iFrame = false;
    }
    #endregion

    private void OnDisable()
    {
        // Unsubscribe from evnts
        playerCombat.StartedAttack -= OnStartedAttack;
        playerCombat.StoppedAttacking -= OnStoppedAttack;
        playerCombat.Dodged -= OnDodged;
        playerCombat.EndedDodge -= OnEndedDodge;
        playerCombat.SuccessfulBlock -= OnBlockedAttack;
    }

    public void CompleteRevive()
    {
        isReviving = false;
    }
}
