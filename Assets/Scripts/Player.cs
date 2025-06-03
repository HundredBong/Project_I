using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("�̵� �� ����")]
    public float moveSpeed = 5;
    public float attackSpeed = 1;
    public float attackRange = 2;
    public float chaseRange = 10;

    [Header("ü��")]
    public float health;
    public float maxHealth = 10;

    [Header("�����")]
    public float damage = 1;
    public float critical = 0;

    [Header("����")]
    public int level = 1;
    public float currentExp = 0;
    public float maxExp = 10;

    //Private Field, ��Ȱ�� ����׸� ���� public���� �س��� ���� ���� private���� �ٲܰ���
    public Enemy targetEnemy;
    public float distanceToTarget;
    public Animator anim;
    public bool isAttacking;
    public bool isFlip;

    private Vector3 originScale;
    private Vector3 flipScale;

    private void Awake()
    {
        anim = GetComponent<Animator>();

        originScale = transform.localScale;
        Vector3 flipVector = new Vector3(-1f, 1f, 1f);
        flipScale = Vector3.Scale(transform.localScale, flipVector);
    }

    private void OnEnable()
    {
        health = maxHealth;
    }

    private void Update()
    {
        UpdateTarget();

        if (targetEnemy != null)
        {
            TryTeleport(); //�߰�
            Move();
            TryStartAttack();
            FlipSprite();
            Debug.Log(targetEnemy.name);
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
        if (distanceToTarget <= chaseRange && isAttacking == false)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetEnemy.transform.position, moveSpeed * Time.deltaTime);
            anim.SetBool("1_Move", true);
        }
        else
        {
            anim.SetBool("1_Move", false);
        }
    }
    private void TryTeleport()
    {
        //Ÿ�ٱ����� �Ÿ��� ���������� ������ ����� 
        if (distanceToTarget > chaseRange)
        {
            //�ڷ���Ʈ ��ġ�� Ÿ�ٰ��� �Ÿ� �����ϸ鼭 ��������
            Vector3 direction = (targetEnemy.transform.position - transform.position).normalized;
            //TODO : ������ ���� ������ �˾ƺ���
            Vector3 newPos = targetEnemy.transform.position - direction * attackRange * 0.8f; //���ݹ��� ���� ����

            transform.position = newPos;
            Debug.Log("�ڷ���Ʈ");
        }
    }
    private void TryStartAttack()
    {
        //Ÿ�ٱ����� �Ÿ��� ���� ������ ���� �����
        if (distanceToTarget <= attackRange && targetEnemy.isDead == false)
        {
            isAttacking = true;
            anim.SetBool("2_Attack", true);
            anim.speed = anim.speed = attackSpeed;
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

    public void GetExp(float exp)
    {
        currentExp += exp;
        Debug.Log($"{exp} ����ġ ȹ����, {currentExp} / {maxExp}");
        while (currentExp >= maxExp)
        {
            currentExp -= maxExp;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        level++;
        maxExp *= 1.2f;

        Debug.Log($"���� �����, ���� ���� : {level}");
        //���� ������� �ø���, ü���� �ø���, ������ �ִ� ���⿡ �ۼ�


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
