using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using System;

public class AISoundManager : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] protected StudioEventEmitter weaponSoundEmitter;
    [SerializeField] protected StudioEventEmitter vocalSoundEmitter;
    protected AIResources aiResources;

    [Header("Impact SFX")]
    [SerializeField, EventRef] protected string bladeAttributeHitSFX;

    [Header("Death")]
    [SerializeField, EventRef] protected string deathSFX;

    protected virtual void Awake()
    {
        aiResources = GetComponent<AIResources>();
    }

    protected virtual void OnEnable()
    {
        // Setup callbacks
        aiResources.HitImpact += OnHitImpact;
        aiResources.EntityDeathEvent += OnEntityDeath;
    }

    private void PlayVocalSound(string eventRef)
    {
        vocalSoundEmitter.Stop();
        vocalSoundEmitter.ChangeEvent(eventRef);
        vocalSoundEmitter.Play();
    }

    #region Animation Events
    public void PlayWeaponSound(string eventRef)
    {
        weaponSoundEmitter.ChangeEvent(eventRef);
        weaponSoundEmitter.Play();
    }

    public void PlayVocalSoundIfAlive(string eventRef)
    {
        if (aiResources.CurrentHealth > 0)
        {
            PlayVocalSound(eventRef);
        }
    }

    public void PlayOneShot(string eventRef)
    {
        RuntimeManager.PlayOneShotAttached(eventRef, gameObject);
    }
    #endregion

    #region Callbacks
    private void OnHitImpact(AttackInfo attackInfo)
    {
        // Handle audio & visual feedback
        switch (attackInfo.AttackAttribute)
        {
            case AttackAttribute.Blade:
                // Play SFX
                RuntimeManager.PlayOneShotAttached(bladeAttributeHitSFX, gameObject);
                break;
        }
    }

    private void OnEntityDeath(IKillable obj)
    {
        PlayVocalSound(deathSFX);
    }
    #endregion

    protected virtual void OnDisable()
    {
        aiResources.HitImpact -= OnHitImpact;
    }
}
