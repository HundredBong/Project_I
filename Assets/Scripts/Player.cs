using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("����")]
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
        //�̹� Ÿ���� �ְ�, ����ִٸ� �״�� ����
        if (targetEnemy != null)
        {
            if (targetEnemy.isDead == false)
            {
                distanceToTarget = Vector3.Distance(transform.position, targetEnemy.transform.position);

                return;
            }
        }

        //Ÿ���� ���ų� �׾��� ��� �� Ÿ�� ã��
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
        //Ÿ�ٱ����� �Ÿ��� �Ѿư��� �Ÿ����� ũ��, ���ݻ��°� �ƴ϶�� �̵�
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
        //Ÿ�ٱ����� �Ÿ����� ���� ������ �� ũ�� ����
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
        //Attack �ִϸ��̼��� �̺�Ʈ�� �����

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
        //���� ����� �׸��°� ���� ��վ�� 
        if (targetEnemy != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, targetEnemy.transform.position);
        }
    }
}
