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
        //Ÿ���� ���ٸ� Idle�� ����
        if (owner.player.TargetEnemy == null)
        {
            owner.ChangeState(StateType.Idle);
            return;
        }

        float distanceToTarget = owner.player.DistanceToTarget;

        //���ݹ��� �ȿ� ������ Attack ���·� ����
        if (distanceToTarget <= owner.player.Stat.attackRange)
        {
            owner.ChangeState(StateType.Attack);
            return;
        }

        //���ݹ��� �ۿ��ְ� �������� �ȿ� �ִٸ� �̵�
        if (distanceToTarget <= owner.player.Stat.chaseRange)
        {
            MoveTowardsTarget();
        }
        //���� ���� ���̶�� �ڷ���Ʈ
        else
        {
            TeleportToTarget();
        }

        //ü���� 0���϶�� Dead���·� ����
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
        Vector3 newPos = owner.player.TargetEnemy.transform.position - direction * owner.player.Stat.attackRange * 0.8f; //���ݹ��� ���� ����
        owner.player.transform.position = newPos;
    }
}