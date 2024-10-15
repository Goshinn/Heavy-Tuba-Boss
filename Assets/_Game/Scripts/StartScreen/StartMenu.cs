using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class StartMenu : MenuFocusGroupManager
{
    [Header("Setup")]
    [SerializeField] private EventSystem canvasEventSystem;

    [Header("Runtime Members")]
    [SerializeField] private MenuFocusGroup activeFocusGroup;
    public ControlSet ControlSet;

    private void OnEnable()
    {
        ControlSet = new ControlSet();

        // Setup callbacks
        ControlSet.UI.Enable();

        ControlSet.UI.ControllerBack.performed += context => AttemptUnfocus();
        ControlSet.UI.KeyboardToggleMenu.performed += context => AttemptUnfocus();
    }

    private void Start()
    {
        if (Gamepad.current == null)
        {
            canvasEventSystem.firstSelectedGameObject = null;
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1, LoadSceneMode.Single);
    }

    public void AttemptUnfocus()
    {
        if (activeFocusGroup.PreviousFocusGroup != null)
        {
            Unfocus();
        }
    }

    public void FocusOnFocusGroup(MenuFocusGroup menuFocusGroup)
    {
        //Debug.Log($"focusing on {menuFocusGroup.gameObject.name}, selecting {menuFocusGroup.DefaultSelectedGO}");
        activeFocusGroup = menuFocusGroup;
        canvasEventSystem.SetSelectedGameObject(menuFocusGroup.DefaultSelectedGO);
        ActivateFocusGroup(activeFocusGroup);
    }

    public void Unfocus()
    {
        ActivateFocusGroup(activeFocusGroup.PreviousFocusGroup);
        canvasEventSystem.SetSelectedGameObject(activeFocusGroup.PreviousSelectedGO);
        activeFocusGroup = activeFocusGroup.PreviousFocusGroup;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void OnDisable()
    {
        // Disable controls
        ControlSet.UI.Disable();

        // Unsubscribe callbacks
        ControlSet.UI.ControllerBack.performed += context => AttemptUnfocus();
        ControlSet.UI.KeyboardToggleMenu.performed += context => AttemptUnfocus();
    }
}
