using System;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class UILottoArg : UIArg
{
    public List<LottoResponse> liLottoResponse;
}

public class UILotto : UIBase
{
    UILottoArg _arg;

    [SerializeField] private Button _topButton;
    [SerializeField] private Button _bottomButton;
    [SerializeField] private LottoScrollView scrollView;

    public override void InitArg(UIArg arg)
    {
        _arg = arg as UILottoArg;
    }

    private void Start()
    {
        _topButton.OnClickAsObservable().Subscribe(_ =>
        {
            scrollView.SetScrollPosition(1);
        }).AddTo(closeDisposable);

        _bottomButton.OnClickAsObservable().Subscribe(_ =>
        {
            scrollView.SetScrollPosition(_arg.liLottoResponse.Count);
        }).AddTo(closeDisposable);

        scrollView.SetItemList(_arg.liLottoResponse);
    }
}
