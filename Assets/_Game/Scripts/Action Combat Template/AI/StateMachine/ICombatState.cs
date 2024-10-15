using UnityEngine;

public interface ICombatState
{
    void EnterCombatState(GameObject target);
    void OnEnterCombatState(GameObject target);
    void CombatState();
    void OnExitCombatState();
}
