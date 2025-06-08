using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    public EnemyType type;

    [Header("�⺻ ����")]
    public float maxHealth = 10f;
    public float moveSpeed = 1f;
    public float attackRange = 1f;
    public float attackDamage = 2f;
    public float chaseRange = 5f;
    public float attackInterval = 1f; //�ִϸ��̼� �̺�Ʈ�� �� ���� �ְ�, ������ �� �Ŵ����� �� ���� ������ �ϴ� �߰���.

    [Header("����")]
    //currentStage���� ������ �� ������ �ϴ� �߰���,
    //������� Enemy�� Die�޼��尡 ����� ��
    //GameManager.Instance.player.GetExp(tageManager.Instance.currentStage) ���� ��������
    public float expValue = 1f;
    public float goldValue = 1f;

    [Header("����ü (���Ÿ� ���� ����)")]
    public GameObject projectilePrefab;
}
