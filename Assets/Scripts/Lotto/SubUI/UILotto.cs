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

    CompositeDisposable _disposable = new CompositeDisposable();

    public override void InitArg(UIArg arg)
    {
        _arg = arg as UILottoArg;
    }

    private void Start()
    {
        _topButton.OnClickAsObservable().Subscribe(_ =>
        {
            scrollView.SetScrollPosition(1);
        }).AddTo(_disposable);

        _bottomButton.OnClickAsObservable().Subscribe(_ =>
        {
            scrollView.SetScrollPosition(_arg.liLottoResponse.Count);
        }).AddTo(_disposable);

        scrollView.SetItemList(_arg.liLottoResponse);
    }
}
