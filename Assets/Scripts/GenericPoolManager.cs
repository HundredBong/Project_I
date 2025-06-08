using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class GenericPoolManager<T> : MonoBehaviour, IPoolManager<T> where T : Component
{
    private Dictionary<T, Stack<T>> pool = new Dictionary<T, Stack<T>>();

    public void Preload(T prefab, int count)
    {
        if (pool.ContainsKey(prefab) == false)
        {
            pool[prefab] = new Stack<T>();
        }

        for (int i = 0; i < count; i++)
        {
            T instance = Instantiate(prefab, transform);
            instance.gameObject.SetActive(false);
            pool[prefab].Push(instance);
        }
    }

    public T Get(T prefab)
    {
        if (pool.TryGetValue(prefab, out var stack) && stack.Count > 0)
        {
            return Activate(stack.Pop());
        }

        T instance = Instantiate(prefab, transform);
        return Activate(instance);
    }

    private T Activate(T instance)
    {
        instance.gameObject.SetActive(true);
        return instance;
    }

    public void Return(T instance)
    {
        instance.gameObject.SetActive(false);

        foreach (var kvp in pool)
        {
            if (instance.name.Contains(kvp.Key.name))
            {
                kvp.Value.Push(instance);
                return;
            }
        }

        Debug.LogWarning($"[GenericPoolManage] 반환 실패함, {instance.name}");
    }

    public void Return(T instance, float t)
    {
        StartCoroutine(ReturnAfterDelayCoroutine(instance, t));
    }

    private IEnumerator ReturnAfterDelayCoroutine(T instance, float t)
    {
        yield return new WaitForSeconds(t);
        Return(instance);
    }
}
