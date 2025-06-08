using System.Collections;
using System.Collections.Generic;
using UnityEditor.Presets;
using UnityEngine;
using static Cinemachine.Editor.CinemachineLensPresets;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    //스테이지 세팅용 
    [SerializeField] private List<StageEnemySetting> enemySettings;

    //스테이지 타입에 어떤 Enemy가 들어있는지 찾는 용도, 스테이지 시작할 때 배열에서 랜덤 돌리기용
    private Dictionary<StageType, GameObject[]> typeToPrefabs = new Dictionary<StageType, GameObject[]>();

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

    private void Start()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        //스테이지 세팅 있는만큼 순회
        foreach (StageEnemySetting setting in enemySettings)
        {
            //스테이지 타입에 해당하는 Enemy배열에 대한 정보가 없다면
            if (typeToPrefabs.ContainsKey(setting.type) == false)
            {
                //스테이지 타입에 Enemy배열 초기화
                typeToPrefabs[setting.type] = setting.enemyPrefabs;
            }

            //스테이지 세팅 안에 있는 Enemy배열 순회
            foreach (GameObject prefab in setting.enemyPrefabs)
            {
                //Enemy를 아직 로드해두지 않았다면
                if (preloadedPrefabs.Contains(prefab) == false)
                {
                    //풀에서 로드하고 HashSet에 넣어서 프리로드 중복 방지
                    ObjectPoolManager.Instance.enemyPool.Preload(prefab, 20);
                    preloadedPrefabs.Add(prefab); //어차피 해쉬셋은 중복 안들어감
                }
            }
        }
    }

    public void SpawnEnemiesForStage(StageType type, int count)
    {
        //스테이지 타입에 대한 정보가 없다면
        if (typeToPrefabs.TryGetValue(type, out GameObject[] prefabs) == false)
        {
            Debug.LogError($"[SpawnManager] {type}에 대한 몬스터 세팅 없음");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Vector2 pos = GetRandomPos();
            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)]; //2 , 0,1
            Enemy enemy = ObjectPoolManager.Instance.enemyPool.Get(prefab);
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
    public StageType type;
    public GameObject[] enemyPrefabs;
}