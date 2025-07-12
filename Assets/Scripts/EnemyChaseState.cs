using UnityEngine;

public class EnemyChaseState : IState
{
    private EnemyStateMachine owner;
    private Animator anim;

    public EnemyChaseState(EnemyStateMachine owner)
    {
        this.owner = owner;
    }

    public void OnEnter()
    {
        //Chase ���¿� ������ Move �ִϸ��̼� ����        
        anim = owner.enemy.Animator;

        if (anim != null)
        {
            anim.SetBool("1_Move", true);
        }
    }

    public void OnExit()
    {
        //Chase ���¸� ���� �� �ִϸ��̼��� ����
        //Attack���� ���ٸ� Attack�� OnEnter���� ó���ҰŰ�,
        //Idle�� ���ٸ� SetBool(false)�� �ϸ� �˾Ƽ� Idle �ִϸ��̼��� ���ðŰ�,
        //Dead�� ���ٸ� Dead�� OnEnter���� ó��
        //�׷� ���⼭�� Chase �ִϸ��̼� ���Ḹ ���ָ� ��

        if (anim != null)
        {
            anim.SetBool("1_Move", false);
        }
    }

    public void Update()
    {
        //�÷��̾ Dead���°� �ƴϸ� ����
        if (owner.enemy.PlayerReference.IsDead == false)
        {
            owner.enemy.transform.position = Vector2.MoveTowards(owner.enemy.transform.position,
            GameManager.Instance.player.transform.position, owner.enemy.moveSpeed * Time.deltaTime);
        }
        else
        {
            //�÷��̾ �׾����� Idle ���·� ����
            owner.ChangeState(StateType.Idle);
        }

        float distanceToPlayer = Vector2.Distance(owner.transform.position, owner.enemy.PlayerReference.transform.position);

        if (distanceToPlayer <= owner.enemy.attackRange)
        {
            //�÷��̾ ���� ������ ������ ���� ���·� ����
            owner.ChangeState(StateType.Attack);
        }

        if (owner.enemy.health <= 0)
        {
            owner.ChangeState(StateType.Dead);
        }
    }
}