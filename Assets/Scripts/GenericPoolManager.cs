using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class GenericPoolManager<T> : MonoBehaviour, IPoolManager<T> where T : Component
{
    //private Dictionary<T, Stack<T>> pool; <- �̷��� �ϸ� � �����տ��� ����������� ��ȣ��
    //Skeleton, Goblin�� Enemy Ÿ����, pool[Enemy]�ϸ� ���� ������ ��, �׷��� �������� Ű�� ������.
    //���� Eenmy�������̶� �������� �ٸ��� �ٸ� Ǯ�� ó����, �ƴϸ� �ʱ�ȭ �ϱ� ������ ����?
    //��ȯ �ÿ��� �̸� �񱳷� ���ϰ� ������� �� ����.
    protected Dictionary<GameObject, Stack<T>> pool = new Dictionary<GameObject, Stack<T>>();

    //�ƴ� �׷� Enemy�� ������ ������ ���ƾ� 10�� ���ٵ�
    //������� ��ƼŬ��?????? ������ ���� �׳� �״�� �����ñ� ��,


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
            return Activate(pool[prefab].Pop());
        }

        Debug.LogWarning($"[GenericPoolManager] {prefab.name} Ǯ�� ������ ������� ����, ���� ������.");
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
        if (instance == null) { return; }

        instance.gameObject.SetActive(false);

        foreach (KeyValuePair<GameObject, Stack<T>> kvp in pool)
        {
            string prefabName = kvp.Key.name;
            string instanceName = instance.gameObject.name.Replace("(Clone)", "").Trim();
            if (instanceName == prefabName)
            {
                kvp.Value.Push(instance);
                return;
            }
        }

        Debug.LogWarning($"[GenericPoolManager] ��ȯ ������, {instance.name}");
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
