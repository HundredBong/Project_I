using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    private IState currentState;
    private StateType currentKey = StateType.None;

    public Player player { get; private set; }

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void OnEnable()
    {
        ChangeState(StateType.Idle);
    }

    private void Update()
    {
        currentState?.Update();
    }

    public void ChangeState(StateType nextState)
    {
        if (currentKey == nextState)
        {
            return;
        }

        currentState?.OnExit();

        currentKey = nextState;
        currentState = CreateState(nextState);

        currentState.OnEnter();
    }

    private IState CreateState(StateType type)
    {
        return type switch
        {
            StateType.Idle => new PlayerIdleState(this),
            _ => null
        };
    }
}
