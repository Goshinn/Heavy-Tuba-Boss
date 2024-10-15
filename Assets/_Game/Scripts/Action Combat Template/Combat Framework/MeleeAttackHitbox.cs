using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class MeleeAttackHitbox : AttackHitbox
{
    [Header("Additional Setup")]
    [SerializeField] protected float hitboxActiveDuration = 0.1f;

    [Header("Attack Setup")]
    protected Vector3 attackDirection;                      // Used to determine orientation of hit VFX, calculated in the instant when the attack lands on entities

    //[Header("Hit FeedBack")]

    [Header("Debug")]
    [SerializeField] protected bool debugHitboxes;

    protected override void OnValidate()
    {
        base.OnValidate();

        if (hitboxActiveDuration <= 0)
        {
            Debug.LogWarning("Please set a positive value for hitboxActiveDuration");
            hitboxActiveDuration = 0.1f;
        }
    }

    // Can be overriden for player to handle camera shake feedback on successful hit etc
    public virtual void ActivateHitbox()
    {
        //gameObject.SetActive(false);    // Trigger OnDisable logic
        gameObject.SetActive(true);       // Enable hitbox

        // Start corout to deactivate hitbox
        StartCoroutine(DeactivateHitbox());
        if (debugHitboxes) Debug.Log($"{name} activated");
    }

    protected override void DealAttackEffectsToTarget(GameObject target, IKillable killable)
    {
        //if (attackDirectionHelper != null)
        //{
        //    attackDirection = attackDirectionHelper.GetAttackDirection();
        //}

        if (debugHitboxes)
        {
            Debug.DrawRay(target.transform.position + Vector3.up, attackDirection, Color.red, 2f);
        }

        // Publish evnt for Camera Shake hit feedback
        //if (hitFeedBackCameraShake != null && !publishedCameraShakeHitFeedBack)
        //{
        //    GetComponent<CameraEffectPublisher>().PublishCameraShake(hitFeedBackCameraShake);
        //    publishedCameraShakeHitFeedBack = true;
        //}

        // Calculate & Inflict damage
        float damage = CalculateDamage();
        AttackInfo attackInfo = new AttackInfo();
        attackInfo.SetAttackHitInfo(damage, attackHitInfo.IsBlockable, attackHitInfo.BlockResourceDrainAmount, attackHitInfo.SkillEffects);
        attackInfo.SetHitFeedBackInfo(ownerGO, target, attackHitInfo.AttackAttribute, attackDirection);
        if (debugHitboxes) Debug.Log($"Sending AttackInfo to {target.name} who currently has {killable.CurrentHealth} HP.");

        killable.HandleHit(attackInfo);

        if (debugHitboxes) Debug.Log($"target HP {killable.CurrentHealth}");
    }

    protected virtual IEnumerator DeactivateHitbox()
    {
        yield return new WaitForSeconds(hitboxActiveDuration);
        gameObject.SetActive(false);
    }

    // For when player stats can affect damage
    private float CalculateDamage()
    {
        return attackHitInfo.BaseDamage;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw attackDirection
        if (debugHitboxes)
        {
            Gizmos.color = Color.yellow;
            Debug.DrawRay(GetComponent<Collider>().bounds.center, attackDirection, Color.yellow);
        }
    }
}