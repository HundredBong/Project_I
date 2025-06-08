using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPoolManager<T> where T : UnityEngine.Component
{
    void Preload(GameObject prefab, int count);
    T Get(GameObject prefab);
    void Return(T instance);
    void Return(T instance, float t);
}
