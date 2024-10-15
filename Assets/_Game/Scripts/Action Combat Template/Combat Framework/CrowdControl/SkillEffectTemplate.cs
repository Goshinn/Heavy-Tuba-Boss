using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffectTemplate : ScriptableObject, ISkillEffect
{
    [SerializeField] protected private int ccDegree = 0;

    public int CCDegree { get { return ccDegree; } }

    public virtual void UseSkillEffect(AttackInfo attackInfo)
    {

    }
}