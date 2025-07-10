using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIParticlePool : GenericPoolManager<PooledUIParticle>
{
    [SerializeField] private GameObject uiParticlePrefab;

    private void Awake()
    {
        Preload(uiParticlePrefab, 10);
    }

    public void PlayParticle(Vector3 pos, Transform parent)
    {
        var particle = Get(uiParticlePrefab);
        particle.transform.SetParent(parent,false);
        particle.transform.localPosition = pos;
        particle.Play();
    }
}
