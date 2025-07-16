using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{



    private bool isAttacking;
    public bool IsFlip { get; private set; }


    private Vector3 originScale;
    private Vector3 flipScale;

    public bool IsDead { get; set; }

    public Enemy TargetEnemy { get; private set; }

    public Animator Animator { get; private set; }

    public float DistanceToTarget { get; private set; }

    public PlayerStats Stat { get; private set; }

    public LayerMask targetLayerMask;

    private PlayerStateMachine stateMachine;

    private void Awake()
    {
        IsDead = false;

        Animator = GetComponent<Animator>();
        Stat = GetComponent<PlayerStats>();
        stateMachine = GetComponent<PlayerStateMachine>();

        originScale = transform.localScale;
        Vector3 flipVector = new Vector3(-1f, 1f, 1f);
        flipScale = Vector3.Scale(transform.localScale, flipVector);
    }

    private void OnEnable()
    {
        Stat.health = Stat.maxHealth;
    }

    private void Update()
    {
        if (TargetEnemy != null)
        {
            FlipSprite();
        }

        float shortestDistance = float.MaxValue;
        Enemy closestEnemy = null;

        foreach (Enemy enemy in GameManager.Instance.enemyList)
        {
            if (enemy == null || enemy.isDead)
            {
                continue;
            }

            float distance = Vector2.Distance(transform.position, enemy.transform.position);

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closestEnemy = enemy;
            }
        }

        TargetEnemy = closestEnemy;
        DistanceToTarget = shortestDistance;
    }

    public void OnAttackStart()
    {
        stateMachine?.CurrentAttackState?.OnAttackStart();
    }

    public void OnAttackEnd()
    {
        stateMachine?.CurrentAttackState?.OnAttackEnd();
    }

    public void OnAttackHit()
    {
        stateMachine?.CurrentAttackState?.OnAttackHit();
    }

    private void FlipSprite()
    {
        IsFlip = TargetEnemy.transform.position.x - transform.position.x > 0 ? true : false;

        transform.localScale = IsFlip ? flipScale : originScale;
    }

    public void GetExp(float exp)
    {
        Stat.GetExp(exp);
    }

    public void GetGold(float gold)
    {
        Stat.GetGold(gold);
    }

    private void OnDrawGizmos()
    {
        if (TargetEnemy != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, TargetEnemy.transform.position);
        }

        //공격 범위 시각화
        //Vector3 dir = (TargetEnemy.transform.position - transform.position).normalized;
        //Vector3 center = transform.position + (dir * Stat.attackRange);

        //Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(center, Stat.attackRange);

        if (TargetEnemy != null)
        {
            Vector3 rawDir = TargetEnemy.transform.position - transform.position;
            Vector3 dir = new Vector3(rawDir.x, 0f, 0f).normalized;
            Vector3 center = transform.position + dir * Stat.attackRange;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(center, Stat.attackRange);
        }

    }

    public void TakeDamage(float damage)
    {

        Stat.TakeDamage(damage); //내부 코드 -> health -= damage

        //if (stats.health <= 0)
        //{
        //    //IsDead = true;
        //    //상태머신 추가하면 ChangeState(StateType.Dead)
        //}
    }
}
