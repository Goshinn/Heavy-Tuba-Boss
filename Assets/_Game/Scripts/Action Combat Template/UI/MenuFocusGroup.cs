using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuFocusGroup : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Utilized by the escape key on PC")]
    [SerializeField] private MenuFocusGroup previousScreen;

    [Header("Controller Support")]
    [SerializeField] private GameObject defaultSelectedGO;
    [SerializeField] private MenuFocusGroup previousFocusGroup;
    [SerializeField] private GameObject previousSelectedGO;

    public MenuFocusGroup PreviousScreen { get => previousScreen; }
    public GameObject DefaultSelectedGO { get => defaultSelectedGO; }
    public MenuFocusGroup PreviousFocusGroup { get => previousFocusGroup; }
    public GameObject PreviousSelectedGO { get => previousSelectedGO; }
}
