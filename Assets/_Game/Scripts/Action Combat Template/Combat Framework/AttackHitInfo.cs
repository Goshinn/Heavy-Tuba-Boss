using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EntityName_Specifics", menuName = "New Attack/AttackHitInfo")]
public class AttackHitInfo : ScriptableObject
{
    [Header("Settings")]
    [SerializeField] private float baseDamage;
    [SerializeField] private bool isBlockable = true;
    [SerializeField] private float blockResourceDrainAmount = 0;
    [SerializeField] private List<SkillEffectTemplate> skillEffects = new List<SkillEffectTemplate>();
    [SerializeField] private AttackAttribute attackAttribute;

    #region Accessors
    public float BaseDamage { get { return baseDamage; } }
    public bool IsBlockable { get { return isBlockable; } }
    public float BlockResourceDrainAmount { get => blockResourceDrainAmount; }
    public List<SkillEffectTemplate> SkillEffects { get { return skillEffects; } }
    public AttackAttribute AttackAttribute { get { return attackAttribute; } }
    #endregion
}
