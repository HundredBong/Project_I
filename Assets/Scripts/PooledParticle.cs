using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledParticle : MonoBehaviour, IPooledObject
{
    //�⺻ ��ƼŬ�ý����� IPooledObject�� �������� �ʾƼ� IPooledObject�� ������ PooledParticle�� ������
    public GameObject prefabReference { get; set; }

    private ParticleSystem par;

    private void Awake()
    {
        par = GetComponent<ParticleSystem>();
    }

    public void Play(Vector3 pos)
    {
        //��ƼŬ ����� Ǯ�� ��ȯ
        par.transform.position = pos;
        par.transform.rotation = Quaternion.identity;
        par.Play();
        DelayCallManager.Instance.CallLater(par.main.duration, () => { ObjectPoolManager.Instance.particlePool.Return(this); });
    }
}
