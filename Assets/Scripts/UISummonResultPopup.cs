using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISummonResultPopup : UIPopup
{
    [SerializeField] private GameObject contentPrefab;
    [SerializeField] private Transform contentRoot;

    private Coroutine showResultCoroutine;
    private WaitForSeconds wait;

    protected override void Awake()
    {
        base.Awake();

        wait = new WaitForSeconds(0.2f);
    }

    public void StartDisplayingResult(Queue<ItemData> data)
    {
        if (showResultCoroutine != null)
        {
            StopDisplayingResult();
        }

        foreach (Transform child in contentRoot)
        {
            Destroy(child.gameObject);
        }

        showResultCoroutine = StartCoroutine(StartDisplayingResultCoroutine(data));
    }

    public void StartDisplayingResult(Queue<SkillData> data)
    {
        if (showResultCoroutine != null)
        {
            StopDisplayingResult();
        }

        foreach (Transform child in contentRoot)
        {
            Destroy(child.gameObject);
        }

        showResultCoroutine = StartCoroutine(StartDisplayingResultCoroutine(data));
    }

    public void StopDisplayingResult()
    {
        if (showResultCoroutine != null)
        {
            StopCoroutine(showResultCoroutine);
            showResultCoroutine = null;
        }
    }

    private IEnumerator StartDisplayingResultCoroutine(Queue<ItemData> data)
    {
        while (data.Count != 0)
        {
            //0.2초 대기
            yield return wait;
            GameObject obj = Instantiate(contentPrefab, contentRoot);
            UIItemSlot slot = obj.GetComponent<UIItemSlot>();
            slot.Init(data.Dequeue());
        }
    }

    private IEnumerator StartDisplayingResultCoroutine(Queue<SkillData> data)
    {
        while (data.Count != 0)
        {
            //0.2초 대기
            yield return wait;
            GameObject obj = Instantiate(contentPrefab, contentRoot);
            UISkillSlot slot = obj.GetComponent<UISkillSlot>();
            Debug.LogWarning(slot == null ? "Slot Null" : "Slot Not Null");
            
            SkillData skillData = data.Dequeue();
            Debug.LogWarning(skillData == null ? "Data Null" : "Data Not Null");
            slot.Init(skillData);
        }
    }
}
