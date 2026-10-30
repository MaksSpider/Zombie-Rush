using UnityEngine;
using System.Collections.Generic;

public class State : ScriptableObject
{
    public virtual void OnEnterState()
    {

    }

    public virtual void OnExitState()
    {

    }

    public virtual void Tick()
    {

    }

    public virtual void FixedTick()
    {

    }

    public virtual void Tick05s()
    {

    }

    public virtual void Tick1s()
    {

    }

    public virtual void Tick5s()
    {

    }

    public virtual void Tick10s()
    {

    }

    public virtual void Tick30s()
    {

    }

    public virtual void Tick1m()
    {

    }
}

public class StateMachine : MonoBehaviour
{
    [SerializeField] private List<State> _states = new List<State>();
    public State CurrentState = null;

    private float _timer05s;
    private float _timer1s;
    private float _timer5s;
    private float _timer10s;
    private float _timer30s;
    private float _timer1m;

    public void SwitchState<aState>()
    {
        print(typeof(aState));
        foreach(State s in _states)
        {
            if (s.GetType() == typeof(aState))
            {
                CurrentState?.OnExitState();
                CurrentState = s;
                CurrentState.OnEnterState();
                return;
            }
        }

        Debug.LogWarning("State does not exist!");
    }

    public virtual void UpdateFixedStateMachine()
    {
        CurrentState?.FixedTick();
    }

    public virtual void UpdateStateMachine()
    {
        CurrentState?.Tick();

        float deltaTime = Time.deltaTime;

        _timer05s += deltaTime;
        _timer1s += deltaTime;
        _timer5s += deltaTime;
        _timer10s += deltaTime;
        _timer30s += deltaTime;
        _timer1m += deltaTime;

        if (_timer05s >= 0.5f)
        {
            _timer05s -= 0.5f;
            CurrentState?.Tick05s();
        }

        if (_timer1s >= 1f)
        {
            _timer1s -= 1f;
            CurrentState?.Tick1s();
        }

        if (_timer5s >= 5f)
        {
            _timer5s -= 5f;
            CurrentState?.Tick5s();
        }

        if (_timer10s >= 10f)
        {
            _timer10s -= 10f;
            CurrentState?.Tick10s();
        }

        if (_timer30s >= 30f)
        {
            _timer30s -= 30f;
            CurrentState?.Tick30s();
        }

        if (_timer1m >= 60f)
        {
            _timer1m -= 60f;
            CurrentState?.Tick1m();
        }
    }

    public bool IsState<aState>()
    {
        if(!CurrentState) return false;
        return CurrentState.GetType() == typeof(aState);
    }
}
