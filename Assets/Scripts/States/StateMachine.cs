using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem.XR.Haptics;

public class State : MonoBehaviour
{
    public virtual void EnterState()
    {

    }

    public virtual void ExitState()
    {

    }

    public virtual void UpdateState()
    {

    }
}

public class StateMachine : MonoBehaviour
{
    public List<State> states = new List<State>();
    public State CurrentState = null;

    public void SwitchState<aState>()
    {
        foreach(State s  in states)
        {
            if(s.GetType() == typeof(aState))
            {
                CurrentState?.ExitState();
                CurrentState = s;
                CurrentState.EnterState();
                return;
            }
        }

        Debug.LogWarning("State does not exist!");
    }

    public virtual void UpdateStateMachine()
    {
        CurrentState?.UpdateState();
    }

    public bool IsState<aState>()
    {
        if(!CurrentState) return false;
        return CurrentState.GetType() == typeof(aState);
    }
}
