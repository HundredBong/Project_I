using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UISummonResultPopup : UIPopup
{
    [SerializeField] private Transform contentRoot;

    private Coroutine showResultCoroutine;
    private WaitForSeconds wait;
    private List<UIResultContent> _contents = new List<UIResultContent>();

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

        foreach (UIResultContent content in _contents)
        {
            content.transform.SetParent(ObjectPoolManager.Instance.contentPool.transform ?? null);
            ObjectPoolManager.Instance.contentPool.Return(content);
        }

        _contents.Clear();

        showResultCoroutine = StartCoroutine(StartDisplayingResultCoroutine(data));
    }

    public void StartDisplayingResult(Queue<SkillData> data)
    {
        if (showResultCoroutine != null)
        {
            StopDisplayingResult();
        }

        foreach (UIResultContent content in _contents)
        {
            content.transform.SetParent(ObjectPoolManager.Instance.contentPool.transform ?? null);
            ObjectPoolManager.Instance.contentPool.Return(content);
        }

        _contents.Clear();

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
            yield return wait;
            UIResultContent content = ObjectPoolManager.Instance.contentPool.GetContent();
            _contents.Add(content);

            content.transform.SetParent(contentRoot);
            content.Initialize(data.Dequeue());
        }
    }

    private IEnumerator StartDisplayingResultCoroutine(Queue<SkillData> data)
    {
        while (data.Count != 0)
        {
            yield return wait;
            UIResultContent content = ObjectPoolManager.Instance.contentPool.GetContent();         
            _contents.Add(content);

            content.transform.SetParent(contentRoot);
            content.Initialize(data.Dequeue());
        }
    }
}
