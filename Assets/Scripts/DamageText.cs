using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class DamageText : PooledUI
{
    private TextMeshPro _text;
    private MeshRenderer _meshRenderer;
    private Sequence _sequence;

    private void Awake()
    {
        _text = GetComponent<TextMeshPro>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.sortingOrder = 50;
    }

    public void Show(float damage, Vector3 pos)
    {
        transform.position = pos;
        _text.text = NumberFormatter.FormatNumber(damage);
        _text.alpha = 1f;

        _sequence?.Kill();

        transform.localScale = Vector3.zero;
        _sequence = DOTween.Sequence();

        _sequence.Append(transform.DOScale(1.2f, 0.2f));
        _sequence.Append(transform.DOScale(1f, 0.5f));

        _sequence.Join(transform.DOMoveY(transform.position.y + 0.5f, 1f));

        _sequence.Join(_text.DOFade(0f, 1f));

        _sequence.OnComplete(() => ObjectPoolManager.Instance.uiPool.Return(this) );
    }
}
