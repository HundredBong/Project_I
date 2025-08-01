using UnityEngine.UI;
using DG.Tweening;
using UnityEngine;
using System;

public static class UITweening
{
    public static void PlayShine(Image image)
    {
        if (image == null) { return; }

        image.DOKill(); //이전 트윈 중지
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0f); //투명하게 초기화

        image.DOFade(1f, 0.2f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    public static void FadeOut(Image fadeImage, float duration)
    {
        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 1f);

        fadeImage.DOFade(0f, duration).SetEase(Ease.OutQuad);
    }

    public static void FadeIn(Image fadeImage, float duration)
    {
        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 0f);

        fadeImage.DOFade(1f, duration).SetEase(Ease.InQuad);
    }

    public static void PlayToast(CanvasGroup group, float moveTime = 1.5f, Action onComplete = null)
    {
        group.alpha = 0f;
        Vector3 startPos = Vector3.zero;
        Vector3 endPos = new Vector3(0, 50, 0);
        group.transform.localPosition = startPos; //ToastRoot의 앵커로 오도록 

        DOTween.Kill(group.transform); //혹시 남아있는 트윈 방지

        Sequence seq = DOTween.Sequence();

        //알파값을 0 ~ 1로 0.2초간 변화
        //변화하면서 동시에 1.5초간 위로 올라감
        //올라가는게 끝나면 1초간 사라지기 시작, 총 2.5초
        //끝나면 콜백 실행

        //CanvasGroup을 인자로 받는 DOFade 따로 있음
        seq.Append(group.DOFade(1f, 0.2f)).Join(group.transform.DOLocalMove(endPos, moveTime))
            .Append(group.DOFade(0f, 1f).OnComplete(() => onComplete?.Invoke()));
    }
}

