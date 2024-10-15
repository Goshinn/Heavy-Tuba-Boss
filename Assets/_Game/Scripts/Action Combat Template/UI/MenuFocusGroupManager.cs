using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class MenuFocusGroupManager : MonoBehaviour
{
    [Header("AutoPopulated References")]
    [SerializeField] private List<MenuFocusGroup> menuFocusGroups = new List<MenuFocusGroup>();

    private void OnValidate()
    {
        menuFocusGroups = GetComponentsInChildren<MenuFocusGroup>(true).ToList();
    }

    /// <summary>
    /// Set active the focusGroup
    /// </summary>
    /// <param name="menuFocusGroupToActivate"></param>
    public void ActivateFocusGroup(MenuFocusGroup menuFocusGroupToActivate)
    {
        foreach (MenuFocusGroup menuFocusGroup in menuFocusGroups)
        {
            menuFocusGroup.gameObject.SetActive(menuFocusGroup == menuFocusGroupToActivate);
        }
    }
}
