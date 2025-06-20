using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : GenericPoolManager<Enemy>
{
    //Enemy 전용 풀링 시스템
    //EnemyPool을 미리 초기화해두고 , EnemyId를 기반으로 프리팹을 로드하여 관리함

    private HashSet<EnemyId> loadedEnemyIds = new HashSet<EnemyId>(); //로드된 EnemyId를 저장하여 중복 로드를 방지

    //실제 프리팹 객체 매핑용 캐시, 없으면 SpawnManager에서 로드할 때마다 Resources.Load를 호출해야함
    //EnemyId가 어떤 프리팹을 가리키는지 매핑하는 딕셔너리
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
            }
        }
    }

    public void InitializePool(int preloadCount = 30)
    {
        //데이터매니저의 스테이지 정보에 있는 Enemy만 추적
        foreach (KeyValuePair<int,StageData> stageEntry in DataManager.Instance.stageDataTable)
        {
            StageData stageData = stageEntry.Value;
            
            //StageData내부의 EnemyId리스트 순회
            foreach (EnemyId enemyId in stageData.Enemies)
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

                prefabCache[enemyId] = enemyPrefab; //프리팹 캐시에 저장, SpawnManager에서 사용 가능

                //preloadCount만큼 미리 프리로드
                Preload(enemyPrefab, preloadCount);

                //중복 방지용 해쉬셋 저장
                loadedEnemyIds.Add(enemyId);
            }
        }
        Debug.Log($"[EnemyPool] Enemy 풀 초기화 됨, {loadedEnemyIds.Count}종류");
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
