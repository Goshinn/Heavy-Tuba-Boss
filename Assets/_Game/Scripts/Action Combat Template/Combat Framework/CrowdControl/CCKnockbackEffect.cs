using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillEffect_Knockback", menuName = "New Attack/SkillEffect/Knockback")]
public class CCKnockbackEffect : SkillEffectTemplate
{
    public override void UseSkillEffect(AttackInfo attackInfo)
    {
        IKnockback knockbackable = attackInfo.HitObj.GetComponent<IKnockback>();
        if (knockbackable != null)
        {
            knockbackable.AttemptKnockback(attackInfo, ccDegree);
        }
    }
}