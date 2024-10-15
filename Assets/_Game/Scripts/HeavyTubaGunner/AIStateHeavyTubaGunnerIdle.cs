using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// State that HTG defaults to in Start().
/// Until htgAI.canBeginBossFight is true, it will keep idling. Otherwise, it will check the vicinity for player and subsequently enter boss fight if the player is found.
/// </summary>
public class AIStateHeavyTubaGunnerIdle : IState
{
    private HeavyTubaGunnerAI htgAI;

    public AIStateHeavyTubaGunnerIdle(HeavyTubaGunnerAI htgAI)
    {
        this.htgAI = htgAI;
    }

    public void Enter()
    {
        
    }

    public void Execute()
    {
        if (htgAI.CanBeginBossFight)
        {
            htgAI.CheckForPlayer();
        }
    }

    public void Exit()
    {

    }
}
