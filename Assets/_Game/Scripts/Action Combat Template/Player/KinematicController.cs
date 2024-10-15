using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;

[RequireComponent(typeof(Animator))]
public class KinematicController : MonoBehaviour, ICharacterController
{
    [Header("References")]
    public KinematicCharacterMotor Motor;
    private Animator anim;

    [Header("Movement Settings")]
    [SerializeField] public float movementSpeed = 4.5f;
    [SerializeField] public float StableMovementSharpness = 15;
    [SerializeField] private float rotationSpeed = 30f;
    private Vector2 moveInput;
    private Vector3 desiredMoveDirection;
    private Vector3 cameraBasePlanarForward;
    private bool shouldUseRootMotion { get { return playerState.IsDodging || playerState.IsAttacking || playerState.IsCrowdControlled || playerState.IsBlockRecoiling || !playerState.IsAlive || playerState.IsReviving; } }
    private Transform lockedOnTarget;
    private Quaternion desiredCharacterRotation
    {
        get
        {
            if (lockedOnTarget != null)
            {
                Vector3 planarDirectionToLockOnTarget = Vector3.ProjectOnPlane(lockedOnTarget.position - transform.position, Vector3.up);
                return Quaternion.LookRotation(planarDirectionToLockOnTarget, Vector3.up);
            }
            else
            {
                return Quaternion.LookRotation(cameraBasePlanarForward, Vector3.up);
            }
        }
    }

    [Header("Combat Settings")]
    [SerializeField] private LayerMask attackRootMotionObstructionLayer;    // If a spherecast from the middle of the character were to hit objects in this layer, RM is cancelled.
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private PlayerState playerState;
    [SerializeField] private float blockMoveSpeed = 3f;
    private Vector3 rootMotionPositionDelta;                // Used for calculating rootmotion and using it alongside the KinematicController to move the character.
    private bool allowRotationWhileAttacking;               // Set to true OnStartedAttack, false via anim evnts

    [Header("Air Movement")]
    [SerializeField] private float AirAccelerationSpeed = 5f;
    [SerializeField] private float Drag = 0.1f;

    [Header("Animation Blending Settings")]
    [SerializeField] private float smoothTime = 0.04f;  // Value used for smoothing horizontal, vertical & inputMagnitude anim params.
    private float smoothForward;
    private float smoothRight;

    [Header("Misc")]
    [SerializeField] Vector3 Gravity = new Vector3(0, -30f, 0);

    private void Awake()
    {
        // Hook up references
        anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        playerCombat.StartedAttack += OnStartedAttack;
        CameraBase.TargetLockedOn += OnTargetLockedOn;
        CameraBase.TargetLockOnEnded += OnTargetLockOnEnded;
    }

    private void OnTargetLockedOn(Transform lockOnObj)
    {
        lockedOnTarget = lockOnObj;
    }

    private void OnTargetLockOnEnded()
    {
        lockedOnTarget = null;
    }

    private void Start()
    {
        // Setup components
        Motor.CharacterController = this;
    }

    public void SetInputs(PlayerCharacterInputs inputs)
    {
        moveInput = inputs.moveInput;

        // Create temp vector3 moveInput
        Vector3 moveInputVector = Vector3.ClampMagnitude(new Vector3(inputs.moveInput.x, 0f, inputs.moveInput.y), 1f);

        // Calculate camera direction and rotation on the character plane
        cameraBasePlanarForward = Vector3.ProjectOnPlane(inputs.CameraBaseRotation * Vector3.forward, Vector3.up).normalized;
        Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraBasePlanarForward, Vector3.up);

        // Move and look inputs
        desiredMoveDirection = cameraPlanarRotation * moveInputVector;
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        if (playerState.IsCrowdControlled || playerState.IsBlockRecoiling || !playerState.IsAlive)
        {
            return;
        }

