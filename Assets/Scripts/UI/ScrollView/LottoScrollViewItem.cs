using ChvjUnityInfra;
using System.Collections.Generic;
using UnityEngine;

public class LottoScrollViewItem : MonoBehaviour
{
    [SerializeField] private CHText _roundText;
    [SerializeField] List<CHButton> _liNumberButton = new List<CHButton>();

    int _index;
    LottoResponse _lottoResponse;

    public void Init(int index, LottoResponse lottoResponse)
    {
        _index = index;
        _lottoResponse = lottoResponse;

        _roundText.SetText(lottoResponse.drwNo);
        _liNumberButton[0].SetText($"{lottoResponse.drwtNo1}");
        _liNumberButton[1].SetText($"{lottoResponse.drwtNo2}");
        _liNumberButton[2].SetText($"{lottoResponse.drwtNo3}");
        _liNumberButton[3].SetText($"{lottoResponse.drwtNo4}");
        _liNumberButton[4].SetText($"{lottoResponse.drwtNo5}");
        _liNumberButton[5].SetText($"{lottoResponse.drwtNo6}");
        _liNumberButton[6].SetText($"{lottoResponse.bnusNo}");
    }
}
