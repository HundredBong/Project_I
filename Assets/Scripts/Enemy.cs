using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class Enemy : MonoBehaviour, IPooledObject
{
    [Header("기본 데이터")]
    [SerializeField] private EnemyId enemyId;
    public bool isDead = false;

    //private필드, 원활한 디버깅을 위해 public으로 함
    //TODO : private으로 변경
    //public EnemyType enemyType;
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

    private bool isFlip;

    public GameObject prefabReference { get; set; }

    public Animator Animator { get; private set; }

    public Player PlayerReference { get; private set; }

    public bool IsBoss { get; private set; }

    private EnemyStateMachine stateMachine;

    public Vector3 OriginScale { get; private set; }
    private Vector3 flipScale;

    private void Awake()
    {
        PlayerReference = GameManager.Instance.player;

        OriginScale = transform.localScale;
        Vector3 flipVector = new Vector3(-1f, 1f, 1f);
        flipScale = Vector3.Scale(transform.localScale, flipVector);

        if (PlayerReference == null)
        {
            Debug.LogError("[Enemy] Player가 Null임");
        }

        Animator = GetComponent<Animator>();

        if (Animator == null)
        {
            Debug.LogError("[Enemy] Animator 컴포넌트가 없음.");
        }

        stateMachine = GetComponent<EnemyStateMachine>();

        if (stateMachine == null)
        {
            Debug.LogError("[Enemy] EnemyStateMachine 컴포넌트가 없음.");
        }


    }


    private void OnEnable()
    {
        isDead = false;

        //Initialize();


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
        isDead = true;
    }

    private void Update()
    {
        if (PlayerReference != null && isDead == false)
        {
            FlipSprite();
        }
    }

    public async void Initialize()
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
        IsBoss = false;
    }

    public void InitializeBoss(StageData stageData, EnemyData enemyData)
    {
        Debug.Log($"보스 소환, 배율 : {stageData.BossStatRate}, 크기 : {transform.lossyScale}");
        IsBoss = true;
        maxHealth = enemyData.HP * stageData.BossStatRate;
        health = maxHealth;
        damage = enemyData.ATK * stageData.BossStatRate;
        defend = enemyData.DEF * stageData.BossStatRate;
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
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    public void OnAttackStart()
    {
        stateMachine?.CurrentAttackState?.OnAttackHit();
    }

    public void OnAttackHit()
    {
        stateMachine?.CurrentAttackState?.OnAttackHit();
    }

    public void OnAttackEnd()
    {
        stateMachine?.CurrentAttackState?.OnAttackEnd();
    }

    private void FlipSprite()
    {
        isFlip = PlayerReference?.transform.position.x - transform.position.x > 0 ? true : false;

        if (IsBoss == false)
        {
            transform.localScale = isFlip ? flipScale : OriginScale;
        }
        else
        {
            transform.localScale = isFlip ? flipScale * 2 : OriginScale * 2;
        }
    }
}
