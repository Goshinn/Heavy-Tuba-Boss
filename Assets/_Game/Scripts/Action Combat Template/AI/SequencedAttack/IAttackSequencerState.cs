using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttackSequencerState
{
    void Enter(EnemyAIBase enemyAI);
    bool Execute();
    void Exit();
}
