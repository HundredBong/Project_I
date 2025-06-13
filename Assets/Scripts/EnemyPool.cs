using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : GenericPoolManager<Enemy>
{
    private HashSet<EnemyId> loadedEnemyIds = new HashSet<EnemyId>();
    private Dictionary<EnemyId, GameObject> prefabCache = new Dictionary<EnemyId, GameObject>();  

    private void Awake()
    {
        InitializePool();
    }

    public override void Preload(GameObject prefab, int count)
    {
        base.Preload(prefab, count);

        if (pool.TryGetValue(prefab, out Stack<Enemy> stack))
        {
            foreach (var enemy in stack)
            {
                enemy.isDead = true;
            }
        }
    }

    public void ReturnAllEnemies()
    {
        //ToArray() : foreach�߿� enemyList.Remove()ȣ��Ǹ� ������ �� ������ �����ϰ� ���纻 ����
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
        //�����͸Ŵ����� �������� ������ �ִ� Enemy�� ����
        foreach (KeyValuePair<int,StageData> stageEntry in DataManager.Instance.stageDataTable)
        {
            StageData stageData = stageEntry.Value;
            
            //StageData������ EnemyId����Ʈ ��ȸ
            foreach (EnemyId enemyId in stageData.Enemies)
            {
                //�ߺ��� EnemyId�� ����
                if (loadedEnemyIds.Contains(enemyId)) { continue; }


                //������ �ε�
                GameObject enemyPrefab = Resources.Load<GameObject>($"Prefabs/Enemies/{enemyId}");

                if (enemyPrefab == null)
                {
                    Debug.LogError($"[EnemyPool] Enemy ������ �ε� ������, {enemyId}");
                    continue;
                }

                prefabCache[enemyId] = enemyPrefab; //������ ĳ�ÿ� ����, SpawnManager���� ��� ����

                //preloadCount��ŭ �̸� �����ε�
                Preload(enemyPrefab, preloadCount);

                //�ߺ� ������ �ؽ��� ����
                loadedEnemyIds.Add(enemyId);
            }
        }
        Debug.Log($"[EnemyPool] Enemy Ǯ �ʱ�ȭ ��, {loadedEnemyIds.Count}����");
    }

    public GameObject GetPrefab(EnemyId enemyId)
    {
        if (prefabCache.TryGetValue(enemyId, out GameObject prefab))
        {
            return prefab;
        }
        else
        {
            Debug.LogError($"[EnemyPool] ������ ĳ�ÿ� {enemyId}�� ����");
            return null;
        }
    }
}
