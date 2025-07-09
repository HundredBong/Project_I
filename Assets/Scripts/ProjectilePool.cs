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
                Debug.LogWarning($"[ProjectilePool] {data.id} 프리팹이 null임");
                continue;
            }

            //이미 캐시에 등록된 프리팹은 무시함
            if (prefabCache.ContainsKey(data.id)) { continue; }

            //프리팹 캐시에 등록하고 미리 로드함
            prefabCache[data.id] = data.prefab;
            Preload(data.prefab, preloadCount);
        }

        Debug.Log($"[ProjectilePool] {prefabCache.Count}개 프리팹 Preload 됨");
    }

    public T GetPrefab<T>(ProjectileId id) where T : Projectile
    {
        //프리팹 캐시에 해당 id가 있는지 확인하고 반환함
        if (prefabCache.TryGetValue(id, out GameObject prefab))
        {
            var projectile = base.Get(prefab);
            return projectile as T;
        }

        Debug.LogError($"[ProjectilePool] {id} 프리팹이 없음");
        return null;
    }
}

[System.Serializable]
public class ProjectilePrefabData
{
    public ProjectileId id;
    public GameObject prefab;
}
