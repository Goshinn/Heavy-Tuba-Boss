using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ActivateBossFightTriggerVolume : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private AIResources bossEnemyResources;
    [SerializeField] private BossFightBGM bossFightBGM;
    private bool startedBossFight;

    private void OnValidate()
    {
        if (bossEnemyResources != null && !bossEnemyResources.TryGetComponent(out IBossEnemy bossEnemy))
        {
            Debug.LogError($"The assigned EnemyBaseAI {bossEnemyResources} does not implement IBossEnemy");
            bossEnemyResources = null;
        }
    }

    private void Awake()
    {
        GetComponent<Rigidbody>().isKinematic = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!startedBossFight && other.CompareTag("Player") && bossEnemyResources.TryGetComponent(out IBossEnemy bossEnemy))
        {
            bossEnemy.CompleteBossIntro();
            bossFightBGM.StartPlayingBGM();
            startedBossFight = true;
        }
    }
}
