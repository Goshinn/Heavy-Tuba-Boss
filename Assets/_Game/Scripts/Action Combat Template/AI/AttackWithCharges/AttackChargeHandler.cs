using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Attach this component to an enemy whose attackList contains an EnemyAttack that inherits from AttackWithCharges.
/// Manages attack charges.
/// </summary>
public class AttackChargeHandler : MonoBehaviour
{
    [Header("Members")]
    private Dictionary<AttackWithCharges, int> attackChargesDictionary = new Dictionary<AttackWithCharges, int>();
    private Dictionary<AttackWithCharges, Coroutine> activeChargeRegenerationDictionary = new Dictionary<AttackWithCharges, Coroutine>();

    // Events
    public Action<AttackWithCharges> UsedCharge;

    public bool AttackHasUsableCharges(AttackWithCharges attackWithCharges)
    {
        if (attackChargesDictionary.ContainsKey(attackWithCharges))
        {
            return attackChargesDictionary[attackWithCharges] > 0;
        }
        else
        {
            AddAttackToDictionary(attackWithCharges);
            return true;
        }
    }

    public void ConsumeCharge(AttackWithCharges attackWithCharges)
    {
        // Decrement attack charge & start regenerating charge if not already regenerating
        attackChargesDictionary[attackWithCharges]--;
        //Debug.Log($"After charge consume: {attackChargesDictionary[attackWithCharges]}");

        if (!activeChargeRegenerationDictionary.ContainsKey(attackWithCharges))
        {
            Coroutine regenerateChargeCorout = StartCoroutine(CoolDownAttackCharge(attackWithCharges));
            activeChargeRegenerationDictionary.Add(attackWithCharges, regenerateChargeCorout);
        }
    }

    private IEnumerator CoolDownAttackCharge(AttackWithCharges attackWithCharges)
    {
        //Debug.Log($"Beginning charge regeneration for {attackWithCharges.name}...");
        while (attackChargesDictionary[attackWithCharges] < attackWithCharges.MaxStoredCharges)
        {
            yield return new WaitForSeconds(attackWithCharges.ChargeRegenerationDuration);
            attackChargesDictionary[attackWithCharges]++;
            //Debug.Log($"Regenerated an attack charge for {attackWithCharges.name}. Current charges: {attackChargesDictionary[attackWithCharges]}");
        }

        //Debug.Log($"Completed charge regeneration for {attackWithCharges.name}");
        activeChargeRegenerationDictionary.Remove(attackWithCharges);
    }

    private void AddAttackToDictionary(AttackWithCharges attackWithCharges)
    {
        attackChargesDictionary.Add(attackWithCharges, attackWithCharges.MaxStoredCharges);
    }
}
