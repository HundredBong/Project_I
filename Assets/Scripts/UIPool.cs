using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPool : GenericPoolManager<PooledUI>
{
    public GameObject resultPrefab;

    public void Start()
    {
        Preload(resultPrefab, 30);
    }

    public T GetUI<T>(GameObject prefab) where T : PooledUI
    {
        return base.Get(prefab) as T;
    }

    public UIResultContent GetResult()
    {
        return base.Get(resultPrefab) as UIResultContent;
    }
}
