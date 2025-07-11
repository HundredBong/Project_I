using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Enemy targetEnemy;
    private float distanceToTarget;

    private bool isAttacking;
    private bool isFlip;

    private PlayerStats stats;
    private Vector3 originScale;
    private Vector3 flipScale;

    public bool IsDead { get; set; }

    public Animator Animator { get; private set; }

    private void Awake()
    {
        IsDead = false;

        Animator = GetComponent<Animator>();
        stats = GetComponent<PlayerStats>();

        originScale = transform.localScale;
        Vector3 flipVector = new Vector3(-1f, 1f, 1f);
        flipScale = Vector3.Scale(transform.localScale, flipVector);
    }

    private void OnEnable()
    {
        stats.health = stats.maxHealth;
    }

    private void Update()
    {
        UpdateTarget();

        if (targetEnemy != null)
        {
            TryTeleport(); 
            Move();
            TryStartAttack();
            FlipSprite();
        }
        else
        {
            Animator.SetBool("1_Move", false);
            Animator.SetBool("2_Attack", false);
            Animator.speed = 1;
        }
    }

    private void UpdateTarget()
    {
        //이미 타겟이 있고, 살아있다면 그대로 유지
        if (targetEnemy != null)
        {
            if (targetEnemy.isDead == false)
            {
                distanceToTarget = Vector3.Distance(transform.position, targetEnemy.transform.position);

                return;
            }
        }

        //타겟이 없거나 죽었을 경우 새 타겟 찾기
        float shortest = float.MaxValue;
        Enemy closestEnemy = null;

        foreach (Enemy enemy in GameManager.Instance.enemyList)
        {
            if (enemy == null || enemy.isDead == true) continue;

            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            //현재 거리보다 test가 크다면 
            if (distance < shortest)
            {
                //test를 현재 거리로 설정함. 추후 반복문 돌다가 더 짧은거리가 있으면 그걸 타겟으로 따라가게 됨.
                shortest = distance;
                closestEnemy = enemy;
            }
        }

        targetEnemy = closestEnemy;

        if (targetEnemy != null)
        {
            distanceToTarget = Vector3.Distance(transform.position, targetEnemy.transform.position);
        }
        else
        {
            distanceToTarget = Mathf.Infinity;
        }

    }

    private void Move()
    {
        //타겟까지의 거리가 추적 가능한 거리라면, 적이 충분히 가깝다면 이동함
        if (distanceToTarget <= stats.chaseRange && isAttacking == false)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetEnemy.transform.position, stats.moveSpeed * Time.deltaTime);
            Animator.SetBool("1_Move", true);
        }
        else
        {
            Animator.SetBool("1_Move", false);
        }
    }
    private void TryTeleport()
    {
        //타겟까지의 거리가 추적가능한 범위를 벗어나면 
        if (distanceToTarget > stats.chaseRange)
        {
            //텔레포트 위치는 타겟과의 거리 유지하면서 앞쪽으로
            Vector3 direction = (targetEnemy.transform.position - transform.position).normalized;
            Vector3 newPos = targetEnemy.transform.position - direction * stats.attackRange * 0.8f; //공격범위 조금 안쪽

            transform.position = newPos;
            Debug.LogError("텔레포트");
        }
    }
    private void TryStartAttack()
    {
        //타겟까지의 거리가 공격 가능한 범위 내라면
        if (distanceToTarget <= stats.attackRange && targetEnemy.isDead == false)
        {
            isAttacking = true;
            Animator.SetBool("2_Attack", true);
            Animator.speed = Animator.speed = stats.attackSpeed;
        }
        else
        {
            isAttacking = false;
            Animator.SetBool("2_Attack", false);
            Animator.speed = 1;
        }
    }


    public void Attack()
    {
        //Attack 애니메이션의 이벤트로 실행됨

        if (targetEnemy != null && !targetEnemy.isDead)
        {
            targetEnemy.TakeDamage(stats.damage);

            if (targetEnemy.isDead)
            {
                isAttacking = false;
            }
        }
    }

    private void FlipSprite()
    {
        isFlip = targetEnemy.transform.position.x - transform.position.x > 0 ? true : false;

        transform.localScale = isFlip ? flipScale : originScale;
    }

    public void GetExp(float exp)
    {
        stats.GetExp(exp);
    }

    public void GetGold(float gold)
    {
        stats.GetGold(gold);
    }

    private void OnDrawGizmos()
    {
        if (targetEnemy != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, targetEnemy.transform.position);
        }
    }

    public void TakeDamage(float damage)
    {

        stats.TakeDamage(damage); //내부 코드 -> health -= damage
        //if (stats.health <= 0)
        //{
        //    //IsDead = true;
        //    //상태머신 추가하면 ChangeState(StateType.Dead)
        //}
    }
}
