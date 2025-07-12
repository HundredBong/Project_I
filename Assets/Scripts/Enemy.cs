using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class Enemy : MonoBehaviour, IPooledObject
{
    [Header("�⺻ ������")]
    [SerializeField] private EnemyId enemyId;
    public bool isDead = true;

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
    public float goldValue;
    public float chaseRange;

    public GameObject prefabReference { get; set; }

    public Animator animator { get; private set; }

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("[Enemy] Animator ������Ʈ�� ����.");
        }
    }


    private void OnEnable()
    {
        //�����½�ũ ���� OnDisable �������������� ���� isDead�� Init�ȿ��� �ϴ� �ȸ���, ��ġ �ű�
        isDead = false;

        Initialize();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.enemyList.Add(this);
        }
        else
        {
            Debug.LogError("[Enemy] GameManager �� Null��");
        }
    }

    private void OnDisable()
    {
        //�̰� ������ Ǯ���� ���� ��Ȱ��ȭ�� ������Ʈ�� ���ݴ���
        //�ٵ� �÷��̾�� activeInHierarchy == false�� �� ���� ���ϸ� ���� �ʳ�
        //�ƴ���, ���߿� Die �ִϸ��̼� �۾��� �� �ʿ���
        isDead = true;
    }

    private async void Initialize()
    {
        await UniTask.WaitUntil(() => StageManager.Instance != null);

        EnemyData enemyData = DataManager.Instance.GetEnemyData(enemyId);

        if (enemyData == null)
        {
            Debug.LogWarning($"[Enemy] {enemyId}�� ���� EnemyData�� �������� ����");
            return;
        }

        int stageId = StageManager.Instance.GetCurrentStage();
        StageData stageData = DataManager.Instance.stageDataTable[stageId];

        if (stageData == null)
        {
            Debug.LogWarning($"[Enemy] {stageId}�� ���� stageData�� �������� ����");
            return;
        }

        maxHealth = enemyData.HP * stageData.HPRate;
        health = maxHealth;
        damage = enemyData.ATK * stageData.ATKRate;
        defend = enemyData.DEF * stageData.DEFRate;
        moveSpeed = enemyData.SPD;
        attackRange = enemyData.Range;
        attackInterval = enemyData.AttackInterval;
        expValue = enemyData.EXP * stageData.RewardRate;
        goldValue = enemyData.Gold * stageData.RewardRate;
        chaseRange = enemyData.ChaseRange;

    }

    public void TakeDamage(float damage)
    {
        health -= damage;

        //Debug.Log($"[Enemy] {damage}�� ���ظ� ����, ���� ü�� : {health}");

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        GameManager.Instance.player.GetExp(expValue);
        GameManager.Instance.player.GetGold(goldValue);
        GameManager.Instance.enemyList.Remove(this);
        StageManager.Instance.NotifyKill();

        isDead = true;
        ObjectPoolManager.Instance.enemyPool.Return(this);
    }
}
