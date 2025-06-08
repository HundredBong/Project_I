using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class GenericPoolManager<T> : MonoBehaviour, IPoolManager<T> where T : Component
{
    private Dictionary<GameObject, Stack<T>> pool = new Dictionary<GameObject, Stack<T>>();

    public void Preload(GameObject prefab, int count)
    {
        GameObject key = prefab.gameObject;

        if (pool.ContainsKey(key) == false)
        {
            pool[key] = new Stack<T>();
        }

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            T comp = obj.GetComponent<T>();
            pool[prefab].Push(comp);
        }
    }

    public T Get(GameObject prefab)
    {
        if (pool.ContainsKey(prefab) == false)
        {
            pool[prefab] = new Stack<T>();
        }

        if (pool[prefab].Count > 0)
        {
            return Activate(pool[prefab].Pop());
        }

        GameObject obj = Instantiate(prefab, transform);
        T comp = obj.GetComponent<T>();
        return Activate(comp);
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
            string prefabName = kvp.Key.name;
            string instanceName = instance.gameObject.name.Replace("(Clone)", "").Trim();

            if (instanceName == prefabName)
            {
                kvp.Value.Push(instance);
                return;
            }
        }

        Debug.LogWarning($"[GenericPoolManager] 반환 실패함, {instance.name}");
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
