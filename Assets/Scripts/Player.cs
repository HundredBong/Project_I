using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("스탯")]
    public float health;
    public float speed;
    public float maxHealth;
    public float maxSpeed;
    public float damage;
    public float critical;
    public float maxChaseDistance = 10;
    public float attackRange;

    //Private Field
    private Enemy targetEnemy;
    private float distanceToTarget;
    private Animator anim;
    private bool isAttacking;
    private bool isFlip;

    private Vector3 originScale;
    private Vector3 flipScale;

    private void Awake()
    {
        anim = GetComponent<Animator>();

        originScale = transform.localScale;
        Vector3 flipVector = new Vector3(-1f, 1f, 1f);
        flipScale = Vector3.Scale(transform.localScale, flipVector);
    }

    private void Update()
    {
        UpdateTarget();

        if (targetEnemy != null)
        {
            Move();
            TryStartAttack();
            FlipSprite();
            Debug.Log(targetEnemy.name);
        }
        else
        {
            anim.SetBool("1_Move", false);
            anim.SetBool("2_Attack", false);
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
            if (distance < shortest)
            {
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
        //타겟까지의 거리가 쫓아가는 거리보다 크고, 공격상태가 아니라면 이동
        if (distanceToTarget > maxChaseDistance && isAttacking == false)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetEnemy.transform.position, speed * Time.deltaTime);
            anim.SetBool("1_Move", true);
        }
        else
        {
            anim.SetBool("1_Move", false);
        }
    }

    private void TryStartAttack()
    {
        //타겟까지의 거리보다 공격 범위가 더 크면 공격
        if (distanceToTarget <= attackRange && targetEnemy.isDead == false)
        {
            isAttacking = true;
            anim.SetBool("2_Attack", true);
        }
        else
        {
            isAttacking = false;
            anim.SetBool("2_Attack", false);
        }
    }


    public void Attack()
    {
        //Attack 애니메이션의 이벤트로 실행됨

        if (targetEnemy != null && !targetEnemy.isDead)
        {
            targetEnemy.TakeDamage(damage);

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
