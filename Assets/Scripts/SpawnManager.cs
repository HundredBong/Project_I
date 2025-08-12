using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
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

    public void SpawnEnemiesForDungeon(int count, DungeonType type, int level)
    {
        DungeonLevelData data = DataManager.Instance.GetDungeonLevelData(type, level);

        if (data == null)
        {
            Debug.LogError("[SpawnManager] 현재 던전 데이터 없음");
            return;
        }

        List<Vector2> positions = GetRandomPositions(count, 2f, 0f, 100f, 0f, 3f);

        for (int i = 0; i < count; i++)
        {
            EnemyId enemyId = data.EnemyIds[Random.Range(0, data.EnemyIds.Count)];
            GameObject prefab = ObjectPoolManager.Instance.enemyPool.GetPrefab(enemyId);

            if (prefab == null)
            {
                Debug.LogError($"[SpawnManager] 프리팹 로드 실패 : Prefabs/Enemies/{enemyId}");
                continue;
            }

            Enemy enemy = ObjectPoolManager.Instance.enemyPool.Get(prefab);
            enemy.Initialize(data);
            enemy.transform.position = positions[i];
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

    private List<Vector2> GetRandomPositions(int count, float minDistance, float minX, float maxX, float minY, float maxY)
    {
        List<Vector2> positions = new List<Vector2>();

        int tries = 0;
        int maxTries = 1000;

        while (positions.Count < count && tries < maxTries)
        {
            float x = Random.Range(minX, maxX);
            float y = Random.Range(minY, maxY);
            Vector2 spawnPos = new Vector2(x, y);

            bool tooClose = false;

            foreach (Vector2 pos in positions)
            {
                if (Vector2.SqrMagnitude(spawnPos - pos) < minDistance * minDistance)
                {
                    tooClose = true;
                    break;
                }
            }

            if (tooClose)
            {
                tries++;
                continue;
            }

            positions.Add(spawnPos);
            tries++;
        }

        if (positions.Count < count)
        {
            Debug.LogWarning($"[SpawnManager] {count} 중 {positions.Count}개만 배치함 (minDistance가 너무 큼)");

            List<Vector2> centerPos = new List<Vector2>(count);

            for (int i = 0; i < count; i++)
            {
                centerPos.Add(new Vector2((minX + maxX) / 2, (minY + maxY) / 2));
            }

            positions.AddRange(centerPos);
        }

        return positions;
    }
}