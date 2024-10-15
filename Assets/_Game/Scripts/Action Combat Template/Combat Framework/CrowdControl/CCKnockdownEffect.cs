using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillEffect_Knockdown", menuName = "New Attack/SkillEffect/Knockdown")]
public class CCKnockdownEffect : SkillEffectTemplate
{
    public override void UseSkillEffect(AttackInfo attackInfo)
    {
        IKnockdown knockdownable = attackInfo.HitObj.GetComponent<IKnockdown>();
        if (knockdownable != null)
        {
            knockdownable.AttemptKnockdown(attackInfo, ccDegree);
        }
    }
}