using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using System;

[RequireComponent(typeof(PlayerState), typeof(PlayerCombat), typeof(PlayerCombatResources))]
public class PlayerSoundManager : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private StudioEventEmitter weaponSoundEmitter;      // For weapon swings etc. Sounds are stopped OnCrowdControlled.
    [SerializeField] private StudioEventEmitter playerHurtSoundEmitter;  // For grunts from getting hit etc

    [Header("Sounds")]
    [SerializeField, EventRef] private string playerHurtEventRef;
    [SerializeField, EventRef] private string playerDeathEventRef;

    [Header("References")]
    private PlayerState playerState;
    private PlayerCombat playerCombat;
    private PlayerCombatResources playerCombatResources;

    private void Awake()
    {
        playerState = GetComponent<PlayerState>();
        playerCombat = GetComponent<PlayerCombat>();
        playerCombatResources = GetComponent<PlayerCombatResources>();
    }

    private void OnEnable()
    {
        // Setup callbacks
        playerState.CrowdControlled += OnCrowdControlled;
        playerCombat.SuccessfulBlock += OnSuccessfulBlock;
        playerCombat.FailedBlock += OnFailedBlock;
        playerCombatResources.EntityDeathEvent += OnPlayerDeath;
        playerCombatResources.PlayerHit += OnPlayerHit;
    }

    #region Animation Events
    public void PlayWeaponSound(PlayerWeapon.PlayerAttacks playerAttacks)
    {
        switch (playerAttacks)
        {
            case PlayerWeapon.PlayerAttacks.Light1:
                weaponSoundEmitter.ChangeEvent("event:/Player/LightAttack/LightAttack01");
                break;
            case PlayerWeapon.PlayerAttacks.Light2:
                weaponSoundEmitter.ChangeEvent("event:/Player/LightAttack/LightAttack02");
                break;
            case PlayerWeapon.PlayerAttacks.Light3:
                weaponSoundEmitter.ChangeEvent("event:/Player/LightAttack/LightAttack03");
                break;
            case PlayerWeapon.PlayerAttacks.Light4:
                weaponSoundEmitter.ChangeEvent("event:/Player/LightAttack/LightAttack04");
                break;
        }

        weaponSoundEmitter.Play();
    }
    #endregion

    #region Callbacks
    private void OnCrowdControlled()
    {
        // Cancel weapon sounds & vocal grunts
        weaponSoundEmitter.Stop();
    }

    private void OnPlayerHit(AttackInfo attackInfo, bool isAlive)
    {
        // Play hit impact sfx
        switch (attackInfo.AttackAttribute)
        {
            case AttackAttribute.Blade:
                RuntimeManager.PlayOneShot("event:/Player/Feedback sounds/PlayerHit Impact/PlayerHit_Bladed");
                break;
            case AttackAttribute.Blunt:
                RuntimeManager.PlayOneShot("event:/Player/Feedback sounds/PlayerHit Impact/PlayerHit_Blunt");
                break;
            case AttackAttribute.Projectile:
                RuntimeManager.PlayOneShot("event:/Player/Feedback sounds/PlayerHit_Impact/PlayerHit_Projectile");
                break;
        }

        // Play "ow" sound if still alive
        if (isAlive)
        {
            // Prevent overlapping vocal hurt sounds 
            playerHurtSoundEmitter.Stop();
            playerHurtSoundEmitter.ChangeEvent(playerHurtEventRef);
            playerHurtSoundEmitter.Play();
        }
    }

    private void OnSuccessfulBlock(AttackInfo attackInfo)
    {
        switch (attackInfo.AttackAttribute)
        {
            case AttackAttribute.Blade:
                break;
            case AttackAttribute.Blunt:
                RuntimeManager.PlayOneShot("event:/Player/Feedback sounds/Shield Blocks/ShieldBlock_Blunt");
                break;
            case AttackAttribute.Projectile:
                RuntimeManager.PlayOneShot("event:/Player/Feedback sounds/Shield Blocks/ShieldBlock_Projectile");
                break;
        }
    }

    public void OnFailedBlock(AttackInfo attackInfo)
    {
        // Play the appropriate block broken sound
        switch (attackInfo.AttackAttribute)
        {
            case AttackAttribute.Blade:
                break;
            case AttackAttribute.Blunt:
                RuntimeManager.PlayOneShot("event:/Player/Feedback sounds/Block Break/BlockBroken_Blunt");
                break;
            case AttackAttribute.Projectile:
                RuntimeManager.PlayOneShot("event:/Player/Feedback sounds/Block Break/BlockBroken_Projectile");
                break;
        }
    }

    private void OnPlayerDeath(IKillable obj)
    {
        // Play death grunt
        playerHurtSoundEmitter.Stop();
        playerHurtSoundEmitter.ChangeEvent(playerDeathEventRef);
        playerHurtSoundEmitter.Play();
    }
    #endregion

    private void OnDisable()
    {
        // Unregister callbacks
        playerState.CrowdControlled -= OnCrowdControlled;
        playerCombat.SuccessfulBlock -= OnSuccessfulBlock;
        playerCombat.FailedBlock -= OnFailedBlock;
        playerCombatResources.EntityDeathEvent -= OnPlayerDeath;
        playerCombatResources.PlayerHit -= OnPlayerHit;
    }
}
