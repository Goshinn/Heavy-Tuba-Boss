using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EnemyInfo is responsible for storing generic info about the enemy, such as its hitpoints, experience, description, item drops etc.
/// </summary>
[CreateAssetMenu(fileName = "New Enemy Info", menuName = "Enemy/EnemyInfo")]
public class EnemyInfo : KillableEntityInfo
{
    [Header("Monster Properties")]
    [SerializeField] private string enemyName;

    #region Accessors
    public string EnemyName { get { return enemyName; } }
    #endregion
}