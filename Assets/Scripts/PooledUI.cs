using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledUI : MonoBehaviour, IPooledObject
{
    public GameObject prefabReference { get; set; }
}
