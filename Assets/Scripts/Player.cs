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

            //���� �Ÿ����� test�� ũ�ٸ� 
            if (distance < shortest)
            {
                //test�� ���� �Ÿ��� ������. ���� �ݺ��� ���ٰ� �� ª���Ÿ��� ������ �װ� Ÿ������ ���󰡰� ��.
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
        //Ÿ�ٱ����� �Ÿ��� ���� ������ �Ÿ����, ���� ����� �����ٸ� �̵���
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
        //Ÿ�ٱ����� �Ÿ��� ���������� ������ ����� 
        if (distanceToTarget > stats.chaseRange)
        {
            //�ڷ���Ʈ ��ġ�� Ÿ�ٰ��� �Ÿ� �����ϸ鼭 ��������
            Vector3 direction = (targetEnemy.transform.position - transform.position).normalized;
            Vector3 newPos = targetEnemy.transform.position - direction * stats.attackRange * 0.8f; //���ݹ��� ���� ����

            transform.position = newPos;
            Debug.LogError("�ڷ���Ʈ");
        }
    }
    private void TryStartAttack()
    {
        //Ÿ�ٱ����� �Ÿ��� ���� ������ ���� �����
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
        //Attack �ִϸ��̼��� �̺�Ʈ�� �����

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

        stats.TakeDamage(damage); //���� �ڵ� -> health -= damage
        //if (stats.health <= 0)
        //{
        //    //IsDead = true;
        //    //���¸ӽ� �߰��ϸ� ChangeState(StateType.Dead)
        //}
    }
}
