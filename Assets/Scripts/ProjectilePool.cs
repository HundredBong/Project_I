using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : GenericPoolManager<Projectile>
{
    [SerializeField] private List<ProjectilePrefabData> projectilePrefabs;

    private Dictionary<ProjectileId, GameObject> prefabCache = new Dictionary<ProjectileId, GameObject>();

    private void Awake()
    {
        InitializePool();
    }

    private void InitializePool(int preloadCount = 10)
    {
        foreach (ProjectilePrefabData data in projectilePrefabs)
        {
            if (data.prefab == null)
            {
                Debug.LogWarning($"[ProjectilePool] {data.id} �������� null��");
                continue;
            }

            //�̹� ĳ�ÿ� ��ϵ� �������� ������
            if (prefabCache.ContainsKey(data.id)) { continue; }

            //������ ĳ�ÿ� ����ϰ� �̸� �ε���
            prefabCache[data.id] = data.prefab;
            Preload(data.prefab, preloadCount);
        }

        Debug.Log($"[ProjectilePool] {prefabCache.Count}�� ������ Preload ��");
    }

    public T GetPrefab<T>(ProjectileId id) where T : Projectile
    {
        //������ ĳ�ÿ� �ش� id�� �ִ��� Ȯ���ϰ� ��ȯ��
        if (prefabCache.TryGetValue(id, out GameObject prefab))
        {
            var projectile = base.Get(prefab);
            return projectile as T;
        }

        Debug.LogError($"[ProjectilePool] {id} �������� ����");
        return null;
    }
}

[System.Serializable]
public class ProjectilePrefabData
{
    public ProjectileId id;
    public GameObject prefab;
}
