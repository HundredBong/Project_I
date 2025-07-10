using UnityEngine.UI;
using DG.Tweening;
using UnityEngine;

public static class UIShineEffect 
{
    public static void PlayShine(Image image)
    {
        if (image == null) { return; }

        image.DOKill(); //���� Ʈ�� ����
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0f); //�����ϰ� �ʱ�ȭ

        image.DOFade(1f, 0.2f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }
}
