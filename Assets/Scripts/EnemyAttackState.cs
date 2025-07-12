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

        //공격중이 아니고, 플레이어가 공격 범위 밖이라면 추적 상태로 전이
        if (IsAttacking == false && owner.enemy.attackRange <= distanceToPlayer)
        {
            owner.ChangeState(StateType.Chase);
        }

        //플레이어가 죽었으면 Idle 상태로 전이
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
        //애니메이션 이벤트로 실행 1
        IsAttacking = true;
    }

    public void OnAttackHit()
    {
        //애니메이션 이벤트로 실행 2

        owner.enemy.PlayerReference.TakeDamage(owner.enemy.damage);
    }

    public void OnAttackEnd()
    {
        //애니메이션 이벤트로 실행 3

        IsAttacking = false;
    }
}