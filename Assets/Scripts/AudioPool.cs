using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPool : GenericPoolManager<PooledAudio>
{
    public GameObject audioSource;

    private void Start()
    {
        Preload(audioSource, 30);
    }

    public PooledAudio GetAudio()
    {
        return base.Get(audioSource) as PooledAudio;
    }
}
