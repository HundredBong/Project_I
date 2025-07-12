using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class Enemy : MonoBehaviour, IPooledObject
{
    [Header("기본 데이터")]
    [SerializeField] private EnemyId enemyId;
    public bool isDead = true;

    //private필드, 원활한 디버깅을 위해 public으로 함
    //TODO : private으로 변경
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
            Debug.LogError("[Enemy] Animator 컴포넌트가 없음.");
        }
    }


    private void OnEnable()
    {
        //유니태스크 쓰니 OnDisable 순서문제인지는 몰라도 isDead를 Init안에서 하니 안먹음, 위치 옮김
        isDead = false;

        Initialize();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.enemyList.Add(this);
        }
        else
        {
            Debug.LogError("[Enemy] GameManager 가 Null임");
        }
    }

    private void OnDisable()
    {
        //이거 없으니 풀에서 나온 비활성화된 오브젝트가 공격당함
        //근데 플레이어에서 activeInHierarchy == false일 때 공격 안하면 되지 않나
        //아니지, 나중에 Die 애니메이션 작업할 때 필요함
        isDead = true;
    }

    private async void Initialize()
    {
        await UniTask.WaitUntil(() => StageManager.Instance != null);

        EnemyData enemyData = DataManager.Instance.GetEnemyData(enemyId);

        if (enemyData == null)
        {
            Debug.LogWarning($"[Enemy] {enemyId}에 대한 EnemyData가 존재하지 않음");
            return;
        }

        int stageId = StageManager.Instance.GetCurrentStage();
        StageData stageData = DataManager.Instance.stageDataTable[stageId];

        if (stageData == null)
        {
            Debug.LogWarning($"[Enemy] {stageId}에 대한 stageData가 존재하지 않음");
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

        //Debug.Log($"[Enemy] {damage}의 피해를 받음, 남은 체력 : {health}");

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
