using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class GenericPoolManager<T> : MonoBehaviour, IPoolManager<T> where T : Component
{
    //private Dictionary<T, Stack<T>> pool; <- 이렇게 하면 어떤 프리팹에서 만들어졌는지 모호함
    //Skeleton, Goblin도 Enemy 타입임, pool[Enemy]하면 뭐가 나올지 모름, 그래서 프리팹을 키로 설정함.
    //같은 Eenmy프리팹이라도 프리팹이 다르면 다른 풀로 처리함, 아니면 초기화 하기 빡센가 왜지?
    //반환 시에도 이름 비교로 편하게 집어넣을 수 있음.
    protected Dictionary<GameObject, Stack<T>> pool = new Dictionary<GameObject, Stack<T>>();

    //아니 그럼 Enemy는 프리팹 개수가 많아야 10개 될텐데
    //오디오랑 파티클은?????? 저번에 쓴거 그냥 그대로 가져올까 아,


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
            return Activate(pool[prefab].Pop());
        }

        Debug.LogWarning($"[GenericPoolManager] {prefab.name} 풀에 공간이 충분하지 않음, 새로 생성함.");
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

        Debug.LogWarning($"[GenericPoolManager] 반환 실패함, {instance.name}");
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
