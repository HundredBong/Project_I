using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : GenericPoolManager<Enemy>
{
    [SerializeField] private List<EnemyPrefabData> enemyPrefabs;

    private Dictionary<EnemyId, GameObject> prefabCache = new Dictionary<EnemyId, GameObject>();

    private void Awake()
    {
        InitializePool();
    }

    public override void Preload(GameObject prefab, int count)
    {
        base.Preload(prefab, count);

        #region base ���� �ڵ�
        //GameObject key = prefab.gameObject;

        //if (pool.ContainsKey(key) == false)
        //{
        //    pool[key] = new Stack<T>();
        //}

        //for (int i = 0; i < count; i++)
        //{
        //    GameObject obj = Instantiate(prefab, transform);
        //    obj.SetActive(false);
        //    T comp = obj.GetComponent<T>();
        //    pool[key].Push(comp);
        //}
        #endregion

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
                GameManager.Instance.enemyList.Remove(enemy);
            }
        }
    }



    public void InitializePool(int preloadCount = 30)
    {
        foreach (EnemyPrefabData data in enemyPrefabs)
        {
            if (data.prefab == null)
            {
                Debug.LogWarning($"[EnemyPool] {data.id} �������� null��");
                continue;
            }

            //�̹� �ε��� �������� �ѱ��
            if(prefabCache.ContainsKey(data.id)) { continue; }

            prefabCache[data.id] = data.prefab;
            Preload(data.prefab, preloadCount);
        }
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

[System.Serializable]
public class EnemyPrefabData
{
    public EnemyId id;
    public GameObject prefab;
}