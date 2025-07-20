using UnityEngine;

public class PlayerAttackState : IState
{
    private PlayerStateMachine owner;
    private Animator anim;

    public bool IsAttacking { get; private set; }

    public PlayerAttackState(PlayerStateMachine owner)
    {
        this.owner = owner;
    }

    public void OnEnter()
    {
        anim = owner.player.Animator;

        if (anim != null)
        {
            anim.SetBool("2_Attack", true);
        }
    }

    public void Update()
    {
        if (owner.player.TargetEnemy == null && IsAttacking == false)
        {
            owner.ChangeState(StateType.Idle);
            return;
        }

        float distanceToTarget = owner.player.DistanceToTarget;

        //공격중이 아니고, 공격범위 밖에 있다면 Chase 상태로 전이
        if (distanceToTarget >= owner.player.Stat.attackRange && IsAttacking == false)
        {
            owner.ChangeState(StateType.Chase);
        }

        if (owner.player.Stat.health <= 0)
        {
            //owner.ChangeState(StateType.Dead);
        }
    }

    public void OnExit()
    {
        anim.SetBool("2_Attack", false);
    }

    public void OnAttackStart()
    {
        IsAttacking = true;
    }

    public void OnAttackEnd()
    {
        IsAttacking = false;
    }

    public void OnAttackHit()
    {
        //Debug.Log("Player Attack Hit");

        //방향계산
        Vector3 rawDir = owner.player.TargetEnemy.transform.position - owner.player.transform.position;

        Vector3 dir = new Vector3(rawDir.x, 0f, 0f).normalized;

        //공격범위의 중심점 계산
        Vector3 center = owner.player.transform.position + (dir * owner.player.Stat.attackRange);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll
            (center, owner.player.Stat.attackRange, owner.player.targetLayerMask);

        //Debug.Log("Player Attack Hit Count : " + hitEnemies.Length);

        foreach (Collider2D col in hitEnemies)
        {
            Enemy enemy = col.GetComponent<Enemy>();

            if (enemy != null && enemy.isDead == false)
            {
                enemy.TakeDamage(owner.player.Stat.damage);
            }
        }
    }
}