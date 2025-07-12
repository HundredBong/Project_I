using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    private IState currentState;
    [SerializeField] private StateType currentKey = StateType.None; //디버깅용 SerializedField

    public Enemy enemy { get; private set; }

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    private void OnEnable()
    {
        Debug.Log("EnemyStateMachine OnEnable 진입"); 
        ChangeState(StateType.Idle);
    }

    private void Update()
    {
        //Debug.Log(currentState == null ? "상태머신 없음" : "상태머신 있음");
        currentState?.Update();
    }

    //상태 전이 담당, 이전 상태의 OnExit호출, 새로운 상태를 만들고 OnEnter 호출
    public void ChangeState(StateType nextState)
    {
        Debug.Log($"ChangeState 진입 : {nextState}");

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

        //새로운 상태에 진입할 때 메서드 실행
        currentState.OnEnter();

    }

    private IState CreateState(StateType type)
    {
        //상태가 추가될 떄마다 여기에도 추가
        //Debug.Log($"CreateState 진입 : {type}");
        return type switch
        {
            StateType.Idle => new EnemyIdleState(this),
            StateType.Chase => new EnemyChaseState(this),
            StateType.Attack => new EnemyAttackState(this),
            StateType.Dead => new EnemyDeadState(this),
            _ => null
        };
    }

    public EnemyAttackState CurrentAttackState
    {
        get
        {
            return currentState as EnemyAttackState; //IState 상속
        }
    }

    //[ContextMenu("테스트")]
    //private void Test()
    //{
    //    Debug.Log("테스트 실행");
    //    currentState?.OnExit();

    //    currentKey = StateType.Chase;
    //    currentState = CreateState(StateType.Chase);

    //    currentState.OnEnter();
    //}
}

