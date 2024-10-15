using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class PlayerAttackHitbox : MeleeAttackHitbox
{
    //[Header("Hit FeedBack")]
    //[Tooltip("Publish a camera shake on successful hit. Leave empty if attack already publishes a camera shake via anim evnt")]
    //[SerializeField] private CameraEffects_CameraShakeInstance hitFeedBackCameraShake;
    //protected bool publishedCameraShakeHitFeedBack;

    protected override void DealAttackEffectsToTarget(GameObject target, IKillable killable)
    {
        base.DealAttackEffectsToTarget(target, killable);

        if (debugHitboxes)
        {
            Debug.DrawRay(target.transform.position + Vector3.up, attackDirection, Color.red, 2f);
        }
    }
}