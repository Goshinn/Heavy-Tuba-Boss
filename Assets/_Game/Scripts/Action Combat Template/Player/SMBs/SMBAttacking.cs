using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SMBAttacking : StateMachineBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool isLightAttack = true;

    [Header("References")]
    private PlayerCombat playerCombat;
    private int lightAttackTagHash = Animator.StringToHash("LightAttack");  // Used to check if nextState is light attack. If so, call PlayerCombat.OnLightAttackStarted()
    private int skillTagHash = Animator.StringToHash("Skill");

    [Header("Members")]
    private bool invokedStoppedAttacking;

    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Hook up references
        playerCombat = animator.GetComponent<PlayerCombat>();

        // Init vars
        invokedStoppedAttacking = false;
        animator.SetInteger("LightAttackChain", 0);
        animator.SetInteger("Skill", 0);

        // Invoke evnts
        playerCombat.StartedAttack?.Invoke();
        if (isLightAttack)
        {
            playerCombat.LightAttackStarted?.Invoke();
        }
    }

    // OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!invokedStoppedAttacking && animator.IsInTransition(layerIndex))
        {
            AnimatorStateInfo nextStateInfo = animator.GetNextAnimatorStateInfo(layerIndex);
            if (nextStateInfo.tagHash != lightAttackTagHash && nextStateInfo.tagHash != skillTagHash)
            {
                //Debug.Log("invoking PlayerCombat.StoppedAttacking");
                playerCombat.StoppedAttacking?.Invoke();
                invokedStoppedAttacking = true;
            }
        }
    }

    // OnStateExit is called before OnStateExit is called on any state inside this state machine
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{

    //}

    // OnStateMove is called before OnStateMove is called on any state inside this state machine
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateIK is called before OnStateIK is called on any state inside this state machine
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}
}