using System.Collections.Generic;
using UnityEngine;

public class LottoScrollViewItem : MonoBehaviour
{
    [SerializeField] List<ButtonEx> _liButtonEx = new List<ButtonEx>();

    int _index;
    LottoResponse _lottoResponse;

    public void Init(int index, LottoResponse lottoResponse)
    {
        _index = index;
        _lottoResponse = lottoResponse;

        _liButtonEx[0].SetText($"{lottoResponse.drwtNo1}");
        _liButtonEx[1].SetText($"{lottoResponse.drwtNo2}");
        _liButtonEx[2].SetText($"{lottoResponse.drwtNo3}");
        _liButtonEx[3].SetText($"{lottoResponse.drwtNo4}");
        _liButtonEx[4].SetText($"{lottoResponse.drwtNo5}");
        _liButtonEx[5].SetText($"{lottoResponse.drwtNo6}");
        _liButtonEx[6].SetText($"{lottoResponse.bnusNo}");
    }
}
