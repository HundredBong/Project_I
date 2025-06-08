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
    [SerializeField] private int spawnBatchSize = 20; //������ ���� ��
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
        SpawnManager.Instance.SpawnEnemiesForStage(currentStage, initalSpawnCount);
    }

    private void SpawnEnemies(int count)
    {
        SpawnManager.Instance.SpawnEnemiesForStage(currentStage, initalSpawnCount);
    }

    public void NotifyKill()
    {
        killCount++;

        //���� ���������� �Ѿ������ �ִ� ų ���� �������� �ʰ�, ���� ų ī��Ʈ�� ������ ���� ���� 0�̸� 
        if (killCount < totalKillsRequired && killCount % spawnBatchSize == 0)
        {
            //�����ŭ ���� �ٽ� ����
            SpawnEnemies(spawnBatchSize);
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
        //�÷��̾� ��ġ �ʱ�ȭ, ���� ���� ����
        killCount = 0;
        GameManager.Instance.player.transform.position = Vector3.zero;

        SpawnEnemies(initalSpawnCount);
    }

    public int GetCurrentStage()
    {
        return currentStage;
    }
}
