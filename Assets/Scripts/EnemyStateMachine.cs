using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    private IState currentState;
    private StateType currentKey = StateType.None;

    public Enemy enemy { get; private set; }

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    private void OnEnable()
    {
        Debug.Log("EnemyStateMachine OnEnable ����"); 
        ChangeState(StateType.Idle);
    }

    private void Update()
    {
        //Debug.Log(currentState == null ? "���¸ӽ� ����" : "���¸ӽ� ����");
        currentState?.Update();
    }

    //���� ���� ���, ���� ������ OnExitȣ��, ���ο� ���¸� ����� OnEnter ȣ��
    public void ChangeState(StateType nextState)
    {
        Debug.Log($"ChangeState ���� : {nextState}");

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

        //���ο� ���¿� ������ �� �޼��� ����
        currentState.OnEnter();

    }

    private IState CreateState(StateType type)
    {
        //���°� �߰��� ������ ���⿡�� �߰�
        //Debug.Log($"CreateState ���� : {type}");
        return type switch
        {
            StateType.Idle => new EnemyIdleState(this),
            StateType.Chase => new EnemyChaseState(this),
            StateType.Attack => new EnemyAttackState(this),
            StateType.Dead => new EnemyDeadState(this),
            _ => null
        };
    }

    //[ContextMenu("�׽�Ʈ")]
    //private void Test()
    //{
    //    Debug.Log("�׽�Ʈ ����");
    //    currentState?.OnExit();

    //    currentKey = StateType.Chase;
    //    currentState = CreateState(StateType.Chase);

    //    currentState.OnEnter();
    //}
}

