using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class PauseMenu : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private MenuFocusGroup defaultFocusGroup;
    [SerializeField] private EventSystem eventSystem;

    [Header("Runtime Members")]
    [SerializeField] private MenuFocusGroup activeFocusGroup;
    // todo: create same logic as activeFocusGroups for navigating to previous screen instead of previousFocusGroup

    public void KeyboardTogglePauseMenu(ControlSet controlSet)
    {
        // if !gameObject.activeSelf, call ShowPauseMenu();
        // else if activeMenuPage.previousSelectedGO == null, call ClosePauseMenu();
        // else, select activeMenuPage.previousSelectedGO

        if (!gameObject.activeSelf)
        {
            ShowPauseMenu(controlSet);
        }
        //else if (activeFocusGroup != null && activeFocusGroup.PreviousScreen != null)
        //{
        //    // navigate to previous screen
        //}
        else
        {
            ClosePauseMenu(controlSet);
        }
    }

    public void ControllerTogglePauseMenu(ControlSet controlSet)
    {
        // if !gameObject.activeSelf, call ShowPauseMenu();
        // else ClosePauseMenu();

        if (!gameObject.activeSelf)
        {
            ShowPauseMenu(controlSet);
        }
        else
        {
            ClosePauseMenu(controlSet);
        }
    }

    public void ControllerBackPerformed(ControlSet controlSet)
    {
        if (activeFocusGroup.PreviousFocusGroup != null)
        {
            Unfocus();
        }
        else
        {
            ClosePauseMenu(controlSet);
        }
    }

    public void ShowPauseMenu(ControlSet controlSet)
    {
        //Debug.Log($"Showing pause menu...");
        gameObject.SetActive(true);

        // stop time, disable gameplay controls, show/hide cursor etc

        // Set variables
        activeFocusGroup = defaultFocusGroup;
        eventSystem.SetSelectedGameObject(defaultFocusGroup.DefaultSelectedGO);

        // Stop time & show cursor
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Disable gameplay input
        controlSet.Movement.Disable();
        controlSet.Combat.Disable();
        return;
    }

    public void ClosePauseMenu(ControlSet controlSet)
    {
        // resume time, enable gameplay controls, show/hide cursor etc

        // Set variables
        activeFocusGroup = null;

        // Resume time & hide cursor
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Enable gameplay input
        controlSet.Movement.Enable();
        controlSet.Combat.Enable();

        // Reset state of pauseMenu
        if (defaultFocusGroup.DefaultSelectedGO.TryGetComponent<BindOptionToDetailScreen>(out BindOptionToDetailScreen binding))
        {
            binding.BindedFocusGroup.transform.GetComponentInParent<MenuFocusGroupManager>().ActivateFocusGroup(binding.BindedFocusGroup);
        }

        gameObject.SetActive(false);
        return;
    }

    public void FocusOnFocusGroup(MenuFocusGroup menuFocusGroup)
    {
        //Debug.Log($"focusing on {menuFocusGroup.gameObject.name}, selecting {menuFocusGroup.DefaultSelectedGO}");
        activeFocusGroup = menuFocusGroup;
        eventSystem.SetSelectedGameObject(menuFocusGroup.DefaultSelectedGO);
    }

    public void Unfocus()
    {
        eventSystem.SetSelectedGameObject(activeFocusGroup.PreviousSelectedGO);
        activeFocusGroup = activeFocusGroup.PreviousFocusGroup;
    }

    private void OnDisable()
    {
        eventSystem.SetSelectedGameObject(null);
    }
}
