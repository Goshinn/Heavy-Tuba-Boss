using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHUDController : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private BossEnemyHUD bossEnemyHUDPrefab;
    [SerializeField] private MobEnemyHUD mobEnemyHUDPrefab;

    [Header("References")]
    [SerializeField] private Transform healthbarHolder;
    private Camera cam;

    [Header("Members")]
    private Dictionary<IKillable, EnemyHUD> healthBars = new Dictionary<IKillable, EnemyHUD>();

    // Events
    public static Action<IKillable> SpawnHealthBar;     // invoked by the entities responsible for making themselves known to the player.
    public static Action<IKillable> DespawnHealthBar;

    private void Awake()
    {
        // Hook up references
        cam = GetComponentInParent<CanvasManager>().Cam;
    }

    private void OnEnable()
    {
        // Register and deactivate On Enable/OnDisable?
        SpawnHealthBar += OnSpawnHealthBar;
        DespawnHealthBar += OnDespawnHealthBar;
    }

    private void OnSpawnHealthBar(IKillable killable)
    {
        if (!healthBars.ContainsKey(killable))
        {
            EnemyAIBase enemyAI = killable.AttachedGameObject.GetComponent<EnemyAIBase>();    
            if (enemyAI != null)
            {
                // Determine which healthbar variant to spawn based on enemy type
                EnemyHUD healthBar;
                switch (enemyAI.EnemyType)
                {
                    case EnemyType.Boss:
                        healthBar = Instantiate(bossEnemyHUDPrefab, healthbarHolder);
                        break;
                    case EnemyType.Mob:
                        healthBar = Instantiate(mobEnemyHUDPrefab, healthbarHolder);
                        break;
                    default:
                        Debug.LogError("Unknown enemyType, defaulting to instantiating mobEnemyHUDPrefab...");
                        healthBar = Instantiate(mobEnemyHUDPrefab, healthbarHolder);
                        break;
                }

                // Initialize, setup callbacks & add healthbar to healthbars dictionary
                healthBar.Initialize(killable, cam);
                killable.EntityDeathEvent += DespawnHealthBar;
                healthBars.Add(killable, healthBar);
            }
            else
            {
                Debug.LogError("Unknown killable. Its not an enemy. Is it a destructible object, a passive killable mob or something?");
                return;
            }
        }
    }

    public void OnDespawnHealthBar(IKillable killable)
    {
        if (healthBars.ContainsKey(killable))
        {
            healthBars[killable].UnregisterFromEvents();
            killable.EntityDeathEvent -= DespawnHealthBar;
            Destroy(healthBars[killable].gameObject);
            healthBars.Remove(killable);
        }
    }

    private void OnDisable()
    {
        SpawnHealthBar -= OnSpawnHealthBar;
        DespawnHealthBar -= OnDespawnHealthBar;
    }
}
