using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILoading : UIBase
{
    [SerializeField]
    private Image _loadingImage;
    [SerializeField]
    private TMP_Text _loadingText;

    private Tween _loadingTween;

    private void OnEnable()
    {
        _loadingTween = _loadingImage.rectTransform
            .DORotate(new Vector3(0, 0, 360), .2f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1);
    }

    private void OnDisable()
    {
        _loadingTween.Kill();
    }
}
