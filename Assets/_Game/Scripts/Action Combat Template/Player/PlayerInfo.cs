using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Info", menuName = "Player/PlayerInfo")]
public class PlayerInfo : KillableEntityInfo
{
    [Header("Player Settings")]
    [SerializeField] private float maxStamina;
    [SerializeField] private float maxBlockResource;

    [Header("Members")]
    private float currentStamina;
    private float currentBlockResource;

    #region Accessors
    public float MaxStamina { get { return maxStamina; } }
    public float MaxBlockResource { get { return maxBlockResource; } }

    public float CurrentStamina { get => currentStamina; }
    public float CurrentBlockResource { get => currentBlockResource; }
    #endregion
}
