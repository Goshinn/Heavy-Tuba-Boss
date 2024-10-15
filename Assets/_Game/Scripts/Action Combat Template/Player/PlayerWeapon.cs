using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [Header("References")]
    private Animator anim;
    private PlayerState playerState;       // To check if player is dodging/blocking to prevent spawning weapon swing vfx due to animator transitions triggering weapon vfx while blending to dodge/block

    [Header("Setup")]
    [SerializeField] private MeleeAttackHitbox hitboxLight1;
    [SerializeField] private MeleeAttackHitbox hitboxLight2;
    [SerializeField] private MeleeAttackHitbox hitboxLight3;
    [SerializeField] private MeleeAttackHitbox hitboxLight4;

    [Header("Members")]
    private Dictionary<PlayerAttacks, MeleeAttackHitbox> attackDictionary = new Dictionary<PlayerAttacks, MeleeAttackHitbox>();

    [Header("Lazy Developer")]
    public bool autoPopulateAttackHitboxes;
    public bool autoPopulateReferences;

    public enum PlayerAttacks
    {
        // Basic Attack (1+)
        Light1 = 1 << 1,
        Light2 = 1 << 2,
        Light3 = 1 << 3,
        Light4 = 1 << 4,
    }

    private void OnValidate()
    {
        if (autoPopulateAttackHitboxes)
        {
            hitboxLight1 = GetComponentsInChildren<MeleeAttackHitbox>(true)[0];
            hitboxLight2 = GetComponentsInChildren<MeleeAttackHitbox>(true)[1];
            hitboxLight3 = GetComponentsInChildren<MeleeAttackHitbox>(true)[2];
            hitboxLight4 = GetComponentsInChildren<MeleeAttackHitbox>(true)[3];
        }
    }

    protected void Awake()
    {
        // Hook up references
        anim = GetComponentInParent<Animator>();
        playerState = GetComponent<PlayerState>();

        // Initialize melee attacks dict
        InitializeAttackDictionary();    
    }

    // Populate greatSwordHitBoxDictionary
    private void InitializeAttackDictionary()
    {
        attackDictionary.Add(PlayerAttacks.Light1, hitboxLight1);
        attackDictionary.Add(PlayerAttacks.Light2, hitboxLight2);
        attackDictionary.Add(PlayerAttacks.Light3, hitboxLight3);
        attackDictionary.Add(PlayerAttacks.Light4, hitboxLight4);
    }

    #region Animation Events
    public void ActivateHitbox(PlayerAttacks hitbox)
    {
        // Do not activate hitboxes if player has been cc'ed etc
        if (!playerState.IsCrowdControlled && playerState.IsAlive)
        {
            attackDictionary[hitbox].ActivateHitbox();
        }
    }
    #endregion
}
