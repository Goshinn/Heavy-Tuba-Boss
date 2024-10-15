using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Empty class to for player & ai attackhitboxes to derive from. 
/// For the purpose of keeping things generic such that we can declare variables of type AttackHitbox and have them serialized in the inspector.
/// Retain access to IAttackHitbox for custom functionality for unique hitboxes
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public abstract class AttackHitbox : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] protected AttackHitInfo attackHitInfo;
    protected GameObject ownerGO;
    protected bool ownerInitialized;

    [Header("References")]
    protected Rigidbody rb;

    [Header("Members")]
    protected List<GameObject> entitiesHitSinceActivation = new List<GameObject>();   // Keep track of the entities that have been hit by this hitbox to prevent hitting the same enemy twice

    protected virtual void OnValidate()
    {
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Collider>().isTrigger = true;
    }

    protected virtual void OnEnable()
    {
        entitiesHitSinceActivation.Clear();
    }

    protected virtual void Awake()
    {
        if (!ownerInitialized)
        {
            rb = GetComponent<Rigidbody>();
            ownerGO = GetComponentInParent<Animator>().gameObject;
            ownerInitialized = true;
        }
    }

    protected bool HasAlreadyHitEntity(GameObject target)
    {
        for (int i = 0; i < entitiesHitSinceActivation.Count; i++)
        {
            if (entitiesHitSinceActivation[i] == target)
            {
                return true;
            }
        }

        return false;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        // Get the killable component; return if killable not found
        IKillable killable = other.transform.GetComponentInParent<IKillable>();
        GameObject target = killable?.AttachedGameObject;
        if (killable == null || killable.CurrentHealth <= 0 || (target != null && target == ownerGO))
        {
            return;
        }

        // DealAttackEffects to killable if it has not been hit by this hitbox yet
        if (!HasAlreadyHitEntity(target))
        {
            // Has not hit this entity since activation yet, add to entitiesHitSinceActivation & DealAttackEffectsToTarget
            entitiesHitSinceActivation.Add(target);
            DealAttackEffectsToTarget(target, killable);
        }
    }

    protected virtual void DealAttackEffectsToTarget(GameObject target, IKillable killable)
    {
        throw new NotImplementedException();
    }

    protected virtual void OnDisable()
    {
        entitiesHitSinceActivation.Clear();
    }
}