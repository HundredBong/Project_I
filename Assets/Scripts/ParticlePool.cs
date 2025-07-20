using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePool : GenericPoolManager<PooledParticle>
{
    [SerializeField] private List<ParticlePrefabData> particlePrefabs;
    private Dictionary<ParticleId, GameObject> prefabCache = new Dictionary<ParticleId, GameObject>();

    private void Awake()
    {
        InitializePool();
    }

    public void InitializePool(int preloadCount = 10)
    {
        foreach (ParticlePrefabData data in particlePrefabs)
        {
            if (data == null)
            {
                Debug.LogWarning($"[ParticlePool] {data.Id} «¡∏Æ∆’¿Ã null¿”");
                continue;
            }

            if(prefabCache.ContainsKey(data.Id)) { continue; }

            prefabCache[data.Id] = data.Prefab;
            Preload(data.Prefab, preloadCount);
        }
        //Debug.Log($"[ParticlePool] {prefabCache.Count}∞≥ «¡∏Æ∆’ Preload µ ");
    }

    public PooledParticle GetPrefab(ParticleId id)
    {
        if (prefabCache.TryGetValue(id, out GameObject prefab))
        {
            return base.Get(prefab);
        }
        Debug.LogError($"[ParticlePool] {id} «¡∏Æ∆’¿Ã æ¯¿Ω");

        return null;
    }
}

[System.Serializable]
public class  ParticlePrefabData
{
    public ParticleId Id;
    public GameObject Prefab;
}