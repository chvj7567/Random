using ChvjUnityInfra;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class CustomRandomScene : MonoBehaviour, IRouletteBackButton
{
    [SerializeField] private TMP_InputField _customInput;
    [SerializeField] private CHButton _menuButton;
    [SerializeField] private CustomScrollView scrollView;
    [SerializeField] private CHButton _plusButton;
    [SerializeField] private CHButton _minusButton;
    [SerializeField] private CHButton _resultButton;

    private ReactiveCollection<string> _liCustomText = new ReactiveCollection<string>();
    private IRouletteSceneAccess _rouletteSceneAccess;

    private void Start()
    {
        _menuButton.OnClick(() =>
        {
            Close();
        });

        _liCustomText.ObserveCountChanged().Subscribe(_ =>
        {
            scrollView.SetItemList(_liCustomText.ToList());
        }).AddTo(this);

        _plusButton.OnClick(() =>
        {
            _liCustomText.Add(_customInput.text);
        });

        _minusButton.OnClick(() =>
        {
            if (_liCustomText.Count == 0)
                return;

            _liCustomText.RemoveAt(_liCustomText.Count - 1);
        });

        _resultButton.OnClick(() =>
        {
            if (_liCustomText.Count > 0)
            {
                CHMUI.Instance.ShowUI(CommonEnum.EUI.UIRoulette, new UIRouletteArg
                {
                    liText = _liCustomText.ToList(),
                });
            }
        });
    }

    public void SetRouletteSceneAccess(IRouletteSceneAccess rouletteSceneAccess)
    {
        _rouletteSceneAccess = rouletteSceneAccess;
    }

    public void Close()
    {
        gameObject.SetActive(false);
        _rouletteSceneAccess.ShowScene(CommonEnum.ERouletteMenu.Menu);
    }
}
