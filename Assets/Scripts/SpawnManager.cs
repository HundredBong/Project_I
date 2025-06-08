using System.Collections;
using System.Collections.Generic;
using UnityEditor.Presets;
using UnityEngine;
using static Cinemachine.Editor.CinemachineLensPresets;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    //�������� ���ÿ� 
    [SerializeField] private List<StageEnemySetting> enemySettings;

    //�������� Ÿ�Կ� � Enemy�� ����ִ��� ã�� �뵵, �������� ������ �� �迭���� ���� �������
    private Dictionary<StageType, GameObject[]> typeToPrefabs = new Dictionary<StageType, GameObject[]>();

    //�̹� �ε��ߴ��� üũ�� �ؽ���
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

        //���� foreach�� ���ٰ� �ص� �������� 100,00���� ��,
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
        //�������� ���� �ִ¸�ŭ ��ȸ
        foreach (StageEnemySetting setting in enemySettings)
        {
            //�������� Ÿ�Կ� �ش��ϴ� Enemy�迭�� ���� ������ ���ٸ�
            if (typeToPrefabs.ContainsKey(setting.type) == false)
            {
                //�������� Ÿ�Կ� Enemy�迭 �ʱ�ȭ
                typeToPrefabs[setting.type] = setting.enemyPrefabs;
            }

            //�������� ���� �ȿ� �ִ� Enemy�迭 ��ȸ
            foreach (GameObject prefab in setting.enemyPrefabs)
            {
                //Enemy�� ���� �ε��ص��� �ʾҴٸ�
                if (preloadedPrefabs.Contains(prefab) == false)
                {
                    //Ǯ���� �ε��ϰ� HashSet�� �־ �����ε� �ߺ� ����
                    ObjectPoolManager.Instance.enemyPool.Preload(prefab, 20);
                    preloadedPrefabs.Add(prefab); //������ �ؽ����� �ߺ� �ȵ�
                }
            }
        }
    }

    public void SpawnEnemiesForStage(StageType type, int count)
    {
        //�������� Ÿ�Կ� ���� ������ ���ٸ�
        if (typeToPrefabs.TryGetValue(type, out GameObject[] prefabs) == false)
        {
            Debug.LogError($"[SpawnManager] {type}�� ���� ���� ���� ����");
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