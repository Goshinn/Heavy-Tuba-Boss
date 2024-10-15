using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyTubaGunnerAIState : EnemyAIState
{
    [Header("References")]
    private HeavyTubaGunnerAI htgAI;

    [Header("Unique States")]
    [SerializeField] private bool isTakingBreak;

    #region Accessors
    public bool IsTakingBreak { get => isTakingBreak; }
    public override bool IsCrowdControlled { get { return isTakingBreak; } }
    #endregion

    protected override void OnEnable()
    {
        base.OnEnable();
        htgAI = GetComponent<HeavyTubaGunnerAI>();

        // Setup callbacks
        htgAI.DamageBreakEntered += OnDamageBreakEntered;
        htgAI.DamageBreakExit += OnDamageBreakExit;
    }

    private void OnDamageBreakEntered()
    {
        isTakingBreak = true;
    }

    private void OnDamageBreakExit()
    {
        isTakingBreak = false;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        // Unregister callbacks
        htgAI.DamageBreakEntered -= OnDamageBreakEntered;
        htgAI.DamageBreakExit -= OnDamageBreakExit;
    }
}
