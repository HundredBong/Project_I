using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IPooledObject
{
    [Header("�⺻ ������")]
    [SerializeField] private EnemyId enemyId;
    [HideInInspector] public bool isDead = false;

    //private�ʵ�, ��Ȱ�� ������� ���� public���� ��
    //TODO : private���� ����
    public EnemyType enemyType;
    public float health;
    public float maxHealth;
    public float damage;
    public float defend;
    public float moveSpeed;
    public float attackRange;
    public float attackInterval;
    public float expValue = 1f;


    public GameObject prefabReference { get; set; }

    //[HideInInspector] public Enemy prefabReference;

    private void OnEnable()
    {
        Initialize();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.enemyList.Add(this);
        }
        else
        {
            Debug.LogError("[Enemy] GameManager �� Null��");
        }

        health = maxHealth;
        isDead = false;
    }

    private void Initialize()
    {
        EnemyData data = DataManager.Instance.GetEnemyData(enemyId);

        if (data == null)
        {
            Debug.LogWarning($"[Enemy] {enemyId}�� ���� EnemyData�� �������� ����");
            return;
        }

        //TODO : StageData�� Rate���̶� �����ϱ�
        maxHealth = data.HP;
        health = maxHealth;
        damage = data.ATK;
        defend = data.DEF;
        moveSpeed = data.SPD;
        attackRange = data.Range;
        attackInterval = data.AttackInterval;
        expValue = data.EXP;
        isDead = false;
        expValue = 1f;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        GameManager.Instance.player.GetExp(expValue);
        GameManager.Instance.enemyList.Remove(this);
        StageManager.Instance.NotifyKill();

        isDead = true;
        ObjectPoolManager.Instance.enemyPool.Return(this);
    }
}
