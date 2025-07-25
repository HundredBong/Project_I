using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor.Presets;
using UnityEngine;

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
            Debug.LogError("[SpawnManager] 현재 스테이지 데이터 없음");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            EnemyId enemyId = stage.Enemies[Random.Range(0, stage.Enemies.Count)];
            GameObject prefab = ObjectPoolManager.Instance.enemyPool.GetPrefab(enemyId);

            if (prefab == null)
            {
                Debug.LogError($"[SpawnManager] 프리팹 로드 실패 : Prefabs/Enemies/{enemyId}");
                continue; //null이면 생성하지 않고 다음 반복 진행
            }

            Enemy enemy = ObjectPoolManager.Instance.enemyPool.Get(prefab);
            enemy.Initialize();
            enemy.transform.position = GetRandomPos();
            enemy.transform.rotation = Quaternion.identity;
        }
    }

    public void SpawnStageBoss()
    {
        int stageId = StageManager.Instance.GetCurrentStage();
        StageData stage = DataManager.Instance.stageDataTable[stageId];

        if (stage == null)
        {
            Debug.LogError("[SpawnManager] 현재 스테이지 데이터 없음");
            return;
        }

        GameObject prefab = ObjectPoolManager.Instance.enemyPool.GetPrefab(stage.BossEnemyId);
        EnemyData enemyData = DataManager.Instance.GetEnemyData(stage.BossEnemyId);
        Enemy boss = ObjectPoolManager.Instance.enemyPool.Get(prefab);
        boss.transform.position = GetRandomPos();
        boss.transform.localScale = boss.OriginScale * 2f; 
        boss.InitializeBoss(stage, enemyData);
    }

    private Vector2 GetRandomPos()
    {
        float x = Random.Range(-30.0f, 30.0f);
        float y = Random.Range(0f, 3.5f);
        return new Vector2(x, y);
    }
}