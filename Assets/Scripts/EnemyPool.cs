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
        foreach (var stageEntry in DataManager.Instance.stageDataTable)
        {
            StageData stageData = stageEntry.Value;
            
            //StageData������ EnemyId����Ʈ ��ȸ
            foreach (var enemyId in stageData.Enemies)
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

                //preloadCount��ŭ �̸� �����ε�
                Preload(enemyPrefab, preloadCount);

                //�ߺ� ������ �ؽ��� ����
                loadedEnemyIds.Add(enemyId);
            }
        }
        Debug.Log($"[EnemyPool] Enemy Ǯ �ʱ�ȭ ��, {loadedEnemyIds.Count}����");
    }
}
