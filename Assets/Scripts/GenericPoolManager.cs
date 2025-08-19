using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class GenericPoolManager<T> : MonoBehaviour, IPoolManager<T> where T : Component, IPooledObject
{
    protected Dictionary<GameObject, Stack<T>> pool = new Dictionary<GameObject, Stack<T>>();

    public virtual void Preload(GameObject prefab, int count)
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
            pool[key].Push(comp);
        }
    }

    public T Get(GameObject prefab)
    {
        if (pool.ContainsKey(prefab) == false)
        {
            Debug.LogWarning($"[GenericPoolManager] {prefab.name} 프리팹이 풀에 등록되지 않음.");
            pool[prefab] = new Stack<T>();
        }

        if (pool[prefab].Count > 0)
        {
            //return Activate(pool[prefab].Pop());
            T instance = Activate(pool[prefab].Pop());
            instance.prefabReference = prefab; //어떤 프리팹에서 나온건지 등록함. 추후 Return에서 사용
            return instance;
        }

        Debug.LogWarning($"[GenericPoolManager] {prefab.name} 풀에 공간이 충분하지 않음, 새로 생성함.");
        GameObject obj = Instantiate(prefab, transform);
        T comp = obj.GetComponent<T>(); //새로 만든건 사용중인 상태니 풀에 들어갈 이유가 없음, Push()안해도 되고, 안쓸때 Pop이나 잘해줄 것
        comp.prefabReference = prefab;
        return Activate(comp);
    }

    private T Activate(T instance)
    {
        instance.gameObject.SetActive(true);
        return instance;
    }

    public void Return(T instance)
    {
        if (instance == null) { return; }

        instance.gameObject.SetActive(false);

        //문자열 비교 느린데다 불안정할거같아서 아래 코드로 바꿈, 
        //string instanceName = instance.gameObject.name.Replace("(Clone)", "").Trim();

        //foreach (var kvp in pool)
        //{
        //    if (kvp.Key.name == instanceName)
        //    {
        //        kvp.Value.Push(instance);
        //        return;
        //    }
        //}

        GameObject prefab = instance.prefabReference;

        if (prefab != null && pool.TryGetValue(prefab, out var stack) == true)
        {
            stack.Push(instance);
        }
        else
        {
            Debug.LogWarning($"[GenericPoolManager] Return 실패함, prefabReference가 null이거나 풀에 없음. {instance.name}");
        }
    }

    public void Return(T instance, float t)
    {
        //StartCoroutine(ReturnAfterDelayCoroutine(instance, t));
        DelayCallManager.Instance.CallLater(t, () => { Return(instance); });
    }

    private IEnumerator ReturnAfterDelayCoroutine(T instance, float t)
    {
        yield return new WaitForSeconds(t);
        Return(instance);
    }
}
