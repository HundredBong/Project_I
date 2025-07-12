using UnityEngine;

public class EnemyAttackState : IState
{
    private EnemyStateMachine owner;
    private Animator anim;
    public bool IsAttacking { get; private set; } 

    public EnemyAttackState(EnemyStateMachine owner)
    {
        IsAttacking = true;
        this.owner = owner;
    }

    public void OnEnter()
    {
        anim = owner.enemy.Animator;

        if(anim != null)
        {
            anim.SetBool("2_Attack",true);
        }
    }

    public void OnExit()
    {
        if (anim != null)
        {
            anim.SetBool("2_Attack", false);
        }
    }

    public void Update()
    {
        float distanceToPlayer = Vector2.Distance(owner.transform.position, owner.enemy.PlayerReference.transform.position);

        //�������� �ƴϰ�, �÷��̾ ���� ���� ���̶�� ���� ���·� ����
        if (IsAttacking == false && owner.enemy.attackRange <= distanceToPlayer)
        {
            owner.ChangeState(StateType.Chase);
        }

        //�÷��̾ �׾����� Idle ���·� ����
        if (IsAttacking == false && owner.enemy.PlayerReference.IsDead == true)
        {
            owner.ChangeState(StateType.Idle);
        }

        if (owner.enemy.health <= 0)
        {
            owner.ChangeState(StateType.Dead);
        }
    }

    public void OnAttackStart()
    {
        //�ִϸ��̼� �̺�Ʈ�� ���� 1
        IsAttacking = true;
    }

    public void OnAttackHit()
    {
        //�ִϸ��̼� �̺�Ʈ�� ���� 2

        owner.enemy.PlayerReference.TakeDamage(owner.enemy.damage);
    }

    public void OnAttackEnd()
    {
        //�ִϸ��̼� �̺�Ʈ�� ���� 3

        IsAttacking = false;
    }
}