        if (playerState.IsDodging)
        {
            // Snap rotation to camera forward upon dodging & keep updating it as long as IsDodging true
            currentRotation = desiredCharacterRotation;
        }
        else if (playerState.IsBlocking)
        {
            // Rotate to camera planar forward
            currentRotation = Quaternion.Slerp(transform.rotation, desiredCharacterRotation, rotationSpeed * deltaTime);
        }
        else if (playerState.IsAttacking)
        {
            if (allowRotationWhileAttacking)
            {
                // Rotate to camera planar forward
                currentRotation = Quaternion.Slerp(transform.rotation, desiredCharacterRotation, rotationSpeed * deltaTime);
            }           
        }
        // Not attacking & is moving
        else if (desiredMoveDirection.sqrMagnitude != 0)
        {
            // Rotate to camera planar forward
            currentRotation = Quaternion.Slerp(transform.rotation, desiredCharacterRotation, rotationSpeed * deltaTime);
        }
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        Vector3 targetMovementVelocity = Vector3.zero;
        if (Motor.GroundingStatus.IsStableOnGround)
        {
            // Calculate target velocity
            Vector3 inputRight = Vector3.Cross(desiredMoveDirection, Motor.CharacterUp);
            Vector3 reorientedInput = Vector3.Cross(Motor.GroundingStatus.GroundNormal, inputRight).normalized * desiredMoveDirection.magnitude; // Reorient input to DirectionTangentToSurface?

            // For situations whereby the velocity is controlled by rootmotion
            if (shouldUseRootMotion)
            {
                Vector3 rootMotionVelocity = rootMotionPositionDelta / deltaTime;

                // If attacking, prevent attack's rootmotion from causing the player to slide past the enemy, causing players to miss their attacks.
                if (playerState.IsAttacking)
                {
                    Vector3 origin = transform.TransformPoint(Motor.CharacterTransformToCapsuleCenter);
                    if (Physics.SphereCast(origin, Motor.Capsule.radius, rootMotionPositionDelta.normalized, out RaycastHit hit, Motor.Capsule.radius, attackRootMotionObstructionLayer))
                    {
                        //Debug.Log($"hit: {hit.collider.gameObject.name}");
                        currentVelocity = Motor.GetDirectionTangentToSurface(Vector3.zero, Motor.GroundingStatus.GroundNormal) * rootMotionVelocity.magnitude;
                    }
                    else
                    {
                        currentVelocity = Motor.GetDirectionTangentToSurface(rootMotionVelocity, Motor.GroundingStatus.GroundNormal) * rootMotionVelocity.magnitude;
                    }
                }
                else
                {
                    currentVelocity = Motor.GetDirectionTangentToSurface(rootMotionVelocity, Motor.GroundingStatus.GroundNormal) * rootMotionVelocity.magnitude;
                }
            }
            // For situations whereby velocity is controlled by user
            else
            {
                float appropriateMoveSpeed = movementSpeed;
                if (playerState.IsBlocking)
                {
                    appropriateMoveSpeed = blockMoveSpeed;
                }

                targetMovementVelocity = reorientedInput * appropriateMoveSpeed;

                // Smooth movement Velocity
                currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp(-StableMovementSharpness * deltaTime));
            }
        }
        else
        {
            // Add move input
            if (desiredMoveDirection.sqrMagnitude > 0f)
            {
                targetMovementVelocity = desiredMoveDirection * movementSpeed;

                // Prevent climbing on un-stable slopes with air movement
                if (Motor.GroundingStatus.FoundAnyGround)
                {
                    Debug.Log("character colliders touching a surface but im not grounded. probably still falling.");
                    Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp).normalized;
                    targetMovementVelocity = Vector3.ProjectOnPlane(targetMovementVelocity, perpenticularObstructionNormal);
                }

                Vector3 velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, Gravity);
                currentVelocity += velocityDiff * AirAccelerationSpeed * deltaTime;
            }

            // Gravity
            currentVelocity += Gravity * deltaTime;

            // Drag
            currentVelocity *= (1f / (1f + (Drag * deltaTime)));
        }

        UpdateAnimationParameters();
    }

    private void UpdateAnimationParameters()
    {
        //if (playerState.IsCrowdControlled)
        //{
        //    // Set anim params
        //    anim.SetFloat("Forward", 0);
        //    anim.SetFloat("Right", 0);
        //    return;
        //}

        // Set anim vars
        float forward, horizontal;

        // Calculate anim params
        forward = Mathf.SmoothDamp(anim.GetFloat("Forward"), moveInput.y, ref smoothForward, smoothTime);
        horizontal = Mathf.SmoothDamp(anim.GetFloat("Right"), moveInput.x, ref smoothRight, smoothTime);

        // Stop jitter if within +/- range of 0.01f
        if (Mathf.Abs(forward) < 0.01f) forward = 0;
        if (Mathf.Abs(horizontal) < 0.01f) horizontal = 0;

        // Set anim params
        anim.SetFloat("Forward", forward);
        anim.SetFloat("Right", horizontal);
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        // Reset root motion deltas
        rootMotionPositionDelta = Vector3.zero;
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void PostGroundingUpdate(float deltaTime)
    {
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
    }

    private void OnAnimatorMove()
    {
        rootMotionPositionDelta += anim.deltaPosition;
    }

    #region Callbacks
    private void OnStartedAttack()
    {
        allowRotationWhileAttacking = true;
    }
    #endregion

    #region Animation Events
    public void DisallowRotatingWhileAttacking()
    {
        allowRotationWhileAttacking = false;
    }
    #endregion

    private void OnDisable()
    {
        playerCombat.StartedAttack -= OnStartedAttack;
        CameraBase.TargetLockedOn -= OnTargetLockedOn;
        CameraBase.TargetLockOnEnded -= OnTargetLockOnEnded;
    }
}
