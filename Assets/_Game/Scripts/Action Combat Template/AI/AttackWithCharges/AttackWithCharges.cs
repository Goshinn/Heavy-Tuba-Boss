using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EntityName_Attack00_Specifics", menuName = "Enemy/New Attack/Attack with Charges")]
public class AttackWithCharges : EnemyAttack
{
    [Header("Charge Settings")]
    [SerializeField] private int maxStoredCharges = 1;
    [SerializeField] private float chargeRegenerationDuration = 2f;

    #region Accessors
    public int MaxStoredCharges { get => maxStoredCharges; }
    public float ChargeRegenerationDuration { get => chargeRegenerationDuration; }
    #endregion
}
