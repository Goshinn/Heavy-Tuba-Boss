using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Goal #1: Allow attacking the moment this state begins transition to locomotion state. Accomplished with PlayerState.IsDodging
/// Goal #2: Only allow dodging again after this state has been fully exited. Accomplished with PlayerState.FinishedDodging
/// </summary>
public class SMBDodgeRoll : StateMachineBehaviour
{
    [Header("References")]
    private PlayerCombat playerCombat;
    private int dodgeTag = Animator.StringToHash("Dodge");

    [Header("Members")]
    private bool invokedEndedDodge;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Hook up references
        playerCombat = animator.GetComponent<PlayerCombat>();

        // Set vars
        invokedEndedDodge = false;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Invoke DodgeEnded upon state transition into other state to end iFrame, allow attacking etc
        //if (!invokedEndedDodge && animator.IsInTransition(0) && animator.GetNextAnimatorStateInfo(0).tagHash != dodgeTag)
        //{
        //    invokedEndedDodge = true;
        //    playerCombat.DodgeEnded?.Invoke();
        //}
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        playerCombat.EndedDodge?.Invoke();   // Invoking FinishedDodge allows player to dodge again
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
