using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContentPool : GenericPoolManager<UIResultContent>
{
    [SerializeField] private GameObject resultPrefab;

    public void Start()
    {
        Preload(resultPrefab, 30);
    }

    public UIResultContent GetContent()
    {
        return Get(resultPrefab);
    }

}

