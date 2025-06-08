using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [SerializeField] private List<StageEnemySetting> enemySettings;
    
    //이미 로드했는지 체크용 해쉬셋
    private HashSet<GameObject> preloadedPrefabs = new HashSet<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        //------------------------------------------------------------------------------------------------

        //이중 foreach문 돈다고 해도 스테이지 100,00개면 엄,
        //foreach (StageEnemySetting setting in enemySettings)
        //{
        //    foreach (var prefab in setting.enemyPrefabs)
        //    {
        //        if (preloadedPrefabs.Contains(prefab) == false)
        //        {
        //            ObjectPoolManager.Instance.enemyPool.Preload(prefab,20);
        //            preloadedPrefabs.Add(prefab);
        //        }
        //    }
        //}
    }

    public void SpawnEnemiesForStage(int stage, int count)
    {
        if (stageToPrefabs.TryGetValue(stage, out GameObject[] prefabs) == false)
        {
            Debug.LogError($"[SpawnManager] 스테이지 {stage}에 대한 적 프리팹이 등록되어 있지 않음");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Vector2 pos = GetRandomPos();
            GameObject randomPrefab = prefabs[Random.Range(0, prefabs.Length)];
            Enemy enemy = ObjectPoolManager.Instance.enemyPool.Get(randomPrefab);
            enemy.transform.position = pos;
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

[System.Serializable]
public class StageEnemySetting
{
    public int stage;
    public GameObject[] enemyPrefabs;
}