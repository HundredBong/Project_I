using System.Collections.Generic;
using UnityEngine;

public class PlayerChaseState : IState
{
    private PlayerStateMachine owner;
    private Animator anim;

    public PlayerChaseState(PlayerStateMachine owner)
    {
        this.owner = owner;
    }

    public void OnEnter()
    {
        anim = owner.player.Animator;

        if (anim != null)
        {
            anim.SetBool("1_Move", true);
        }
    }

    public void Update()
    {
        //타겟이 없다면 Idle로 전이
        if (owner.player.TargetEnemy == null)
        {
            owner.ChangeState(StateType.Idle);
            return;
        }

        float distanceToTarget = owner.player.DistanceToTarget;

        //공격범위 안에 들어오면 Attack 상태로 전이
        if (distanceToTarget <= owner.player.Stat.attackRange)
        {
            owner.ChangeState(StateType.Attack);
            return;
        }

        //공격범위 밖에있고 추적범위 안에 있다면 이동
        if (distanceToTarget <= owner.player.Stat.chaseRange)
        {
            MoveTowardsTarget();
        }
        //추적 범위 밖이라면 텔레포트
        else
        {
            TeleportToTarget();
        }

        //체력이 0이하라면 Dead상태로 전이
        if (owner.player.Stat.health <= 0)
        {
            //owner.ChangeState(StateType.Dead);
        }
    }

    public void OnExit()
    {
        if (anim != null)
        {
            anim.SetBool("1_Move", false);
        }
    }

    private void MoveTowardsTarget()
    {

        owner.player.transform.position = Vector2.MoveTowards(owner.player.transform.position,
            owner.player.TargetEnemy.transform.position, owner.player.Stat.moveSpeed * Time.deltaTime);

    }

    private void TeleportToTarget()
    {
        Vector3 direction = (owner.player.TargetEnemy.transform.position - owner.player.transform.position).normalized;
        Vector3 newPos = owner.player.TargetEnemy.transform.position - direction * owner.player.Stat.attackRange * 0.8f; //공격범위 조금 안쪽
        owner.player.transform.position = newPos;
    }
}