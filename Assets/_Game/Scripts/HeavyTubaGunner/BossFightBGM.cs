using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using System;

public class BossFightBGM : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private AIResources enemy;
    [SerializeField, EventRef] private string bossFightBGM;

    [Header("Members")]
    private FMOD.Studio.EventInstance bgmInstance;
    private FMOD.Studio.PARAMETER_DESCRIPTION bossIsDeadPD;

    private void OnEnable()
    {
        enemy.EntityDeathEvent += OnEntityDeathEvent;
    }

    private void Start()
    {
        // Assign variables
        bgmInstance = RuntimeManager.CreateInstance(bossFightBGM);
        RuntimeManager.GetEventDescription(bossFightBGM).getParameterDescriptionByName("BossIsDead", out bossIsDeadPD);
    }

    public void StartPlayingBGM()
    {
        bgmInstance.start();
    }

    private void OnEntityDeathEvent(IKillable killable)
    {
        // End the boss fight BGM
        bgmInstance.setParameterByID(bossIsDeadPD.id, 1);
        bgmInstance.release(); // Necessary as this FMOD.Studio.EventInstance is one that we manually created
    }

    private void OnDisable()
    {
        enemy.EntityDeathEvent -= OnEntityDeathEvent;
    }
}
