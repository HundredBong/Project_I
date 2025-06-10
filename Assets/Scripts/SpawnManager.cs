using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor.Presets;
using UnityEngine;
using static Cinemachine.Editor.CinemachineLensPresets;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

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

    public void SpawnEnemiesForCurrentStage(int count)
    {
        int stageId = StageManager.Instance.GetCurrentStage();
        StageData stage = DataManager.Instance.stageDataTable[stageId];

        if (stage == null)
        {
            Debug.LogError("[SpawnManager] ���� �������� ������ ����");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            EnemyId enemyId = stage.Enemies[Random.Range(0, stage.Enemies.Count)];
            GameObject prefab = Resources.Load<GameObject>($"Prefabs/Enemies/{enemyId}");


            if (prefab == null)
            {
                Debug.LogError($"[SpawnManager] ������ �ε� ����: Prefabs/Enemies/{enemyId}");
                continue; // null�̸� �������� �ʰ� ���� �ݺ� ����
            }

            Enemy enemy = ObjectPoolManager.Instance.enemyPool.Get(prefab);
            enemy.transform.position = GetRandomPos(); // �� �Լ��� ���� SpawnManager�� ����
            enemy.transform.rotation = Quaternion.identity;
        }
    }

    private Vector2 GetRandomPos()
    {
        float x = Random.Range(-30.0f, 30.0f);
        float y = Random.Range(0f, 3.5f);
        return new Vector2(x, y);
    }
}