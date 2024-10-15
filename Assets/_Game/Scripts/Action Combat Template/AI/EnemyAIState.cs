using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(EnemyAIBase))]
public class EnemyAIState : MonoBehaviour, IFlinch
{
    [Header("References")]
    private Animator anim;
    private EnemyAIBase enemyAI;

    [Header("Settings")]
    [SerializeField] private int defaultSuperArmorValue;

    [Header("Combat")]
    [SerializeField] private int superArmor;
    [SerializeField] private bool isAttacking;
    [SerializeField] private bool isFlinched;
    [SerializeField] private int ccDegree;
    [SerializeField] private bool isCrowdControlled;
    // private CCDatabase ccDatabase;

    #region Accessors
    public bool IsAttacking { get { return isAttacking; } }
    public bool IsFlinched { get { return isFlinched; } }
    //public int CCDegree
    //{
    //    get
    //    {
    //        if (isFlinched)
    //        {
    //            return ccDatabase.FlinchSO.CCDegree;
    //        }
    //    }
    //}
    public virtual bool IsCrowdControlled { get { return isCrowdControlled; } }     // Make sure to override this if enemy has unique CC states such as taking a break, etc.
    #endregion

    protected virtual void Awake()
    {
        // Hook up references
        anim = GetComponent<Animator>();
        enemyAI = GetComponent<EnemyAIBase>();
    }

    protected virtual void Start()
    {
        superArmor = defaultSuperArmorValue;
        ResetEntityState();
    }

    protected virtual void OnEnable()
    {
        // Subscribe to events
        enemyAI.StartedAttack += OnStartedAttack;
        enemyAI.EndedAttack += OnEndedAttack;
    }

    #region Callbacks
    private void OnStartedAttack()
    {
        isAttacking = true;
    }

    private void OnEndedAttack()
    {
        isAttacking = false;
    }
    #endregion

    #region Utility
    private void ResetEntityState()
    {
        isAttacking = false;
    }

    private void SnapRotationToAttacker(GameObject attackerGO)
    {
        // Snap rotation to lookAt attacker
        Quaternion desiredRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(attackerGO.transform.position - transform.position, Vector3.up));
        transform.rotation = desiredRotation;
    }
    #endregion


    #region IFlinch
    public void AttemptFlinch(AttackInfo attackInfo, int thisCCDegree)
    {
        // Check superarmor & cc degree
    }

    public void Flinch(AttackInfo attackInfo)
    {
        // Set vars + animate
        isFlinched = true;
        anim.SetInteger("CrowdControl", (int)CrowdControlEnumeration.Flinch);

        // Snap rotation to lookAt attacker
        SnapRotationToAttacker(attackInfo.AttackerObj);
    }

    public void EndFlinch()
    {
        isFlinched = false;
        ResetEntityState();
    }
    #endregion

    protected virtual void OnDisable()
    {
        // Unsubscribe to events
        enemyAI.StartedAttack -= OnStartedAttack;
        enemyAI.EndedAttack -= OnEndedAttack;
    }
}
