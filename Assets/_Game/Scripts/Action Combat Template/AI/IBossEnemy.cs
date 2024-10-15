using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IBossEnemy
{
    // Properties
    bool HasFinishedIntro { get; }          // Condition required for boss fight to begin
    bool PlayerIsInBossFightArea { get; }   // Condition required for boss fight to begin
    bool CanBeginBossFight { get; }         // Allows boss to run AI logic in statemachine to kick off boss fight
    GameObject AttachedGameObject { get; }

    // Events
    event Action BossFightBeginConditionFulfilled;

    // Methods
    void CompleteBossIntro();
    void StartBossFight();
}
