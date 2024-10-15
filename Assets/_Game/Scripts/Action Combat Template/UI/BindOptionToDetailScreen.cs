using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BindOptionToDetailScreen : MonoBehaviour
{
    [Header("Bind button to")]
    [Tooltip("When button is clicked, the binded screen is brought into focus")]
    [SerializeField] private MenuFocusGroup bindedFocusGroup;

    public MenuFocusGroup BindedFocusGroup { get => bindedFocusGroup; }
}
