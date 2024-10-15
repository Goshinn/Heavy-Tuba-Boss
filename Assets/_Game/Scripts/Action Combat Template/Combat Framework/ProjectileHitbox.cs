using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileHitbox : AttackHitbox
{
    [Header("Projectile Settings")]
    [SerializeField] private float projectileLifetime = 3f;
    [SerializeField] private float projectileSpeed = 15f;
    private Transform projectilePoolerGO;

    protected override void Awake()
    {
        base.Awake();
        projectilePoolerGO = transform.parent;
    }

    public void ShootFromTowards(Vector3 shootOrigin, Vector3 targetDirection)
    {
        gameObject.SetActive(false);        // Stop running coroutines (if any) and trigger OnDisable logic (clearing hit enemies etc)
        gameObject.SetActive(true);         // It is impt to set active first before unparenting as our Awake() references parent.
        StartCoroutine(BeginLifeTime());
        transform.parent = null;
        transform.position = shootOrigin;
        StartCoroutine(MoveProjectileTowards(targetDirection));
    }

    private IEnumerator MoveProjectileTowards(Vector3 moveDirection)
    {
        // Move this projectile with its rb
        while (gameObject.activeSelf)
        {
            yield return null;
            transform.position += moveDirection * projectileSpeed * Time.deltaTime;
        }
    }

    private IEnumerator BeginLifeTime()
    {
        float elapsed = 0;
        while (elapsed < projectileLifetime)
        {
            yield return null;
            elapsed += Time.deltaTime;
        }

        // End of Life reached, deactivate this projectile and return it to owner (object pool GO)
        OnEndOfLifeReached();
    }

    private void OnEndOfLifeReached()
    {
        //Debug.Log("EOL of hornblast projectile reached.");

        // Repositions the projectile to its original parent
        transform.parent = projectilePoolerGO;
        transform.localPosition = Vector3.zero;

        // Deactivate gameObject
        gameObject.SetActive(false);
    }

    protected override void DealAttackEffectsToTarget(GameObject target, IKillable killable)
    {
        // Calculate damage & call HandleHit() from the target killable
        Vector3 attackDirection = Vector3.ProjectOnPlane(transform.position - target.transform.position, Vector3.up).normalized;
        AttackInfo attackInfo = new AttackInfo();
        attackInfo.SetAttackHitInfo(attackHitInfo.BaseDamage, attackHitInfo.IsBlockable, attackHitInfo.BlockResourceDrainAmount, attackHitInfo.SkillEffects);
        attackInfo.SetHitFeedBackInfo(ownerGO, target, attackHitInfo.AttackAttribute, attackDirection);
        killable.HandleHit(attackInfo);
    }
}