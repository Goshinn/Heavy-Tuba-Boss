using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "SkillEffect_Flinch", menuName = "New Attack/SkillEffect/Flinch")]
public class CCFlinchEffect : SkillEffectTemplate
{
    public override void UseSkillEffect(AttackInfo attackInfo)
    {
        IFlinch flinchable = attackInfo.HitObj.GetComponent<IFlinch>();
        if (flinchable != null)
        {
            flinchable.AttemptFlinch(attackInfo, ccDegree);
        }
    }
}
