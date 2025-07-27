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
                Debug.LogWarning($"[ParticlePool] {data.Id} ÇÁ¸®ÆÕÀÌ nullÀÓ");
                continue;
            }

            if(prefabCache.ContainsKey(data.Id)) { continue; }

            prefabCache[data.Id] = data.Prefab;
            Preload(data.Prefab, preloadCount);
        }
        //Debug.Log($"[ParticlePool] {prefabCache.Count}°³ ÇÁ¸®ÆÕ Preload µÊ");
    }

    public PooledParticle GetPrefab(ParticleId id)
    {
        if (prefabCache.TryGetValue(id, out GameObject prefab))
        {
            return base.Get(prefab);
        }
        Debug.LogError($"[ParticlePool] {id} ÇÁ¸®ÆÕÀÌ ¾øÀ½");

        return null;
    }

    public void ReturnAllParticles()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);

            PooledParticle praticle = child.GetComponent<PooledParticle>();
            if (praticle != null && child.gameObject.activeSelf == true)
            {
                Return(praticle);
            }
        }
    }
}

[System.Serializable]
public class  ParticlePrefabData
{
    public ParticleId Id;
    public GameObject Prefab;
}