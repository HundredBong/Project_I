using UnityEngine;

public class PlayerAttackState : IState
{
    private PlayerStateMachine owner;
    private Animator anim;
    private readonly Collider2D[] _buffer = new Collider2D[64];

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

            float attackSpeed = Mathf.Max(1f, owner.player.Stat.attackSpeed);
            anim.SetFloat("AttackSpeed", attackSpeed);
        }
    }

    public void Update()
    {
        if (anim != null)
        {
            float attackSpeed = Mathf.Max(1f, owner.player.Stat.attackSpeed);
            anim.SetFloat("AttackSpeed", attackSpeed);
        }

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
            owner.ChangeState(StateType.Dead);
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

        //방향계산
        if (owner.player.TargetEnemy == null) { return; }

        int count = Physics2D.OverlapCircleNonAlloc(owner.player.transform.position, owner.player.Stat.attackRange, _buffer, owner.player.targetLayerMask);

        float baseDamage = owner.player.Stat.damage; //베이스 대미지
        float chance = Mathf.Clamp01(owner.player.Stat.criticalChance); //크리 확률 가져오기
        float criBonus = owner.player.Stat.criticalDamage; //크리 대미지 가져오기

        //Random.value는 1 안나옴
        bool isCritical = Random.value < chance;

        //최종 대미지 계산, 크리일시 baseDamage로, 아니면 baseDamage에다가 크리티컬 보너스 추가
        float finalDamage = isCritical ? baseDamage * (2f + (criBonus * 0.01f)) : baseDamage;

        //for (int i = 0; i < count; i++)
        //{
        //    Collider2D col = _buffer[i];
        //    if (col != null && col.TryGetComponent<Enemy>(out Enemy enemy) && enemy.isDead == false)
        //    {
        //        enemy.TakeDamage(finalDamage);
        //    }
        //}
        Vector3 rawDir = owner.player.TargetEnemy.transform.position - owner.player.transform.position;

        Vector3 dir = new Vector3(rawDir.x, 0f, 0f).normalized;

        Vector3 center = owner.player.transform.position + (dir * owner.player.Stat.attackRange);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll
            (center, owner.player.Stat.attackRange, owner.player.targetLayerMask);
        foreach (Collider2D col in hitEnemies)
        {
            Enemy enemy = col.GetComponent<Enemy>();

            if (enemy != null && enemy.isDead == false)
            {
                enemy.TakeDamage(finalDamage);
            }
        }

        //ObjectPoolManager.Instance.audioPool.GetAudio().PlaySFX("Player_Attack_Hit");
    }
}