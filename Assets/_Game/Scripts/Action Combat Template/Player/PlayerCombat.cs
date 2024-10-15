using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This script is responsible for handling combat logic such as attacking, blocking & dodging.
/// </summary>
//[RequireComponent(typeof(Animator), typeof(CombatResources), typeof(PlayerState))]
public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    private Animator anim;
    private PlayerCombatResources combatResources;
    private PlayerState playerState;
    public GameObject attackerTestDummy;

    [Header("Light Attack")]
    [SerializeField] private int lightAttackChainLength = 3;                        // Used to determine basicComboChainWrapValue. 
    [SerializeField] private int[] lightAttackStaminaConsumption = new int[] { };   // Stamina consumption for every light attack hit
    [SerializeField] private float lightAttackComboResetDuration = 1.2f;            // Begins the moment light attack starts
    private int lightComboChain;                                                    // Sets anim param "LightAttack" to this value to animate light attack combo chain
    private int lightComboChainWrapValue;                                           // When basicAttackChain reaches this value, it gets reset to 1
    private Coroutine resetLightComboChainCorout;
    [SerializeField] private bool desiresLightAttack;

    [Header("Blocking")]
    [Range(0, 1)] public float blockDamageMitigation = 0.8f;    // Range from 0 to 1, 1 means all damage is blocked, 0 means no damage is blocked
    [Range(0, 360)] public float effectiveBlockAngle = 165f;    // Angle enemy is away from player's forward for block to take effect if blocking
    [SerializeField] private float blockCoolDownDuration = 0.6f;
    private float blockCoolDown;
    private bool blockIsHeldDown;
    
    //[SerializeField] private CameraEffects_CameraShakeInstance blockFeedBack;
    //[SerializeField] private GameObject blockVFXPrefab;
    //public static event Action<VFXInfo> NewVisualFeedBack;

    [Header("Dodging")]
    [SerializeField] private float dodgeStaminaCost = 30f;
    private Vector2 dodgeDirection;

    [Header("Events")]
    public Action StartedAttack;                // Used to set variables on PlayerState
    public Action StoppedAttacking;             // Sets isAttacking to false etc
    public Action LightAttackStarted;           // Invoked by StateMachineBehaviours, this class listens to this event and handles comboing with light attacks etc

    public Action StartedBlock;                 // Listened to by KinematicController to handle blocking locomotion
    public Action StoppedBlock;                 // Listened to by KinematicController to handle blocking locomotion
    public Action<AttackInfo> SuccessfulBlock;
    public Action<AttackInfo> FailedBlock;

    public Action Dodged;                       // Used to set variables on PlayerState
    public Action EndedDodge;                   // Listened to by PlayerState to set variables
    public Action CanceledAttack;               // Listened to by AttackVFX_SwordSwing to deactivate PE on attack canceled with dodge/block

    private void OnValidate()
    {
        if (lightAttackStaminaConsumption.Length != lightAttackChainLength)
        {
            Debug.LogWarning("Please refrain from setting the lightAttackStaminaConsumption array to an incorrect size.");
            lightAttackStaminaConsumption = new int[lightAttackChainLength];
        }
    }

    private void Awake()
    {
        // Hook up references
        anim = GetComponent<Animator>();
        combatResources = GetComponent<PlayerCombatResources>();
        playerState = GetComponent<PlayerState>();

        // Testing
        ControlSet controlSet = new ControlSet();
        controlSet.Testing.Enable();
        controlSet.Testing.FlinchingAttack.performed += context => TestFlinchAttack();
    }

    private void OnEnable()
    {
        // Subscribe to events
        LightAttackStarted += OnLightAttackStarted;
        combatResources.EntityDeathEvent += OnEntityDeathEvent;
        playerState.CrowdControlled += OnCrowdControlled;
        SuccessfulBlock += OnSuccessfulBlock;
    }

    private void Start()
    {
        lightComboChainWrapValue = lightAttackChainLength + 1;
    }

    private void Update()
    {
        if (desiresLightAttack)
        {
            AttemptLightAttack();
        }

        if (blockIsHeldDown && !playerState.IsBlocking && combatResources.BlockResource > 0)
        {
            AttemptBlock();
        }
    }

    private void TestFlinchAttack()
    {
        List<SkillEffectTemplate> skillEffects = new List<SkillEffectTemplate>();
        AttackInfo testAttackInfo = new AttackInfo();
        testAttackInfo.SetAttackHitInfo(10, true, 30, skillEffects);
        testAttackInfo.SetHitFeedBackInfo(attackerTestDummy, gameObject, AttackAttribute.Blunt, Vector3.up);
        combatResources.HandleHit(testAttackInfo);

        if (combatResources.CurrentHealth > 0)
        {
            playerState.AttemptFlinch(testAttackInfo, 1);
            //playerState.AttemptKnockdown(testAttackInfo, 2);
        }
    }

    // Called on left click.
    public void OnLightAttackAttempted()
    {
        //Debug.Log("attempted light atk");
        // Conduct checks to see if player is allowed to start/continue a combo chain
        int staminaConsumptionIndex = lightComboChain % lightAttackChainLength;  // Prevent index out of range exception
        if (IsAbleToDoCombat() && combatResources.Stamina >= lightAttackStaminaConsumption[staminaConsumptionIndex])
        {
            IncrementBasicAttackChain();
            desiresLightAttack = true;
        }
    }

    /// <summary>
    /// Called whenever desiresLightAttack is true in Update loop.
    /// Whenever ready, attack.
    /// </summary>
    private void AttemptLightAttack()
    {
        if (playerState.AllowAttacking && IsAbleToDoCombat())
        {
            anim.SetInteger("LightAttackChain", lightComboChain);
            desiresLightAttack = false;
        }
    }

    private void IncrementBasicAttackChain()
    {
        lightComboChain++;
        if (lightComboChain == lightComboChainWrapValue)
        {
            lightComboChain = 1;
        }
    }

    private bool IsAbleToDoCombat()
    {
        // Conduct checks to see if player is allowed to start/continue a combo chain
        if (Cursor.lockState != CursorLockMode.Locked || !playerState.IsAlive || playerState.IsReviving
            || playerState.IsCrowdControlled || playerState.IsBlocking || !playerState.AllowActionQueuing)
        {
            //Debug.Log($"{Cursor.lockState != CursorLockMode.Locked} || {!playerState.IsAlive} || {playerState.IsCrowdControlled} || {playerState.IsBlocking} || {!playerState.AllowActionQueuing}");
            return false;
        }

        return true;
    }

    #region Callbacks
    private void OnLightAttackStarted()
    {
        // Null check
        int index = lightComboChain - 1;
        if (index < 0 || index >= lightAttackStaminaConsumption.Length)
        {
            Debug.Log($"attempted to access lightAttackStaminaConsumption with index {index}, forcing to 0");
            index = 0;
            //Debug.Break();
        }

        // Deduct stamina  
        combatResources.Stamina = Mathf.Clamp(combatResources.Stamina - lightAttackStaminaConsumption[index], 0, 100f);

        // Start timer to break lightComboChain
        if (resetLightComboChainCorout != null)
        {
            StopCoroutine(resetLightComboChainCorout);
        }
        resetLightComboChainCorout = StartCoroutine(DelayResetLightAttackChain());
    }

    private void OnCrowdControlled()
    {
        if (playerState.IsBlocking)
        {
            EndBlock();
        }
    }
    #endregion

    private IEnumerator DelayResetLightAttackChain()
    {
        //Debug.Log($"waiting {lightAttackComboResetDuration}s to reset comboChain");
        yield return new WaitForSeconds(lightAttackComboResetDuration);
        //Debug.Log("setting basicAttackChain to 0");
        lightComboChain = 0;
        resetLightComboChainCorout = null;
    }

    #region Blocking
    // Called by PlayerInput
    public void OnBlockKeyDown()
    {
        blockIsHeldDown = true;
    }

    // Called while blockIsHeldDown if !playerState.IsBlocking
    private void AttemptBlock()
    {
        if (IsAbleToDoCombat() && !playerState.IsBlocking && playerState.CanBlock && playerState.BlockCooledDown && !playerState.IsDodging)
        {
            StartBlocking();
        }
    }

    // Called by PlayerInput
    public void OnBlockKeyUp()
    {
        blockIsHeldDown = false;
        if (playerState.IsBlocking)
        {
            StopBlocking();
        }
    }

    private void StartBlocking()
    {
        // Publish evnt
        StartedBlock?.Invoke();

        // Animate block
        anim.SetBool("Blocking", true);

        // Set vars
        playerState.SetIsBlocking(true);
        playerState.SetBlockCooledDown(false);

        // Publish evnts
        if (playerState.IsAttacking)
        {
            CanceledAttack?.Invoke();
        }

        // Play block sound
        //SFXSystem.soundSystem.PlaySoundEffect(SFXSystem.soundSystem.greatSwordBlock);
    }

    private void StopBlocking()
    {
        // Publish evnt
        StoppedBlock?.Invoke();

        // Set vars
        playerState.SetIsBlocking(false);

        // Set anim params
        anim.SetBool("Blocking", false);

        // Cool down block to prevent block spam
        StartCoroutine(CoolDownBlock());
    }

    private IEnumerator CoolDownBlock()
    {
        blockCoolDown = blockCoolDownDuration;

        while (blockCoolDown > 0)
        {
            yield return null;
            blockCoolDown -= Time.deltaTime;
        }

        playerState.SetBlockCooledDown(true);
    }

    // Called OnCrowdControlled
    private void EndBlock()
    {
        playerState.SetIsBlocking(false);
        anim.SetBool("Blocking", false);
    }

    public bool WasAttackWithinEffectiveBlockCoverage(GameObject enemy)
    {
        // Check if enemy attack angle falls within block coverage angle
        Vector3 enemyPos = enemy.transform.position;
        float angleFromForward = Mathf.Abs(Vector3.SignedAngle(transform.forward, enemyPos - transform.position, Vector3.up));
        return angleFromForward <= effectiveBlockAngle / 2;
    }

    public void OnSuccessfulBlock(AttackInfo attackInfo)
    {  
        //Debug.Log($"successful block");
        // Animate block feedback anim
        anim.SetTrigger("BlockedAttack");
        anim.SetBool("BlockRecoiling", true);  // Set to false OnStateExit of recoil state

        // Publish evnts
        //VFXInfo vfxInfo = new VFXInfo(gameObject, blockVFXPrefab, GetComponent<CharacterController>().bounds.center + Vector3.up * GetComponent<CharacterController>().height / 4);
        //NewVisualFeedBack?.Invoke(vfxInfo);

        // Publish camera shake
        //if (blockFeedBack != null)
        //{
        //    GetComponent<CameraEffectPublisher>()?.PublishCameraShake(blockFeedBack);
        //}
    }

    private void OnEntityDeathEvent(IKillable killable)
    {
        // Since this player has died, we turn off the "Blocking" anim param bool to allow the die anim to play without interruptions etc
        anim.SetBool("Blocking", false);
        playerState.SetIsBlocking(false);
    }
    #endregion

    #region Dodging
    public void OnDodgeAttempted(InputAction.CallbackContext context, PlayerCharacterInputs playerInputs)
    {
        if (IsAbleToDoCombat() && !playerState.IsDodging && playerState.CanAnimCancel && combatResources.Stamina >= dodgeStaminaCost)
        {
            // Deduct stamina
            combatResources.Stamina = Mathf.Clamp(combatResources.Stamina - dodgeStaminaCost, 0, 100f);

            // Set dodgeDirectionAngle; if no input, dodge backwards, else dodge in desiredMoveDirection
            dodgeDirection = new Vector2(0, -1);
            if (playerInputs.moveInput.magnitude != 0)
            {
                dodgeDirection = playerInputs.moveInput;
            }

            // Animate dodge & Start updating position
            anim.SetFloat("DodgeX", dodgeDirection.x);
            anim.SetFloat("DodgeY", dodgeDirection.y);
            anim.SetTrigger("DodgeRoll");

            // Figure out how to move the character

            // Publish evnts
            Dodged?.Invoke();

            // Play flashstep sound
            //FMODUnity.RuntimeManager.PlayOneShotAttached("event:/SFX/Special/Shunpo", gameObject);

            // Cancel sword swing PE on attack canceled
            if (playerState.IsAttacking)
            {
                CanceledAttack?.Invoke();
            }
        }
    }
    #endregion

    private void OnDisable()
    {
        // Unsubscribe from events
        LightAttackStarted -= OnLightAttackStarted;
        combatResources.EntityDeathEvent -= OnEntityDeathEvent;
        playerState.CrowdControlled -= OnCrowdControlled;
        SuccessfulBlock -= OnSuccessfulBlock;
    }
}