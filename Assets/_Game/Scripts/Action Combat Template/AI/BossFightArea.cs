using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The BossFightArea script is responsible for spawning the boss healthbar upon entering the boss fight area, with additional conditions if required. 
/// In our case, we need the boss to finish his intro cutscene before allowing the boss fight to start; hence the playerHasEnteredBossFightArea bool.
/// Logic: When player enters the boss fight area && canBeginBossFight, fade in boss hb. 
/// If player dies, respawn outside bossFightArea.
/// Upon entering again, check the necessary conditions (which in our case would be fulfilled; bossEnemyInterface.CanBeginBossFight) and decide to show boss hb or not.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BossFightArea : MonoBehaviour
{
    // Events
    public event Action PlayerEnteredBossFightArea;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerEnteredBossFightArea?.Invoke();
        }
    }
}
