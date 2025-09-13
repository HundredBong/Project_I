using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPool : GenericPoolManager<PooledAudio>
{
    public GameObject pooledAudio;

    private void Start()
    {
        Preload(pooledAudio, 30);
    }

    public PooledAudio GetAudio()
    {
        return base.Get(pooledAudio) as PooledAudio;
    }
}
