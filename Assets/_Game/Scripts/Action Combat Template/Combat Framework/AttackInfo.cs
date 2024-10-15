using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AttackInfo
{
    public float Damage { get; private set; }
    public bool IsBlockable { get; private set; }
    public float BlockResourceDrain { get; private set; }
    public List<SkillEffectTemplate> SkillEffects { get; private set; }

    public GameObject AttackerObj { get; private set; }
    public GameObject HitObj { get; private set; }
    public AttackAttribute AttackAttribute { get; private set; }
    public Vector3 AttackDirection { get; private set; }

    public void SetAttackHitInfo(float damage, bool isBlockable, float blockResourceDrain, List<SkillEffectTemplate> skillEffects)
    {
        Damage = damage;
        IsBlockable = isBlockable;
        BlockResourceDrain = blockResourceDrain;
        SkillEffects = skillEffects;
    }

    // VFX orientation for spawning vfx in right orientation
    // Collision point for determining stagger dir / hardcoded direction vector
    public void SetHitFeedBackInfo(GameObject attackerObj, GameObject hitObj, AttackAttribute attackAttribute, Vector3 attackDirection)
    {
        // Hit feedback data
        //CameraShakeInstance = cameraShakeInstance
        AttackerObj = attackerObj;
        HitObj = hitObj;
        AttackAttribute = attackAttribute;
        AttackDirection = attackDirection;
    }
}