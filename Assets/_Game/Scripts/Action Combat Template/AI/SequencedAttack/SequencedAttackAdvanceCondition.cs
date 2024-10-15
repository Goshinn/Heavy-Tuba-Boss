using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SequencedAttackAdvanceCondition : ScriptableObject, IAttackSequencerState
{
    public virtual void Enter(EnemyAIBase enemyAI)
    {
        throw new System.NotImplementedException();
    }

    public virtual bool Execute()
    {
        throw new System.NotImplementedException();
    }

    public virtual void Exit()
    {
        throw new System.NotImplementedException();
    }
}
