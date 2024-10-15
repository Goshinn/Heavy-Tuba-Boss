using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;
using System.Linq;

[RequireComponent(typeof(HeavyTubaGunnerAIState))]
public class HeavyTubaGunnerAI : EnemyAIBase, IBossEnemy, ICombatState
{
    [Header("Additional Setup")]
    [SerializeField] private AttackChargeHandler attackChargesHandler;
    private HeavyTubaGunnerAIState htgAIState;

    [Header("Statemachine Settings")]
    [SerializeField] private float stateMachineTickInterval = 0.1f;
    private float timeSinceLastStateMachineTick;

    [Header("Animation Blending")]
    [SerializeField] private float locomotionSmoothRate = 0.05f;    // smooth value for Mathf.SmoothDamp when smoothing animation parameters for locomotion blendtree
    private float smoothVelRight;                                   // ref for Mathf.SmoothDamp
    private float smoothVelForward;                                 // ref for Mathf.SmoothDamp
    [SerializeField] private float maxHeadLookAtWeight = 0.6f;      
    [SerializeField] private float headLookAtWeightLerpSpeed = 0.6f;
    private float headLookAtWeight;

    [Header("Combat Settings")]
    [SerializeField] private List<EnemyAttack> attackList = new List<EnemyAttack>();
    [SerializeField] private float attackingAngleTolerance = 90f;
    [SerializeField] private float dynamicDistanceCoverStoppingDistance = 1f;
    private List<EnemyAttack> availableActionsList = new List<EnemyAttack>();
    private float actionListProbabilityWeightMaximum;
    private Dictionary<EnemyAttack, float> attackCoolDownDictionary;                // To be created at runtime based off of attackList
    private AttackSequencingStateMachine attackSequencer = new AttackSequencingStateMachine();
    private int attackSequence = 0;

    [Header("Damage Break Settings")]
    [Range(0, 1)] public List<float> damageBreakHealthPercentages = new List<float>();
    [SerializeField] private float damageBreakDuration = 5f;
    private Stack<float> activeDamageBreakHealthPercentages = new Stack<float>();
    private Coroutine recoverFromDamageBreakCorout;

    [Header("Behavioural Settings")]
    [SerializeField] private float combatMoveSpeed = 3.5f;
    [SerializeField] private float desiredCombatIdleDistance = 3.2f;
    [SerializeField] private float repositionDistance = 4f;
    private bool reachedDesiredCombatIdleDistance;

    [Header("Combat - Members")]
    [SerializeField] private float angleToTarget;
    [SerializeField] private float planarDistanceToAggroedTarget;
    [SerializeField] private bool allowAttacking;

    [Header("Boss Enemy Setup")]
    [SerializeField] private BossFightArea bossFightArea;
    private bool hasFinishedIntro;
    private bool playerIsInBossFightArea;

    // Events
    public event Action BossFightBeginConditionFulfilled;
    public event Action DamageBreakEntered;
    public event Action DamageBreakExit;

    #region Accessors
    public bool HasFinishedIntro 
    {
        get { return hasFinishedIntro; }
        private set
        {
            hasFinishedIntro = value;
            if (value)
            {
                BossFightBeginConditionFulfilled?.Invoke();
            }
        }
    }
    public bool PlayerIsInBossFightArea
    {
        get { return playerIsInBossFightArea; }
        private set
        {
            playerIsInBossFightArea = value;
            if (value)
            {
                BossFightBeginConditionFulfilled?.Invoke();
            }
        }
    }
    public bool CanBeginBossFight { get; private set; }         // Allows boss to run AI logic in statemachine to kick off boss fight
    public GameObject AttachedGameObject { get => gameObject; }
    #endregion

    [Header("Debug")]
    public bool debugDetectionRange;

