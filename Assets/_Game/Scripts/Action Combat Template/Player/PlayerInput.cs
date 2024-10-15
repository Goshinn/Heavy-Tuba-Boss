using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public struct PlayerCharacterInputs
{
    public Vector2 moveInput;
    public Quaternion CameraBaseRotation;
    public Quaternion CameraRotation;
}

/// <summary>
/// Responsible for receiving player input and sending the input data wherever needed.
/// </summary>
public class PlayerInput : MonoBehaviour
{
    [Header("References")]
    public SettingsProfile settingsProfile;
    public KinematicController controller;
    public PlayerCombat playerCombat;
    public CameraBase cameraBase;
    public Transform cameraFollowObj;
    public CanvasManager canvasManager;

    [Header("Settings")]
    [SerializeField] private bool enableInputOnAwake;

    [Header("Members")]
    private Vector2 rightJoystickInput;

    // Properties
    public ControlSet ControlSet { get; private set; }

    private void Awake()
    {
        // Setup the camera & initialize input
        if (cameraBase == null || controller == null || cameraFollowObj == null || playerCombat == null || canvasManager == null)
        {
            Debug.LogError($"Please check {name} to ensure that all references have been hooked up in inspector.");
            return;
        }

        ControlSet = new ControlSet();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cameraBase.AssignToPlayer(controller.gameObject, cameraFollowObj, settingsProfile);
        InitializeGameplayInput();

        if (enableInputOnAwake)
        {
            EnablePlayerInput();
        }
    }

    private void Update()
    {
        HandleCharacterInputs();

        if (rightJoystickInput != Vector2.zero)
        {
            cameraBase.FreeLookJoystick(rightJoystickInput);
        }
    }

    public void InitializeGameplayInput()
    {
        // Mouse look
        ControlSet.Movement.CameraLook.performed += context => cameraBase.OnFreeLookAttempted(context);
        ControlSet.Movement.CameraLookJoystick.performed += context => rightJoystickInput = context.ReadValue<Vector2>();
        ControlSet.Movement.CameraLookJoystick.canceled += context => rightJoystickInput = Vector2.zero;

        // Block
        ControlSet.Combat.Block.performed += context => playerCombat.OnBlockKeyDown();
        ControlSet.Combat.Block.canceled += context => playerCombat.OnBlockKeyUp();

        // Dodgeroll
        ControlSet.Combat.DodgeRoll.performed += context => playerCombat.OnDodgeAttempted(context, BuildCharacterInputsStruct());

        // Attacking
        ControlSet.Combat.LightAttack.performed += context => playerCombat.OnLightAttackAttempted();

        // Miscellaneous
        ControlSet.Miscellaneous.ToggleLockOnTarget.performed += context => cameraBase.ToggleLockOnTarget(ControlSet);

        // Pause Menu
        ControlSet.UI.KeyboardToggleMenu.performed += context => canvasManager.OnKeyboardToggleMenu(ControlSet);
        ControlSet.UI.ControllerToggleMenu.performed += context => canvasManager.OnControllerToggleMenu(ControlSet);
        ControlSet.UI.ControllerBack.performed += context => canvasManager.OnControllerBackPerformed(ControlSet);

        // uhh
        InputUser thisUser = InputUser.CreateUserWithoutPairedDevices();
    }

    public void EnablePlayerInput()
    {
        // Enable inputs
        ControlSet.Movement.Enable();
        ControlSet.Combat.Enable();
        ControlSet.Miscellaneous.Enable();
        ControlSet.UI.Enable();
    }

    private PlayerCharacterInputs BuildCharacterInputsStruct()
    {
        // Build the CharacterInputs struct
        PlayerCharacterInputs characterInputs = new PlayerCharacterInputs();
        characterInputs.moveInput = ControlSet.Movement.MovementInput.ReadValue<Vector2>(); // We're just gonna read raw input here since this works better with joysticks
        characterInputs.CameraBaseRotation = cameraBase.transform.rotation;
        characterInputs.CameraRotation = cameraBase.Cam.transform.rotation;

        return characterInputs;
    }

    private void HandleCharacterInputs()
    {
        // Build the CharacterInputs struct
        PlayerCharacterInputs characterInputs = BuildCharacterInputsStruct();

        // Apply inputs to character
        controller.SetInputs(characterInputs);
    }

    private void OnInputDeviceChanged(InputUser inputUser, InputUserChange arg2, InputDevice arg3)
    {
        Debug.Log($"controlScheme switched to: {inputUser.controlScheme.Value}, aka {inputUser.controlScheme.ToString()}");
    }

    private void OnDisable()
    {
        // Mouse look
        ControlSet.Movement.CameraLook.performed -= context => cameraBase.OnFreeLookAttempted(context);

        // Block
        ControlSet.Combat.Block.performed -= context => playerCombat.OnBlockKeyDown();
        ControlSet.Combat.Block.canceled -= context => playerCombat.OnBlockKeyUp();

        // Dodgeroll
        ControlSet.Combat.DodgeRoll.performed -= context => playerCombat.OnDodgeAttempted(context, BuildCharacterInputsStruct());

        // Attacking
        ControlSet.Combat.LightAttack.performed -= context => playerCombat.OnLightAttackAttempted();

        // Miscellaneous
        ControlSet.Miscellaneous.ToggleLockOnTarget.performed -= context => cameraBase.ToggleLockOnTarget(ControlSet);

        // Pause Menu
        ControlSet.UI.KeyboardToggleMenu.performed -= context => canvasManager.OnKeyboardToggleMenu(ControlSet);
        ControlSet.UI.ControllerToggleMenu.performed -= context => canvasManager.OnControllerToggleMenu(ControlSet);
        ControlSet.UI.ControllerBack.performed -= context => canvasManager.OnControllerBackPerformed(ControlSet);
    }
}
