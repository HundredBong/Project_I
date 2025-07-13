using UnityEngine;

public class PlayerIdleState : IState
{
    private PlayerStateMachine owner;

    public PlayerIdleState(PlayerStateMachine owner)
    {
        this.owner = owner;
    }

    public void OnEnter()
    {

    }

    public void OnExit()
    {

    }

    public void Update()
    {
        //TargetEnemy�� �����Ǹ� �������·� ����
        if (owner.player.TargetEnemy != null)
        {
            owner.ChangeState(StateType.Chase);
        }

        if (owner.player.Stat.health <= 0)
        {
            //owner.ChangeState(StateType.Dead);
        }
    }

}