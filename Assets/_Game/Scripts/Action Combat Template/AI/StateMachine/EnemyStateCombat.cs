using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Calls OnEnterCombatState(), CombatState() & OnExitCombatState() on AI that have implemented the ICombatState.
/// </summary>
public class EnemyStateCombat : IState
{
    [Header("References")]
    private ICombatState aiCombat;
    private GameObject prey;

    public EnemyStateCombat(ICombatState ai, GameObject prey)
    {
        aiCombat = ai;
        this.prey = prey;
    }

    public void Enter()
    {
        aiCombat.OnEnterCombatState(prey);
    }

    public void Execute()
    {
        aiCombat.CombatState();
    }

    public void Exit()
    {
        aiCombat.OnExitCombatState();
    }
}
