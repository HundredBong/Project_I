using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    //이 클래스는 변수에 따라 행동만 하도록, 변수는 다른곳에서 불러서 쓰기

    //Private Field, 원활한 디버그를 위해 public으로 해놓고 추후 전부 private으로 바꿀거임
    private Enemy targetEnemy;
    private float distanceToTarget;
    private Animator anim;
    private bool isAttacking;
    private bool isFlip;

    private PlayerStats stats;
    private Vector3 originScale;
    private Vector3 flipScale;

    private void Awake()
    {
        anim = GetComponent<Animator>();
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
            TryTeleport(); //추가
            Move();
            TryStartAttack();
            FlipSprite();
            //Debug.Log(targetEnemy.name);
        }
        else
        {
            anim.SetBool("1_Move", false);
            anim.SetBool("2_Attack", false);
            anim.speed = 1;
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
            anim.SetBool("1_Move", true);
        }
        else
        {
            anim.SetBool("1_Move", false);
        }
    }
    private void TryTeleport()
    {
        //타겟까지의 거리가 추적가능한 범위를 벗어나면 
        if (distanceToTarget > stats.chaseRange)
        {
            //텔레포트 위치는 타겟과의 거리 유지하면서 앞쪽으로
            Vector3 direction = (targetEnemy.transform.position - transform.position).normalized;
            //TODO : 계산식이 무슨 뜻인지 알아보기
            Vector3 newPos = targetEnemy.transform.position - direction * stats.attackRange * 0.8f; //공격범위 조금 안쪽

            transform.position = newPos;
            //Debug.Log("텔레포트");
        }
    }
    private void TryStartAttack()
    {
        //타겟까지의 거리가 공격 가능한 범위 내라면
        if (distanceToTarget <= stats.attackRange && targetEnemy.isDead == false)
        {
            isAttacking = true;
            anim.SetBool("2_Attack", true);
            anim.speed = anim.speed = stats.attackSpeed;
        }
        else
        {
            isAttacking = false;
            anim.SetBool("2_Attack", false);
            anim.speed = 1;
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

    private void OnDrawGizmos()
    {
        //저는 기즈모 그리는게 제일 재밌어요 
        if (targetEnemy != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, targetEnemy.transform.position);
        }
    }
}
