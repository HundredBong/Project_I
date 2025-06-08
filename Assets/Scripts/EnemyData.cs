using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    public EnemyType type;

    [Header("기본 스탯")]
    public float maxHealth = 10f;
    public float moveSpeed = 1f;
    public float attackRange = 1f;
    public float attackDamage = 2f;
    public float chaseRange = 5f;
    public float attackInterval = 1f; //애니메이션 이벤트로 쓸 수도 있고, 딜레이 콜 매니저로 쓸 수도 있으니 일단 추가함.

    [Header("보상")]
    //currentStage에서 정해질 수 있으나 일단 추가함,
    //예를들어 Enemy의 Die메서드가 실행될 때
    //GameManager.Instance.player.GetExp(tageManager.Instance.currentStage) 등의 공식으로
    public float expValue = 1f;
    public float goldValue = 1f;

    [Header("투사체 (원거리 몬스터 전용)")]
    public GameObject projectilePrefab;
}
