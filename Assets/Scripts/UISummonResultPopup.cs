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
            //0.2√  ¥Î±‚
            yield return wait;
            GameObject obj = Instantiate(contentPrefab, contentRoot);
            UIItemSlot slot = obj.GetComponent<UIItemSlot>();
            slot.Init(data.Dequeue());
        }
    }
}
