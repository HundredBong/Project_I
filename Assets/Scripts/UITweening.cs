using UnityEngine.UI;
using DG.Tweening;
using UnityEngine;

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
}
