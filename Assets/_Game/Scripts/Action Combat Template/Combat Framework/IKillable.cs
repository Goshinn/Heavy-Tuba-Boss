using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IKillable
{
    #region Properties
    string KillableName { get; }
    float MaxHealth { get; }
    float CurrentHealth { get; }
    Transform HealthbarPositionTrackerObject { get; }
    GameObject AttachedGameObject { get; }
    #endregion

    #region Public Methods
    void HandleHit(AttackInfo attackInfo);
    void Die();
    #endregion

    #region Events
    event Action HealthLost;
    event Action HealthChanged;
    event Action<IKillable> EntityDeathEvent;
    #endregion
}