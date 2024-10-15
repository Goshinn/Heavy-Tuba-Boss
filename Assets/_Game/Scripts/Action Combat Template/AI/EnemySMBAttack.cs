using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Make sure all attack states have been tagged as "Attack"
/// </summary>
public class EnemySMBAttack : StateMachineBehaviour
{
    [Header("References")]
    private EnemyAIBase enemyAI;
    private int attackTagHash = Animator.StringToHash("Attack");

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Hook up references
        enemyAI = animator.GetComponent<EnemyAIBase>();

        // Reset anim params
        animator.SetInteger("Attack", 0);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.GetCurrentAnimatorStateInfo(layerIndex).tagHash != attackTagHash)
        {
            enemyAI.EndedAttack?.Invoke();
        }
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
