using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : GenericPoolManager<Enemy>
{
    HashSet<EnemyId> loadedEnemyIds = new HashSet<EnemyId>();

    private void Awake()
    {
        InitializePool();
    }

    public override void Preload(GameObject prefab, int count)
    {
        base.Preload(prefab, count);

        Stack<Enemy> stack;

        if (pool.TryGetValue(prefab, out stack))
        {
            foreach (var enemy in stack)
            {
                enemy.isDead = true;
            }
        }
    }

    public void ReturnAllEnemies()
    {
        //ToArray() : foreach중에 enemyList.Remove()호출되면 오류날 수 있으니 안전하게 복사본 돌림
        foreach (Enemy enemy in GameManager.Instance.enemyList.ToArray())
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                Return(enemy);
            }
        }
    }

    public void InitializePool(int preloadCount = 30)
    {
        //데이터매니저의 스테이지 정보에 있는 Enemy만 추적
        foreach (var stageEntry in DataManager.Instance.stageDataTable)
        {
            StageData stageData = stageEntry.Value;
            
            //StageData내부의 EnemyId리스트 순회
            foreach (var enemyId in stageData.Enemies)
            {
                //중복된 EnemyId는 무시
                if (loadedEnemyIds.Contains(enemyId)) { continue; }


                //프리팹 로드
                GameObject enemyPrefab = Resources.Load<GameObject>($"Prefabs/Enemies/{enemyId}");

                if (enemyPrefab == null)
                {
                    Debug.LogError($"[EnemyPool] Enemy 프리팹 로드 실패함, {enemyId}");
                    continue;
                }

                //preloadCount만큼 미리 프리로드
                Preload(enemyPrefab, preloadCount);

                //중복 방지용 해쉬셋 저장
                loadedEnemyIds.Add(enemyId);
            }
        }
        Debug.Log($"[EnemyPool] Enemy 풀 초기화 됨, {loadedEnemyIds.Count}종류");
    }
}
