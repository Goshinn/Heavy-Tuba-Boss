using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillableEntityInfo : ScriptableObject
{
    [Header("Killable Settings")]
    [SerializeField] private float maxHealth;
    protected float currentHealth;

    #region Accessors
    public float MaxHealth { get { return maxHealth; } }
    public float CurrentHealth { get { return currentHealth; } }
    #endregion
}
