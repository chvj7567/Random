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

    [SerializeField] private ButtonEx _topButton;
    [SerializeField] private ButtonEx _bottomButton;
    [SerializeField] private LottoScrollView scrollView;

    public override void InitUI(CommonEnum.EUI uiType, UIArg arg)
    {
        base.InitUI(uiType, arg);

        _arg = arg as UILottoArg;

        _topButton.OnClick(() =>
        {
            scrollView.SetScrollPosition(0);
        });

        _bottomButton.OnClick(() =>
        {
            scrollView.SetScrollPosition(_arg.liLottoResponse.Count - 1);
        });

        scrollView.SetItemList(_arg.liLottoResponse);
    }
}
