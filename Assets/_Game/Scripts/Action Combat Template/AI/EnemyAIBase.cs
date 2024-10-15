using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;

/// <summary>
/// All enemies should be able to store a reference to aggroedTarget.
/// </summary>
[RequireComponent(typeof(NavMeshAgent), typeof(EnemyDespawner), typeof(AIResources))]
//[RequireComponent(typeof(EnemyAIState))] // Commented because AI could have unique AI states
public abstract class EnemyAIBase : MonoBehaviour
{
    [Header("References")]
    protected Animator anim;
    protected NavMeshAgent ai;
    protected AIResources aiResources;
    protected EnemyAIState aiState;

    [Header("Setup")]
    [SerializeField] private EnemyType enemyType;
    [SerializeField] private Transform lockOnObj;

    [Header("Player Detection Settings")]
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] protected float detectionRange = 10f;
    [Tooltip("Objects in this layer can obstruct the enemy's field of view and prevent enemy from successfully detecting the player, should include playerLayer & environment")]
    [SerializeField] protected LayerMask obstructionLayer;

    [Header("Combat - Members")]
    [SerializeField] protected private GameObject aggroedPlayer;
    protected PlayerState aggroedPlayerState;
    protected PlayerCombat aggroedPlayerCombat;
    protected StateMachine stateMachine = new StateMachine();

    // Properties
    public Vector3 PointOfOrigin { get; private set; }

    [Header("Events")]
    public Action StartedAttack;                    // For Player_AttackIndicators to indicate enemy attack
    public Action EndedAttack;                      // To set vars

    #region Accessors
    public EnemyType EnemyType { get => enemyType; }
    public GameObject AggroedPlayer { get { return aggroedPlayer; } }
    public Transform LockOnObj { get => lockOnObj; }
    #endregion

    protected virtual void Awake()
    {
        ai = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        aiState = GetComponent<EnemyAIState>();
        aiResources = GetComponent<AIResources>();

        if (aiState == null)
        {
            Debug.LogError($"AIState missing from {name}");
        }
    }

    protected virtual void OnEnable()
    {
        // Subscribe to events
        aiResources.EntityDeathEvent += OnEntityDeathEvent;
    }

    protected virtual void Start()
    {
        // Init vars
        PointOfOrigin = transform.position;
    }

    public virtual void CheckForPlayer()
    {
        Collider[] playerCols = Physics.OverlapSphere(transform.position, detectionRange, playerLayer);

        // Colliders in playerLayer detected, pick out the GO that is the player & enter combat...
        for (int i = 0; i < playerCols.Length; i++)
        {
            IKillable killable = playerCols[i].gameObject.GetComponent<IKillable>();
            if (killable != null && killable.CurrentHealth > 0)
            {
                // Check for clear line of sight to player
                GameObject target = killable.AttachedGameObject;
                RaycastHit hit;
                Vector3 directionToPlayer = Vector3.ProjectOnPlane(target.transform.position - transform.position, Vector3.up).normalized;
                if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out hit, detectionRange, obstructionLayer))
                {
                    if (hit.collider.gameObject == target)
                    {
                        GetComponent<ICombatState>().EnterCombatState(target);
                        break;
                    }
                }
                else
                {
                    //Debug.Log("obstructed");
                    Debug.DrawRay(transform.position + Vector3.up, directionToPlayer * detectionRange, Color.red);
                }
            }
        }
    }

    #region Callbacks
    private void OnEntityDeathEvent(IKillable killable)
    {
        // Switch to dead state
        stateMachine.ChangeState(new EnemyStateDead());

        // Stop all movement coroutines?
        StopAllCoroutines();

        // Publish necessary evnts

        // Call necessary funcs  

        // Unsubscribe from evnts etc
        UnsubscribeFromEvents();

        // Prevent AI from moving but allow its gravity to do its work
        //ai.isStopped = true;

        // Deactivate scripts that are safe to disable
        aiResources.enabled = false;
        aiState.enabled = false;
    }
    #endregion

    public void RotateToAggroedTargetWithMaxDelta(float maxAngleDelta)
    {
        if (aggroedPlayer != null)
        {
            Vector3 planarDirectionToTarget = Vector3.ProjectOnPlane(aggroedPlayer.transform.position - transform.position, Vector3.up).normalized;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(planarDirectionToTarget), maxAngleDelta);
        }
    }

    protected virtual void UnsubscribeFromEvents()
    {
        aiResources.EntityDeathEvent -= OnEntityDeathEvent;
    }
}
