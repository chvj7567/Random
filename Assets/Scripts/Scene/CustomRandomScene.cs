using UnityEngine;
using UnityEngine.UI;
using UniRx;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class CustomRandomScene : MonoBehaviour
{
    [SerializeField] private TMP_InputField _customInput;
    [SerializeField] private ButtonEx _menuButton;
    [SerializeField] private CustomScrollView scrollView;
    [SerializeField] private ButtonEx _plusButton;
    [SerializeField] private ButtonEx _minusButton;
    [SerializeField] private ButtonEx _resultButton;

    private ReactiveCollection<string> _liCustomText = new ReactiveCollection<string>();
    private IMainSceneManager _mainSceneManager;

    private void OnEnable()
    {
        _resultButton.SetText("Random");
    }

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
            if (_liCustomText.Count == 0)
            {
                _resultButton.SetText(string.Empty);
            }
            else
            {
                _resultButton.SetText(_liCustomText[Random.Range(0, _liCustomText.Count)]);
            }
        });
    }

    public void SetManager(IMainSceneManager manager)
    {
        _mainSceneManager = manager;
    }

    public void Close()
    {
        gameObject.SetActive(false);
        _mainSceneManager.ShowMainScene();
    }
}
