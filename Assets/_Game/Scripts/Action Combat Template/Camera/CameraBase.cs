using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This script sits on Camera Base; makes cameraBase follow the cameraFollowObj in addition to 
/// rotating the CameraBase based on joystick/mouse input
/// </summary>
public class CameraBase : MonoBehaviour
{
    [Header("Auto-Populated References")]
    private GameObject assignedPlayer;
    private Transform cameraFollowObj;      // Initialize when spawning player
    private Camera cam;

    [Header("References")]
    private SettingsProfile settingsProfile;
    private PlayerCombatResources combatResources;

    [Header("Settings")]
    [SerializeField] private float cameraMoveSpeed = 120f;
    [SerializeField] private float upwardVerticalClamp = 80f;
    [SerializeField] private float downwardVerticalClamp = 45f;

    [Header("Target Lock On")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float lockOnRadius = 20f;              // Enemies need to be within this distance from the player character to lock on
    [SerializeField] private float lockOnSpeed = 15f;               // Rate at which camera rotates towards lockOnTarget        
    [SerializeField] private float lockOnMaxAngleDifference = 5f;   // Max angle difference from camerabase forward to prevent bad camera angles (eg. character looking to the side where side is lockOnTarget)
    private float angleDifference;
    private EnemyAIBase lockOnTarget;

    [Header("Members")]
    private float rotX;
    private float rotY;

    // Properties
    public GameObject AssignedPlayer { get => assignedPlayer; }
    public Camera Cam { get => cam; }

    // Events
    public static event Action<Transform> TargetLockedOn;
    public static event Action TargetLockOnEnded;

    private void Awake()
    {
        // Hook up references
        cam = GetComponentInChildren<Camera>();

        // Init vars
        Vector3 rot = transform.localEulerAngles;
        rotY = rot.y;
        rotX = rot.x;
    }

    private void Update()
    {
        if (lockOnTarget != null)
        {
            RotateToLockOnTarget();
        }
    }

    // Call CameraUpdater after player has moved in LateUpdate()
    private void LateUpdate()
    {
        if (cameraFollowObj != null) CameraBaseUpdater();
    }

    // Called when spawning player
    public void AssignToPlayer(GameObject player, Transform followTransform, SettingsProfile settingsProfile)
    {
        assignedPlayer = player;
        cameraFollowObj = followTransform;
        this.settingsProfile = settingsProfile;

        // Setup callbacks
        combatResources = player.GetComponent<PlayerCombatResources>();
        combatResources.PlayerStartedRevive += OnPlayerStartedRevive;
        combatResources.EntityDeathEvent += OnPlayerDeath;
    }

    // Called from PlayerInput
    public void OnFreeLookAttempted(InputAction.CallbackContext context)
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            rotY += context.ReadValue<Vector2>().x * settingsProfile.MouseSensitivity * Time.deltaTime;
            rotX += context.ReadValue<Vector2>().y * settingsProfile.MouseSensitivity * Time.deltaTime;

            rotX = Mathf.Clamp(rotX, -downwardVerticalClamp, upwardVerticalClamp);

            Quaternion localRotation = Quaternion.Euler(rotX, rotY, 0);
            transform.rotation = localRotation;
        }
    }

    public void FreeLookJoystick(Vector2 joystickInput)
    {
        if (lockOnTarget == null)
        {
            rotY += joystickInput.x * settingsProfile.ControllerHorizontalSensitivity * Time.deltaTime;
            rotX += joystickInput.y * settingsProfile.ControllerVerticalSensitivity * Time.deltaTime;

            rotX = Mathf.Clamp(rotX, -downwardVerticalClamp, upwardVerticalClamp);

            Quaternion localRotation = Quaternion.Euler(rotX, rotY, 0);
            transform.rotation = localRotation;
        }
    }

    // Move cameraBase toward the game obj that is the target
    private void CameraBaseUpdater()
    {
        transform.position = Vector3.MoveTowards(transform.position, cameraFollowObj.position, cameraMoveSpeed * Time.deltaTime);
    }

    // Reset local rotX & rotY variables to prevent snapping to previous rotation upon moving mouse/joystick
    private void ResetCameraBaseRotation()
    {
        float eulerX = transform.rotation.eulerAngles.x;    // as euler angles range only from 0-360 but in quaternions(?) it goes from -180 to 180, we have to do a conversion.
        float eulerY = transform.rotation.eulerAngles.y;
        if (eulerX > 180f)
        {
            eulerX -= 360f;
        }
        if (eulerY > 180f)
        {
            eulerY -= 360f;
        }
        rotX = eulerX;
        rotY = eulerY;
    }
    
    public void ToggleLockOnTarget(ControlSet controlSet)
    {
        if (assignedPlayer.GetComponent<IKillable>().CurrentHealth <= 0)
        {
            return;
        }

        if (lockOnTarget == null)
        {
            AttemptLockOn();
        }
        else
        {
            EndTargetLockOn();
        }
    }

    public void AttemptLockOn()
    {
        // Get all enemies within x distance from player
        //Debug.Log($"getting enemies within {lockOnRadius}m from player...");
        Collider[] enemyCols = Physics.OverlapSphere(assignedPlayer.transform.position, lockOnRadius, enemyLayer);
        //Debug.Log($"found {enemyCols.Length} colliders.");

        // Get closest enemy to the middle of the viewport
        //Debug.Log($"getting closest to viewport...");
        float closestToMiddleOfViewPort = 0.707f;   // Pythagoras theorem; magnitude of 0.707f is the farthest one can be from the middle of the viewport
        EnemyAIBase closestEnemyToCenterOfViewPort = null;
        Vector3 centerOfViewPort = new Vector2(0.5f, 0.5f);
        for (int i = 0; i < enemyCols.Length; i++)
        {
            // Check if enemy is in front of player 

            Vector2 entityViewPortPosition = cam.WorldToViewportPoint(enemyCols[i].transform.position);
            //Debug.Log($"{enemyCols[i].name} viewport position: {entityViewPortPosition}");
            float magnitude = Vector2.Distance(entityViewPortPosition, centerOfViewPort);
            //Debug.Log($"{enemyCols[i].name} magnitude: {magnitude}");
            if (magnitude < closestToMiddleOfViewPort)
            {
                closestToMiddleOfViewPort = magnitude;
                closestEnemyToCenterOfViewPort = enemyCols[i].GetComponentInParent<EnemyAIBase>();

                if (closestEnemyToCenterOfViewPort == null)
                {
                    Debug.LogError($"enemyAI not found on {enemyCols[i].name}");
                }
            }
        }

        // Set lockOnTarget & invoke events
        if (closestEnemyToCenterOfViewPort != null)
        {
            LockOnToTarget(closestEnemyToCenterOfViewPort);
        }
    }

    private void LockOnToTarget(EnemyAIBase enemy)
    {
        //Debug.Log($"locking on to : {enemy.name}");

        // Subscribe to EntityDeathEvent
        enemy.GetComponent<IKillable>().EntityDeathEvent += OnLockedOnTargetDeath;

        // Set variables & invoke events
        lockOnTarget = enemy;
        TargetLockedOn?.Invoke(lockOnTarget.LockOnObj);
    }

    private void EndTargetLockOn()
    {
        // Unsubscribe from events
        lockOnTarget.GetComponent<IKillable>().EntityDeathEvent -= OnLockedOnTargetDeath;

        // Set variables & invoke events
        lockOnTarget = null;
        TargetLockOnEnded?.Invoke();
        ResetCameraBaseRotation();
    }

    private void RotateToLockOnTarget()
    {
        //Quaternion targetRotation = Quaternion.LookRotation(lockOnTarget.LockOnObj.position - Cam.transform.position, Vector3.up);
        //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lockOnSpeed * Time.deltaTime);

        Vector3 cameraToLockOnTarget = lockOnTarget.LockOnObj.position - cam.transform.position;
        Vector3 cameraBaseToLockOnTarget = lockOnTarget.LockOnObj.position - transform.position;

        angleDifference = Vector3.Angle(cameraToLockOnTarget, cameraBaseToLockOnTarget);
        if (angleDifference < lockOnMaxAngleDifference)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lockOnTarget.LockOnObj.position - Cam.transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lockOnSpeed * Time.deltaTime);
        }
        else
        {
            Vector3 targetLookVector = Quaternion.AngleAxis(Mathf.Clamp(angleDifference - lockOnMaxAngleDifference, 0, lockOnMaxAngleDifference), Vector3.up) * cameraToLockOnTarget;
            Quaternion targetRotation = Quaternion.LookRotation(targetLookVector, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lockOnSpeed * Time.deltaTime);
        }
    }

    #region Callbacks
    private void OnPlayerDeath(IKillable obj)
    {
        if (lockOnTarget != null)
        {
            EndTargetLockOn();
        }
    }

    private void OnPlayerStartedRevive(RespawnPoint respawnPoint)
    {
        // Move cameraBase to player position (since he must have been moved to a respawn point)
        transform.position = cameraFollowObj.position;
        transform.rotation = respawnPoint.transform.rotation;
        ResetCameraBaseRotation();
    }

    private void OnLockedOnTargetDeath(IKillable obj)
    {
        EndTargetLockOn();
    }
    #endregion

    private void OnDestroy()
    {
        combatResources.PlayerStartedRevive -= OnPlayerStartedRevive;
        combatResources.EntityDeathEvent -= OnPlayerDeath;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, lockOnRadius);
    }
}
