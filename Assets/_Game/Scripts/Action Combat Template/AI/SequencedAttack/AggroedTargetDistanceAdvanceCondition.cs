using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EntityName_Attack00_Specifics", menuName = "Enemy/New Attack/Sequenced Attack/Sequence Advance Condition/DistanceAdvanceCondition")]
public class AggroedTargetDistanceAdvanceCondition : SequencedAttackAdvanceCondition, IAttackSequencerState
{
    [Header("Distance to advance attack")]
    [SerializeField] private float minimumDistanceToAdvanceAttack;
    [SerializeField] private float conditionValidDuration = 2f;
    [SerializeField] private bool forceAdvanceSequenceOnConditionValidDurationExceeded;

    [Header("Members")]
    private EnemyAIBase owner;
    private float elapsed;

    public override void Enter(EnemyAIBase enemyAI)
    {
        owner = enemyAI;
        elapsed = 0;
    }

    public override bool Execute()
    {
        float planarDistanceToTarget = Vector3.ProjectOnPlane(owner.AggroedPlayer.transform.position - owner.transform.position, Vector3.up).magnitude;
        if (planarDistanceToTarget <= minimumDistanceToAdvanceAttack)
        {
            // Callback to attackSequencer statemachine to advance attack sequence
            return true;
        }

        // If forceAdvanceSequence && conditionValidDuration exceeded, return true so that attackSequencer statemachine can advance attack sequence
        elapsed += Time.deltaTime;
        if (elapsed >= conditionValidDuration)
        {
            if (forceAdvanceSequenceOnConditionValidDurationExceeded)
            {
                return true;
            }
            else
            {
                // Figure out a way to empty the attackSequencer of this current state and make sure AI exits the sequenced attack state
            }
        }
        return false;
    }

    public override void Exit()
    {
        
    }
}
