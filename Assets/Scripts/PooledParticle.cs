using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledParticle : MonoBehaviour, IPooledObject
{
    //기본 파티클시스템은 IPooledObject를 구현하지 않아서 IPooledObject를 구현한 PooledParticle로 래핑함
    public GameObject prefabReference { get; set; }

    private ParticleSystem par;

    private void Awake()
    {
        par = GetComponent<ParticleSystem>();
    }

    public void Play(Vector3 pos, bool isReturn = true)
    {
        //파티클 재생후 풀에 반환
        par.transform.position = pos;
        par.transform.rotation = Quaternion.identity;
        par.Play();

        if (isReturn)
        {
            DelayCallManager.Instance.CallLater(par.main.duration, () => { ObjectPoolManager.Instance.particlePool.Return(this); });
        }
    }

    public void Play(Vector3 pos, Transform parent)
    {
        transform.SetParent(parent);
        Play(pos);
    }

    public void Stop()
    {
        par.Stop();
        ObjectPoolManager.Instance.particlePool.Return(this);
    }
}