    private void OnValidate()
    {
        float highestHealthPercentage = 1;
        for (int i = 0; i < damageBreakHealthPercentages.Count; i++)
        {
            if (damageBreakHealthPercentages[i] < highestHealthPercentage)
            {
                highestHealthPercentage = damageBreakHealthPercentages[i];
                continue;
            }
            else
            {
                Debug.Log("You cannot have a damage break with a higher health percentage than the previous damage break. Set it lower pls.");
                damageBreakHealthPercentages[i] = highestHealthPercentage - 0.001f;
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();

        // Hook up references from additional setup
        attackChargesHandler = GetComponent<AttackChargeHandler>();
        htgAIState = GetComponent<HeavyTubaGunnerAIState>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        EndedAttack += OnEndedAttack;
        attackSequencer.AdvanceAttackSequence += OnAdvanceAttackSequence;
        aiResources.HealthLost += OnHealthLost;
        bossFightArea.PlayerEnteredBossFightArea += OnPlayerEnteredBossFightArea;
        BossFightBeginConditionFulfilled += OnBossFightBeginConditionFulfilled;
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.ChangeState(new AIStateHeavyTubaGunnerIdle(this));
        attackSequencer.InitializeAttackSequencer(this);

        // Initialize activeDamageBreakHealthPercentages stack
        for (int i = damageBreakHealthPercentages.Count - 1; i >= 0; i--)
        {
            activeDamageBreakHealthPercentages.Push(damageBreakHealthPercentages[i]);
        }
    }

    private void Update()
    {
        // Run state machine in specified tick interval
        if (timeSinceLastStateMachineTick >= stateMachineTickInterval)
        {
            stateMachine.ExecuteUpdate();
            timeSinceLastStateMachineTick = 0;
        }
        else
        {
            timeSinceLastStateMachineTick += Time.deltaTime;
        }

        attackSequencer.ExecuteUpdate();    // Will run active state if there is an active state
        AnimateEnemy();
    }

    #region RootMotion
    private void OnAnimatorMove()
    {
        // Attacks
        if (aiState.IsAttacking)
        {
            // Calculate next position & rotation
            Quaternion nextRotation = CalculateNextRotationToAggroedPlayer(anim.GetFloat("AttackMaxRotationSpeed"));
            Vector3 deltaPosition = ProcessRootMotionForAttacks();
            CustomFinalizeMovement(deltaPosition, nextRotation);
        }
        else if (aiResources.CurrentHealth > 0)
        {
            Vector3 deltaPosition = anim.deltaPosition;
            CustomFinalizeMovement(deltaPosition, transform.rotation);
        }
    }

    /// <summary>
    /// Intended use: Moves the AI with gravity, given rootmotion deltaPosition
    /// </summary>
    /// <param name="deltaPosition"></param>
    /// <param name="nextRotation"></param>
    private void CustomFinalizeMovement(Vector3 deltaPosition, Quaternion nextRotation)
    {
        ai.Move(deltaPosition);
        transform.rotation = nextRotation;
    }

    private Quaternion CalculateNextRotationToAggroedPlayer(float rotationSpeed)
    {
        if (aggroedPlayer != null)
        {
            Vector3 planarDirectionToAggroedPlayer = Vector3.ProjectOnPlane(aggroedPlayer.transform.position - transform.position, Vector3.up).normalized;
            Quaternion rotationToPlayer = Quaternion.LookRotation(Vector3.ProjectOnPlane(planarDirectionToAggroedPlayer, Vector3.up));
            return Quaternion.RotateTowards(transform.rotation, rotationToPlayer, rotationSpeed * Time.deltaTime);
        }
        else
        {
            return transform.rotation;
        }
    }

    // Calcuate nextPosition; only move if enemy is still far from player, else stop moving when close enough
    private Vector3 ProcessRootMotionForAttacks()
    {
        Vector3 deltaPosition = Vector3.zero;
        if (planarDistanceToAggroedTarget > dynamicDistanceCoverStoppingDistance || aggroedPlayer == null)
        {
            return deltaPosition += anim.deltaPosition;
        }
        else
        {
            Vector3 planarDirectionToAggroedPlayer = Vector3.ProjectOnPlane(aggroedPlayer.transform.position - transform.position, Vector3.up).normalized;
            return Vector3.ProjectOnPlane(deltaPosition, planarDirectionToAggroedPlayer);
        }
    }
    #endregion

    private void OnAnimatorIK()
    {
        // Look at player via modifying rig
        if (aggroedPlayer == null)
        {
            return;
        }

        float desiredHeadLookAtWeight = aiResources.CurrentHealth > 0 ? maxHeadLookAtWeight : 0;
        headLookAtWeight = Mathf.Lerp(headLookAtWeight, desiredHeadLookAtWeight, headLookAtWeightLerpSpeed * Time.deltaTime);
        anim.SetLookAtPosition(aggroedPlayer.transform.position + Vector3.up * 1.7f);
        anim.SetLookAtWeight(headLookAtWeight, 0, 0.75f, 0, 0.75f);
    }

    private void AnimateEnemy()
    {
        // Eliminate jitter due to normalized near zero velocities
        float right = 0;
        float forward = 0;

        // Calculate anim params
        if (ai.velocity.magnitude > 0.1f && !ai.isStopped)
        {
            int sign = Vector3.Dot(ai.velocity.normalized, transform.right) > 0 ? 1 : -1;
            right = Mathf.Clamp(Vector3.Project(ai.velocity.normalized, transform.right).magnitude, -1, 1f) * sign;
            sign = Vector3.Dot(ai.velocity.normalized, transform.forward) > 0 ? 1 : -1;
            forward = Mathf.Clamp(Vector3.Project(ai.velocity.normalized, transform.forward).magnitude, -1, 1f) * sign;
        }

        // Smooth anim params
        float smoothedRight = Mathf.SmoothDamp(anim.GetFloat("Right"), right, ref smoothVelRight, locomotionSmoothRate);
        float smoothedForward = Mathf.SmoothDamp(anim.GetFloat("Forward"), forward, ref smoothVelForward, locomotionSmoothRate);

        // Set anim params
        //anim.SetFloat("Speed", ai.velocity.magnitude);
        anim.SetFloat("Forward", smoothedForward);
        anim.SetFloat("Right", smoothedRight);
    }

    public void SetAICanMove()
    {
        ai.isStopped = aiState.IsAttacking || reachedDesiredCombatIdleDistance;

        //if (!ai.isStopped)
        //{
        //    Debug.Log($"({aiState.IsAttacking} || {reachedDesiredCombatIdleDistance}");
        //}
    }

    /// <summary>
    /// Updates planarDistanceToAggroedTarget
    /// </summary>
    public void UpdateDistanceToTarget()
    {
        planarDistanceToAggroedTarget = Vector3.ProjectOnPlane(aggroedPlayer.transform.position - transform.position, Vector3.up).magnitude;
    }

    /// <summary>
    /// Looks towards the target direction while navigating to the aggroedTarget
    /// </summary>
    private void MoveToAggroedTarget()
    {
        ai.destination = aggroedPlayer.transform.position;
        FaceTarget();
    }

    public void SetAngleToTarget()
    {
        if (aggroedPlayer != null)
        {
            Vector3 directionToTarget = Vector3.ProjectOnPlane(aggroedPlayer.transform.position - transform.position, Vector3.up).normalized;
            angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
        }
        else
        {
            angleToTarget = 0;
        }
    }

    private void FaceTarget()
    {
        //Debug.Log("Facing target");
        Quaternion desiredRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(aggroedPlayer.transform.position - transform.position, Vector3.up), Vector3.up);
        transform.rotation = desiredRotation;
    }

    // Called on CrowdControlled, when attacking & when exiting combat state
    private void StopAllMovement()
    {
        ai.ResetPath();
    }

    /// <summary>
    /// Calculates & sets action.calculatedWeightFrom, action.calculatedWeightTo and actionListProbabilityWeightMaximum.
    /// Run this everytime after updating availableAttacks list before rolling an action to take.
    /// actionListProbabilityWeightMaximum is used as the random range for picking out actions with calculatedWeightFrom & actionListProbabilityWeightMaximum
    /// </summary>
    private void CalculateAvailableActionListWeights()
    {
        if (availableActionsList.Count > 0)
        {
            actionListProbabilityWeightMaximum = 0f; //The current weight of all items in this table. 

            // Sets the weight ranges of the selected items.
            foreach (EnemyAttack enemyAttack in availableActionsList)
            {
                enemyAttack.calculatedWeightFrom = actionListProbabilityWeightMaximum; //Sets the item's pick range from (example) 0 if there was no previous item.
                actionListProbabilityWeightMaximum += enemyAttack.AttackWeight;        //Adds the item's weight to the total weight of all items in the list.
                enemyAttack.calculatedWeightTo = actionListProbabilityWeightMaximum;   //Sets the item's pick range to (example) 60 if there was no previous item. 
            }
        }
    }

    private void InitializeAttackCoolDownDictionary()
    {
        attackCoolDownDictionary = new Dictionary<EnemyAttack, float>();
        foreach (EnemyAttack attack in attackList)
        {
            if (attack == null)
            {
                Debug.Log($"there is a null attack in attackList of {name}");
                continue;
            }

            attackCoolDownDictionary.Add(attack, 0);
        }
    }

    #region Combat State
    // Called internally within this AI script
    public void EnterCombatState(GameObject target)
    {
        //Debug.Log($"entering combat with {target.name}");
        stateMachine.ChangeState(new EnemyStateCombat(this, target));
    }

    // Called via stateMachine
    public void OnEnterCombatState(GameObject targetedPlayer)
    {
        // Set vars
        this.aggroedPlayer = targetedPlayer;
        aggroedPlayerState = targetedPlayer.GetComponent<PlayerState>();
        aggroedPlayerCombat = targetedPlayer.GetComponent<PlayerCombat>();
        SetAngleToTarget();
        InitializeAttackCoolDownDictionary();
        allowAttacking = true;

        // Initialize navmeshagent component
        ai.speed = combatMoveSpeed;
        ai.updateRotation = false;
        ai.autoBraking = false;

        // Subscribe to events
        aggroedPlayer.GetComponent<PlayerCombatResources>().EntityDeathEvent += OnAggroedTargetDeathEvent;

        // Set anim params
        //anim.SetBool("InCombat", true);
    }

    public void CombatState()
    {
        UpdateDistanceToTarget();
        SetAICanMove();
        AttackerBehaviour();
    }

    public void OnExitCombatState()
    {
        // Unsubcribe from evnts
        aggroedPlayer.GetComponent<PlayerCombatResources>().EntityDeathEvent -= OnAggroedTargetDeathEvent;

        // Stop all movement
        StopAllMovement();

        // Reset vars
        this.aggroedPlayer = null;
        aggroedPlayerState = null;
        aggroedPlayerCombat = null;

        // Set anim params
        //anim.SetBool("InCombat", false);
    }
    #endregion

    #region CombatState - Attacker Behaviour
    public void AttackerBehaviour()
    {
        SetAngleToTarget();

        // Populate availableAttacks list with attacks that are within attacking range
        availableActionsList.Clear();
        if (allowAttacking)
        {
            DetermineAvailableActions();
        }

        // If there are actions that can be executed, pick one and do it.
        if (availableActionsList.Count > 0 && Mathf.Abs(angleToTarget) <= attackingAngleTolerance)
        {
            RollAction();
        }
        else 
        {
            //Debug.Log("no actions.");

            if (!aiState.IsAttacking)
            {
                // Attempt to reach desiredCombatIdleDistance
                if (!reachedDesiredCombatIdleDistance)
                {
                    // Continue tracking player position and move towards player until desiredCombatIdleDistance is reached.
                    if (planarDistanceToAggroedTarget > desiredCombatIdleDistance)
                    {
                        //Debug.Log("Moving to target...");
                        MoveToAggroedTarget();
                    }
                    // Now within desiredCombatIdleDistance; do combat stuff here
                    else if (planarDistanceToAggroedTarget <= desiredCombatIdleDistance)
                    {
                        //Debug.Log("reached desiredCombatIdleDistance");
                        reachedDesiredCombatIdleDistance = true;
                        return;
                    }
                }
                // If distance to player exceeds repositionDistance, move to player again
                else if (planarDistanceToAggroedTarget > repositionDistance)
                {
                    //Debug.Log("Repositioning to player...");
                    reachedDesiredCombatIdleDistance = false;
                    MoveToAggroedTarget();
                }
            }
        }
    }

    private void DetermineAvailableActions()
    {
        foreach (EnemyAttack attack in attackList)
        {
            // Ignore attack in attackList if attack isnt hooked up in inspector (aka null)
            if (attack == null)
            {
                continue;
            }

            // Check if player distance adheres to this attack's range requirements & attack is cooled down
            if (attackCoolDownDictionary[attack] <= 0 && attack.MinimumAttackDistance <= planarDistanceToAggroedTarget && planarDistanceToAggroedTarget <= attack.MaximumAttackingDistance)
            {
                // Check if this attack has charges
                AttackWithCharges attackWithCharges = attack as AttackWithCharges;
                if (attackWithCharges != null)
                {
                    if (attackChargesHandler.AttackHasUsableCharges(attackWithCharges))
                    {
                        availableActionsList.Add(attack);
                    }

                    continue;
                }

                // This attack is a NormalAttack, hence there is no further need to check for conditions to determine the readiness of this attack.
                availableActionsList.Add(attack);
            }
        }
    }

    public void RollAction()
    {
        // Decide which attack variation to execute 
        CalculateAvailableActionListWeights();
        bool determinedAttack = false;
        float rng = UnityEngine.Random.Range(0, actionListProbabilityWeightMaximum);

        // Loop through availableActionsList to get the selected attack
        for (int i = 0; i < availableActionsList.Count; i++)
        {
            if (availableActionsList[i].calculatedWeightFrom <= rng && rng <= availableActionsList[i].calculatedWeightTo)
            {
                determinedAttack = true;
                EnemyAttack selectedAttack = availableActionsList[i];

                // Stop all movement
                StopAllMovement();
                allowAttacking = false;     // Set to true OnStateExit of attack via SMB
                StartedAttack?.Invoke();

                // Animate atk & begin CD
                anim.SetInteger("Attack", selectedAttack.AttackID);  // Set to 0 via SMB OnStateEnter
                StartCoroutine(CoolDownAction(selectedAttack));

                // Check if selected attack is a sequenced attack
                SequencedEnemyAttack sequencedAttack = selectedAttack as SequencedEnemyAttack;
                if (sequencedAttack != null)
                {
                    // Run sequence advance conditions
                    attackSequencer.ChangeState(sequencedAttack.AdvanceCondition);
                }

                // Check if selected attack has charges
                AttackWithCharges attackWithCharges = selectedAttack as AttackWithCharges;
                if (attackWithCharges != null)
                {
                    // Consume a charge 
                    attackChargesHandler.ConsumeCharge(attackWithCharges);
                }
                break;
            }
        }

        if (!determinedAttack)
        {
            Debug.LogError("Could not determine attack");
        }

        // Prevent animation blending from causing weird movement
        //anim.SetFloat("Right", 0);
        //anim.SetFloat("Forward", 0);
    }

    private IEnumerator CoolDownAction(EnemyAttack attack)
    {
        attackCoolDownDictionary[attack] = attack.CoolDownDuration;
        while (attackCoolDownDictionary[attack] > 0)
        {
            yield return null;
            attackCoolDownDictionary[attack] -= Time.deltaTime;
        }
    }
    #endregion

    #region Callbacks
    private void OnEndedAttack()
    {
        allowAttacking = true;
        attackSequence = 0;
        anim.SetInteger("AttackSequence", 0);
    }

    private void OnAggroedTargetDeathEvent(IKillable killable)
    {
        Debug.Log("aggroedTarget has died.");

        // Reset variables
        CanBeginBossFight = false;
        PlayerIsInBossFightArea = false; // HARDCODE

        // Change back to idle state
        stateMachine.ChangeState(new AIStateHeavyTubaGunnerIdle(this));
    }

    private void OnAdvanceAttackSequence()
    {
        attackSequence++;
        anim.SetInteger("AttackSequence", attackSequence);
    }

    // Check if health is below certain percentages. If below certain percentages, trigger take break state.
    private void OnHealthLost()
    {
        if (aiResources.CurrentHealth <= 0)
        {
            return;
        }

        if (activeDamageBreakHealthPercentages.Count == 0)
        {
            return;
        }

        float healthPercentile = aiResources.CurrentHealth / aiResources.MaxHealth;
        float nextDamageBreakPercentile = activeDamageBreakHealthPercentages.Peek();
        //Debug.Log($"currPercentile: {healthPercentile}, nextPercentile: {nextDamageBreakPercentile}");
        if (healthPercentile <= nextDamageBreakPercentile && !htgAIState.IsTakingBreak)
        {
            EnterDamageBreakState();

            // Start corout to recover from damage break
            if (recoverFromDamageBreakCorout != null)
            {
                StopCoroutine(recoverFromDamageBreakCorout);
            }
            recoverFromDamageBreakCorout = StartCoroutine(RecoverFromDamageBreak());
        }
    }

    private void OnPlayerEnteredBossFightArea()
    {
        if (!playerIsInBossFightArea)
        {
            PlayerIsInBossFightArea = true;
        }
    }

    #endregion
    private void EnterDamageBreakState()
    {
        // Manage activeDamageBreakHealthPercentages stack, animate enter rest & publish evnts

        Debug.Log("Entering dmg break state");
        activeDamageBreakHealthPercentages.Pop();
        anim.SetBool("Rest", true);
        DamageBreakEntered?.Invoke();
    }

    private IEnumerator RecoverFromDamageBreak()
    {
        yield return new WaitForSeconds(damageBreakDuration);

        // Animate recovery + publish evnts
        anim.SetBool("Rest", false);
        DamageBreakExit?.Invoke();

        recoverFromDamageBreakCorout = null;
    }

    public void CompleteBossIntro()
    {
        HasFinishedIntro = true;
    }

    private void OnBossFightBeginConditionFulfilled()
    {
        //Debug.Log($"PlayerIsInBossFightArea: {PlayerIsInBossFightArea}, HasFinishedIntro: {HasFinishedIntro}");
        if (PlayerIsInBossFightArea && HasFinishedIntro)
        {
            StartBossFight();
        }
    }

    public void StartBossFight()
    {
        //Debug.Log("Started boss fight");
        CanBeginBossFight = true;
        EnemyHUDController.SpawnHealthBar.Invoke(GetComponent<IKillable>());
    }

    protected override void UnsubscribeFromEvents()
    {
        base.UnsubscribeFromEvents();
        EndedAttack -= OnEndedAttack;
        attackSequencer.AdvanceAttackSequence -= OnAdvanceAttackSequence;
        aiResources.HealthLost -= OnHealthLost;
        bossFightArea.PlayerEnteredBossFightArea -= OnPlayerEnteredBossFightArea;
    }

    private void OnDrawGizmosSelected()
    {
        if (debugDetectionRange)
        {
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }

        if (ai != null && ai.destination != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(ai.destination, 0.2f);
        }
    }
}
