using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AttackSequencingStateMachine
{
    [Header("Members")]
    private EnemyAIBase enemyAIBase;

    [Header("Members")]
    private IAttackSequencerState currentlyRunningState;

    // Events
    public Action AdvanceAttackSequence;          // Invoked by classes inheriting from SequencedAttackAdvanceCondition to and listened to by AI to advance attack sequence.

    public void InitializeAttackSequencer(EnemyAIBase enemyAI)
    {
        // Hook up references
        enemyAIBase = enemyAI;
    }

    public void ChangeState(IAttackSequencerState newState)
    {
        //Debug.Log($"AttackSequencer changing state to {newState.ToString()}");
        currentlyRunningState?.Exit();
        currentlyRunningState = newState;
        currentlyRunningState.Enter(enemyAIBase);
    }

    public void ExecuteUpdate()
    {
        if (currentlyRunningState != null && currentlyRunningState.Execute())
        {
            AdvanceAttackSequence?.Invoke();
            EmptyCurrentState();
        }
    }

    public void EmptyCurrentState()
    {
        currentlyRunningState?.Exit();
        currentlyRunningState = null;
    }
}
