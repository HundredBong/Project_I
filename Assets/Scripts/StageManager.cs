using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [SerializeField] private int currentStage = 1; //���� ��������
    [SerializeField] private int killCount = 0; //���� ų ī��Ʈ
    [SerializeField] private int totalKillsRequired = 100; //�������� Ŭ��� �ʿ��� ų ī��Ʈ
    [SerializeField] private int spawnBatchSize = 20; //������� ���� ��
    [SerializeField] private int initalSpawnCount = 40; //�ʱ� ���� ��

    public bool isLoop; //���� ���������� �ݺ��� �������� ���� ����

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadStageData();
        StartStage();
    }

    private void LoadStageData()
    {
        //�ϴ� �����ϰ� ���� ���̾�̽����� �ҷ�����
        currentStage = 1;
        killCount = 0;
    }

    private void StartStage()
    {
        killCount = 0;

        SpawnManager.Instance.SpawnEnemiesForStage(GetStageType(currentStage), initalSpawnCount);
    }


    public void NotifyKill()
    {
        killCount++;

        //���� ���������� �Ѿ������ �ִ� ų ���� �������� �ʰ�, ���� ų ī��Ʈ�� ������ ���� ���� 0�̸� 
        //20 < 100 && 20 % 20 == 0 
        if (killCount < totalKillsRequired && killCount % spawnBatchSize == 0)
        {
            //�����ŭ ���� �ٽ� ����
            Debug.Log("[StageManager] ���� �����");
            SpawnManager.Instance.SpawnEnemiesForStage(GetStageType(currentStage), spawnBatchSize);
        }

        //���� ų ī��Ʈ�� �������� Ŭ��� �ʿ��Ѹ�ŭ �����ϸ�
        if (killCount >= totalKillsRequired)
        {
            OnStageClear();
        }
    }

    private void OnStageClear()
    {
        Debug.Log($"[StageManager] {currentStage}Ŭ���� ��");

        if (isLoop == false)
        {
            currentStage++;
        }

        SaveStageData();
        ResetStage();
    }

    private void SaveStageData()
    {
        //���̾�̽��� �����ؾ���
    }

    private void ResetStage()
    {
        Debug.Log($"[SpawnManager] �������� ����");
        //�÷��̾� ��ġ �ʱ�ȭ, ���� ���� ����
        killCount = 0;

        GameManager.Instance.player.transform.position = Vector3.zero;

        SpawnManager.Instance.SpawnEnemiesForStage(GetStageType(currentStage), initalSpawnCount);
    }

    private StageType GetStageType(int stageNumber)
    {
        //1 ~ 100 ������ 0 -> Forest ��ȯ
        //101~200 ������ 1 -> �ٸ� �������� ��ȯ
        //��� �̰� �������� 10�� ������ �� ���� �´°ǰ� �Ӹ��� �ȵ��� ��
        //881 ������ 880 / (100 % 2) => 880 / 0 = 0
        //151 ������ 150 / 100 % 2 => 200 / 0 = 0
        //�� 0�� ���� �ʳ�
        //�ƴ� ���� 
        //880 / 100 % 2 => 8 % 2 = 0 ??
        //150 / 100 % 2 => 1 % 2 = 1 ��

        return (StageType)(((stageNumber - 1) / 100) % Enum.GetValues(typeof(StageType)).Length);
    }
}
