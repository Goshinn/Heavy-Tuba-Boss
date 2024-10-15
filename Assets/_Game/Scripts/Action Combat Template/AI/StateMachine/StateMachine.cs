using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    private IState currentlyRunningState;
    private IState previousState;

    public void ChangeState(IState newState)
    {
        currentlyRunningState?.Exit();
        previousState = currentlyRunningState;

        currentlyRunningState = newState;
        currentlyRunningState.Enter();
    }

    public void ExecuteUpdate()
    {
        currentlyRunningState?.Execute();
    }

    public void SwitchToPreviousState()
    {
        ChangeState(previousState);
    }

    public IState GetCurrentState()
    {
        return currentlyRunningState != null ? currentlyRunningState : null;
    }
}
