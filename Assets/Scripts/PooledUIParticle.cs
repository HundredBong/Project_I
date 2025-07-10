using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Coffee.UIExtensions;

public class PooledUIParticle : MonoBehaviour, IPooledObject
{
    public GameObject prefabReference { get; set; }

    private UIParticle uiParticle;
    private ParticleSystem par;

    private void Awake()
    {
        uiParticle = GetComponent<UIParticle>();
        par = GetComponent<ParticleSystem>();

        if (uiParticle == null || par == null)
        {
            Debug.LogError("[PooledUIParticle] UIParticle, ParticleSystem 컴포넌트가 없음");
        }
    }

    public void Play()
    {
        uiParticle.Play();
        DelayCallManager.Instance.CallLater(par.main.duration, () => { ObjectPoolManager.Instance.uiParticlePool.Return(this); });
    }
}

