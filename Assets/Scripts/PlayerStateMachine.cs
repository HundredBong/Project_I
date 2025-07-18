using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    private IState currentState;
    [SerializeField] private StateType currentKey = StateType.None; //����׿� SerializedField

    public Player player { get; private set; }

    private SkillData _currentSkillData;

    public SkillData CurrentSkillData
    {
        get
        {
            return _currentSkillData;
        }
    }

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

    public void ChangeState(StateType nextState, SkillData skillData)
    {
        _currentSkillData = skillData;
        ChangeState(nextState); //���� �޼��� ȣ��
    }

    private IState CreateState(StateType type)
    {
        return type switch
        {
            StateType.Idle => new PlayerIdleState(this),
            StateType.Chase => new PlayerChaseState(this),
            StateType.Attack => new PlayerAttackState(this),
            StateType.Dead => new PlayerDeadState(this),
            StateType.Charge => new PlayerChargeState(this),
            _ => null
        };
    }

    public PlayerAttackState CurrentAttackState
    {
        get
        {
            return currentState as PlayerAttackState;
        }
    }

    [ContextMenu("�׽�Ʈ")]
    private void Test()
    {
        ChangeState(currentKey == StateType.Charge ? StateType.Idle : StateType.Charge);
    }
}