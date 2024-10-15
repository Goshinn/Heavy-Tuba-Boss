using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HeavyTubaGunnerWeapon))]
public class HeavyTubaGunnerSoundManager : AISoundManager
{
    [Header("Additional Setup")]
    protected HeavyTubaGunnerWeapon htgWeapon;

    [Header("Sounds")]
    [SerializeField, FMODUnity.EventRef] private string hornBlast;
    [SerializeField, FMODUnity.EventRef] private string crowdControlEntered;

    protected override void Awake()
    {
        base.Awake();
        htgWeapon = GetComponent<HeavyTubaGunnerWeapon>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        htgWeapon.FiredHornBlast += OnFiredHornBlast;
    }

    #region Callbacks
    private void OnFiredHornBlast()
    {
        weaponSoundEmitter.ChangeEvent(hornBlast);
        weaponSoundEmitter.Play();
    }
    #endregion

    protected override void OnDisable()
    {
        base.OnDisable();
        htgWeapon.FiredHornBlast -= OnFiredHornBlast;
    }
}
