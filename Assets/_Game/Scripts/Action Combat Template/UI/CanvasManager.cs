using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CanvasManager : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private PlayerCombatResources combatResources;
    [SerializeField] private Camera cam;

    [Header("References")]
    [SerializeField] private PauseMenu pauseMenu;

    #region Accessors
    public PlayerCombatResources CombatResources { get => combatResources; }
    public Camera Cam { get => cam; }
    #endregion

    private void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// Escape was pressed. 
    /// Navigate to previous menu page, if any.
    /// Else, hide UI canvas.
    /// </summary>
    public void OnKeyboardToggleMenu(ControlSet controlSet)
    {
        pauseMenu.KeyboardTogglePauseMenu(controlSet);
    }

    public void OnControllerToggleMenu(ControlSet controlSet)
    {
        pauseMenu.ControllerTogglePauseMenu(controlSet);
    }

    public void OnControllerBackPerformed(ControlSet controlSet)
    {
        pauseMenu.ControllerBackPerformed(controlSet);
    }

    #region Public Methods
    public void QuitGame()
    {
        Application.Quit();
    }
    #endregion
}
