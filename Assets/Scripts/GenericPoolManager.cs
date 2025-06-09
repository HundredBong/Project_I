using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class GenericPoolManager<T> : MonoBehaviour, IPoolManager<T> where T : Component, IPooledObject
{
    //private Dictionary<T, Stack<T>> pool; <- �̷��� �ϸ� � �����տ��� ����������� ��ȣ��
    //Skeleton, Goblin�� Enemy Ÿ����, pool[Enemy]�ϸ� ���� ������ ��, �׷��� �������� Ű�� ������.
    //���� Eenmy�������̶� �������� �ٸ��� �ٸ� Ǯ�� ó����, �ƴϸ� �ʱ�ȭ �ϱ� ������ ����?
    //��ȯ �ÿ��� �̸� �񱳷� ���ϰ� ������� �� ����.
    protected Dictionary<GameObject, Stack<T>> pool = new Dictionary<GameObject, Stack<T>>();

    //�ƴ� �׷� Enemy�� ������ ������ ���ƾ� 10�� ���ٵ�
    //������� ��ƼŬ��?????? ������ ���� �׳� �״�� �����ñ� ��,
    //��ƼŬ�� �̰� �� �� �ִٰ� �ĵ� ������� Ŭ�� �ϳ� �ٲٰ� ������ȭ �ؾ���,
    //������� ���� �Ĵ°� �´°Ű���

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
            Debug.LogWarning($"[GenericPoolManager] {prefab.name} �������� Ǯ�� ��ϵ��� ����.");
            pool[prefab] = new Stack<T>();
        }

        if (pool[prefab].Count > 0)
        {
            //return Activate(pool[prefab].Pop());
            T instance = Activate(pool[prefab].Pop());
            instance.prefabReference = prefab; //� �����տ��� ���°��� �����. ���� Return���� ���
            return instance;
        }

        Debug.LogWarning($"[GenericPoolManager] {prefab.name} Ǯ�� ������ ������� ����, ���� ������.");
        GameObject obj = Instantiate(prefab, transform);
        T comp = obj.GetComponent<T>(); //���� ����� ������� ���´� Ǯ�� �� ������ ����, Push()���ص� �ǰ�, �Ⱦ��� Pop�̳� ������ ��
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

        //���ڿ� �� �������� �Ҿ����ҰŰ��Ƽ� �Ʒ� �ڵ�� �ٲ�, 
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
            Debug.LogWarning($"[GenericPoolManager] Return ������, prefabReference�� null�̰ų� Ǯ�� ����. {instance.name}");
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
