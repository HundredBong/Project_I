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
        //Idle 상태에 들어오면 애니메이션 실행
        //Animator anim = owner.enemy.animator;

        //if(anim != null)
        //{
        //    anim.SetBool("Idle", true);
        //}

        //애니메이터 Entry에서 Idle로 넘어감
        //애니메이션은 나중에 하고 일단 상태머신 만들기
    }

    public void OnExit()
    {

    }

    public void Update()
    {
        //Debug.Log("Enemy Idle State Update");  
        float distanceToPlayer = Vector2.Distance(owner.transform.position, owner.enemy.PlayerReference.transform.position);

        //플레이어와의 거리가 추적 범위 이내인지 확인
        if (distanceToPlayer <= owner.enemy.chaseRange)
        {
            //플레이어가 공격 범위에 들어오면 공격 상태로 전이
            owner.ChangeState(StateType.Chase);
        }

        if (owner.enemy.health <= 0)
        {
            owner.ChangeState(StateType.Dead);
        }
    }
}