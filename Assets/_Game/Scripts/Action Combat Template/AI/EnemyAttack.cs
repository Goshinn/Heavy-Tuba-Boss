using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "EntityName_Attack00_Specifics", menuName = "Enemy/New Attack/Normal Attack")]
public class EnemyAttack : ScriptableObject
{
    [Header("Attack Settings")]
    [Tooltip("Chance of attack being selected against other attacks")]
    [SerializeField] private float attackWeight;
    [SerializeField] private int attackID;
    [SerializeField] private float coolDownDuration;

    [Header("Activation Requirements")]
    [SerializeField] private float minimumAttackDistance;
    [SerializeField] private float maximumAttackingDistance;

    [Header("Members")]
    [HideInInspector] public float calculatedWeightFrom;    // Used by AI to calculate which action to pick 
    [HideInInspector] public float calculatedWeightTo;      // Used by AI to calculate which action to pick 

    #region Accessors
    public float AttackWeight { get { return attackWeight; } }
    public int AttackID { get { return attackID; } }
    public float CoolDownDuration { get { return coolDownDuration; } }
    public float MinimumAttackDistance { get { return minimumAttackDistance; } }
    public float MaximumAttackingDistance { get { return maximumAttackingDistance; } }
    #endregion

    private void OnValidate()
    {
        if (attackWeight < 0)
        {
            Debug.LogWarning($"Having negative attack weight is not allowed. Setting {name}'s attackWeight to 0");
            attackWeight = 0;
        }
    }
}