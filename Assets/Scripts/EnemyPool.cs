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

        #region base 원본 코드
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
        //ToArray() : foreach중에 enemyList.Remove()호출되면 오류날 수 있으니 안전하게 복사본 돌림
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
                Debug.LogWarning($"[EnemyPool] {data.id} 프리팹이 null임");
                continue;
            }

            //이미 로드한 프리팹은 넘기기
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
            Debug.LogError($"[EnemyPool] 프리팹 캐시에 {enemyId}가 없음");
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