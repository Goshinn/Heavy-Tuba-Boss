using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EntityName_Attack00_Specifics", menuName = "Enemy/New Attack/Sequenced Attack/New Sequenced Attack")]
public class SequencedEnemyAttack : EnemyAttack
{
    [Header("Attack Sequencing")]
    [SerializeField] private SequencedAttackAdvanceCondition advanceAttackSequenceConditon;

    public SequencedAttackAdvanceCondition AdvanceCondition { get { return advanceAttackSequenceConditon; } }
}
