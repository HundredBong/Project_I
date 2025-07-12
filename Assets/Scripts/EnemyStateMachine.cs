using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    private IState currentState;
    private StateType currentKey;

    public Enemy enemy { get; private set; }    

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    private void OnEnable()
    {
        ChangeState(StateType.Idle);
    }

    private void Update()
    {
        currentState?.Update();
    }

    //상태 전이 담당, 이전 상태의 OnExit호출, 새로운 상태를 만들고 OnEnter 호출
    public void ChangeState(StateType nextState)
    {
        //전이할 상태가 현재 상태와 같다면 아무것도 하지 않음
        if (currentKey == nextState)
        {
            return;
        }

        //상태에서 나갈 때 메서드 실행
        currentState?.OnExit();
        //상태 전이
        currentKey = nextState;
        currentState = CreateState(nextState);
        //상태에 진입할 때 메서드 실행
        currentState.OnEnter();

    }

    private IState CreateState(StateType type)
    {
        return type switch
        {
            StateType.Idle => new EnemyIdleState(this),
            _ => null
        };
    }
}

