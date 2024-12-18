using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class UILotto : MonoBehaviour
{
    [SerializeField] int index;
    [SerializeField] private Button lastRoundButton;
    [SerializeField] private LottoScrollView scrollView;

    public void Init(List<LottoResponse> liLottoResponse)
    {
        lastRoundButton.OnClickAsObservable().Subscribe(_ =>
        {
            scrollView.SetScrollPosition(index);
        }).AddTo(this);

        scrollView.SetItemList(liLottoResponse);
    }
}
