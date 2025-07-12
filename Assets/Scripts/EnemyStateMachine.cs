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

    //���� ���� ���, ���� ������ OnExitȣ��, ���ο� ���¸� ����� OnEnter ȣ��
    public void ChangeState(StateType nextState)
    {
        //������ ���°� ���� ���¿� ���ٸ� �ƹ��͵� ���� ����
        if (currentKey == nextState)
        {
            return;
        }

        //���¿��� ���� �� �޼��� ����
        currentState?.OnExit();
        //���� ����
        currentKey = nextState;
        currentState = CreateState(nextState);
        //���¿� ������ �� �޼��� ����
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

