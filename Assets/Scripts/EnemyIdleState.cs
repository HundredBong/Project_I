using UnityEngine;
using UnityEngine.Rendering;

public class EnemyIdleState : IState
{
    private EnemyStateMachine owner;

    public EnemyIdleState(EnemyStateMachine owner)
    {
        this.owner = owner;
    }

    public void OnEnter()
    {
        //Idle ���¿� ������ �ִϸ��̼� ����
        //Animator anim = owner.enemy.animator;

        //if(anim != null)
        //{
        //    anim.SetBool("Idle", true);
        //}

        //�ִϸ����� Entry���� Idle�� �Ѿ
        //�ִϸ��̼��� ���߿� �ϰ� �ϴ� ���¸ӽ� �����
    }

    public void OnExit()
    {

    }

    public void Update()
    {
        float distanceToPlayer = Vector2.Distance(owner.transform.position, GameManager.Instance.player.transform.position);

        //�÷��̾���� �Ÿ��� ���� ���� �̳����� Ȯ��
        if (distanceToPlayer <= owner.enemy.chaseRange)
        {
            //�÷��̾ ���� ������ ������ ���� ���·� ����
            owner.ChangeState(StateType.Chase);
        }
    }
}