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
        //Chase 상태에 들어오면 Move 애니메이션 실행        
        anim = owner.enemy.Animator;

        if (anim != null)
        {
            anim.SetBool("1_Move", true);
        }
    }

    public void OnExit()
    {
        //Chase 상태를 나갈 때 애니메이션을 종료
        //Attack으로 간다면 Attack의 OnEnter에서 처리할거고,
        //Idle로 간다면 SetBool(false)만 하면 알아서 Idle 애니메이션이 나올거고,
        //Dead로 간다면 Dead의 OnEnter에서 처리
        //그럼 여기서는 Chase 애니메이션 종료만 해주면 됨

        if (anim != null)
        {
            anim.SetBool("1_Move", false);
        }
    }

    public void Update()
    {
        //플레이어가 Dead상태가 아니면 추적
        if (owner.enemy.PlayerReference.IsDead == false)
        {
            owner.enemy.transform.position = Vector2.MoveTowards(owner.enemy.transform.position,
            GameManager.Instance.player.transform.position, owner.enemy.moveSpeed * Time.deltaTime);
        }
        else
        {
            //플레이어가 죽었으면 Idle 상태로 전이
            owner.ChangeState(StateType.Idle);
        }

        float distanceToPlayer = Vector2.Distance(owner.transform.position, owner.enemy.PlayerReference.transform.position);

        if (distanceToPlayer <= owner.enemy.attackRange)
        {
            //플레이어가 공격 범위에 들어오면 공격 상태로 전이
            owner.ChangeState(StateType.Attack);
        }

        if (owner.enemy.health <= 0)
        {
            owner.ChangeState(StateType.Dead);
        }
    }
}